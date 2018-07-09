using AForge.Math;
using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class HypeTraining
    {
        public event EventHandler GestureRecognized;

        private const int TIME = 180;
        private const float ANGLE = 75f;
        private const float ERROR_RATE_STRETCH = 0.2f;
        private const float ERROR_RATE_ANGLE = 15f;
        //private const bool VERBOSE = false;

        private static int frame;

        bool _complete;

        public bool Complete
        {
            get
            {
                return _complete;
            }
        }

        bool _spread;

        public bool Spread
        {
            get
            {
                return _spread;
            }
        }

        bool _stretch;

        public bool Stretch
        {
            get
            {
                return _stretch;
            }
        }

        bool _up;

        public bool Up
        {
            get
            {
                return _up;
            }
        }

        public void Update(Skeleton sk)
        {
            _complete = false;
            _spread = false;
            _stretch = false;
            _up = false;
            frame++;

            bool angleRightOK = false;
            bool angleLeftOK = false;
            bool stretchRightOK = false;
            bool stretchLeftOK = false;
            bool up = false;

            Point3D handRight = new Point3D(sk.Joints[JointType.HandRight].Position);
            Point3D handLeft = new Point3D(sk.Joints[JointType.HandLeft].Position);
            Point3D elbowRight = new Point3D(sk.Joints[JointType.ElbowRight].Position);
            Point3D elbowLeft = new Point3D(sk.Joints[JointType.ElbowLeft].Position);
            Point3D shoulderRight = new Point3D(sk.Joints[JointType.ShoulderRight].Position);
            Point3D shoulderLeft = new Point3D(sk.Joints[JointType.ShoulderLeft].Position);
            Point3D head = new Point3D(sk.Joints[JointType.Head].Position);

            if (TrainingWithAvatarViewModel.Get().SkeletonList != null && TrainingWithAvatarViewModel.canBeInterrupted)
            {
                DrawingSheetAvatarViewModel.displayCustomText = "Your turn ! Raise your arms";
            }

            //calculation of the angle formed by the right arm
            double lenghtHeadShoulderRight = Math.Sqrt(Math.Pow(head.X - shoulderRight.X, 2) + Math.Pow(head.Y - shoulderRight.Y, 2) + Math.Pow(head.Z - shoulderRight.Z, 2));
            double lenghtShoulderHandRight = Math.Sqrt(Math.Pow(shoulderRight.X - handRight.X, 2) + Math.Pow(shoulderRight.Y - handRight.Y, 2) + Math.Pow(shoulderRight.Z - handRight.Z, 2));
            double lenghtHeadHandRight = Math.Sqrt(Math.Pow(head.X - handRight.X, 2) + Math.Pow(head.Y - handRight.Y, 2) + Math.Pow(head.Z - handRight.Z, 2));

            double cosAngleRight = (Math.Pow(lenghtShoulderHandRight, 2) - (Math.Pow(lenghtHeadShoulderRight, 2) + Math.Pow(lenghtHeadHandRight, 2))) / (-2 * lenghtHeadShoulderRight * lenghtHeadHandRight);
            double angleRight = Math.Acos(cosAngleRight) * 180 / Math.PI;

            if (Math.Abs(angleRight - ANGLE) < ERROR_RATE_ANGLE)
                angleRightOK = true;

            //calculation of the angle formed by the left arm
            double lenghtHeadShoulderLeft = Math.Sqrt(Math.Pow(head.X - shoulderLeft.X, 2) + Math.Pow(head.Y - shoulderLeft.Y, 2) + Math.Pow(head.Z - shoulderLeft.Z, 2));
            double lenghtShoulderHandLeft = Math.Sqrt(Math.Pow(shoulderLeft.X - handLeft.X, 2) + Math.Pow(shoulderLeft.Y - handLeft.Y, 2) + Math.Pow(shoulderLeft.Z - handLeft.Z, 2));
            double lenghtHeadHandLeft = Math.Sqrt(Math.Pow(head.X - handLeft.X, 2) + Math.Pow(head.Y - handLeft.Y, 2) + Math.Pow(head.Z - handLeft.Z, 2));

            double cosAngleLeft = (Math.Pow(lenghtShoulderHandLeft, 2) - (Math.Pow(lenghtHeadShoulderLeft, 2) + Math.Pow(lenghtHeadHandLeft, 2))) / (-2 * lenghtHeadShoulderLeft * lenghtHeadHandLeft);
            double angleLeft = Math.Acos(cosAngleLeft) * 180 / Math.PI;

            if (Math.Abs(angleLeft - ANGLE) < ERROR_RATE_ANGLE)
                angleLeftOK = true;

            //check if the arms are stretched (version 1)
            /*Vector3 vectorShoulderElbowRight = new Vector3((float)(elbowRight.X - shoulderRight.X), (float)(elbowRight.Y - shoulderRight.Y), (float)(elbowRight.Z - shoulderRight.Z));
            Vector3 vectorElbowHandRight = new Vector3((float)(handRight.X - elbowRight.X), (float)(handRight.Y - elbowRight.Y), (float)(handRight.Z - elbowRight.Z));

            float crossProductX = vectorShoulderElbowRight.Y * vectorElbowHandRight.Z - vectorShoulderElbowRight.Z * vectorElbowHandRight.Y;
            float crossProductY = vectorShoulderElbowRight.Z * vectorElbowHandRight.X - vectorShoulderElbowRight.X * vectorElbowHandRight.Z;
            float crossProductZ = vectorShoulderElbowRight.X * vectorElbowHandRight.Y - vectorShoulderElbowRight.Y * vectorElbowHandRight.X;

            if (Math.Abs(crossProductX) < errorRateStretch && Math.Abs(crossProductY) < errorRateStretch && Math.Abs(crossProductZ) < errorRateStretch)
                stretchRightOK = true;

            Vector3 vectorShoulderElbowLeft = new Vector3((float)(elbowLeft.X - shoulderLeft.X), (float)(elbowLeft.Y - shoulderLeft.Y), (float)(elbowLeft.Z - shoulderLeft.Z));
            Vector3 vectorElbowHandLeft = new Vector3((float)(handLeft.X - elbowLeft.X), (float)(handLeft.Y - elbowLeft.Y), (float)(handLeft.Z - elbowLeft.Z));

            crossProductX = vectorShoulderElbowLeft.Y * vectorElbowHandLeft.Z - vectorShoulderElbowLeft.Z * vectorElbowHandLeft.Y;
            crossProductY = vectorShoulderElbowLeft.Z * vectorElbowHandLeft.X - vectorShoulderElbowLeft.X * vectorElbowHandLeft.Z;
            crossProductZ = vectorShoulderElbowLeft.X * vectorElbowHandLeft.Y - vectorShoulderElbowLeft.Y * vectorElbowHandLeft.X;

            if (Math.Abs(crossProductX) < errorRateStretch && Math.Abs(crossProductY) < errorRateStretch && Math.Abs(crossProductZ) < errorRateStretch)
                stretchLeftOK = true;*/

            //check if the arms are stretched (version 2)
            double directionVector = (elbowRight.Y - shoulderRight.Y) / (elbowRight.X - shoulderRight.X);
            double k = shoulderRight.Y - (shoulderRight.X * directionVector);

            double theoreticalYRight = handRight.X * directionVector + k;

            if (Math.Abs(theoreticalYRight - handRight.Y) < ERROR_RATE_STRETCH)
                stretchRightOK = true;

            directionVector = (elbowLeft.Y - shoulderLeft.Y) / (elbowLeft.X - shoulderLeft.X);
            k = shoulderLeft.Y - (shoulderLeft.X * directionVector);

            double theoreticalYLeft = handLeft.X * directionVector + k;

            if(Math.Abs(theoreticalYLeft - handLeft.Y) < ERROR_RATE_STRETCH)
                stretchLeftOK = true;

            //checked if the arms are up
            if (handLeft.Y > head.Y && handRight.Y > head.Y)
                up = true;
            
            /*if(VERBOSE)
            {
                Console.WriteLine("#######################");
                Console.WriteLine("angle left: " + angleLeft);
                Console.WriteLine("angle right: " + angleRight);
                Console.WriteLine("stretch left: " + Math.Abs(theoreticalYLeft - handLeft.Y));
                Console.WriteLine("stretch right: " + Math.Abs(theoreticalYRight - handRight.Y));
            }*/

            if(up && stretchLeftOK && stretchRightOK && angleLeftOK && angleRightOK)
            {
                frame = 0;
                _complete = true;

                DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                GestureRecognized?.Invoke(this, new EventArgs());
            }
            else if(frame > TIME)
            {
                frame = 0;
                if(!up)
                {
                    _up = true;

                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }
                else if(!stretchLeftOK || !stretchRightOK)
                {
                    _stretch = true;

                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }
                else if(!angleLeftOK || !angleRightOK)
                {
                    _spread = true;

                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }
            }
        }
    }
}
