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

        private static int errorMax = 40;
        private static int countError = 0;
        private static int nbFrames = 0;
        private static int nbStay = 0;
        private static int tooSlow = 240;
        private static int tooFast = 480;
        private static int waitingTime = 120;
        private static bool start = false;
        private static bool begin = false;
        public static bool complete;
        public static bool stay;
        public static bool slow;
        public static bool fast;

        bool _slow;

        public bool Slow
        {
            get
            {
                return _slow;
            }
        }

        bool _fast;

        public bool Fast
        {
            get
            {
                return _fast;
            }
        }

        bool _stay;

        public bool Stay
        {
            get
            {
                return _stay;
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
            _stay = false;
            _slow = false;
            _complete = false;
            nbFrames++;

            Point3D hand = new Point3D(sk.Joints[JointType.HandRight].Position);
            Point3D elbow = new Point3D(sk.Joints[JointType.ElbowRight].Position);
            Point3D shoulder = new Point3D(sk.Joints[JointType.ShoulderRight].Position);
            Point3D shoulderLeft = new Point3D(sk.Joints[JointType.ShoulderLeft].Position);
            Point3D head = new Point3D(sk.Joints[JointType.Head].Position);

            //calculation of the angle formed by the arm
            double lenghtHandElbow = Math.Sqrt(Math.Pow(hand.X - elbow.X, 2) + Math.Pow(hand.Y - elbow.Y, 2) + Math.Pow(hand.Z - elbow.Z, 2));
            double lenghtHandShoulder = Math.Sqrt(Math.Pow(hand.X - shoulder.X, 2) + Math.Pow(hand.Y - shoulder.Y, 2) + Math.Pow(hand.Z - shoulder.Z, 2));
            double lenghtElbowShoulder = Math.Sqrt(Math.Pow(shoulder.X - elbow.X, 2) + Math.Pow(shoulder.Y - elbow.Y, 2) + Math.Pow(shoulder.Z - elbow.Z, 2));

            double cosAngle = (Math.Pow(lenghtElbowShoulder, 2) - Math.Pow(lenghtHandShoulder, 2) - Math.Pow(lenghtHandElbow, 2)) / (-2*lenghtHandShoulder*lenghtHandElbow);
            double angle = Math.Acos(cosAngle) * 180/Math.PI;

            //arm alignment check (version 1)
            Vector3 vectorShoulder = new Vector3((float)(shoulder.X - shoulderLeft.X), (float)(shoulder.Y - shoulderLeft.Y), (float)(shoulder.Z - shoulderLeft.Z));
            Vector3 vectorShoulderElbow = new Vector3((float)(elbow.X - shoulder.X), (float)(elbow.Y - shoulder.Y), (float)(elbow.Z - shoulder.Z));

            float crossProductX = vectorShoulder.Y * vectorShoulderElbow.Z - vectorShoulder.Z * vectorShoulderElbow.Y;
            float crossProductY = vectorShoulder.Z * vectorShoulderElbow.X - vectorShoulder.X * vectorShoulderElbow.Z;
            float crossProductZ = vectorShoulder.X * vectorShoulderElbow.Y - vectorShoulder.Y * vectorShoulderElbow.X;

            //arm alignment check (version 2)
            double directionVector = (shoulder.Y - shoulderLeft.Y) / (shoulder.X - shoulderLeft.X);
            double k = shoulder.Y - (shoulder.X * directionVector);

            double theoreticalY = elbow.X * directionVector + k;
            
            //distance between the head and the hand
            double distance = Math.Sqrt(Geometry.distanceSquare(head, hand));

            //if (angle > 42 && angle < 48 && Math.Abs(crossProductX) < 0.03 &&
            //    Math.Abs(crossProductY) < 0.03 && Math.Abs(crossProductZ) < 0.03 &&
            //   distance < 0.25 && Math.Abs(head.Y - hand.Y) < 0.1) //headradius = 0.15f

            if(!begin && nbFrames > tooSlow)
            {
                countError = 0;
                nbFrames = 0;
                _slow = true;

                if (GestureRecognized != null)
                {
                    GestureRecognized(this, new EventArgs());
                }
            }


            if(angle > 42 && angle < 48 && Math.Abs(theoreticalY - elbow.Y) < 0.1 && Math.Abs(head.Y - hand.Y) < 0.1)
            {
                start = true;
                begin = true;
                nbStay++;

                if(nbStay >= waitingTime)
                {
                    start = false;
                    begin = false;
                    countError = 0;
                    nbFrames = 0;
                    nbStay = 0;
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
                    countError++;

                if (start && nbFrames < tooFast && countError > errorMax)
                {
                    countError = 0;
                    nbFrames = 0;
                    nbStay = 0;
                    start = false;
                    begin = false;
                    _stay = true;

                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new EventArgs());
                    }
                }
            }
        }
    }
}
