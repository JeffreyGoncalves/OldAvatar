using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class ArmsWide
    {
        public static event EventHandler<LongFeedback> armsWideEvent;
        public static bool compare = true;
        public static string armsWideText = "Arms wide!";

        public static bool start = false;
        public static bool lofferstart = false;
        public static bool rofferstart = false;
        static double distance, prevdist;


        public static void testCompare(Skeleton sk)
        {
            double errorGap;
            double errorGap2;

            errorGap = Math.Sqrt(Geometry.distanceSquare(new Point3D(sk.Joints[JointType.ShoulderCenter].Position), new Point3D(sk.Joints[JointType.ShoulderLeft].Position)));
            errorGap2 = 3 * errorGap / 4;
            if (Math.Abs(sk.Joints[JointType.ShoulderLeft].Position.Y - sk.Joints[JointType.ElbowLeft].Position.Y) < errorGap2 &&
                Math.Abs(sk.Joints[JointType.ElbowLeft].Position.Y - sk.Joints[JointType.HandLeft].Position.Y) < errorGap2 &&
                Math.Abs(sk.Joints[JointType.ShoulderRight].Position.Y - sk.Joints[JointType.ElbowRight].Position.Y) < errorGap2 &&
                Math.Abs(sk.Joints[JointType.ElbowRight].Position.Y - sk.Joints[JointType.HandRight].Position.Y) < errorGap2 &&
                armsWideEvent != null)
            {
                distance = Math.Sqrt(Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandLeft].Position), new Point3D(sk.Joints[JointType.HandRight].Position)));
                if (distance < errorGap)
                {
                    start = true;
                    prevdist = distance;
                    armsWideEvent(null, new LongFeedback("WELCOME START!",true));
                }
                else if (distance < 2.5*errorGap && start)
                {
                    if ((distance - prevdist) > 0.01 * errorGap)
                    {
                        prevdist = distance;
                        armsWideEvent(null, new LongFeedback("WELCOME CONTINUES!", true));
                    }
                    else
                    {
                        start = false;
                        armsWideEvent(null, new LongFeedback("WELCOME FAILED!", true));
                    }
                }
                else if (start)
                    {
                    armsWideEvent(null, new LongFeedback("WELCOME COMPLETE!", true));
                    }

            }
            else
            {
                if(start) armsWideEvent(null, new LongFeedback("WELCOME: DROPPED ELBOW!", true));
                start = false;
            }
          
            if (Math.Abs(sk.Joints[JointType.ShoulderLeft].Position.X - sk.Joints[JointType.ElbowLeft].Position.X) < errorGap &&
                Math.Abs(sk.Joints[JointType.ShoulderLeft].Position.Y - sk.Joints[JointType.ElbowLeft].Position.Y) > 0.5*errorGap &&
                  Math.Abs(sk.Joints[JointType.ElbowLeft].Position.Y - sk.Joints[JointType.HandLeft].Position.Y) < errorGap &&
                  armsWideEvent != null)
            {
                distance = sk.Joints[JointType.ElbowLeft].Position.X - sk.Joints[JointType.HandLeft].Position.X;
                if (Math.Abs(distance) < errorGap)
                {
                    lofferstart = true;
                   //armsWideEvent(null, new LongFeedback("LEFT OFFER START!",true));
                }
                else if (distance > errorGap && lofferstart)
                {
                    //armsWideEvent(null, new LongFeedback("LEFT OFFER FINISH!",true));
                }

            }
            else
            {
                lofferstart = false;
            }

            if (Math.Abs(sk.Joints[JointType.ShoulderRight].Position.X - sk.Joints[JointType.ElbowRight].Position.X) < errorGap &&
                Math.Abs(sk.Joints[JointType.ShoulderRight].Position.Y - sk.Joints[JointType.ElbowRight].Position.Y) > 0.5*errorGap &&
                  Math.Abs(sk.Joints[JointType.ElbowRight].Position.Y - sk.Joints[JointType.HandRight].Position.Y) < errorGap &&
                  armsWideEvent != null)
            {
                distance = sk.Joints[JointType.ElbowRight].Position.X - sk.Joints[JointType.HandRight].Position.X;
                if (Math.Abs(distance) < errorGap)
                {
                    rofferstart = true;
                    //armsWideEvent(null, new LongFeedback("RIGHT OFFER START!",true));
                }
                else if (distance < -errorGap && rofferstart)
                {
                   //armsWideEvent(null, new LongFeedback("RIGHT OFFER FINISH!",true));
                }

            }
            else
            {
                rofferstart = false;
            }


        }

        public static void raiseArmsWideEvent()
        {
            armsWideEvent(null, new LongFeedback(armsWideText,true));
        }

        public static void raiseArmsWideEvent(ServerFeedback feedback)
        {
            armsWideEvent(null, new LongFeedback(feedback.feedbackMessage,feedback.display));
        }
    }
}
