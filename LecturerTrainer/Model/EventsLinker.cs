using LecturerTrainer.Model.BodyAnalysis;
using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LecturerTrainer.Model
{
    public class EventsLinker
    {
        public static event EventHandler<InstantFeedback> enthusiasmEvent;
       // private static Timer enthusiasmTimer;

        // We want to link the hands raised event with the emotion event "happy" to generate an "enthusiasm" event
        private void enthusiasmHandler(object sender, InstantFeedback e)
        {
            if(e.feedback == HandsRaised.bothHandsRaised)
            {

            }
        }
    }
}
