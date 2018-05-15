using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;

namespace LecturerTrainer.Model.BodyAnalysis
{
    /// <summary>
    /// class used to recognize the hands in the pocket
    /// Experimental : The kinect doesn't make the difference between hands in pockets and hands on pockets
    /// </summary>
    /// <remarks>Add by Florian BECHU: Summer 2016</remarks>
    class HandsInPocket
    {
        public static event EventHandler<InstantFeedback> handsInPocketEvent;
        public static bool compare = true;
        public static string handPocketText = "Hands in pocket";

        public static bool feedHandsInPocket = false;

        public static void testCompare(Skeleton sk)
        {
            double errorx = 0.16;
            //double errory = 0.3;


            //The left hand is aligned with the left wrist on the X axe
            bool condHandLeftX = Math.Abs(sk.Joints[JointType.WristLeft].Position.X - sk.Joints[JointType.HipLeft].Position.X) < errorx;

            bool condHandLeftY = (sk.Joints[JointType.HipLeft].Position.Y - sk.Joints[JointType.WristLeft].Position.Y) > 0.01 &&
                                 (sk.Joints[JointType.HipLeft].Position.Y - sk.Joints[JointType.WristLeft].Position.Y) < 0.2;

            bool condHandLeftTracked = sk.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Inferred;

            bool condHandRightX = Math.Abs(sk.Joints[JointType.WristRight].Position.X - sk.Joints[JointType.HipRight].Position.X) < errorx;

            bool condHandRightY = (sk.Joints[JointType.HipRight].Position.Y - sk.Joints[JointType.WristRight].Position.Y) > 0.01 &&
                                 (sk.Joints[JointType.HipRight].Position.Y - sk.Joints[JointType.WristRight].Position.Y) < 0.2;

            bool condHandRightTracked = sk.Joints[JointType.HandRight].TrackingState == JointTrackingState.Inferred;

            if ((condHandLeftX && condHandLeftY && condHandLeftTracked) || (condHandRightX && condHandRightY && condHandRightTracked))
            {
                handsInPocketEvent(null, new InstantFeedback("Hands in the Pocket"));
                feedHandsInPocket = true;
            }
            else
            {
                feedHandsInPocket = false;
            }
        }

        public static void raiseHandsInPocketEvent()
        {
            handsInPocketEvent(null, new InstantFeedback(handPocketText));
        }

        public static void raiseArmsCrossedEvent(ServerFeedback feedback)
        {
            handsInPocketEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }
    }
}
