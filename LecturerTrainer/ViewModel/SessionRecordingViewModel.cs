using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LecturerTrainer.View;
using LecturerTrainer.Model;
using System.Windows.Input;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Timers;
using System.Windows;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// Bind with the session recoding xaml to manage the recoding session
    /// Added by Baptiste Germond
    /// </summary>
    class SessionRecordingViewModel : ViewModelBase
    {
        #region attribut

        /// <summary>
        /// Command link to the button of the panel
        /// </summary>
        private ICommand cancelCommand;
        private ICommand okCommand;

        /// <summary>
        /// Value choose by the user to determine the time of the record and the time to wait before starting the record
        /// </summary>
        private int secRecord = 30;
        private int minRecord = 0;
        private int secWait = 3;

        /// <summary>
        /// Value use to determine which things will be analyse during the record
        /// </summary>
        private bool faceRecognition = false;
        private bool bodyRecognition = true;
        private bool voiceRecognition = false;

        /// <summary>
        /// Boolean to know if the record has been launched
        /// </summary>
        public static bool inRecord = false;

        /// <summary>
        /// Own instance to return with the getter
        /// </summary>
        private static SessionRecordingViewModel sessionRecordingViewModel = null;

        /// <summary>
        /// Instance of the view link to this viewmodel (SessionRecording.xaml)
        /// </summary>
        private SessionRecording viewSess;

        /// <summary>
        /// Timer used for the waiting time
        /// </summary>
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        int secLaunchedTimer = 0;
        String stringTimer ="0";

        #endregion

        #region constructor
        public SessionRecordingViewModel(SessionRecording newSess)
        {
            viewSess = newSess;
            sessionRecordingViewModel = this;
        }
        #endregion

        #region properties
        public static SessionRecordingViewModel Get()
        {
            if (sessionRecordingViewModel == null)
            {
                sessionRecordingViewModel = new SessionRecordingViewModel(null);
            }
            return sessionRecordingViewModel;
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                    this.cancelCommand = new RelayCommand(() => this.Cancel(), () => this.CanCancel());

                return this.cancelCommand;
            }
        }
        public ICommand OkCommand
        {
            get
            {
                if (this.okCommand == null)
                    this.okCommand = new RelayCommand(() => this.Ok(), () => this.CanOk());

                return this.okCommand;
            }
        }

        public int SecRecord
        {
            get
            {
                return secRecord;
            }
            set
            {
               secRecord = value;
               OnPropertyChanged("SecRecord");
            }
        }

        public int MinRecord
        {
            get
            {
                return minRecord;
            }
            set
            {
                minRecord = value;
                OnPropertyChanged("MinRecord");
            }
        }

        public int SecWait
        {
            get
            {
                return secWait;
            }
            set
            {
                secWait = value;
                OnPropertyChanged("SecWait");
            }
        }

        public bool FaceRecognition
        {
            get
            {
                return faceRecognition;
            }
            set
            {
                faceRecognition = value;
                if (value == true)
                    MessageBox.Show("The face needs to be tracked before enabling the analyze and the recording of the face","Face Tracking", MessageBoxButton.OK, MessageBoxImage.Information);
                OnPropertyChanged("FaceRecognition");
            }
        }

        public bool BodyRecognition
        {
            get
            {
                return bodyRecognition;
            }
            set
            {
                bodyRecognition = value;
                OnPropertyChanged("BodyRecognition");
            }
        }

        public bool VoiceRecognition
        {
            get
            {
                return voiceRecognition;
            }
            set
            {

                voiceRecognition = value;
                OnPropertyChanged("VoiceRecognition");
            }
        }

        public String countStartRecord
        {
            get { return stringTimer; }
            set
            {
                stringTimer = value;
                OnPropertyChanged("countStartRecord");
            }
        }
        #endregion

        #region methods
        private bool CanCancel()
        {
            return true;
        }

        public void Cancel()
        {
            if (dispatcherTimer.IsEnabled == true)
                dispatcherTimer.Stop();
            this.viewSess.Close();
        }

        private bool CanOk()
        {    
            return true;
        }     

        public void Ok()
        {
            if (!ReplayViewModel.isReplaying)
            {
                this.viewSess.OkButton.IsEnabled = false;
                this.viewSess.timerLaunchRecord.Visibility = System.Windows.Visibility.Visible;
                this.viewSess.checkBox1.IsEnabled = false;
                this.viewSess.checkBox2.IsEnabled = false;
                this.viewSess.checkBox3.IsEnabled = false;
                this.viewSess.recordMin.IsEnabled = false;
                this.viewSess.recordSec.IsEnabled = false;
                this.viewSess.waitSec.IsEnabled = false;

                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                dispatcherTimer.Start();
                secLaunchedTimer = DateTime.Now.Second;
                countStartRecord = secWait.ToString();

            }
            else
            {
                new ErrorMessageBox("Starting a record", "You need to close the replay mode before\nlaunching any new record").ShowDialog();
            }

        }        

        /// <summary>
        /// Event for the waiting time, to update the view and launch the record when it is finished 
        /// </summary>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            secWait -= 1;
            countStartRecord = secWait.ToString();
            CommandManager.InvalidateRequerySuggested();

            if (secWait <= 0)
            {
                dispatcherTimer.Stop();
                this.viewSess.Close();
                launchRecordingAfterWait();
            }
        }

        /// <summary>
        /// Launch the record when the waiting time is over
        /// </summary>
        public void launchRecordingAfterWait()
        {
            inRecord = true;

            allFalse();
            if (FaceRecognition == true)
            {
                TrackingSideToolViewModel tracking = TrackingSideToolViewModel.get();
                tracking.FaceTracking = true;
                tracking.emo = true;
                //tracking.Mouth = true;
                //tracking.Mouth2 = true; //Those feature are not working for now
                //tracking.pupilR = true;
                tracking.LookR = true;
            }
            if (VoiceRecognition == true)
            {
                TrackingSideToolViewModel tracking = TrackingSideToolViewModel.get();
                //tracking.PeakDetection = true; //Create lags when in record
                tracking.SpeedRate = true;
                tracking.ShowTextOnScreen = true;
                /*tracking.VoiceMonotony = true; 
                tracking.BadVoiceReflex = true;*/ //Features are not working

            }
            if (BodyRecognition == true)
            {
                TrackingSideToolViewModel.get().UseFeedback = true;
            }          

            SideToolsViewModel.Get().chooseTraining();

            TrainingSideToolViewModel tstvm = TrainingSideToolViewModel.Get();
            tstvm.limitedTimeHours = "0";
            tstvm.limitedTimeMinutes = MinRecord.ToString();
            tstvm.limitedTimeSeconds = SecRecord.ToString();
            tstvm.isTimeLimited = true;

            /*Depending on which view the user was using when he start his session record
             * the session recording will record the view the user was using*/
            if (MainWindow.drawingSheet.getMode() == SheetMode.AvatarMode)
            {
                tstvm.ToggleAvatarVideoRecording = true;
                tstvm.ToggleAvatarOpenGLRecording = true;
            }
            else if (MainWindow.drawingSheet.getMode() == SheetMode.StreamMode)
            {
                tstvm.ToggleStreamRecording = true;
            }
            tstvm.ToggleAudioRecording = true;

            //Launch the record
            ButtonAutomationPeer peer = new ButtonAutomationPeer(TrainingSideTool.Get().StartRecordingButton);

            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }

        /// <summary>
        /// Reset every option used for the recording session
        /// </summary>
        public void allFalse()
        {
            TrackingSideToolViewModel tracking = TrackingSideToolViewModel.get();
            tracking.FaceTracking = false;
            tracking.emo = false;
            //tracking.Mouth = false;
            //tracking.Mouth2 = false; //Same as enabling it
            //tracking.pupilR = false;
            tracking.LookR = false;
            tracking.PeakDetection = false;
            tracking.SpeedRate = false;
            tracking.ShowTextOnScreen = false;
            /*tracking.VoiceMonotony = false;
            tracking.BadVoiceReflex = false;*/
            tracking.UseFeedback = false;
        }
        #endregion

    }
}
