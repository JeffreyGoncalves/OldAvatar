using LecturerTrainer.ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Windows;
using System.Windows.Media;

namespace LecturerTrainer.Model
{

    /// <summary>
    /// Queue of the positions of a joint during [time] seconds.
    /// </summary>
    /// <remarks>Author: Clement Michard</remarks>
    class Agitation : Queue<Point3D>
    {

        #region STATIC PROPERTIES
        
        /// <summary>
        /// I know that the kinect frame rate is 30 fps but how to get this value from the Microsoft.Kinect classes?
        /// </summary>
        private static int KINECT_RATE = 30;

        /// <summary>
        /// Default value of an agitation time interval. 
        /// </summary>
        private static double TIME = 1.3;

        /// <summary>
        /// Different tresholds for the main part of the body
        /// Added by Baptiste Germond
        /// </summary>
        private static double relevantTresholdHand = 0.7;//0.36;//0.0015
        private static double relevantTresholdHip = 0.011;//0.0096;//0.0004;
        private static double relevantTresholdShoulder = 0.1;//0.084;//0.00035;
        //private static double relevantTresholdElbow = 0.5;
        private static double relevantTresholdKnee = 0.3;//0.24;//0.001;

        private static double agitationSensitivity = 1;
        /// <summary>
        /// Get and set the value of the agitation sensitivity bar
        /// Modifier by Baptiste Germond
        /// </summary>
        public static double RelevantTreshold
        {
            get
            {
                return agitationSensitivity * 100;
            }
            set
            {
                agitationSensitivity = value / 100;
            }
        }

        /// <summary>
        /// This collection allow to detect agitation of each joint.
        /// </summary>
        public static Dictionary<JointType, Agitation> agitation = new Dictionary<JointType, Agitation>();

        /// <summary>
        /// List of too agitated joints
        /// </summary>
        private static List<JointType> tooAgitatedJoints = new List<JointType>();

        /// <summary>
        /// The program set 'detect' to true when the kinect is ready so that the detection start.
        /// </summary>
        public static bool detect = false;

        /// <summary>
        /// Event fired when a joint is too agitated and when it stopped.
        /// </summary>
        public static event EventHandler<InstantFeedback> agitationEvent;

        /// <summary>
        /// Feedback to display when the skeleton is too agitated.
        /// </summary>
        public static string tooAgitatedText = "Too agitated!";

        /// <summary>
        /// Allow to record for each joint when it was agitated and when it was not.
        /// </summary>
        //public static Dictionary<JointType, Pair > agitNotAgit = null;
        public static Dictionary<JointType, List<int>> agitNotAgit = null;

        /// <summary>
        /// True if we have to record the proportion agitated / not agitated.
        /// </summary>
        private static bool rec = false;

        public static bool feedAg = false;

		/// <summary>
		/// For recording the times when the user was too agitated.
		/// </summary>
		public static Dictionary<double, byte> agitationRecord = new Dictionary<double, byte>();

        /// <summary>
        /// Public attribute for rec. Allows to reset the structures.
        /// </summary>
        public static bool record
        {
            get
            {
                return rec;
            }
            set
            {
                rec = value;
                if (rec)
                {
                    agitNotAgit = new Dictionary<JointType, List<int>>();
                }
            }
        }

        /// <summary>
        /// Count the number of frame recorded
        /// Added by Baptiste Germond
        /// </summary>
        private static int nbFrameRecorded = 0;
        public static int getnbFrameRecorded()
        {
            return nbFrameRecorded;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Time (in seconds) of an agitation. Beyond, an event will be fired to notify the user that he is too agitated.
        /// </summary>
        private double time;

        /// <summary>
        /// Treshold above which the distance traveled by the joint is too long (it means too agitated).
        /// </summary>
        private double treshold;

        /// <summary>
        /// Joint observed.
        /// </summary>
        private JointType jointType;

        #endregion
        
        #region CONSTRUCTOR

        /// <summary>
        /// Construct an Agitation.
        /// </summary>
        /// <param name="time">Duration of an agitation beyond it can be "too agitated".</param>
        /// <param name="j">Joint to observe.</param>
        /// <param name="treshold">Treshold above which the distance traveled by the joint is too long (it means too agitated).</param>
        /// <remarks>Author: Clement Michard</remarks>
        public Agitation(JointType joint)
        {
            this.jointType = joint;
            this.time = TIME;
            this.treshold = getRelevantTreshold(jointType);
        }

        #endregion

        #region STATIC MEMBERS

        /// <summary>
        /// Launch the detection of the agitation.
        /// </summary>
        /// <param name="sk">Skeleton to observe.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public static void testAgitation(Skeleton sk)
        {
			//System.Diagnostics.Debug.WriteLine(Environment.TickCount - ticksTest); ~15ms or so
            //Modified and added by Nicolas DONORE
            List<JointType> testedJoints = new List<JointType>();
            testedJoints.Add(JointType.HandLeft);
            testedJoints.Add(JointType.HandRight);
            //testedJoints.Add(JointType.ElbowLeft);
            //testedJoints.Add(JointType.ElbowRight);
            testedJoints.Add(JointType.HipCenter);
            testedJoints.Add(JointType.ShoulderLeft);
            testedJoints.Add(JointType.ShoulderRight);
            testedJoints.Add(JointType.KneeLeft);
            testedJoints.Add(JointType.KneeRight);

            foreach (JointType joint in testedJoints)
            {
                agitationJoint(sk, joint);
            }

        }


        /// <summary>
        /// Lauch the detection of the agitation of a joint.
        /// </summary>
        /// <param name="sk">Skeleton containing the joint.</param>
        /// <param name="j">Joint to observe.</param>
        /// <remarks>Author: Clement Michard</remarks>
        /// Modified by Baptiste Germond, changing to counting the total of frame to do less calcul
        /// And deactivating the agitation of the legs when not tracked
        private static void agitationJoint(Skeleton sk, JointType j)
			{
            if (sk.Joints[j].TrackingState == JointTrackingState.Tracked)
            {
                /*Ensure that if the leg are not tracked it will not count them in the agitation method*/
                if ( !( DrawingSheetAvatarViewModel.Get().LegTracked == false && 
                        ( sk.Joints[j].JointType == JointType.HipCenter 
                        //|| sk.joints[j].JointType == JointType.HipLeft
                        //|| sk.Joints[j].JointType == JointType.HipRight 
                        || sk.Joints[j].JointType == JointType.KneeLeft
                        || sk.Joints[j].JointType == JointType.KneeRight )))
                {
                    //If we record the performance and the List conting the agitation of the joint has not the joint observe
                    if (rec && !agitNotAgit.Keys.Contains(j))
                    {
                        //We create the entry in the List
                        agitNotAgit[j] = new List<int>();
                    }
                    //If the dictionary don't contain the joint observed
                    if (!agitation.ContainsKey(j))
                    {
                        //We create the new Agitation and put it in the Dictionary
                        agitation[j] = new Agitation(j);
                    }
                    //Adding the position of the joint
                    agitation[j].Enqueue(Geometry.refKinectToSkeleton(new Point3D(sk.Joints[j].Position), sk)); 
					if (agitation[j].Count == agitation[j].time * KINECT_RATE)
                    {
                        //Return wether the joint is too agitated or not
                        bool agitated = agitation[j].tooAgitated();
                        //If we are recoring
                        if (rec)
                        {
                            if (j == JointType.HandLeft)//We had to choose one joint to count only one time each frame
                            {
                                //Counting the number of frame recorded
                                nbFrameRecorded++;
                            }
                            if (!agitNotAgit[j].Contains((int)(Tools.getStopWatch() / 100 )) && agitated)
                            {
                                //Adding the time where the joint was agitated during the recording
                                agitNotAgit[j].Add((int)(Tools.getStopWatch() /100 ));
                            }
                        }
                        //If the joint is too agitated
                        if (!tooAgitatedJoints.Contains(j) && agitated)
                        {
                            //Adding it to the List and raising the feedback
                            tooAgitatedJoints.Add(j);
                            agitationEvent(j, new InstantFeedback(tooAgitatedText));
                            feedAg = true;
                        }
                        //If not agitated
                        else if (tooAgitatedJoints.Contains(j) && !agitated)
                        {
                            //Removing it from the list
                            tooAgitatedJoints.Remove(j);
                            //If the list is empty we stop the feedback
                            if (tooAgitatedJoints.Count == 0)
                            {
                                feedAg = false;
                            }
                        }
						// Somehow this is useful for the agitation icon to be displayed correctly during replays
                        else if (agitated) 
                        {
                            agitationEvent(j, new InstantFeedback(tooAgitatedText));
                        }

						// Recording of the values necessary for the .csv file
						if(rec && j == JointType.HandLeft){
							if (feedAg) agitationRecord.Add(Tools.getStopWatch() / 1000.0, 1);
							else agitationRecord.Add(Tools.getStopWatch() / 1000.0, 0);
						}
                    }
                }
            }
			// If a joint is no longer tracked, we consider it as "not agitated"
			else{
				tooAgitatedJoints.Remove(j);
                //If the list is empty we stop the feedback
                if (tooAgitatedJoints.Count == 0)
				{
					feedAg = false;
				}
			}
        }

        public static void removeAgitation(Skeleton sk)
        {
            feedAg = false;
        }

        /// <summary>
        /// Adding the values of agitation for each joint for the statistics
        /// </summary>
        /// <remarks>Added by Baptiste Germond</remarks>
        public static List<KeyValuePair<JointType, bool>> getCatchedJoin()
        {
            List<KeyValuePair<JointType, bool>> tab = new List<KeyValuePair<JointType, bool>>();

            if (agitNotAgit != null)
            {
                foreach (JointType join in Enum.GetValues(typeof(JointType)))
                {
                    if (agitNotAgit.ContainsKey(join))
                    {
                        tab.Add(new KeyValuePair<JointType, bool>(join, true));
                    }
                }
            }
            return tab;
        }

        /// <summary>
        /// Method for obtaining charts of the body agitation
        /// </summary>
        /// <returns>a list of IGraph of agitation charts</returns>
        /// <remarks>Add by Florian BECHU: Summer 2016</remarks>
        public static List<IGraph> getAgitationStats()
        {
            List<IGraph> list = new List<IGraph>();
            bool hleft = false, hright = false, sleft=false, sright=false, kleft = false, kright=false;

            /*****     HIPS     *****/
            if (agitNotAgit.ContainsKey(JointType.HipCenter))
            {
                var chart = new CartesianGraph();
                chart.title = "Hips agitation";
                chart.subTitle = Tools.ChooseTheCorrectUnitTime();
                if (!Tools.addSeriesToCharts(chart,new ColumnSeries(),"Hips", agitNotAgit[JointType.HipCenter],"Total Hips agitation: ",false))
                    list.Add(Tools.createEmptyGraph("Hips were not agitated"));
                else
                    list.Add(chart);
            }
            else
                list.Add(Tools.createEmptyGraph("Hips were not caught"));

            /*****     HANDS     *****/
            if (!agitNotAgit.ContainsKey(JointType.HandLeft)) // if the left hand wasn't catched
                list.Add(Tools.createEmptyGraph("Left hand wasn't caught"));
            else
                hleft = true;

            if (!agitNotAgit.ContainsKey(JointType.HandRight)) // if the right hand wasn't catched
                list.Add(Tools.createEmptyGraph("Right hand wasn't caught"));
            else
                hright = true;

            if (hleft && hright)
            {
                CartesianGraph cl = new CartesianGraph();
                cl.title = "Hands agitation";
                cl.subTitle = "Time unit: " + Tools.ChooseTheCorrectUnitTime();
                cl.XTitle = "Time";
                cl.YTitle = "Value";
                bool leftChart = Tools.addSeriesToCharts(cl, new ColumnSeries(), "Left Hand", agitNotAgit[JointType.HandLeft],"Total Left Hand Agitation: ",true);
                bool rightChart = Tools.addSeriesToCharts(cl, new ColumnSeries(), "Right Hand", agitNotAgit[JointType.HandRight], "Total Right Hand Agitation: ",true);
                
                if (!leftChart && !rightChart)
                    list.Add(Tools.createEmptyGraph("Hands were not agitated"));
                else
                    list.Add(cl);

                if (!leftChart)
                    list.Add(Tools.createEmptyGraph("Left hand wasn't agitated"));
                else
                {
                    var chart = new CartesianGraph();
                    chart.title = "Left Hand agitation";
                    chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                    if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Left hand", agitNotAgit[JointType.HandLeft], "Total Left Hand Agitation: ",false))
                        list.Add(Tools.createEmptyGraph("Left hand wasn't agitated"));
                    else
                        list.Add(chart);
                }


                if (!rightChart)
                    list.Add(Tools.createEmptyGraph("Right hand wasn't agitated"));
                else
                {
                    var chart = new CartesianGraph();
                    chart.title = "Right Hand agitation";
                    chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                    if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Right hand", agitNotAgit[JointType.HandRight], "Total Right Hand Agitation: ",false))
                        list.Add(Tools.createEmptyGraph("Right hand wasn't agitated"));
                    else
                        list.Add(chart);
                }


            }
            else if(hleft)
            {
                var chart = new CartesianGraph();
                chart.title = "Left Hand agitation";
                chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Left hand", agitNotAgit[JointType.HandLeft], "Total Left Hand Agitation: ",false))
                    list.Add(Tools.createEmptyGraph("Left hand wasn't agitated"));
                else
                    list.Add(chart);
            }
            else if(hright)
            {
                var chart = new CartesianGraph();
                chart.title = "Right Hand agitation";
                chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Right hand", agitNotAgit[JointType.HandRight], "Total Right Hand Agitation: ",false))
                    list.Add(Tools.createEmptyGraph("Right hand wasn't agitated"));
                else
                    list.Add(chart);
            }

            /*****     SHOULDERS     *****/
            if (!agitNotAgit.ContainsKey(JointType.ShoulderLeft)) // if the left shoulder wasn't catched
                list.Add(Tools.createEmptyGraph("Left shoulder wasn't caught"));
            else
                sleft = true;

            if (!agitNotAgit.ContainsKey(JointType.ShoulderRight)) // if the right shoulder wasn't catched
                list.Add(Tools.createEmptyGraph("Right shoulder wasn't caught"));
            else
                sright = true;

            if (sleft && sright)
            {
                CartesianGraph cl = new CartesianGraph();
                cl.title = "Shoulders agitation";
                cl.subTitle = "Time unit: " + Tools.ChooseTheCorrectUnitTime();

                bool leftChart = Tools.addSeriesToCharts(cl, new ColumnSeries(), "Left Shoulder", agitNotAgit[JointType.ShoulderLeft],"Total Left Shoulder Agitation: ",true);
                bool rightChart = Tools.addSeriesToCharts(cl, new ColumnSeries(), "Right Shoulder", agitNotAgit[JointType.ShoulderRight], "Total Right Shoulder Agitation: ",true);

                if (!leftChart && !rightChart)
                    list.Add(Tools.createEmptyGraph("Shoulders were not agitated"));
                else
                    list.Add(cl);

                if (!leftChart)
                    list.Add(Tools.createEmptyGraph("Left shoulder wasn't agitated"));
                else
                {
                    var chart = new CartesianGraph();
                    chart.title = "Left Shoulder agitation";
                    chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                    if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Left shoulder", agitNotAgit[JointType.ShoulderLeft], "Total Left Shoulder Agitation: ",false))
                        list.Add(Tools.createEmptyGraph("Left shoulder wasn't agitated"));
                    else
                        list.Add(chart);
                }


                if (!rightChart)
                    list.Add(Tools.createEmptyGraph("Right shoulder wasn't agitated"));
                else
                {
                    var chart = new CartesianGraph();
                    chart.title = "Right Shoulder agitation";
                    chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                    if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Right Shoulder", agitNotAgit[JointType.ShoulderRight], "Total Right Shoulder Agitation: ",false))
                        list.Add(Tools.createEmptyGraph("Right shoulder wasn't agitated"));
                    else
                        list.Add(chart);
                }


            }
            else if (sleft)
            {
                var chart = new CartesianGraph();
                chart.title = "Left Shoulder agitation";
                chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Left shoulder", agitNotAgit[JointType.ShoulderLeft], "Total Left Shoulder Agitation: ",false))
                    list.Add(Tools.createEmptyGraph("Left shoulder wasn't agitated"));
                else
                    list.Add(chart);
            }
            else if (sright)
            {
                var chart = new CartesianGraph();
                chart.title = "Right Shoulder agitation";
                chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Right Shoulder", agitNotAgit[JointType.ShoulderRight], "Total Right Shoulder Agitation: ",false))
                    list.Add(Tools.createEmptyGraph("Right shoulder wasn't agitated"));
                else
                    list.Add(chart);
            }

            /*****     KNEES     *****/
            if (!agitNotAgit.ContainsKey(JointType.KneeLeft)) // if the left knee wasn't catched
                list.Add(Tools.createEmptyGraph("Left knee wasn't caught"));
            else
                kleft = true;

            if (!agitNotAgit.ContainsKey(JointType.KneeRight)) // if the right knee wasn't catched
                list.Add(Tools.createEmptyGraph("Right knee wasn't caught"));
            else
                kright = true;

            if (kleft && kright)
            {
                CartesianGraph cl = new CartesianGraph();
                cl.title = "Knees agitation";
                cl.subTitle = "Time unit: " + Tools.ChooseTheCorrectUnitTime();

                bool leftChart = Tools.addSeriesToCharts(cl, new ColumnSeries(), "Left Knee", agitNotAgit[JointType.KneeLeft], "Total Left Knee Agitation: ",true);
                bool rightChart = Tools.addSeriesToCharts(cl, new ColumnSeries(), "Right Knee", agitNotAgit[JointType.KneeRight],"Total Right Knee Agitation: ",true);

                if (!leftChart && !rightChart)
                    list.Add(Tools.createEmptyGraph("Knees were not agitated"));
                else
                    list.Add(cl);

                if (!leftChart)
                    list.Add(Tools.createEmptyGraph("Left knee wasn't agitated"));
                else
                {
                    var chart = new CartesianGraph();
                    chart.title = "Left Knee agitation";
                    chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                    if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Left Knee", agitNotAgit[JointType.KneeLeft], "Total Left Knee Agitation: ",false))
                        list.Add(Tools.createEmptyGraph("Left knee wasn't agitated"));
                    else
                        list.Add(chart);
                }


                if (!rightChart)
                    list.Add(Tools.createEmptyGraph("Right knee wasn't agitated"));
                else
                {
                    var chart = new CartesianGraph();
                    chart.title = "Right Knee agitation";
                    chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                    if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Right Knee", agitNotAgit[JointType.KneeRight], "Total Right Knee Agitation: ",false))
                        list.Add(Tools.createEmptyGraph("Right knee wasn't agitated"));
                    else
                        list.Add(chart);
                }

            }
            else if (kleft)
            {
                var chart = new CartesianGraph();
                chart.title = "Left Knee agitation";
                chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Left Knee", agitNotAgit[JointType.KneeLeft], "Total Left Knee Agitation: ",false))
                    list.Add(Tools.createEmptyGraph("Left knee wasn't agitated"));
                else
                    list.Add(chart);
            }
            else if (kright)
            {
                var chart = new CartesianGraph();
                chart.title = "Right Knee agitation";
                chart.subTitle = Tools.ChooseTheCorrectUnitTime();

                if (!Tools.addSeriesToCharts(chart, new ColumnSeries(), "Right Knee", agitNotAgit[JointType.KneeRight], "Total Right Knee Agitation: ",false))
                    list.Add(Tools.createEmptyGraph("Right knee wasn't agitated"));
                else
                    list.Add(chart);
            }

            return list;
        }

        /// <summary>
        /// Return a relevant treshold for the agitation depending of the joint.
        /// </summary>
        /// <param name="j">Related joint.</param>
        /// <returns>The treshold.</returns>
        /// <remarks>Added by Baptiste Germond</remarks>
        private static double getRelevantTreshold(JointType j)
        {
            switch (j)
            {
                case JointType.HandLeft:
                case JointType.HandRight:
                    return relevantTresholdHand * agitationSensitivity;

                //case JointType.ElbowLeft:
                //case JointType.ElbowRight:
                //    return relevantTresholdElbow * agitationSensitivity;


                case JointType.HipCenter:
                    return relevantTresholdHip * agitationSensitivity;

                case JointType.ShoulderLeft:
                case JointType.ShoulderRight:
                    return relevantTresholdShoulder * agitationSensitivity;

                case JointType.KneeLeft:
                case JointType.KneeRight:
                    return relevantTresholdKnee * agitationSensitivity;
            }
            return 10000; // High value if the joint type is not known
        }
        #endregion

        #region MEMBERS

        public static void raiseAgitationEvent(ServerFeedback feedback)
        {
            agitationEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }

        /// <summary>
        /// Add a position to the collection.
        /// </summary>
        /// <param name="p">Position to add.</param>
        /// <remarks>Author: Clement Michard</remarks>
        private new void Enqueue(Point3D p)
        {
            base.Enqueue(p);
            if (this.Count > time * KINECT_RATE)
            {
                this.Dequeue();
            }
        }

        /// <summary>
        /// Compute the agitation level (the square distance traveled by the joint during the "time").
        /// </summary>
        /// <returns>Agitation level.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        private double getAgitationLevel()
        {
            double res = 0;

            for (int i = 0; i < Count - 1; i++)
            {
                res += Geometry.distanceSquare(this.ElementAt(i), this.ElementAt(i + 1));
            }
            return res;
        }

        /// <summary>
        /// Return true if the joint is too agitated.
        /// </summary>
        /// <returns>Return true if the joint is too agitated.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        private bool tooAgitated(){
            return getAgitationLevel() > treshold;
        }

        #endregion
    }
}