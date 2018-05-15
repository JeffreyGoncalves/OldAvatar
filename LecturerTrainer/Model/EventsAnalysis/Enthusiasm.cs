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
    // We want to link the hands raised event with the emotion event "happy" to generate an "enthusiasm" event
    public class Enthusiasm
    {
        public static event EventHandler<InstantFeedback> enthusiasmEvent;
        /// <summary>
        /// The message that will be displayed
        /// </summary>
        public static string enthusiasmMessage = "Really enthusiastic!";

        /// <summary>
        /// A timer allowing to detect the two events in a limited time
        /// </summary>
        private static Timer timer;

        /// <summary>
        /// Boolean values setted to true when the good event is detected
        /// </summary>
        private static bool happyDetected = false;
        private static bool handsRaisedDetected = false;

        /// <summary>
        /// This method will receive either handsRaised feedback or emotion feedback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void enthusiasmHandler(object sender, Feedback e)
        {
            // If it is the first time that an event is caught, the timer is initialized
            if (timer == null)
            {
                // 2 seconds is the time slot in whiwh the two events have to be caught
                timer = new Timer(2000);
                timer.Elapsed += resetDetection;
                timer.Start();
            }

            if (e.feedback.StartsWith(HandsRaised.bothHandsRaised))
                handsRaisedDetected = true;
            else if (e.feedback == "Happy")
                happyDetected = true;

            // If both events have been detected, an enthusiasm event is raised
            if (handsRaisedDetected && happyDetected)
                enthusiasmEvent(null, new InstantFeedback(enthusiasmMessage));
        }


        /// <summary>
        /// Every 2 seconds, the two boolean values are updated to false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void resetDetection(object sender, ElapsedEventArgs e)
        {
            happyDetected = false;
            handsRaisedDetected = false;
        }

        public static void raiseEnthusiasmEvent()
        {
            enthusiasmEvent(null, new InstantFeedback(enthusiasmMessage));
        }

        public static void raiseEnthusiasmEvent(ServerFeedback feedback)
        {
            enthusiasmEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }
    }
}
