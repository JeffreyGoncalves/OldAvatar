using LecturerTrainer.Model;
using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
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
	public partial class TrackingSideTool : UserControl
    {
        private static TrackingSideTool instance = null;

		public TrackingSideTool()
		{
            TrackingSideToolViewModel tstvm = TrackingSideToolViewModel.get();
			this.InitializeComponent();
            this.DataContext = tstvm; 
            SideToolsViewModel.Get().setTrackingV(this);
            SideToolsViewModel.Get().setTrackingVM(tstvm); 
		}

        public static TrackingSideTool Get()
        {
            if (instance == null)
                instance = new TrackingSideTool();
            return instance;
        }

        private void CheckBoxF_Checked(object sender, RoutedEventArgs e)
        {
            DrawingSheetAvatarViewModel.Get().diplayFeedback = false;
            {
                FeedbackBodyCheckBox.IsChecked = true;
                DrawingSheetAvatarViewModel.Get().diplayBodyFeedback = false;
            }
            if(FeedbackFaceCheckBox.IsEnabled)
            {
                FeedbackFaceCheckBox.IsChecked = true;
                DrawingSheetAvatarViewModel.Get().diplayFaceFeedback = false;
            }
        }

        private void CheckBoxF_Unchecked(object sender, RoutedEventArgs e)
        {
            DrawingSheetAvatarViewModel.Get().diplayFeedback = true;
            {
                FeedbackBodyCheckBox.IsChecked = false;
                DrawingSheetAvatarViewModel.Get().diplayBodyFeedback = true;
            }
            {
                FeedbackFaceCheckBox.IsChecked = false;
                DrawingSheetAvatarViewModel.Get().diplayFaceFeedback = true;
            }
        }

        private void CheckBoxFB_Checked(object sender, RoutedEventArgs e)
        {
            DrawingSheetAvatarViewModel.Get().diplayBodyFeedback = false;
            if (FeedbackFaceCheckBox.IsChecked.Value)
            {
                DrawingSheetAvatarViewModel.Get().diplayFeedback = false;
                FeedbackCheckBox.IsChecked = true;
            }
        }

        private void CheckBoxFB_Unchecked(object sender, RoutedEventArgs e)
        {
            DrawingSheetAvatarViewModel.Get().diplayBodyFeedback = true;
        }

        private void CheckBoxFF_Checked(object sender, RoutedEventArgs e)
        {
            DrawingSheetAvatarViewModel.Get().diplayFaceFeedback = false;
            if (FeedbackBodyCheckBox.IsChecked.Value)
            {
                DrawingSheetAvatarViewModel.Get().diplayFeedback = false;
                FeedbackCheckBox.IsChecked = true;
            }
        }

        private void CheckBoxFF_Unchecked(object sender, RoutedEventArgs e)
        {
            DrawingSheetAvatarViewModel.Get().diplayFaceFeedback = true;
        }
    }
}