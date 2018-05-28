using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LecturerTrainer.Model;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// feedbackPool is a class that manages a pool of feedback 
    /// when a feedback is added to the pool it is displayed on the UI thanks to methods related to it in TrainingSideToolViewModel 
    /// after a fix timeout a feedback is removed from the pool automatically 
    /// </summary>
    public class FeedbackPool
    {

        #region attributes
        /// <summary>
        /// string that will be displayed for each label
        /// </summary>
        private String[] displayLabels;
        /// <summary>
        /// the feedback related to each label 
        /// </summary>
        private Feedback[] label;
        /// <summary>
        /// the time at which we added each feedback to the pool 
        /// </summary>
        private DateTime[] startTimeLabel;
        /// <summary>
        /// fix displaying duration 
        /// </summary>
        private int timeOut = 600;
        /// <summary>
        /// an empty feedback used by internal functions 
        /// </summary>
        private Feedback nullFb;

        private static FeedbackPool instance = null;

        #endregion

        #region CONSTRUCTOR
        /// <summary>
        /// creates a feedback pool with the given array of string
        /// the size of that array will define the size of the pool 
        /// </summary>
        /// <param name="displayLabels"></param>
        public FeedbackPool(String[] displayLabels)
        {
            int nb;
            // we create an empty feedback pool if we get null displaylabels
            if (displayLabels == null)
                nb = 0;
            else
                // the size of the pool is the number of labels 
                nb = displayLabels.Length;
            // get displayLabels 
            this.displayLabels = displayLabels;
            // we initialize each other attribute 
            label = new Feedback[nb];
            startTimeLabel = new DateTime[nb];
            nullFb = new LongFeedback("", false);
            for (int i = 0; i < nb; i++)
            {
                displayLabels[i] = "";
                label[i] = nullFb;
                startTimeLabel[i] = DateTime.MinValue;
            }

            instance = this;
        }

        public static FeedbackPool Get()
        {
            if (instance == null)
            {
                instance = new FeedbackPool(null);
            }
            return instance;
        }
        #endregion

        #region METHODS

        /// <summary>
        ///  returns true if the pool is full and false if not 
        /// </summary>
        /// <returns></returns>
        public bool isFull()
        {
            for (int i = 0; i < label.Length; i++)
            {
                if (label[i].Equals(nullFb))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// returns true if and only if we already have a feedback with the same name in the pool
        /// </summary>
        /// <param name="fb"></param>
        /// <returns></returns>
        public bool isPresent(Feedback fb)
        {
            for (int i = 0; i < label.Length; i++)
            {
                if (fb.feedback.Equals(displayLabels[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// add a feedback to the pool only if the pool is not full
        /// true is returned if a feedback has been added
        /// </summary>
        /// <param name="fb"></param>
        /// <returns></returns>
        public bool addFeedback(Feedback fb)
        {
            // if the pool is full or if the feedback is already present we can ignore adding it
            if (this.isFull() == true || this.isPresent(fb) == true)
                return false;

            for (int i = 0; i < label.Length; i++)
            {
                // when we discover a free place we put the feedback in
                if (label[i].Equals(nullFb))
                {
                    label[i] = fb;
                    startTimeLabel[i] = DateTime.Now;
                    displayLabels[i] = fb.feedback;
                    return true;
                }
            }
            // should never happen
            return false;
        }

        /// <summary>
        /// add a feedback in the pool if it is not full
        /// will remove the feedback to delete before if it exists 
        /// </summary>
        /// <param name="fb"></param>
        /// <param name="fbToDel"></param>
        /// <returns>true if a feedback had been added</returns>
        public bool addFeedbackAndRemoveOne(Feedback fb, String fbToDel)
        {
            for (int i = 0; i < label.Length; i++)
            {
                if (displayLabels[i].Equals(fbToDel))
                {
                    removeFb(i);
                    break;
                }
            }
            return addFeedback(fb);
        }

        /// <summary>
        /// update the pool and remove elements if necessary : it means they passed the timeout
        /// they won't be displayed until another feedback is added 
        /// </summary>
        /// <returns>
        ///     A string array that contains all strings that should be displayed
        ///     It is recommended to use those by affecting those to each Label property 
        /// </returns>
        public string[] update()
        {
            for (int i = 0; i < label.Length; i++)
            {
                // when we arrive at an empty cell all nexts will be empty too 
                if (label[i].Equals(nullFb) == true)
                {
                    break;
                }
                else
                {
                    if (label[i] is LongFeedback)
                    {
                        if (((LongFeedback)label[i]).display == false)
                        {
                            this.removeFb(i);
                        }
                    }
                    else if (startTimeLabel[i].AddMilliseconds(timeOut) <= DateTime.Now)
                    {
                        this.removeFb(i);
                    }
                }
            }
            return displayLabels;
        }

        /// <summary>
        /// removes a feedback from arrays when we reach the timeout
        /// </summary>
        /// <param name="j"></param>
        private void removeFb(int j)
        {
            // case where it's the last element of the stack // should not happen
            if (j == label.Length - 1)
            {
                displayLabels[j] = "";
                label[j] = nullFb;
                startTimeLabel[j] = DateTime.MinValue;
            }
            else
            {
                // we get next elements and put them in the current cell in order to replace
                // the current feedback 
                for (int i = j; i < label.Length - 1; i++)
                {
                    displayLabels[i] = displayLabels[i + 1];
                    label[i] = label[i + 1];
                    startTimeLabel[i] = startTimeLabel[i + 1];
                }
                // then the last cell is empty 
                displayLabels[label.Length - 1] = "";
                label[label.Length - 1] = nullFb;
                startTimeLabel[label.Length - 1] = DateTime.MinValue;
            }

        }

        /// <summary>
        /// will remove a feedback if it is present in the feedbackPool
        /// </summary>
        /// <param name="fb"></param>
        /// <returns></returns>
        public bool removeFb(Feedback fb)
        {
            for (int i = 0; i < label.Length; i++)
            {
                if (displayLabels[i].Equals(fb.feedback))
                {
                    removeFb(i);
                    return true;
                }
            }
            return false;
        }
        #endregion

    }
}
