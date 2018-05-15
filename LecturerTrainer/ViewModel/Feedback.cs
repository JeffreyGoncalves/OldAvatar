using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace LecturerTrainer.ViewModel
{
    
    /// <summary>
    /// Represent a feedback to display.
    /// </summary>
    /// <remarks>Author: Clement Michard</remarks>
    public class Feedback : EventArgs
    {
        #region PROPERTIES

        private string _feedback; 

        /// <summary>
        /// The feedback message.
        /// </summary>
        public string feedback
        {
            get
            {
                return _feedback; 
            }
            set 
            {
                _feedback = value;
            }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construct a feedback object.
        /// </summary>
        /// <param name="feedback">Message of the feedback.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public Feedback(string feedback) : base()
        {
            this.feedback = feedback;
        }

        #endregion
    }

    /// <summary>
    /// Feedback to display between a start event and an end event.
    /// </summary>
    /// <remarks>Author: Clement Michard</remarks>
		
		
    public class LongFeedback : Feedback
    {
         #region PROPERTIES

       /// <summary>
        /// If display is true, it means that the event is the start of the event. It has to be displayed.
        /// If display is false, it means that the event is the end of the event. It has to be erased.
        /// </summary>
        public bool display;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Construct the long feedback.
        /// </summary>
        /// <param name="feedback">Message of the feedback.</param>
        /// <param name="display">True of it is the start of the event, false if it is the end.</param>
        /// <remarks>Author: Clement Michard.</remarks>
        public LongFeedback(string feedback, bool display)
            : base(feedback)
        {
            this.display = display;
        }

        #endregion
    }

    /// <summary>
    /// Feedback of a single event to display during a short time.
    /// </summary>
    /// <remarks>Author: Clement Michard.</remarks>
    public class InstantFeedback : Feedback
    {
        #region STATIC PROPERTIES

        /// <summary>
        /// Feedback to erase at the end of the timer. It allows not to remove the current feedback if it is not the feedback to erase anymore.
        /// </summary>
        protected static string feedbackToErase = null;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Timer used to display a feedback during a short time.
        /// </summary>
        public Timer timerLog;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Construct a feedback.
        /// </summary>
        /// <param name="feedback">Message to display.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public InstantFeedback(string feedback)
            : base(feedback)
        {
        }

        #endregion
    }



    /// <summary>
    /// Feedback of a single event to display. It is linked to a value which represent the "power" of the feedback.
    /// </summary>
    public class ValuedFeedback : LongFeedback
    {
       #region PROPERTIES

       /// <summary>
        /// Power of the feedback
        /// </summary>
        public int value;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Construct the valued feedback.
        /// </summary>
        /// <param name="feedback">Message of the feedback.</param>
        /// <param name="display">True of it is the start of the event, false if it is the end.</param>
        /// <remarks>Author: Clement Michard.</remarks>
        public ValuedFeedback(string feedback, bool display, int value)
            : base(feedback, display)
        {
            this.value = value;
        }

        #endregion
    }

    /// <summary>
    /// Represent a feedback which contain an int.
    /// </summary>
    public class IntFeedback : EventArgs
    {
        #region PROPERTIES

        private int _feedback;

        /// <summary>
        /// The feedback message.
        /// </summary>
        public int feedback
        {
            get
            {
                return _feedback;
            }
            set
            {
                _feedback = value;
            }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construct a feedback object.
        /// </summary>
        /// <param name="feedback">Message of the feedback.</param>
        public IntFeedback(int feedback) : base()
        {
            this.feedback = feedback;
        }

        #endregion
    }
}
