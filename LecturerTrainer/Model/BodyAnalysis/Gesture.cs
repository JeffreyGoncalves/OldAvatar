using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace LecturerTrainer.Model
{ 
    /// <summary>
    /// Each joint has a List containing the position of a joint at each frame.
    /// </summary>
    /// <remarks>The features with '*' means that the current interface does not allow to use the feature. The view doesn't make use of these features. These features allows to add specific gestures in the computer with feedbacks.</remarks>
    /// <remarks>Author: Clement Michard</remarks>
    /// This class is not used
    public class Gesture : Dictionary<JointType, List<Point3D>>
    {

        #region STATIC PROPERTIES

        /// <summary>
        /// The maximum size of a gesture is NB_MAX_FRAMES frames.
        /// </summary>
        public static int NB_MAX_FRAMES = 90;

        /// <summary>
        /// List of gestures which are comparing.
        /// </summary>
        private static Dictionary<Gesture, List<Gesture>> testing;

        /// <summary>
        /// The program set 'compare' to true when the kinect is ready so that the comparison start.
        /// </summary>
        public static bool compare = false;

        /// <summary>
        /// Event fired when a gesture is recognized.
        /// </summary>
        public static event EventHandler<InstantFeedback> gestureEvent;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The joint which has traveled the more during the gesture. It can be usefull to describe the gesture.
        /// For example, when the user put his hand up, all the joints of the arm moved but the most important is the hand.
        /// If we want to display the path of the gesture on the screen, we might represent the path of this joint.
        /// </summary>
        public JointType jointMaxMove;

        /// <summary>
        /// The position of the beginning of the gesture.
        /// </summary>
        public Point3D startPoint = null;

        /// <summary>
        /// The position of the end of the gesture.
        /// </summary>
        public Point3D endPoint = null;

        /// <summary>
        /// The name of the gesture (when it is imported from a file).
        /// </summary>
        public string name {get; private set; }

        /// <summary>
        /// The feedback associated with the imported gesture.
        /// </summary>
        public string feedback { get; private set; }

        /// <summary>
        /// Accuracy of the imported gesture. If the accuracy is set to high, the user's gesture must follow exactly the same path than the imported gesture.
        /// </summary>
        public double accuracy { get; private set; }
        
        /// <summary>
        /// Number of frames.
        /// </summary>
        private int nbFrames = 0;

        /// <summary>
        /// Watched skeleton.
        /// </summary>
        private Skeleton skeleton;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Construct an empty Gesture object.
        /// </summary>
        /// <param name="sk">Skeleton observed.</param>
        /// <param name="sk">Accuracy of the gesture.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public Gesture(Skeleton sk, double accuracy) : base()
        {
            skeleton = sk;
            this.accuracy = accuracy;
            for (JointType jointType = 0; (int)jointType < 20; jointType++)
            {
                base.Add(jointType, new List<Point3D>());
            }
        }

        /// <summary>
        /// Construct an empty Gesture object with name.
        /// </summary>
        /// <param name="sk">Skeleton observed.</param>
        /// <param name="name">Name of the gesture.</param>
        /// <param name="feedback">Feedback to associate with the gesture.</param>
        /// <param name="accuracy">Accuracy of the gesture.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public Gesture(Skeleton sk, string name, string feedback, double accuracy) : this(sk, accuracy)
        {
            this.name = name;
            this.feedback = feedback;
        }

        /// <summary>
        /// Import a gesture from a file.
        /// </summary>
        /// <param name="filename">File of the gesture</param>
        /// <remarks>Author: Clement Michard</remarks>
        /// <remarks>*</remarks>
        public Gesture(string filename)
            : base()
        {
            StreamReader file = new StreamReader(filename);
            if (file == null)
            {
                throw new Exception("File " + filename + " not found");
            }
            string line;
            if (null != (line = file.ReadLine()))
            {
                string[] sp = line.Split(' ');
                name = sp[1];
                startPoint = new Point3D(Tools.str2Double(sp[3]), Tools.str2Double(sp[4]), Tools.str2Double(sp[5]));
                endPoint = new Point3D(Tools.str2Double(sp[7]), Tools.str2Double(sp[8]), Tools.str2Double(sp[9]));
                jointMaxMove = (JointType)int.Parse(sp[11]);
                accuracy = Tools.str2Double(sp[13]);
            }

            feedback = file.ReadLine();
            int numFrame = 0;
            for (JointType jointType = 0; (int)jointType < 20; jointType++)
            {
                base.Add(jointType, new List<Point3D>());
            }
            while (null != (line = file.ReadLine()))
            {
                string[] frame = line.Split('\t');
                foreach (string joint in frame)
                {
                    if (joint != "")
                    {
                        string[] component = joint.Split(' ');
                        this[(JointType)int.Parse(component[0])].Add(new Point3D(Tools.str2Double(component[1]), Tools.str2Double(component[2]), Tools.str2Double(component[3])));
                    }
                }
                numFrame++;
            }
            nbFrames = numFrame;
        }

        #endregion

        #region STATIC MEMBERS

        /// <summary>
        /// Manage gestures and start the comparisons.
        /// </summary>
        /// <param name="sk">Skeleton observed</param>
        /// <remarks>Author: Clement Michard</remarks>
        public static void testCompare(Skeleton sk)
        {
            Dictionary<Gesture, List<Gesture>> removeFromTesting = new Dictionary<Gesture, List<Gesture>>();
            foreach (Gesture g in testing.Keys)
            {
                removeFromTesting[g] = new List<Gesture>();
            }
            foreach (Gesture g in testing.Keys)
            {
                Point3D jMaxMovePos = Geometry.refKinectToSkeleton(new Point3D(sk.Joints[g.jointMaxMove].Position), sk);
                if (Geometry.distanceSquare(g.startPoint, jMaxMovePos) < g.accuracy)
                {
                    Gesture newGesture = new Gesture(sk, g.accuracy);
                    testing[g].Add(newGesture);
                }
                foreach (Gesture gTry in testing[g])
                {
                    bool timeOut = !gTry.AddJoints(sk);
                    if (timeOut || Geometry.distanceSquare(g.endPoint, jMaxMovePos) < g.accuracy)
                    {
                        Gesture finishedGesture = gTry.complete();
                        removeFromTesting[g].Add(gTry);
                        if (finishedGesture.Equals(g))
                        {
                            //Console.WriteLine(g.feedback);
                            gestureEvent(g, new InstantFeedback(g.feedback));
                        }
                    }
                }
            }

            foreach (Gesture g in removeFromTesting.Keys)
            {
                foreach (Gesture gTry in removeFromTesting[g])
                    testing[g].Remove(gTry);
            }
        }


        /// <summary>
        /// Init the static fields of Gesture.
        /// </summary>
        /// <remarks>Author: Clement Michard</remarks>
        public static void init()
        {
            testing = new Dictionary<Gesture, List<Gesture>>();
            //importGestures(new DirectoryInfo(path));
            importGestures(new DirectoryInfo(Directory.GetCurrentDirectory()));
        }

        /// <summary>
        /// Lauch the import of all gestures from files.
        /// </summary>
        /// <param name="dir">Directory from which to import.</param>
        /// <remarks>Author: Clement Michard</remarks>
        private static void importGestures(DirectoryInfo dir)
        {
            try
            {
                foreach (FileInfo file in dir.GetFiles("*.gesture"))
                {
                    testing[new Gesture(file.Name)] = new List<Gesture>();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Incorrect path!");
                throw new Exception();
            }
        }

        #endregion

        #region MEMBERS

        public static void raiseGestureEvent(ServerFeedback feedback)
        {
            gestureEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }

        /// <summary>
        /// Compute the start point of the recorded gesture, relative to the skeleton reference.
        /// </summary>
        /// <remarks>Author: Clement Michard</remarks>
        private void setStartPtAndEndPt()
        {
            jointMaxMove = getJointMaxMove();
            startPoint = new Point3D(this[jointMaxMove][0].X, this[jointMaxMove][0].Y, this[jointMaxMove][0].Z);
            int iLast = nbFrames-1;
            endPoint = new Point3D(this[jointMaxMove][iLast].X, this[jointMaxMove][iLast].Y, this[jointMaxMove][iLast].Z);
        }

        /// <summary>
        /// Add the position in the Dictionary of the Gesture for the joint j.
        /// </summary>
        /// <param name="j">The joint to add.</param>
        /// <param name="p">The position of the joint.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public void Add(JointType j, Point3D p)
        {
            this[j].Add(Geometry.refKinectToSkeleton(p, skeleton));
        }

        /// <summary>
        /// Called when a gesture is ended.
        /// </summary>
        /// <returns>The gesture.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public Gesture complete()
        {
            setStartPtAndEndPt();
            return this;
        }

        /// <summary>
        /// Search which joint has traveled the max distance.
        /// </summary>
        /// <returns>The joint.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        private JointType getJointMaxMove()
        {
            double maxDistance2 = 0;
            JointType jointMaxMove = 0;

            foreach (JointType j in Keys)
            {
                if (this[j].Count != 0)
                {
                    double dist2 = 0;

                    for (int i = 1; i < this[j].Count; i++)
                    {
                        dist2 += Geometry.distanceSquare(this[j][i], this[j][i - 1]);
                    }
                    if (dist2 > maxDistance2)
                    {
                        maxDistance2 = dist2;
                        jointMaxMove = j;
                    }
                }
            }
            return jointMaxMove;
        }

        /// <summary>
        /// Compare two gestures.
        /// </summary>
        /// <param name="g">The gesture to compare.</param>
        /// <returns>true if the gestures are equals, false otherwise.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public bool Equals(Gesture g)
        {
            double stepThis, stepG, errorTreshold;
            if (this.nbFrames < g.nbFrames)
            {
                stepThis = 1;
                stepG = g.nbFrames / this.nbFrames;
                errorTreshold = accuracy * this.nbFrames;
            }
            else
            {
                stepThis = this.nbFrames / g.nbFrames;
                stepG = 1;
                errorTreshold = accuracy * g.nbFrames;
            }

            foreach (JointType j in Keys)
            {
                if (g[j].Count != 0)
                {
                    double error = 0;
                    for (double iThis = stepThis, iG = stepG; iThis < this.nbFrames && iG < g.nbFrames; iThis += stepThis, iG += stepG)
                        error += Geometry.distanceSquare(this[j][(int)iThis], g[j][(int)iG]);
                    if (error > errorTreshold) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Write the gesture in a file.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <remarks>Author: Clement Michard</remarks>
        /// <remarks>*</remarks>
        public void writeInFile(string filename)
        {
            StreamWriter file = new StreamWriter(filename);
            file.WriteLine("Name " + name + " StartPoint " + startPoint.ToString() + " EndPoint " + endPoint.ToString() + " JointMaxMove " + (int)jointMaxMove + " Accuracy " + Tools.format(accuracy));
            file.WriteLine(feedback);
            for (int i = 0; i < nbFrames; i++)
            {
                foreach (JointType j in Keys)
                {
                    if (this[j].Count != 0)
                        file.Write("" + (int)j + " " + this[j][i].ToString() + "\t");
                }
                file.Write("\n");
            }
            file.Flush();
            file.Close();
        }

        /// <summary>
        /// Add the positions of joints to the Dictionary of the Gesture.
        /// </summary>
        /// <param name="sk">Skeleton to record.</param>
        /// <param name="jointsToRecord">Joints to record in the skeleton.</param>
        /// <returns>true if the joints have been added, false otherwise.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public bool AddJoints(Skeleton sk, bool[] jointsToRecord)
        {
            for (int i = 0; i < 20; i++)
            {
                if (nbFrames == NB_MAX_FRAMES) return false;
                if (jointsToRecord == null || jointsToRecord[i])
                    this.Add((JointType)i, new Point3D(sk.Joints[(JointType)i].Position));
            }
            nbFrames++;
            return true;
        }

        /// <summary>
        /// Add all the joints to the Dictionary of the Gesture.
        /// </summary>
        /// <param name="sk">Skeleton to record</param>
        /// <returns>true if the joints has been added, false otherwise.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public bool AddJoints(Skeleton sk)
        {
            return AddJoints(sk, null);
        }

        #endregion
    }
}