using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Record the position of the joints of a posture.
    /// </summary>
    /// <remarks>The features with '*' means that the current interface does not allow to use the feature. The view doesn't make use of these features. These features allows to add specific postures in the computer with feedbacks.</remarks>
    /// <remarks>Author: Clement Michard</remarks>
    /// Class not used
    class Posture : Dictionary<JointType,Point3D>
    {
        
        #region STATIC PROPERTIES

        /// <summary>
        /// List of postures which are comparing.
        /// </summary>
        private static List<Posture> testing = null;

        /// <summary>
        /// The program set 'compare' to true when the kinect is ready so that the comparison start.
        /// </summary>
        public static bool compare = false;

        /// <summary>
        /// Event fired when a posture is recognized.
        /// </summary>
        public static event EventHandler<InstantFeedback> postureEvent;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Accuracy of the imported posture. If the accuracy is set to high, the user's posture must be exactly the same than the imported posture.
        /// </summary>
        public double accuracy { get; private set; }

        /// <summary>
        /// The name of the posture (when it is imported from a file).
        /// </summary>
        public string name {get; private set;}

        /// <summary>
        /// The feedback associated with the imported posture.
        /// </summary>
        public string feedback { get; private set; }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Construct the current posture of the skeleton.
        /// </summary>
        /// <param name="watchedSkeletonAbs">Skeleton from which to construct the posture.</param>
        /// <param name="postureName">Name of the posture.</param>
        /// <param name="feedback">Associated feedback.</param>
        /// <param name="accuracy">Accuracy of the positions that joints have to get in order to be recognize.</param>
        /// <remarks>Author: Clement Michard</remarks>
        /// <remarks>*</remarks>
        public Posture(Skeleton watchedSkeletonAbs, string postureName, string feedback, double accuracy)
            : base()
        {
            this.accuracy = accuracy;
            this.name = postureName;
            this.feedback = feedback;
            foreach (Joint j in watchedSkeletonAbs.Joints)
            {
                if (Main.jointsToObserve[(int)j.JointType])
                {
                    this[j.JointType] = Geometry.refKinectToSkeleton(new Point3D(watchedSkeletonAbs.Joints[j.JointType].Position), watchedSkeletonAbs);
                }
            }
        }

        /// <summary>
        /// Create a posture from a file.
        /// </summary>
        /// <param name="filename">Name of the file containing the posture.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public Posture(string filename)
            : base()
        {
            StreamReader file = new StreamReader(filename);
            string line;
            if (file == null)
            {
                throw new Exception("File " + filename + " not found");
            }
            if (null != (line = file.ReadLine()))
            {
                string[] sp = line.Split(' ');
                name = sp[1];
                accuracy = Tools.str2Double(sp[3]);
            }
            feedback = file.ReadLine();
            while (null != (line = file.ReadLine()))
            {
                string[] jointInfos = line.Split(' ');
                this[(JointType)int.Parse(jointInfos[0])] = new Point3D(Tools.str2Double(jointInfos[1]), Tools.str2Double(jointInfos[2]), Tools.str2Double(jointInfos[3]));
            }
        }

        #endregion

        #region STATIC MEMBERS

        /// <summary>
        /// Initialize the posture's fields.
        /// </summary>
        /// <remarks>Author: Clement Michard</remarks>
        public static void init()
        {
            testing = new List<Posture>();
            importPostures(new DirectoryInfo(Directory.GetCurrentDirectory()));
        }

        /// <summary>
        /// Import .posture files from directory.
        /// </summary>
        /// <param name="dir">Directory containing the .posture files.</param>
        /// <remarks>Author: Clement Michard</remarks>
        private static void importPostures(DirectoryInfo dir)
        {
            try
            {
                foreach (FileInfo file in dir.GetFiles("*.posture"))
                {
                    testing.Add(new Posture(file.Name));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Incorrect path!");
                throw new Exception();
            }
        }


        /// <summary>
        /// Try to recognize the current posture.
        /// </summary>
        /// <param name="sk">Current skeleton.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public static void testCompare(Skeleton sk)
        {
            foreach (Posture p in testing)
            {
                if (p.isEquiv(sk))
                {
                    postureEvent(p, new InstantFeedback(p.feedback));
                }
            }
        }

        #endregion

        #region MEMBERS
        public static void raisePostureEvent(ServerFeedback feedback)
        {
            postureEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }

        /// <summary>
        /// Create a file .posture containing the posture.
        /// </summary>
        /// <param name="filename">File name</param>
        /// <remarks>Author: Clement Michard</remarks>
        /// <remarks>*</remarks>
        public void writeInFile(string filename)
        {
            StreamWriter file = new StreamWriter(filename);
            file.WriteLine("Name " + name + " Accuracy " + Tools.format(accuracy));
            file.WriteLine(feedback);
            foreach (JointType j in Keys)
            {
                file.WriteLine("" + (int)j + " " + this[j].ToString());
            }
            file.Flush();
            file.Close();
        }


        /// <summary>
        /// Compare the posture and the current skeleton.
        /// </summary>
        /// <param name="sk">Skeleton to compare.</param>
        /// <returns>true if the skeleton has the posture, false otherwise.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        private bool isEquiv(Skeleton sk)
        {
            foreach (JointType joint in Keys)
            {
                if (Geometry.distanceSquare(this[joint], Geometry.refKinectToSkeleton(new Point3D(sk.Joints[joint].Position), sk)) > accuracy)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}