using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.EmotionRecognizer
{
    class mouthOpened
    {

        public static bool detect = false; //this variable is used in SkeletonFaceTracker to detect that the checkbox has been checked
        public static event EventHandler<LongFeedback> mouthEvent;

        private static double maxDistance = 0.01;
        private static bool opened = false;

        private static int count = 30;
        private static bool faceDetected = false;

        public static void mouthRecognizer()
        {
            /*Using a counter so it won't go to much in the try/catch and make the software lag*/
            if (count == 30 || faceDetected == true)
            {
                count = 0;
                try
                {
                    faceDetected = true;
                    var temp = KinectDevice.skeletonFaceTracker.facePoints3D;
                    var upperLip = temp.ElementAt(7);
                    var lowerLip = temp.ElementAt(40);
                    //          if((Math.Sqrt(Math.Pow(upperLip.X - lowerLip.X, 2)) > maxDistance))
                    if (!opened && Math.Abs(upperLip.Y - lowerLip.Y) > maxDistance)
                    {
                        opened = true;
                        mouthEvent(null, new LongFeedback("Mouth Open", true));
                    }
                    else if (opened)
                    {
                        opened = false;
                        mouthEvent(null, new LongFeedback("Mouth Open", false));
                    }
                }
                catch (NullReferenceException) { faceDetected = false; }
                catch (ArgumentNullException) { faceDetected = false; }
            }
            else { count++; }

        }

        public static void raiseMouthEvent(ServerFeedback feedback)
        {
            mouthEvent(null, new LongFeedback(feedback.feedbackMessage, feedback.display));
        }
    }
}