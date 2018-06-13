using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LecturerTrainer.Model;
using LecturerTrainer.View;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Timers;
using Microsoft.Win32;
using LecturerTrainer.Model.AudioAnalysis;
using LecturerTrainer.Model.EmotionRecognizer;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using Microsoft.Kinect.Toolkit;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// creates all the lateral panel and manages them 
    /// </summary>
    public class SideToolsViewModel : ViewModelBase
    {
        #region fields
        /// <summary>
        /// Instance of the class
        /// </summary>
        private static SideToolsViewModel stvm = null;

        /// <summary>
        /// Linked view
        /// </summary>
        private SideToolsView sideToolV;

        /// <summary>
        /// General tools subclass view
        /// </summary>
        private GeneralSideTool generalV;

        /// <summary>
        /// tracking tools subclass view
        /// </summary>
        private TrackingSideTool trackingV;

        /// <summary>
        /// training tools subclass view
        /// </summary>
        private TrainingSideTool trainingV;

        /// <summary>
        /// general tools subclass viewmodel
        /// </summary>
        private GeneralSideToolViewModel generalVM;

        /// <summary>
        /// tracking tools subclass viewmodel
        /// </summary>
        private TrackingSideToolViewModel trackingVM;

        /// <summary>
        /// training tools subclass viewmodel
        /// </summary>
        private TrainingSideToolViewModel trainingVM;
        #endregion

        #region constructor (empty) and Get
        private SideToolsViewModel()
        {

        }

        public static SideToolsViewModel Get()
        {
            if (stvm == null)
                stvm = new SideToolsViewModel();
            return stvm;
        }

        #endregion

        #region methods

        public void setToolV (SideToolsView stv)
        {
            this.sideToolV = stv; 
        }

        public void setGeneralV(GeneralSideTool gst)
        {
            this.generalV = gst; 
        }

        public void setTrackingV(TrackingSideTool tst)
        {
            this.trackingV = tst; 
        }

        public void setTrainingV(TrainingSideTool tst)
        {
            this.trainingV = tst; 
        }

        /// <summary>
        /// Set the GeneralSideToolViewModel
        /// </summary>
        /// <param name="gstvm"></param>
        /// <remarks>As the other setters in this class, this is actually only used as an initializer for the variable generalVM,
        /// called in the constructor of the GeneralSideToolViewModel
        /// </remarks>
        public void setGeneralVM(GeneralSideToolViewModel gstvm)
        {
            this.generalVM = gstvm;
        }

        public void setTrackingVM(TrackingSideToolViewModel tstvm)
        {
            this.trackingVM = tstvm; 
        }

        public void setTrainingVM(TrainingSideToolViewModel tstvm)
        {
            this.trainingVM = tstvm; 
        }

        /// <summary>
        /// Voice control main function. Links every key to an action.
        /// </summary>
        /// <param name="value"></param>
        public void speechRecognized(string value)
        {
            if (generalV.VoiceControlCheckBox.IsChecked == true)
            {
                Storyboard sb = null;
                switch (value)
                {
                    case "QUIT":
                        Application.Current.Shutdown();
                        break;
                    case "STREAM":
                        this.generalV.Stream.IsChecked = true;
                        activeStream();
                        break;
                    case "AUTOELEVATION":
                        this.generalV.AutoElevation.IsChecked = true;
                        activeAutoElevation();
                        break;
                    case "MANUAL":
                        this.generalV.AutoElevation.IsChecked = false;
                        disableAutoElevation();
                        break;
                    case "GENERAL":
                        sb = this.sideToolV.FindResource("generalTabSelected") as Storyboard;
                        sb.Begin();
                        sb = this.sideToolV.FindResource("GeneralTabVisible") as Storyboard;
                        sb.Begin();
                        break;
                    case "BODY":
                    case "FACE":
                    case "SOUND":
                    case "TRACKING":
                        sb = this.sideToolV.FindResource("TrackingTabSelected") as Storyboard;
                        sb.Begin();
                        sb = this.sideToolV.FindResource("TrackingTabVisible") as Storyboard;
                        sb.Begin();
                        break;
                    case "TRAINING":
                        sb = this.sideToolV.FindResource("TrainingSelected") as Storyboard;
                        sb.Begin();
                        sb = this.sideToolV.FindResource("TrainingTabVisible") as Storyboard;
                        sb.Begin();
                        break;
                    case "RECORD":
                        if (this.trainingVM.State == IRecordingState.Paused || this.trainingVM.State == IRecordingState.RequestedStop || this.trainingVM.State == IRecordingState.Stopped)
                        {
                            Button b = this.trainingV.FindName("startRecordingButton") as Button;
                            b.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            b.Command.Execute(null);
                        }
                        else if (this.trainingVM.State == IRecordingState.Monitoring || this.trainingVM.State == IRecordingState.Recording)
                        {
                            Button b = this.trainingV.FindName("stopRecordingButton") as Button;
                            b.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            b.Command.Execute(null);
                        }
                        break;
                    case "UP":
                        (GeneralSideTool.Get().DataContext as GeneralSideToolViewModel).upElevation();
                        break;
                    case "DOWN":
                        (GeneralSideTool.Get().DataContext as GeneralSideToolViewModel).downElevation();
                        break;
                }
            }
        }

        public void activeStream()
        {
            generalVM.SelectStream.Execute(generalVM.SelectStream);
        }

        public void activeAutoElevation()
        {
            generalVM.UseAutoElevation = true;
        }

        public void disableAutoElevation()
        {
            generalVM.UseAutoElevation = false;
        }

        private void addSideToolsVM(object sender, EventArgs e)
        {
            Main.isReady -= addSideToolsVM;
        }

        public void disableTrackingTab()
        {
            (this.sideToolV.FindResource("TrainingSelected") as Storyboard).Begin();
            (this.sideToolV.FindResource("TrainingTabVisible") as Storyboard).Begin();
            (this.sideToolV.FindResource("DisableTrackingTabAction") as Storyboard).Begin();
        }

        public void disableTrackingAndTrainingTab()
        {
            if (this.sideToolV != null)
                (this.sideToolV.FindResource("DisableTrackingAndTraining") as Storyboard).Begin();
        }
        public void enableTrackingAndTrainingTab()
        {
            if (this.sideToolV != null)
                (this.sideToolV.FindResource("EnableTrackingAndTraining") as Storyboard).Begin();
        }

        public void allTabsSelectable()
        {
            /*Modified by Baptiste Germond 
             * -- Ensure that the actions are synchronised with the current thread to avoid conflict*/
           App.Current.Dispatcher.Invoke((Action)delegate
            {
                (this.sideToolV.FindResource("AllTabsAction") as Storyboard).Begin();
            });
        }

        public void chooseTraining()
        {
            Storyboard sb1 = this.sideToolV.FindResource("TrainingSelected") as Storyboard;
            Storyboard sb2 = this.sideToolV.FindResource("TrainingTabVisible") as Storyboard;
            sb1.Begin();
            sb2.Begin();

        }
        #endregion
    }
}
