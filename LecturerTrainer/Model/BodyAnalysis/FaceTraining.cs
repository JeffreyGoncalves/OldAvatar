using LecturerTrainer.Model.AudioAnalysis;
using LecturerTrainer.View;
using LecturerTrainer.ViewModel;
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
        
        private const int NB_FRAME_MAX = 460;
        private const float AUDIO_LOUDNESS = 530f;

        private static bool right;
        private static bool left;
        private static bool start;
        private static int frame;

        private static bool verbose = false;

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
            if(!AudioProvider.detectionActive)
            {
                TrackingSideTool.Get().SpeedRateDetectionCheckBox.IsChecked = true;
            }

            _up = false;
            _complete = false;
            _lookingDir = false;

            frame++;
            
            if(frame > NB_FRAME_MAX)
            {
                frame = 0;
                start = false;
                right = false;
                left = false;
                _up = true;

                DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                GestureRecognized?.Invoke(this, new EventArgs());
            }

            if (TrainingWithAvatarViewModel.Get().SkeletonList != null && TrainingWithAvatarViewModel.canBeInterrupted && !start)
            {
                DrawingSheetAvatarViewModel.displayCustomText = "Look at the direction of your raised arm then speak";
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

                if (verbose)
                {
                    Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                    Console.WriteLine("frame: " + frame);
                    Console.WriteLine("direction: " + (rightEye.Z - leftEye.Z));
                    Console.WriteLine("right: " + right);
                    Console.WriteLine("left: " + left);
                }

                if (Math.Abs(handRight.X - elbowRight.X) > 0.05 && handRight.Y > shoulder.Y)
                {
                    if (rightEye.Z - leftEye.Z < -0.015)
                    {
                        if (AudioProvider.currentIntensity > AUDIO_LOUDNESS)
                        {
                            start = true;
                            right = true;
                            DrawingSheetAvatarViewModel.displayCustomText = "Do the same with the other arm";

                            if (left)
                            {
                                frame = 0;
                                start = false;
                                right = false;
                                left = false;
                                _complete = true;

                                DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                                GestureRecognized?.Invoke(this, new EventArgs());
                            }
                        }
                    }
                    else if (!right)
                    {
                        frame = 0;
                        start = false;
                        right = false;
                        left = false;
                        _lookingDir = true;

                        DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                        GestureRecognized?.Invoke(this, new EventArgs());
                    }
                }

                if (Math.Abs(handLeft.X - elbowLeft.X) > 0.05 && handLeft.Y > shoulder.Y)
                {
                    if (rightEye.Z - leftEye.Z > 0.015)
                    {
                        if (AudioProvider.currentIntensity > AUDIO_LOUDNESS)
                        {
                            start = true;
                            left = true;
                            DrawingSheetAvatarViewModel.displayCustomText = "Do the same with the other arm";

                            if (right)
                            {
                                frame = 0;
                                start = false;
                                right = false;
                                left = false;
                                _complete = true;

                                DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                                GestureRecognized?.Invoke(this, new EventArgs());
                            }
                        }
                    }
                    else if (!left)
                    {
                        frame = 0;
                        start = false;
                        right = false;
                        left = false;
                        _lookingDir = true;

                        DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
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
