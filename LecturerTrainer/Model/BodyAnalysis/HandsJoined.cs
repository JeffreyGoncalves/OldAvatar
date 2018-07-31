using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Windows;
using System.Windows.Media;
using LecturerTrainer.View;
using System.Collections.ObjectModel;
using LiveCharts.Wpf;
using LiveCharts;
using System.Diagnostics;

namespace LecturerTrainer.Model
{
    class HandsJoined
    {
        public static bool detect = false;
        /* the variable detect is changed to true when the checkbox is checked
         * this variable is changed in the useFeedbackFunc in SideToolsViewModel.cs
         * useFeedbackFunc is called by useFeedback (also in SideToolsViewModel.cs
         * useFeedback is bound to the Feedbacks checkbos in SideToolsView.xaml
         * detect also occurs in KinectDevice.cs
         * if it is true then the function startDetection (below) is called in OnAllFramesReady
         */

        public static event EventHandler<InstantFeedback> handsJoinedEvent;
        /*this variable handsJoinedEvent is of type event
         * this happens in the function below
         * handsJoinedEvent is used in LogUserViewModel to display new feedback on the screen
         * this happens in the function LogUserViewModel
         */

        /// <summary>
        /// During a record
        /// This list will contain all the moment when the user has his hands joined
        /// </summary>
        public static List<int> handsjoined = null;

        /// <summary>
        /// During a record
        /// This list will contain all the moment when the user touches his hands
        /// </summary>
        public static List<int> handsjoinedCounter = null;

		/// <summary>
		/// For recording the times where the user had is hands joined
		/// </summary>
		public static Dictionary<double, byte> handsJoinedRecord = new Dictionary<double, byte>();

        /// <summary>
        /// True if we have to record the superposition of the hand
        /// </summary>
        private static bool rec = false;

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
                    handsjoined = new List<int>();
                    handsjoinedCounter = new List<int>();
                }
            }
        }


        /// <summary>
        /// bool that allowes DrawingSheetAvatarViewModel to dispay the handsJoined feedback
        /// </summary>
        public static bool hands = false;

        /// <summary>
        /// it is set to true when the user touches his hands and false when he takes away his hands 
        /// </summary>
        public static bool handsJoined = false;

        /// <summary>
        /// stopwatch for the feedback
        /// </summary>
        public static Stopwatch sw = new Stopwatch();

        /// <summary>
        /// function called to detect the hands joined
        /// </summary>
        /// <param name="sk">the skeleton</param>
        /// <author>Alban Descottes 2018</author>
        public static void startDetection(Skeleton sk)
        {
            if (Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandLeft].Position), new Point3D(sk.Joints[JointType.HandRight].Position)) < 0.01)
            {
                // it starts a timer, the feedback is displayed if the user keeps his hands joined more than 500 milliseconds 
                if (!sw.IsRunning)
                    sw.Start();
                if(sw.ElapsedMilliseconds / 100 > 5)
                {
                    handsJoinedEvent(null, new InstantFeedback("Hands are joined"));
                    hands = true;
                }
                // if the user records himself, it adds in the two lists the time (rounded to one tenth of a second)
                if (rec)
                {
                    //Console.WriteLine("add rec");
                    if (!handsjoined.Contains((int)(Tools.getStopWatch() / 100 )))
                    {
                        handsjoined.Add((int)(Tools.getStopWatch() / 100 ));
                    }
                }
                if (rec)
                {
                    if (!handsjoinedCounter.Contains((int)(Tools.getStopWatch() / 100)) && !handsJoined)
                    {
                        Console.WriteLine("add counter");
                        handsjoinedCounter.Add((int)(Tools.getStopWatch() / 100));
                    }
                    handsJoined = true;  
					handsJoinedRecord.Add(Tools.getStopWatch() / 1000.0, 1);
                }

            }
            // else it resets the timer, and sets the handsJoined to false
            else
            {
                if(sw.IsRunning)
                    sw.Reset();
                hands = false;
                handsJoined = false;
                if (rec)
					handsJoinedRecord.Add(Tools.getStopWatch() / 1000.0, 0);
            }

        }

        public static void raiseHandsJoinedEvent(ServerFeedback feedback)
        {
            handsJoinedEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }


        /// <summary>
        /// function to obtain the number of hands joined, and the duration
        /// </summary>
        /// <returns>the graph</returns>
        /// <author>Alban Descottes 2018</author>
        public static List<IGraph> getHandStatistics()
        {
            List<IGraph> list = new List<IGraph>();
            var chart1 = new CartesianGraph();
            chart1.title = "Hands joined counter";
            chart1.subTitle = Tools.ChooseTheCorrectUnitTime();
            // if there is no hands joined during all the record, it creates just a empty chart
            if (!Tools.addSeriesToCharts(chart1, new ColumnSeries(), "Hand joined", handsjoinedCounter, "Total hands joined: ", false))
            {
                list.Add(Tools.createEmptyGraph("Hands were not joined"));
            }
            // else it adds the hands joined counter first and this about the duration after
            else
            {
                list.Add(chart1);
                var chart2 = new CartesianGraph();
                chart2.title = "Hands joined duration";
                chart2.subTitle = Tools.ChooseTheCorrectUnitTime();
                if (Tools.addSeriesToCharts(chart2, new ColumnSeries(), "Hand joined", handsjoined, "Total time hands joined: ", false))
                    list.Add(chart2);
            }
            return list;
        }
    }
}

/*
 * when you create a new detector you must do the following things :
 * - create a file with the name of the detector
 * in that file create a variable detect which is changed by the checkbox on the interface
 * creat an event which changes when the detector detects something
 * edit KinectDevices and SideToolsViewModel to include the new variable detect
 * edit LogUserViewModel to include the event in order to display the feedback
 * 
*/