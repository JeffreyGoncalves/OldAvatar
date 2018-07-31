using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class HandTraining
    {
        public event EventHandler GestureRecognized;

        private const int NB_APPLAUSE = 3;

        private static double distance;
        private static int frames;

        private static bool start;
        private static bool up;
        private static bool down;

        private bool _complete;

        public bool Complete
        {
            get
            {
                return _complete;
            }
        }

        private int _count;

        public int Count
        {
            get
            {
                return _count;
            }
        }

        private bool _slow;

        public bool Slow
        {
            get
            {
                return _slow;
            }
        }

        public void Update(Skeleton sk)
        {
            _complete = false;
            _slow = false;

            distance = Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandLeft].Position), new Point3D(sk.Joints[JointType.HandRight].Position));

            if (distance < 0.01 && !start)
            {
                start = true;
                down = true;
            }

            if (TrainingWithAvatarViewModel.Get().SkeletonList != null && TrainingWithAvatarViewModel.canBeInterrupted && _count == 0)
            {
                DrawingSheetAvatarViewModel.displayCustomText = "Your turn ! Clap " + NB_APPLAUSE.ToString() + " times";
            }

            if (start)
            {
                if (_count != 0)
                    DrawingSheetAvatarViewModel.displayCustomText = _count.ToString();

                if (_count == NB_APPLAUSE)
                {
                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    frames = 0;
                    _count = 0;
                    start = false;
                    down = false;
                    up = false;
                    _complete = true;
                    GestureRecognized?.Invoke(this, new EventArgs());
                }

                frames++;
                //if(frames == 10)
                {
                    frames = 0;

                    if (down)
                    {
                        if (distance > 0.05)
                        {
                            up = true;
                            down = false;
                        }
                        /*else
                        {
                            start = false;
                            down = false;
                            count = 0;
                            _slow = true;
                            GestureRecognized?.Invoke(this, new EventArgs());
                        }*/
                    }
                    else if (up)
                    {
                        if (distance < 0.01)
                        {
                            down = true;
                            up = false;
                            _count++;

                            GestureRecognized?.Invoke(this, new EventArgs());
                        }
                        /*else
                        {
                            start = false;
                            up = false;
                            count = 0;
                            _slow = true;
                            GestureRecognized?.Invoke(this, new EventArgs());
                        }*/
                    }
                }
            }
        }
    }

}
