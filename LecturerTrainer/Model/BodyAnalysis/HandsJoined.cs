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
        /// Allow to record when the hands are joined
        /// </summary>
        public static List<int> handsjoined = null;

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
                }
            }
        }

        public static bool hands = false;
        public static bool eventfinished = true;

        public static Stopwatch sw = new Stopwatch();

        /// <summary>
        /// function called to detect the hands joined
        /// </summary>
        /// <param name="sk">the skeleton</param>
        public static void startDetection(Skeleton sk)
        {
            if (Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandLeft].Position), new Point3D(sk.Joints[JointType.HandRight].Position)) < 0.01)
            {
                if(!sw.IsRunning)
                    sw.Start();
                if(sw.ElapsedMilliseconds / 100 > 7)
                {
                    handsJoinedEvent(null, new InstantFeedback("Hands are joined"));
                    hands = true;
                    if (rec)
                    {
                        if (!handsjoined.Contains((int)(Tools.getStopWatch() / 100 )))
                        {
                            handsjoined.Add((int)(Tools.getStopWatch() / 100 ));
                        }
                    }
                }
            }
            else
            {
                if(sw.IsRunning)
                    sw.Reset();
                hands = false;
                eventfinished = true;
            }

        }

        public static void raiseHandsJoinedEvent(ServerFeedback feedback)
        {
            handsJoinedEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }


        /// <summary>
        /// function to obtain the count of the hand joined
        /// </summary>
        /// <returns>the graph</returns>
        /// <remarks>Add by Florian BECHU: Summer 2016</remarks>
        public static List<IGraph> getHandStatistics()
        {
            List<IGraph> list = new List<IGraph>();
            var chart = new CartesianGraph();
            chart.title = "Hand joined Counter";
            chart.subTitle = Tools.ChooseTheCorrectUnitTime();
            if (!Tools.addSeriesToCharts(chart, new LineSeries(), "Hand joined", handsjoined, "Total hand joined: ", false))
            {
                list.Add(Tools.createEmptyGraph("Hands were not joined"));
            }
            else
            {
                foreach(string str in chart.listTotalValue)
                    
                list.Add(chart);
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