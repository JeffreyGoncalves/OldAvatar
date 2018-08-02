using LecturerTrainer.Model;
using LecturerTrainer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// Added by Alban Descottes
    /// </summary>
    class ChoiceFeedbackViewModel
    {

        private ChoiceFeedbackView choiceFeedbackView;

        public ChoiceFeedbackViewModel()
        {
            choiceFeedbackView = ChoiceFeedbackView.Get();
        }

        #region command ok and cancel
        /// <summary>
        /// ICommand for the ok button
        /// </summary>
        private ICommand okCustomizeFeedback;
        public ICommand OKCustomizeFeedback
        {
            get
            {
                if (this.okCustomizeFeedback == null)
                    this.okCustomizeFeedback = new RelayCommand(() => this.ChangeFeedbacks());

                return this.okCustomizeFeedback;
            }
        }

        /// <summary>
        /// ICommand for the cancel Button
        /// </summary>
        private ICommand cancelCustomiseFeedback;
        public ICommand CancelCustomizeFeedback
        {
            get
            {
                if (this.cancelCustomiseFeedback == null)
                    this.cancelCustomiseFeedback = new RelayCommand(() => LaunchCancel());

                return this.cancelCustomiseFeedback;
            }
        }

        /// <summary>
        /// if the user press ok
        /// </summary>
        private void ChangeFeedbacks()
        {
            DrawingSheetAvatarViewModel.Get().displayAgitationFeedback = choiceFeedbackView.AgitationChoiceFeedback.IsChecked.Value;
            DrawingSheetAvatarViewModel.Get().displayArmsCrossedFeedback = choiceFeedbackView.ArmsCrossedChoiceFeedback.IsChecked.Value;
            DrawingSheetAvatarViewModel.Get().displayHandsJoinedFeedback = choiceFeedbackView.HandsJoinedChoiceFeedback.IsChecked.Value;
            if (TrackingSideTool.Get().ActivateFaceTrackingCheckBox.IsChecked.Value)
            {
                DrawingSheetAvatarViewModel.Get().displayEmotionFeedback = choiceFeedbackView.EmotionChoiceFeedback.IsChecked.Value;
                DrawingSheetAvatarViewModel.Get().displayLookDirFeedback = choiceFeedbackView.LookDirecChoiceFeedback.IsChecked.Value;
            }
            choiceFeedbackView.Close();
        }

        /// <summary>
        /// if the user press cancel
        /// </summary>
        public void LaunchCancel()
        {
            choiceFeedbackView.Close();
        }
        #endregion
    }
}
