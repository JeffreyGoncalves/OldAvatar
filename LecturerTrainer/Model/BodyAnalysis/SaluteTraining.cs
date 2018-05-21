using AForge.Math;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class SaluteTraining
    {
        public event EventHandler GestureRecognized;

        public static DateTime departure;
        public static DateTime currentTime;
        private static bool start;
        private static int nbFrames = 0;
        public static bool complete;
        public static bool wrong;
        public static bool slow;

        bool _slow;

        public bool Slow
        {
            get
            {
                return _slow;
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

        bool _wrong;

        public bool Wrong
        {
            get
            {
                return _wrong;
            }
        }

        public void Update(Skeleton sk)
        {
            complete = false;
            wrong = false;
            start = false;
            slow = false;

            //&& Math.Abs(sk.Joints[JointType.HandRight].Position.Y - sk.Joints[JointType.Head].Position.Y) < 0.05

            Point3D hand = new Point3D(sk.Joints[JointType.HandRight].Position);
            Point3D elbow = new Point3D(sk.Joints[JointType.ElbowRight].Position);
            Point3D shoulder = new Point3D(sk.Joints[JointType.ShoulderRight].Position);
            Point3D shoulderCenter = new Point3D(sk.Joints[JointType.ShoulderRight].Position);

            double lenghtHandElbow = Math.Sqrt(Math.Pow(hand.X - elbow.X, 2) + Math.Pow(hand.Y - elbow.Y, 2) + Math.Pow(hand.Z - elbow.Z, 2));
            double lenghtHandShoulder = Math.Sqrt(Math.Pow(hand.X - shoulder.X, 2) + Math.Pow(hand.Y - shoulder.Y, 2) + Math.Pow(hand.Z - shoulder.Z, 2));
            double lenghtElbowShoulder = Math.Sqrt(Math.Pow(shoulder.X - elbow.X, 2) + Math.Pow(shoulder.Y - elbow.Y, 2) + Math.Pow(shoulder.Z - elbow.Z, 2));

            double cosAngle = (Math.Pow(lenghtElbowShoulder, 2) - Math.Pow(lenghtHandShoulder, 2) - Math.Pow(lenghtHandElbow, 2)) / (-2*lenghtHandShoulder*lenghtHandElbow);
            double angle = Math.Acos(cosAngle) * 180/Math.PI;

            Vector3 vectorShoulder = new Vector3((float)(shoulder.X - shoulderCenter.X), (float)(shoulder.Y - shoulderCenter.Y), (float)(shoulder.Z - shoulderCenter.Z));
            Vector3 vectorShoulderElbow = new Vector3((float)(elbow.X - shoulder.X), (float)(elbow.Y - shoulder.Y), (float)(elbow.Z - shoulder.Z));

            float ratioX = vectorShoulder.X/ vectorShoulderElbow.X;
            float ratioY = vectorShoulder.Y/ vectorShoulderElbow.Y;
            float ratioZ = vectorShoulder.Z/ vectorShoulderElbow.Z;

            //Math.Abs(sk.Joints[JointType.HandRight].Position.Y - sk.Joints[JointType.Head].Position.Y) < 0.1 &&
            //Math.Abs(sk.Joints[JointType.HandRight].Position.X - sk.Joints[JointType.Head].Position.X) < 0.20) //headradius = 0.15f

            Console.WriteLine("angle " + angle);
            //Console.WriteLine("x : " + Math.Abs(sk.Joints[JointType.HandRight].Position.X - sk.Joints[JointType.Head].Position.X));
            //Console.WriteLine("y : " +Math.Abs(sk.Joints[JointType.HandRight].Position.Y - sk.Joints[JointType.Head].Position.Y));

            if (angle > 30 && angle < 40 && ratioX - ratioY > -0.01 && ratioX - ratioY < 0.01 &&
                ratioX - ratioZ > -0.01 && ratioX - ratioZ < 0.01 && ratioZ - ratioY > -0.01 && ratioZ - ratioY < 0.01 &&
                Math.Abs(sk.Joints[JointType.HandRight].Position.X - sk.Joints[JointType.Head].Position.X) < 0.20) //headradius = 0.15f
            {
                start = true;
                nbFrames++;

                if(nbFrames >= 120)
                {
                    _complete = true;

                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new EventArgs());
                    }
                }
            }
            else
            {
                if(start)
                {
                    nbFrames = 0;
                    start = false;
                    _wrong = true;
                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new EventArgs());
                    }
                }
            }
        }
    }
}
