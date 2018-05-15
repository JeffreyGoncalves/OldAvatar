using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.EmotionRecognizer
{
    class mouthShut
    {

        public static bool detect = false; //this variable is used in SkeletonFaceTracker to detect that the checkbox has been checked
        public static event EventHandler<LongFeedback> mouth2Event;

        private static double maxDistance = 0.01;
        private static bool shut = false;

        private static int count = 30;
        private static bool faceDetected = false;

        public static void mouth2Recognizer()
        {
            /*Using a counter so it won't go to much in the try/catch and make the software lag*/
            if (count == 30 || faceDetected==true)
            {
                count = 0;
                try {
                    faceDetected = true;
                    var temp = KinectDevice.skeletonFaceTracker.facePoints3D;
                    var upperLip = temp.ElementAt(7);
                    var lowerLip = temp.ElementAt(40);
                    //          if((Math.Sqrt(Math.Pow(upperLip.X - lowerLip.X, 2)) > maxDistance))
                    if (!shut && Math.Abs(upperLip.Y - lowerLip.Y) < maxDistance)
                    {
                        shut = true;
                        mouth2Event(null, new LongFeedback("Mouth Shut", true));
                    }
                    else if (shut)
                    {
                        shut = false;
                        mouth2Event(null, new LongFeedback("Mouth Shut", false));
                    }
                }
                catch (NullReferenceException) { faceDetected = false; }
                catch (ArgumentNullException) { faceDetected = false; }
            }
            else { count++; }

        }

        public static void raiseShutEvent(ServerFeedback feedback)
        {
            mouth2Event(null, new LongFeedback(feedback.feedbackMessage, feedback.display));
        }
    }
}