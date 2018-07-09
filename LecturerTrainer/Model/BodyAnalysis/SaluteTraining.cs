using AForge.Math;
using LecturerTrainer.ViewModel;
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

        private const int ERROR_MAX = 120;
        private const int TOO_FAST = 480;
        private const int TOO_SLOW = 240;
        private const int WAITING_TIME = 2;

        private static int countError = 0;
        private static int nbFrames = 0;
        private static int nbStay = 0;
        private static bool start = false;

        bool _slow;

        public bool Slow
        {
            get
            {
                return _slow;
            }
        }

        bool _leftHand;

        public bool LeftHand
        {
            get
            {
                return _leftHand;
            }
        }

        bool _angleB;

        public bool AngleB
        {
            get
            {
                return _angleB;
            }
        }

        bool _alignment;

        public bool Alignment
        {
            get
            {
                return _alignment;
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
            _angleB = false;
            _alignment = false;
            _leftHand = false;

            nbFrames++;

            Point3D hand = new Point3D(sk.Joints[JointType.HandRight].Position);
            Point3D elbow = new Point3D(sk.Joints[JointType.ElbowRight].Position);
            Point3D shoulder = new Point3D(sk.Joints[JointType.ShoulderRight].Position);
            Point3D shoulderLeft = new Point3D(sk.Joints[JointType.ShoulderLeft].Position);
            Point3D head = new Point3D(sk.Joints[JointType.Head].Position);
            Point3D handLeft = new Point3D(sk.Joints[JointType.HandLeft].Position);
            Point3D hipLeft = new Point3D(sk.Joints[JointType.HipLeft].Position);

            if (TrainingWithAvatarViewModel.Get().SkeletonList != null && TrainingWithAvatarViewModel.canBeInterrupted)
            {
                DrawingSheetAvatarViewModel.displayCustomText = "Your turn ! Do the military salute";
            }

            //calculation of the angle formed by the arm
            double lenghtHandElbow = Math.Sqrt(Math.Pow(hand.X - elbow.X, 2) + Math.Pow(hand.Y - elbow.Y, 2) + Math.Pow(hand.Z - elbow.Z, 2));
            double lenghtHandShoulder = Math.Sqrt(Math.Pow(hand.X - shoulder.X, 2) + Math.Pow(hand.Y - shoulder.Y, 2) + Math.Pow(hand.Z - shoulder.Z, 2));
            double lenghtElbowShoulder = Math.Sqrt(Math.Pow(shoulder.X - elbow.X, 2) + Math.Pow(shoulder.Y - elbow.Y, 2) + Math.Pow(shoulder.Z - elbow.Z, 2));

            double cosAngle = (Math.Pow(lenghtElbowShoulder, 2) - Math.Pow(lenghtHandShoulder, 2) - Math.Pow(lenghtHandElbow, 2)) / (-2*lenghtHandShoulder*lenghtHandElbow);
            double angle = Math.Acos(cosAngle) * 180/Math.PI;

            //arm alignment check (version 1)
            /*Vector3 vectorShoulder = new Vector3((float)(shoulder.X - shoulderLeft.X), (float)(shoulder.Y - shoulderLeft.Y), (float)(shoulder.Z - shoulderLeft.Z));
            Vector3 vectorShoulderElbow = new Vector3((float)(elbow.X - shoulder.X), (float)(elbow.Y - shoulder.Y), (float)(elbow.Z - shoulder.Z));

            float crossProductX = vectorShoulder.Y * vectorShoulderElbow.Z - vectorShoulder.Z * vectorShoulderElbow.Y;
            float crossProductY = vectorShoulder.Z * vectorShoulderElbow.X - vectorShoulder.X * vectorShoulderElbow.Z;
            float crossProductZ = vectorShoulder.X * vectorShoulderElbow.Y - vectorShoulder.Y * vectorShoulderElbow.X;*/

            //arm alignment check (version 2)
            double directionVector = (shoulder.Y - shoulderLeft.Y) / (shoulder.X - shoulderLeft.X);
            double k = shoulder.Y - (shoulder.X * directionVector);

            double theoreticalY = elbow.X * directionVector + k;

            //distance between left hand and hip
            double distance = Math.Sqrt(Geometry.distanceSquare(handLeft, hipLeft));

            //if the salute is not made in time
            if (!start && nbFrames > TOO_SLOW)
            {
                countError = 0;
                nbFrames = 0;

                //wrong left hand position
                if (!(distance < 0.3))
                {
                    _leftHand = true;

                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }
                //wrong arm alignment
                else if (!(Math.Abs(theoreticalY - elbow.Y) < 0.05))
                {
                    _alignment = true;

                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }
                //wrong arm angle
                else if (!(Math.Abs(angle - 45) < 1))
                {
                    _angleB = true;

                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }
            }

            //good position of salute
            if(/*Math.Abs(angle - 45) < 5 &&*/ Math.Abs(theoreticalY - elbow.Y) < 0.1 && Math.Abs(head.Y - hand.Y) < 0.1 && 
                Math.Abs(head.Z - hand.Z) < 0.1 && Math.Abs(head.X - hand.X) < 0.3 && distance < 0.3)
            {
                start = true;
                nbStay++;

                //salute complete
                if(nbStay >= WAITING_TIME)
                {
                    start = false;
                    countError = 0;
                    nbFrames = 0;
                    nbStay = 0;
                    _complete = true;

                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }
            }
            else
            {
                if(start)
                    countError++;
                
                //if the user does not stay waiting
                if (start && nbFrames < TOO_FAST && countError > ERROR_MAX) 
                {
                    countError = 0;
                    nbFrames = 0;
                    nbStay = 0;
                    start = false;
                    _stay = true;

                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }
            }
        }
    }
}
