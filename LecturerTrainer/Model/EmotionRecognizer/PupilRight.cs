using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.EmotionRecognizer
{
    class pupilRight
    {

        public static bool detect = false; //this variable is used in SkeletonFaceTracker to detect that the checkbox has been checked
        public static event EventHandler<LongFeedback> pupilREvent;

        private static double maxDistance = 0.02;
        private static bool right = false;

        private static int count = 30;
        private static bool faceDetected = false;

        public static void pupilRecognizer()
        {
            /*Using a counter so it won't go to much in the try/catch and make the software lag*/
            if (count == 30 || faceDetected == true)
            {
                count = 0;
                try
                {
                    faceDetected = true;
                    var temp = KinectDevice.skeletonFaceTracker.facePoints3D;
                    var rightEye = temp.ElementAt(20);
                    var pupil = temp.ElementAt(71);
                    //          if((Math.Sqrt(Math.Pow(upperLip.X - lowerLip.X, 2)) > maxDistance))
                    if (!right && Math.Abs(rightEye.X - pupil.X) < maxDistance)
                    {
                        right = true;
                        pupilREvent(null, new LongFeedback("Pupil Right", true));
                    }
                    else if (right)
                    {
                        right = false;
                        pupilREvent(null, new LongFeedback("Pupil Right", false));
                    }
                }
                catch (NullReferenceException) { faceDetected = false; }
                catch (ArgumentNullException) { faceDetected = false; }
            }
            else { count++; }

        }

        public static void raisePupilREvent(ServerFeedback feedback)
        {
            pupilREvent(null, new LongFeedback(feedback.feedbackMessage, feedback.display));
        }
    }
}