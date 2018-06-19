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

        private bool right;
        private bool left;

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
            _complete = false;

            try
            {
                var face = KinectDevice.skeletonFaceTracker.facePoints3D;

                Vector3DF rightEye = face.ElementAt(20);
                Vector3DF leftEye = face.ElementAt(53);

                if ((rightEye.Z - leftEye.Z < -0.015) && Math.Abs(sk.Joints[JointType.HandRight].Position.X - sk.Joints[JointType.ElbowRight].Position.X) < 0.05 &&
                sk.Joints[JointType.HandRight].Position.Y - sk.Joints[JointType.ElbowRight].Position.Y > 0.2)
                {
                    right = true;

                    if (left)
                    {
                        right = false;
                        left = false;
                        _complete = true;

                        if (GestureRecognized != null)
                        {
                            GestureRecognized(this, new EventArgs());
                        }
                    }
                }

                if (rightEye.Z - leftEye.Z > 0.015 && Math.Abs(sk.Joints[JointType.HandLeft].Position.X - sk.Joints[JointType.ElbowLeft].Position.X) < 0.05 &&
                sk.Joints[JointType.HandLeft].Position.Y - sk.Joints[JointType.ElbowLeft].Position.Y > 0.2)
                {
                    left = true;

                    if (right)
                    {
                        right = false;
                        left = false;
                        _complete = true;

                        if (GestureRecognized != null)
                        {
                            GestureRecognized(this, new EventArgs());
                        }
                    }
                }
            }
            catch(NullReferenceException) //the kinect don't catch the face
            {
                System.Windows.Forms.MessageBox.Show("the kinect don't catch the face !");
            }
            /*catch(Exception e1)
            {
                Console.Error.WriteLine(e1);
            }*/
        }
    }
}
