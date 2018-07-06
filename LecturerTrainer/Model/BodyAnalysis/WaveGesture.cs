using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    ///<summary>
    /// Class to detect the movement of waving
    /// Added by Baptiste Germond
    ///</summary>
    public class WaveGesture
    {
        /// <summary>
        /// Number of frame max between every part of the movement
        /// </summary>
        readonly int WINDOW_SIZE = 50;

        /// <summary>
        /// Define the if the movement is done with the right or the left hand
        /// </summary>
        const int LEFT = 0;
        const int RIGHT = 1;

        /// <summary>
        /// List of the movement that needs to be detectedto detect the whole movement
        /// </summary>
        IGestureSegment[] _segments;

        /// <summary>
        /// Counters for the detection
        /// </summary>
        int _currentSegment = 0;
        int _frameCount = 0;
        int _frameGesture = 0;

        /// <summary>
        /// At the begining we don't know which hand the user will use
        /// </summary>
        static int currentWaveSide = -1;

        public int FrameGesture
        {
            get
            {
                return _frameGesture;
            }
        }
         
        /// <summary>
        /// Event handler when the lmovement is recognized
        /// </summary>
        public event EventHandler GestureRecognized;

        public WaveGesture()
        {
            //Create the different parts of the movement
            WaveSegment1 waveSegment1 = new WaveSegment1();
            WaveSegment2 waveSegment2 = new WaveSegment2();

            //And define the order to execute theme to create the movement
            _segments = new IGestureSegment[]
            {
                    waveSegment1,
                    waveSegment2,
                    waveSegment1,
                    waveSegment2
            };
        }

        public void Update(Skeleton skeleton)
        {
            //Test if the software recognize a part of the movement on the left side
            GesturePartResult resultLEFT = _segments[_currentSegment].Update(skeleton,LEFT);
            //Test if the software recognize a part of the movement on the right side
            GesturePartResult resultRIGHT = _segments[_currentSegment].Update(skeleton, RIGHT);
            GesturePartResult result = GesturePartResult.Failed;

            //If the user is doing the movement with his left arm
            if (resultLEFT == GesturePartResult.Succeeded)
            {
                //If he wasnt's doing it with his left arm before
                if (currentWaveSide != LEFT)
                {
                    //We reset the counters to detect the movement
                    Reset();
                    currentWaveSide = LEFT;
                }
                //We indicate that one of the part of the gesture has been recognized
                result = GesturePartResult.Succeeded;
            }
            //If the user is doing the movement with his right arm
            if (resultRIGHT == GesturePartResult.Succeeded)
            {
                //If he wasnt's doing it with his right arm before
                if (currentWaveSide != RIGHT)
                {
                    //We reset the counters to detect the movement
                    Reset();
                    currentWaveSide = RIGHT;
                }
                //We indicate that one of the part of the gesture has been recognized
                result = GesturePartResult.Succeeded;
            }

            if (result == GesturePartResult.Succeeded)
            {
                //If the movement as not been recognized entirely
                if (_currentSegment + 1 < _segments.Length)
                {
                    //We advance to the next part of the mouvement and reset the number of frame the user had to do this part of the gesture
                    _currentSegment++;
                    _frameCount = 0;
                    _frameGesture++;
                }
                else
                {
                    //If the gesture has been recognized, we throw the event
                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new EventArgs());
                        Reset();
                    }
                }
            }
            //If the user took too much time to do the nextpart of the gesture
            else if (_frameCount == WINDOW_SIZE)
            {
                //We erset the recognition
                Reset();
            }
            else
            {
                //We keep counting the nuber of frame to do the entire movement
                if (_currentSegment != 0)
                    _frameGesture++;
                _frameCount++;
            }
        }

        public void Reset()
        {
            _currentSegment = 0;
            _frameCount = 0;
            _frameGesture = 0;
        }

        public interface IGestureSegment
        {
            GesturePartResult Update(Skeleton skeleton, int type);
        }

        /// <summary>
        /// The two part of the waving movement
        /// </summary>
        public class WaveSegment1 : IGestureSegment
        {
            public GesturePartResult Update(Skeleton skeleton, int type)
            {
                // Hand above elbow
                if ((type == LEFT && skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y) 
                    ||(type==RIGHT && skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y))
                {
                    // Hand right of elbow
                    if ((type == LEFT && skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ElbowLeft].Position.X)
                        || (type == RIGHT && skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X))
                    {
                        return GesturePartResult.Succeeded;
                    }
                }

                // Hand dropped
                return GesturePartResult.Failed;
            }
        }

        public class WaveSegment2 : IGestureSegment
        {
            public GesturePartResult Update(Skeleton skeleton,int type)
            {
                // Hand above elbow
                if ((type == LEFT && skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y)
                    || (type == RIGHT && skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y))
                {
                    // Hand left of elbow
                    if ((type == LEFT && skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X)
                        || (type == RIGHT && skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X ))
                    {
                        return GesturePartResult.Succeeded;
                    }
                }

                // Hand dropped
                return GesturePartResult.Failed;
            }
        }

        public enum GesturePartResult
        {
            Failed,
            Succeeded
        }
    }
}
