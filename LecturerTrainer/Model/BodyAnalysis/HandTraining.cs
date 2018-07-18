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
        private static int count;

        private static bool start;
        private static bool up;
        private static bool down;

        bool _complete;

        public bool Complete
        {
            get
            {
                return _complete;
            }
        }

        bool _slow;

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

            if(distance < 0.01 && !start)
            {
                start = true;
                down = true;
            }

            switch(count)
            {
                case 1:
                    DrawingSheetAvatarViewModel.displayCustomText = "One";
                    break;
                case 2:
                    DrawingSheetAvatarViewModel.displayCustomText = "Two";
                    break;
                case 3:
                    DrawingSheetAvatarViewModel.displayCustomText = "Three";
                    break;
            }

            if(TrainingWithAvatarViewModel.Get().SkeletonList != null && TrainingWithAvatarViewModel.canBeInterrupted && count == 0)
            {
                DrawingSheetAvatarViewModel.displayCustomText = "Your turn ! Clap three times";
            }

            if (start)
            {
                if(count == NB_APPLAUSE)
                {
                    DrawingSheetAvatarViewModel.displayCustomText = String.Empty;
                    frames = 0;
                    count = 0;
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

                    if(down)
                    {
                        if(distance > 0.05)
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
                    else if(up)
                    {
                        if(distance < 0.01)
                        {
                            down = true;
                            up = false;
                            count++;
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
