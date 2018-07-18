using LecturerTrainer.Model;
using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LecturerTrainer.View
{
    public partial class ChoiceFeedbackView : Window
    {

        private static ChoiceFeedbackView instance = null;

        #region Contructor and Getter
        public ChoiceFeedbackView()
        {
            instance = this;
            InitializeComponent();
            ChoiceFeedbackViewModel cfvm = new ChoiceFeedbackViewModel();
            this.DataContext = cfvm;
            InitCheckbox();
            if (TrackingSideTool.Get().ActivateFaceTrackingCheckBox.IsChecked.Value)
            {
                FaceChoiceFeedback.IsEnabled = true;
                EmotionChoiceFeedback.IsEnabled = true;
                LookDirecChoiceFeedback.IsEnabled = true;
            }
            else
            {
                FaceChoiceFeedback.Opacity = 0.5;
                EmotionChoiceFeedback.Opacity = 0.5;
                LookDirecChoiceFeedback.Opacity = 0.5;

            }
            AudioChoiceFeedback.Opacity = 0.5;
            WpmChoiceFeedback.Opacity = 0.5;

        }

        public static ChoiceFeedbackView Get()
        {
            if (instance == null)
                instance = new ChoiceFeedbackView();
            return instance;
        }
        #endregion

        private void InitCheckbox()
        {
            AgitationChoiceFeedback.IsChecked = DrawingSheetAvatarViewModel.Get().displayAgitationFeedback;
            if (DrawingSheetAvatarViewModel.Get().displayHandsJoinedFeedback && DrawingSheetAvatarViewModel.Get().displayArmsCrossedFeedback)
                ArmsChoiceFeedback.IsChecked = true;
            HandsJoinedChoiceFeedback.IsChecked = DrawingSheetAvatarViewModel.Get().displayHandsJoinedFeedback;
            ArmsCrossedChoiceFeedback.IsChecked = DrawingSheetAvatarViewModel.Get().displayArmsCrossedFeedback;
            if (TrackingSideTool.Get().ActivateFaceTrackingCheckBox.IsChecked.Value)
            {
                if (DrawingSheetAvatarViewModel.Get().displayEmotionFeedback && DrawingSheetAvatarViewModel.Get().displayLookDirFeedback)
                    FaceChoiceFeedback.IsChecked = true;
                EmotionChoiceFeedback.IsChecked = DrawingSheetAvatarViewModel.Get().displayEmotionFeedback;
                LookDirecChoiceFeedback.IsChecked = DrawingSheetAvatarViewModel.Get().displayLookDirFeedback;
            }
        }

        private void FeedbackArms_Checked(object sender, RoutedEventArgs e)
        {
            HandsJoinedChoiceFeedback.IsChecked = true;
            ArmsCrossedChoiceFeedback.IsChecked = true;
        }

        private void FeedbackArms_Unchecked(object sender, RoutedEventArgs e)
        {
            HandsJoinedChoiceFeedback.IsChecked = false;
            ArmsCrossedChoiceFeedback.IsChecked = false;
        }

        private void FeedbackFace_Checked(object sender, RoutedEventArgs e)
        {
            EmotionChoiceFeedback.IsChecked = true;
            LookDirecChoiceFeedback.IsChecked = true;
        }

        private void FeedbackFace_Unchecked(object sender, RoutedEventArgs e)
        {
            EmotionChoiceFeedback.IsChecked = false;
            LookDirecChoiceFeedback.IsChecked = false;
        }

        private void FeedbackHandsJoined_Checked(object sender, RoutedEventArgs e)
        {
            if (ArmsCrossedChoiceFeedback.IsChecked.Value)
                ArmsChoiceFeedback.IsChecked = true;
        }

        private void FeedbackArmsCrossed_Checked(object sender, RoutedEventArgs e)
        {
            if (HandsJoinedChoiceFeedback.IsChecked.Value)
                ArmsChoiceFeedback.IsChecked = true;
        }

        private void FeedbackA_UnChecked(object sender, RoutedEventArgs e)
        {
            ArmsChoiceFeedback.Unchecked -= FeedbackArms_Unchecked;
            ArmsChoiceFeedback.IsChecked = false;
            ArmsChoiceFeedback.Unchecked += FeedbackArms_Unchecked;
        }

        private void FeedbackEmotion_Checked(object sender, RoutedEventArgs e)
        {
            if (LookDirecChoiceFeedback.IsChecked.Value)
                FaceChoiceFeedback.IsChecked = true;
        }

        private void FeedbackLookDir_Checked(object sender, RoutedEventArgs e)
        {
            if (EmotionChoiceFeedback.IsChecked.Value)
                FaceChoiceFeedback.IsChecked = true;
        }

        private void FeedbackF_UnChecked(object sender, RoutedEventArgs e)
        {
            FaceChoiceFeedback.Unchecked -= FeedbackFace_Unchecked;
            FaceChoiceFeedback.IsChecked = false;
            FaceChoiceFeedback.Unchecked += FeedbackFace_Unchecked;
        }
    }
}
