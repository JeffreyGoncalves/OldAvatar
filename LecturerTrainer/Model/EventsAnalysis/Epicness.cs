using LecturerTrainer.Model.BodyAnalysis;
using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LecturerTrainer.Model.EventsAnalysis
{
    public class Epicness
    {
        // We want to link the hands raised event with the keyWordEvent "Dream" to generate an "epic" event
        public static event EventHandler<InstantFeedback> epicnessEvent;
        /// <summary>
        /// The message that will be displayed
        /// </summary>
        public static string epicnessMessage = "Epic!";

        /// <summary>
        /// A timer allowing to detect the two events in a limited time
        /// </summary>
        private static Timer timer;

        /// <summary>
        /// Boolean values setted to true when the good event is detected
        /// </summary>
        private static bool KeyWordDetected = false;
        private static bool handsRaisedDetected = false;

        /// <summary>
        /// This method will receive either handsRaised feedback or keyword feedback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void epicnessHandler(object sender, Feedback e)
        {
            // If it is the first time that an event is caught, the timer is initialized
            if (timer == null)
            {
                // 3 seconds is the time slot in whiwh the two events have to be caught
                timer = new Timer(3000);
                timer.Elapsed += resetDetection;
                timer.Start();
            }

            if (e.feedback.StartsWith(HandsRaised.leftHandRaised) ||
                e.feedback.StartsWith(HandsRaised.rightHandRaised) ||
                e.feedback.StartsWith(HandsRaised.bothHandsRaised))
                handsRaisedDetected = true;
            else if (e.feedback == "Dream")
                KeyWordDetected = true;

            // If both events have been detected, an epicness event is raised
            if (handsRaisedDetected && KeyWordDetected)
                epicnessEvent(null, new InstantFeedback(epicnessMessage));
        }

        /// <summary>
        /// Every 3 seconds, the two boolean values are updated to false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void resetDetection(object sender, ElapsedEventArgs e)
        {
            KeyWordDetected = false;
            handsRaisedDetected = false;
        }

        public static void raisEpicnessEvent()
        {
            epicnessEvent(null, new InstantFeedback(epicnessMessage));
        }

        public static void raisEpicnessEvent(ServerFeedback feedback)
        {
            epicnessEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }
    }
}
