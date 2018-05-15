using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    class HandsRaised
    {
        /// <summary>
        /// The event to raise
        /// </summary>
        public static event EventHandler<InstantFeedback> handsRaisedEvent; 

        public static bool compare = true;

        /// <summary>
        /// Each counter is linked to a position : Left hand raised, right hand raised and both hands raised
        /// </summary>
        public static int LCounter = 0;
        public static int RCounter = 0;
        public static int LRCounter = 0;

        /// <summary>
        /// These boolean values indicate if the position was already reached
        /// It avoids counting two times the same raising event
        /// </summary>
        public static bool LRaised = false;
        public static bool RRaised = false;
        public static bool LRRaised = false;

        /// <summary>
        /// Text that will be displayed in feedback pools
        /// </summary>
        public static string leftHandRaised = "left hand raised";
        public static string rightHandRaised = "right hand raised"; 
        public static string bothHandsRaised = "hands raised";

        /// <summary>
        /// minimum distance that must exist between the vetical position of elbow and the vertical position of hand
        /// </summary>
        private static double gap = 0.05; 

        /// <summary>
        /// The method used to detect hands position in the skeleton
        /// </summary>
        /// <param name="sk"></param>
        public static void testCompare(Skeleton sk)
        {
            // all the process 

            // boolean values indicating if the left hand and respectivly the right one are raised
            bool leftHand = false, rightHand = false;

            if (Math.Abs(sk.Joints[JointType.HandLeft].Position.X - sk.Joints[JointType.ElbowLeft].Position.X) < gap &&
                sk.Joints[JointType.HandLeft].Position.Y - sk.Joints[JointType.ElbowLeft].Position.Y > 0.2 )
                leftHand = true;
            if (Math.Abs(sk.Joints[JointType.HandRight].Position.X - sk.Joints[JointType.ElbowRight].Position.X) < gap &&
                sk.Joints[JointType.HandRight].Position.Y - sk.Joints[JointType.ElbowRight].Position.Y > 0.2)
                rightHand = true;
            if (handsRaisedEvent != null)
            {
                // Both hands are in the air
                if (leftHand && rightHand)
                {
                    if (!LRRaised)
                    {
                        // If it was not the case before, the event is raised
                        LRRaised = true;
                        LRCounter++;
                        handsRaisedEvent(null, new InstantFeedback(HandsRaised.bothHandsRaised)); // + " " + LRCounter + " time(s)"));
                    }
                }
                // Only left hand is in the air
                else if (leftHand)
                {
                    if (!LRaised)
                    {
                        // If it was not the case before, the event is raised
                        LRaised = true;
                        LCounter++;
                        handsRaisedEvent(null, new InstantFeedback(HandsRaised.leftHandRaised)); // + " " + LCounter + " time(s)"));
                    }
                }
                // Only right hand is in the air
                else if (rightHand)
                {
                    if (!RRaised)
                    {
                        // If it was not the case before, the event is raised
                        RRaised = true;
                        RCounter++;
                        handsRaisedEvent(null, new InstantFeedback(HandsRaised.rightHandRaised)); // + " " + RCounter + " time(s)"));
                    }
                }
            }

            // If hands are not raised, boolean values are setted to false
            if (!leftHand)
            {
                LRaised = false;
                LRRaised = false;
            }
            if (!rightHand)
            {
                RRaised = false;
                LRRaised = false;
            }
        }

        /// <summary>
        /// Method reseting counters
        /// </summary>
        public static void resetCounters()
        {
            RCounter = 0;
            LCounter = 0;
            LRCounter = 0;
        }

        public static void raiseHandsRaisedEvent()
        {
            handsRaisedEvent(null, new InstantFeedback("hands raised in the air"));
        }

        public static void raiseHandsRaisedEvent(ServerFeedback feedback)
        {
            handsRaisedEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }
    }
}
