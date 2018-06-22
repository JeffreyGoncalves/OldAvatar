using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class FaceTraining
    {
        public event EventHandler GestureRecognized;

        private const bool VERBOSE = false;
        private const int NB_FRAME_MAX = 460;

        private static bool right;
        private static bool left;
        private static int frame;

        bool _complete;

        public bool Complete
        {
            get
            {
                return _complete;
            }
        }

        bool _lookingDir;

        public bool LookingDir
        {
            get
            {
                return _lookingDir;
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
            _up = false;
            _complete = false;
            _lookingDir = false;

            frame++;
            
            if(frame > NB_FRAME_MAX)
            {
                frame = 0;
                right = false;
                left = false;
                _up = true;

                GestureRecognized?.Invoke(this, new EventArgs());
            }

            try
            {
                var face = KinectDevice.skeletonFaceTracker.facePoints3D;

                Vector3DF rightEye = face.ElementAt(20);
                Vector3DF leftEye = face.ElementAt(53);

                Point3D handRight = new Point3D(sk.Joints[JointType.HandRight].Position);
                Point3D handLeft = new Point3D(sk.Joints[JointType.HandLeft].Position);
                Point3D elbowRight = new Point3D(sk.Joints[JointType.ElbowRight].Position);
                Point3D elbowLeft = new Point3D(sk.Joints[JointType.ElbowLeft].Position);
                Point3D shoulder = new Point3D(sk.Joints[JointType.ShoulderCenter].Position);

                if (VERBOSE)
                {
                    Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                    Console.WriteLine("frame: " + frame);
                    Console.WriteLine("direction: " + (rightEye.Z - leftEye.Z));
                    Console.WriteLine("right: " + right);
                    Console.WriteLine("left: " + left);
                }

                if (Math.Abs(handRight.X - elbowRight.X) > 0.05 && handRight.Y > shoulder.Y)
                {
                    if(rightEye.Z - leftEye.Z < -0.015)
                    {
                        right = true;

                        if (left)
                        {
                            frame = 0;
                            right = false;
                            left = false;
                            _complete = true;

                            GestureRecognized?.Invoke(this, new EventArgs());
                        }
                    }
                    else
                    {
                        frame = 0;
                        right = false;
                        left = false;
                        _lookingDir = true;

                        GestureRecognized?.Invoke(this, new EventArgs());
                    }
                }

                if (Math.Abs(handLeft.X - elbowLeft.X) > 0.05 && handLeft.Y > shoulder.Y)
                {
                    if(rightEye.Z - leftEye.Z > 0.015)
                    {
                        left = true;

                        if (right)
                        {
                            frame = 0;
                            right = false;
                            left = false;
                            _complete = true;

                            GestureRecognized?.Invoke(this, new EventArgs());
                        }
                    }
                    else
                    {
                        frame = 0;
                        right = false;
                        left = false;
                        _lookingDir = true;

                        GestureRecognized?.Invoke(this, new EventArgs());
                    }
                }
            }
            catch(Exception e) //the kinect don't catch the face
            {
                //System.Windows.Forms.MessageBox.Show("the kinect don't catch the face !");
                Console.Error.WriteLine(e);
            }
        }
    }
}
