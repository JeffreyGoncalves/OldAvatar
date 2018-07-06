using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class WelcomeTraining
    {
        public event EventHandler GestureRecognized;
        
        private static bool start = false;
        private static bool goodjob = false;
        private static bool elbows = false;
        private static bool slow = false;

        private static double distance, prevdist;

        private static int frames;

        bool _slow;

        public bool Slow
        {
            get
            {
                return _slow;
            }
        }

        bool _dropped;

        public bool Dropped
        {
            get
            {
                return _dropped;
            }
        }


        bool _complete;

        public bool Complete
        {
            get
            {
                return _complete;
            }
        }

        public void Update(Skeleton sk)
        {
            double errorGap;
            double errorGap2;

            _slow = false;
            _dropped = false;
            _complete = false;
            goodjob = false;
            elbows = false;
            slow = false;

            /*
                    You can write a gesture as a set of nested if-statements. The outer condition is the broadest and triggers the gesture recognition
                    process. Then you have a series of nested conditions each one more specific than the previous one.
                    If a particular condition fails, you exit with GestureRecognised and set a boolen variable to tell the avatar training where it failed
                    The innermost condition is the correct gesture and exits with bool complete = true
            */

            errorGap = Math.Sqrt(Geometry.distanceSquare(new Point3D(sk.Joints[JointType.ShoulderCenter].Position), new Point3D(sk.Joints[JointType.ShoulderLeft].Position)));
            errorGap2 = 3 * errorGap / 4;
            if (Math.Abs(sk.Joints[JointType.ShoulderLeft].Position.Y - sk.Joints[JointType.ElbowLeft].Position.Y) < errorGap2 &&
                Math.Abs(sk.Joints[JointType.ElbowLeft].Position.Y - sk.Joints[JointType.HandLeft].Position.Y) < errorGap2 &&
                Math.Abs(sk.Joints[JointType.ShoulderRight].Position.Y - sk.Joints[JointType.ElbowRight].Position.Y) < errorGap2 &&
                Math.Abs(sk.Joints[JointType.ElbowRight].Position.Y - sk.Joints[JointType.HandRight].Position.Y) < errorGap2)
            {
                distance = Math.Sqrt(Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandLeft].Position), new Point3D(sk.Joints[JointType.HandRight].Position)));
                frames++;
                if (distance < errorGap)
                {
                    goodjob = false;
                    elbows = false;
                    slow = false;
                    start = true;
                    prevdist = distance;
                    //armsWideEvent(null, new LongFeedback("WELCOME START!", true));
                }
                /*else if (distance < 5 * errorGap && start)
                {
                    if ((distance - prevdist) > 0.01 * errorGap)
                    {
                        prevdist = distance;
                        //armsWideEvent(null, new LongFeedback("WELCOME CONTINUES!", true));
                    }
                    else
                    {
                        goodjob = false; elbows = false;
                        start = false;
                        _slow = true;
                        slow = true;
                        //armsWideEvent(null, new LongFeedback("WELCOME FAILED!", true));
                        if (GestureRecognized != null)
                        {
                            GestureRecognized(this, new EventArgs());
                        }
                    }
                }*/
                else if(frames > 180)
                {
                    frames = 0;
                    goodjob = false;
                    elbows = false;
                    start = false;
                    _slow = true;
                    slow = true;
                    //armsWideEvent(null, new LongFeedback("WELCOME FAILED!", true));
                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new EventArgs());
                    }
                }
                else if (distance > errorGap * 4.5 && start)
                {
                    //armsWideEvent(null, new LongFeedback("WELCOME COMPLETE!", true));
                    frames = 0;
                    goodjob = true;
                    _complete = true;
                    start = false;
                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new EventArgs());
                    }
                }
            }
            else
            {
                if (start)
                {
                    //armsWideEvent(null, new LongFeedback("WELCOME: DROPPED ELBOW!", true));
                    frames = 0;
                    start = false;
                    _dropped = true; elbows = true;
                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new EventArgs());
                    }
                }
            }
        }
    }

}
