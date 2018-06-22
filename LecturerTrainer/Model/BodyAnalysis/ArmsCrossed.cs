using LecturerTrainer.ViewModel;
using LiveCharts.Wpf;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;


namespace LecturerTrainer.Model.BodyAnalysis
{
    /// <summary>
    /// class used to recognize the arms crossed
    /// </summary>
    /// <remarks>Add by Florian BECHU: Summer 2016</remarks>
    class ArmsCrossed
    {
        public static event EventHandler<InstantFeedback> armsCrossedEvent;
        public static bool compare = true;
        public static string armsCrossedText = "Arms Crossed";

        /// <summary>
        /// Allow to record when the arms are crossed
        /// </summary>
        public static List<int> armscrossed = null;

        /// <summary>
        /// True if we have to record the crossing of the arms
        /// </summary>
        private static bool rec = false;

        /// <summary>
        /// Public attribute for rec. Allows to reset the structures.
        /// </summary>
        public static bool record
        {
            get { return rec; }
            set
            {
                rec = value;
                if (rec)
                {
                    armscrossed = new List<int>();
                }
            }
        }

        public static bool feedArmsCrossed = false;
        public static bool eventfinished = false;

        public static void testCompare(Skeleton sk)
        {
            double errorGap = 0.13; //Experimental value, by Florian BECHU, Summer 2016


            double test1 = Math.Abs(sk.Joints[JointType.HandLeft].Position.X - sk.Joints[JointType.ElbowRight].Position.X);
            double test2 = Math.Abs(sk.Joints[JointType.HandRight].Position.X - sk.Joints[JointType.ElbowLeft].Position.X);

            if (Math.Abs(sk.Joints[JointType.HandLeft].Position.X - sk.Joints[JointType.ElbowRight].Position.X) < errorGap &&
                Math.Abs(sk.Joints[JointType.HandLeft].Position.Y - sk.Joints[JointType.ElbowRight].Position.Y) < errorGap &&
                Math.Abs(sk.Joints[JointType.HandRight].Position.X - sk.Joints[JointType.ElbowLeft].Position.X) < errorGap &&
                Math.Abs(sk.Joints[JointType.HandRight].Position.Y - sk.Joints[JointType.ElbowLeft].Position.Y) < errorGap)
            {
                armsCrossedEvent(null, new InstantFeedback("Arms Crossed"));
                feedArmsCrossed = true;
                if (rec && eventfinished)
                {
                    if (!armscrossed.Contains((int)(Tools.getStopWatch() / 1000 )))
                    {
                        armscrossed.Add((int)(Tools.getStopWatch() / 1000 ));
                    }
                    eventfinished = false;
                }
            }
            else
            {
                feedArmsCrossed = false;
                eventfinished = true;
            }

        }


        public static void raiseArmsCrossedEvent()
        {
            armsCrossedEvent(null, new InstantFeedback(armsCrossedText));
        }

        public static void raiseArmsCrossedEvent(ServerFeedback feedback)
        {
            armsCrossedEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }

        /// <summary>
        /// function to obtain the count of the arms crossed
        /// </summary>
        /// <returns>the graph</returns>
        public static List<IGraph> getArmsStatistics()
        {
            List<IGraph> list = new List<IGraph>();
            var chart = new CartesianGraph();
            chart.title = "Arms Crossed Counter";
            chart.subTitle = Tools.ChooseTheCorrectUnitTime();

            if (!Tools.addSeriesToCharts(chart, new LineSeries(), "Arms Crossed", armscrossed, "Total arms crossed: ",false))
                list.Add(Tools.createEmptyGraph("Arms were not crossed"));
            else
                list.Add(chart);
            return list;
        }
    }
}
