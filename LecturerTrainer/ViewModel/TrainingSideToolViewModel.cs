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
using AForge.Video.FFMPEG;
using System.IO;
using System.Drawing;
using System.Windows.Controls;
using AForge.Video.Kinect;
using AForge.Video;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Collections;
using LecturerTrainer.Model.BodyAnalysis;
using LecturerTrainer.Model.EventsAnalysis;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Microsoft.Kinect;
using LiveCharts.Wpf;
using System.Xml;
using LecturerTrainer.Model.Exceptions;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.Concurrent;

namespace LecturerTrainer.ViewModel
{
    public class TrainingSideToolViewModel : ViewModelBase
    {
        #region fields

        private DispatcherTimer feedbackRefreshTimer;

        public StoringFeedbackThreadData storingFeedbackThreadData { get; private set; }
        private Thread storingFeedbackThread = null;

        // here we keep the copy of our unique instance of this class 
        private static TrainingSideToolViewModel trainingSideToolViewModel = null;

        /* GUI related commands, allow us to define our behavior depending of the command launched  */
        private ICommand importSpeech;
        private ICommand startTrainingCommand;
        private ICommand replayModeCommand;
        public ICommand BeginRecordingCommand { get { return beginRecordingCommand; } }
        public ICommand StopRecordingCommand { get { return stopCommand; } }
        public ICommand GoToResultsCommand { get { return goToResults; } }
        public ICommand ReplayModeCommand { get { return replayModeCommand; } }


        private readonly RelayCommand beginRecordingCommand;
        private readonly RelayCommand stopCommand;
        private readonly RelayCommand goToResults;
        
        private IRecordingState state;
        private Chronometre stopwatch;
        private String chrono;
        private string speechPath = "IMPORT SPEECH";

        private bool _isTimeLimited;
        private int _limitedTimeHours;
        private int _limitedTimeMinutes;
        private int _limitedTimeSeconds;
        // in seconds
        private int _limitedTimeSum;

        /// <summary>
        /// video recording variables 
        /// </summary>
        public int videoWidth { get; set; }
        public int videoHeight { get; set; }
        // run in background, unusable by user 

        public static int nbVideos = 1;

        private bool _ToggleStreamRecording;
        private bool _ToggleAvatarVideoRecording;
        private bool _ToggleAudioRecording;
        private bool _ToggleAvatarOpenGLRecording;

        private bool isReplayingMode = false;
        public bool IsReplayingMode
        {
            get
            {
                return isReplayingMode;
            }
        }

        private bool _isRecording = false;
        public bool isRecording
        {
            get
            {
                return _isRecording;
            }
            set
            {
                _isRecording = value;
            }
        }


        private ReplayViewModel replayViewModel = null;
        public TrainingWithAvatarViewModel trainingWAVM = null;
        #endregion

        #region poolFieldsBindings
        /// <summary>
        /// here we set up the names of pool that we will need in other classes to add feedback here
        /// </summary>
        public const string bodypool = "body";
        public const string facepool = "face";
        public const string voicepool = "voice";
        public const string commentpool = "comment";

        /// <summary>
        /// each pool owns its own area on the tool side 
        /// </summary>
        public FeedbackPool BodyPool;
        public FeedbackPool FacePool;
        public FeedbackPool VoicePool;
        public FeedbackPool CommentPool;

        /// <summary>
        /// Lanels which belong to each pool 
        /// </summary>
        private string bodyLabel1;
        private string bodyLabel2;
        private string bodyLabel3;

        private string faceLabel1;
        private string faceLabel2;
        private string faceLabel3;

        private string voiceLabel1;
        private string voiceLabel2;
        private string voiceLabel3;

        private string commentLabel1;
        private string commentLabel2;
        private string commentLabel3;

        /// <summary>
        /// public bindings of these labels 
        /// </summary>
        public string BodyLabel1
        {
            get
            {
                return bodyLabel1;
            }
            set
            {
                bodyLabel1 = value;
                OnPropertyChanged("BodyLabel1");
            }
        }
        public string BodyLabel2
        {
            get
            {
                return bodyLabel2;
            }
            set
            {
                bodyLabel2 = value;
                OnPropertyChanged("BodyLabel2");
            }
        }
        public string BodyLabel3
        {
            get
            {
                return bodyLabel3;
            }
            set
            {
                bodyLabel3 = value;
                OnPropertyChanged("BodyLabel3");
            }
        }

        public string FaceLabel1
        {
            get
            {
                return faceLabel1;
            }
            set
            {
                faceLabel1 = value;
                OnPropertyChanged("FaceLabel1");
            }
        }
        public string FaceLabel2
        {
            get
            {
                return faceLabel2;
            }
            set
            {
                faceLabel2 = value;
                OnPropertyChanged("FaceLabel2");
            }
        }
        public string FaceLabel3
        {
            get
            {
                return faceLabel3;
            }
            set
            {
                faceLabel3 = value;
                OnPropertyChanged("FaceLabel3");
            }
        }

        public string VoiceLabel1
        {
            get
            {
                return voiceLabel1;
            }
            set
            {
                voiceLabel1 = value;
                OnPropertyChanged("VoiceLabel1");
            }
        }
        public string VoiceLabel2
        {
            get
            {
                return voiceLabel2;
            }
            set
            {
                voiceLabel2 = value;
                OnPropertyChanged("VoiceLabel2");
            }
        }
        public string VoiceLabel3
        {
            get
            {
                return voiceLabel3;
            }
            set
            {
                voiceLabel3 = value;
                OnPropertyChanged("VoiceLabel3");
            }
        }

        public string CommentLabel1
        {
            get
            {
                return commentLabel1;
            }
            set
            {
                commentLabel1 = value;
                OnPropertyChanged("CommentLabel1");
            }
        }
        public string CommentLabel2
        {
            get
            {
                return commentLabel2;
            }
            set
            {
                commentLabel2 = value;
                OnPropertyChanged("CommentLabel1");
            }
        }
        public string CommentLabel3
        {
            get
            {
                return commentLabel3;
            }
            set
            {
                commentLabel3 = value;
                OnPropertyChanged("CommentLabel1");
            }
        }
        #endregion

        #region constructor
        private TrainingSideToolViewModel()
        {
            // we bind events with local methods which will ensure that we do the correct treatment when an event is raised 
            Agitation.agitationEvent += iconAgitation;
            FFT.reflexEvent += iconBadReflex;
            Pitch.BoringEvent += iconBoringVoice;
            Pitch.PeakEvent += peakFeedbackHandler;
            AudioProvider.tooFastEvent += iconTooFast;
            Gesture.gestureEvent += gesture;
            Posture.postureEvent += posture;
            HandsJoined.handsJoinedEvent += handsJoined;
            HandsInPocket.handsInPocketEvent += handsinpocket;
            lookingDirection.lookEvent += lookingEvent;
            mouthOpened.mouthEvent += mOpened;
            mouthShut.mouth2Event += mShut;
            pupilRight.pupilREvent += pupilR;
            AudioProvider.ticEvent += tic;
            HandsRaised.handsRaisedEvent += myDetectorEventHandler;
            ArmsWide.armsWideEvent += armsWideHandler;
            ArmsCrossed.armsCrossedEvent += armsCrossed;
            Enthusiasm.enthusiasmEvent += enthusiasmEventHandler;
            AudioProvider.keyWordEvent += keyWordEventHandler;
            Epicness.epicnessEvent += epicnessEventHandler;
            EmotionRecognition.emoEvent += iconEmotion;
            // Links to create for the enthusiasm event
            EmotionRecognition.emoEvent += Enthusiasm.enthusiasmHandler;
            HandsRaised.handsRaisedEvent += Enthusiasm.enthusiasmHandler;

            // Links to create for the epicness event
            AudioProvider.keyWordEvent += Epicness.epicnessHandler;
            HandsRaised.handsRaisedEvent += Epicness.epicnessHandler;


            // creating arrays for instanciation of pools 
            string[] bodyFields = new string[3];
            string[] faceFields = new string[3];
            string[] voiceFields = new string[3];
            string[] commentFields = new string[3];
            // initializing labels 
            BodyLabel1 = ""; BodyLabel2 = ""; BodyLabel3 = "";
            FaceLabel1 = ""; FaceLabel2 = ""; FaceLabel3 = "";
            VoiceLabel1 = ""; VoiceLabel2 = ""; VoiceLabel3 = "";
            CommentLabel1 = ""; CommentLabel2 = ""; CommentLabel3 = "";
            // prepare arrays 
            bodyFields[0] = BodyLabel1; bodyFields[1] = BodyLabel2; bodyFields[2] = BodyLabel3;
            faceFields[0] = FaceLabel1; faceFields[1] = FaceLabel2; faceFields[2] = FaceLabel3;
            voiceFields[0] = VoiceLabel1; voiceFields[1] = VoiceLabel2; voiceFields[2] = VoiceLabel3;
            commentFields[0] = CommentLabel1; commentFields[1] = CommentLabel2; voiceFields[2] = CommentLabel3;
            // creating pools 
            BodyPool = new FeedbackPool(bodyFields);
            FacePool = new FeedbackPool(faceFields);
            VoicePool = new FeedbackPool(voiceFields);
            CommentPool = new FeedbackPool(commentFields);

            // setting up the chronometre 
            this.stopwatch = new Chronometre();
            this.stopwatch.TimeToUpdate.Elapsed += UpdateChrono;
            beginRecordingCommand = new RelayCommand(BeginRecording,
                () => State == IRecordingState.Stopped ||
                      State == IRecordingState.Monitoring ||
                      State == IRecordingState.Paused);
            stopCommand = new RelayCommand(Stop,
                () => State == IRecordingState.Recording);
            goToResults = new RelayCommand(ShowResults,
                () => State == IRecordingState.Stopped && this.stopwatch.isStarted);
            replayModeCommand = new RelayCommand(ChoosePerfToReplay);

            // prepare video recorders 
            ToggleAvatarVideoRecording = false;
            ToggleStreamRecording = false;
            ToggleAudioRecording = false;
            ToggleAvatarOpenGLRecording = true;


            feedbackRefreshTimer = new DispatcherTimer();
            feedbackRefreshTimer.Interval = TimeSpan.FromMilliseconds(200);
            feedbackRefreshTimer.IsEnabled = true;
            feedbackRefreshTimer.Tick += UpdatePools;
			// ajout louche
			feedbackRefreshTimer.Tick += DrawingSheetStreamViewModel.Get().hideFeedbacks;
				
		}

        // unique accessor to the instance of the class 
        public static TrainingSideToolViewModel Get()
        {
            if (trainingSideToolViewModel == null)
            {
                trainingSideToolViewModel = new TrainingSideToolViewModel();
            }
            return trainingSideToolViewModel;
        }

        #endregion

        #region feedbacksBindings

        private void myDetectorEventHandler(object sender, InstantFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.bodypool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(
                    StoringFeedbackThreadData.FeedbackToText(e, "handsRaisedEvent"));

            if (e.feedback == HandsRaised.rightHandRaised)
            {
                addFeedbackInPoolAndRemoveOne(e, HandsRaised.leftHandRaised, TrainingSideToolViewModel.bodypool);
                addFeedbackInPoolAndRemoveOne(e, HandsRaised.bothHandsRaised, TrainingSideToolViewModel.bodypool);
            }
            else if (e.feedback == HandsRaised.leftHandRaised)
            {
                addFeedbackInPoolAndRemoveOne(e, HandsRaised.rightHandRaised, TrainingSideToolViewModel.bodypool);
                addFeedbackInPoolAndRemoveOne(e, HandsRaised.bothHandsRaised, TrainingSideToolViewModel.bodypool);
            }
            else
            {
                addFeedbackInPoolAndRemoveOne(e, HandsRaised.rightHandRaised, TrainingSideToolViewModel.bodypool);
                addFeedbackInPoolAndRemoveOne(e, HandsRaised.leftHandRaised, TrainingSideToolViewModel.bodypool);
            }
        }

		/* The following events were changed so they directly call the methods that displays the feedback HUD on screen 
		 * instead of using the pools */
		private void armsCrossed(object sender, InstantFeedback e)
        {
			if(!ReplayViewModel.isReplaying)
			{
				DrawingSheetStreamViewModel.Get().feedbackDisplay_ArmsCrossed();
				if (this.State == IRecordingState.Recording)
					storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "armsCrossedEvent"));
			}
        }

		private void iconAgitation(object sender, InstantFeedback e)
        {
			if(!ReplayViewModel.isReplaying){
				DrawingSheetStreamViewModel.Get().feedbackDisplay_Agitation();
				if (this.State == IRecordingState.Recording)
					storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "agitationEvent"));
			}
		}

		private void handsJoined(object sender, InstantFeedback e)
        {
			if(!ReplayViewModel.isReplaying){
				DrawingSheetStreamViewModel.Get().feedbackDisplay_HandsJoined();
				if (this.State == IRecordingState.Recording)
					storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "handsJoinedEvent"));
			}
		}

		private void lookingEvent(object sender, InstantFeedback e)
        {
			// LookCenter, LookRight and LookLeft 
			DrawingSheetStreamViewModel.Get().feedbackEventDisplay_LookingDirection(e);
			if (this.State == IRecordingState.Recording)
            storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "lookEvent"));
        }

		private void iconEmotion(object sender, LongFeedback e)
        {
			// Happy and Surprised
            DrawingSheetStreamViewModel.Get().feedbackEventDisplay_Emotion(e);
			if (TrainingSideToolViewModel.Get().State == IRecordingState.Recording)
                TrainingSideToolViewModel.Get().storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "emotionEvent"));
        }

		/* Changes were no applied to the following feedbacks events yet*/
        private void iconTooFast(object sender, LongFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.voicepool);
            if( this.State == IRecordingState.Recording  )
                storingFeedbackThreadData.addTextFeedbackInQueue( StoringFeedbackThreadData.FeedbackToText(e, "tooFastEvent"));
        }

        private void iconBoringVoice(object sender, LongFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.voicepool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "boringEvent"));
        }

        private void peakFeedbackHandler(object sender, InstantFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.voicepool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "peakEvent"));
        }

        private void iconBadReflex(object sender, LongFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.voicepool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "reflexEvent"));
        }

        private void pupilR(object sender, LongFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.facepool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "pupilREvent"));
        }

        private void mShut(object sender, LongFeedback e)
        {
            addFeedbackInPoolAndRemoveOne(e, "Mouth Open", TrainingSideToolViewModel.facepool) ;
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "mouth2Event"));
        }

        private void mOpened(object sender, LongFeedback e)
        {
            addFeedbackInPoolAndRemoveOne(e, "Mouth Shut", TrainingSideToolViewModel.facepool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "mouthEvent"));
        }

        private void handsinpocket(object sender, InstantFeedback e)
        {
            addFeedbackInPoolAndRemoveOne(e, "Hands in the Pocket", TrainingSideToolViewModel.bodypool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "handsInPocketEvent"));
        }

        private void posture(object sender, InstantFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.bodypool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "postureEvent"));
        }

        private void gesture(object sender, InstantFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.bodypool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "gestureEvent"));
        }

        private void tic(object sender, InstantFeedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.voicepool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "ticEvent"));
        }

        private void armsWideHandler(object sender, LongFeedback e)
        {
            addFeedbackInPoolAndRemoveOne(e, "Arms Wide!", TrainingSideToolViewModel.bodypool);
            if (this.State == IRecordingState.Recording)
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "armsWideEvent"));
        }

        private void enthusiasmEventHandler(object sender, Feedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.facepool);
            if (this.State == IRecordingState.Recording)
            {
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "enthusiasmEvent"));
            }
        }

        private void keyWordEventHandler(object sender, Feedback e)
        {
            Console.WriteLine("Keyword :" + e.feedback);
        }

        // To put in a new pool : maybe "other" or "various"
        private void epicnessEventHandler(object sender, Feedback e)
        {
            addFeedbackInPool(e, TrainingSideToolViewModel.voicepool);
            addFeedbackInPool(e, TrainingSideToolViewModel.bodypool);
            if (this.State == IRecordingState.Recording)
            {
                storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(e, "epicnessEvent"));
            }
        }


        #endregion

        #region properties

        // chronometre binding
        public String Chrono
        {
            get { return chrono; }
            set
            {
                chrono = value;
                OnPropertyChanged("Chrono");
            }
        }

        public bool ToggleStreamRecording
        {
            get { return _ToggleStreamRecording; }

            set
            {
                _ToggleStreamRecording = value;
                OnPropertyChanged("ToggleStreamRecording");
            }
        }

        public bool ToggleAvatarVideoRecording
        {
            get { return _ToggleAvatarVideoRecording; }

            set
            {
                _ToggleAvatarVideoRecording = value;
                OnPropertyChanged("ToggleAvatarVideoRecording");
            }
        }

        public bool ToggleAvatarOpenGLRecording
        {
            get { return _ToggleAvatarOpenGLRecording; }
            set
            {
                _ToggleAvatarOpenGLRecording = value;
                OnPropertyChanged("ToggleAvatarOpenGLRecording");
            }
        }

        public bool ToggleAudioRecording
        {
            get { return _ToggleAudioRecording; }
            set
            {
                _ToggleAudioRecording = value;
                OnPropertyChanged("ToggleAudioRecording");
            }
        }

        public ICommand ImportSpeech
        {
            get
            {
                if (this.importSpeech == null)
                    this.importSpeech = new RelayCommand(() => this.speechImport(), () => this.CanImportSpeech());

                return this.importSpeech;
            }
        }

        public ICommand StartTrainingCommand
        {
            get
            {
                if (this.startTrainingCommand == null)
                    this.startTrainingCommand = new RelayCommand(() => this.LaunchTraining(), () => this.CanLaunchTraining());

                return this.startTrainingCommand;
            }
        }

        public IRecordingState State
        {
            get
            {
                return state;
            }
        }

        public string SpeechPath
        {
            get
            {
                return speechPath;
            }
            set
            {
                speechPath = value;
                MainWindow.main.audioProvider.loadGammarPath(speechPath);
                OnPropertyChanged("SpeechPath");
            }
        }

        public bool isTimeLimited
        {
            get { return _isTimeLimited; }
            set
            {
                _isTimeLimited = value;
                if (value)
                {
                    TrainerStopwatchViewModel.Get().reset();
                    TrainerStopwatchViewModel.Get().enable();
                    IconViewModel.get().TimeVisibility = Visibility.Visible;
                }
                else
                {
                    TrainerStopwatchViewModel.Get().disable();
                    TrainerStopwatchViewModel.Get().reset();
                    IconViewModel.get().TimeVisibility = Visibility.Collapsed;
                }
                OnPropertyChanged("isTimeLimited");
            }
        }

        public String limitedTimeHours
        {
            get { return _limitedTimeHours.ToString(); }
            set
            {
                if (value.Length > 0 && value.All(char.IsDigit))
                {
                    int tmp = Convert.ToInt32(value);
                    if (tmp >= 0 && tmp <= 24)
                    {
                        _limitedTimeHours = tmp;
                    }
                }
                else if (value.Length == 0)
                {
                    _limitedTimeHours = 0;
                }
                OnPropertyChanged("limitedTimeHours");
            }
        }

        public String limitedTimeMinutes
        {
            get { return _limitedTimeMinutes.ToString(); }
            set
            {
                if (value.Length > 0 && value.All(char.IsDigit))
                {
                    int tmp = Convert.ToInt32(value);
                    if (tmp >= 0 && tmp <= 60)
                    {
                        _limitedTimeMinutes = tmp;
                    }
                }
                else if (value.Length == 0)
                {
                    _limitedTimeMinutes = 0;
                }
                OnPropertyChanged("limitedTimeMinutes");
            }
        }

        public String limitedTimeSeconds
        {
            get { return _limitedTimeSeconds.ToString(); }
            set
            {
                if (value.Length > 0 && value.All(char.IsDigit))
                {
                    int tmp = Convert.ToInt32(value);
                    if (tmp >= 0 && tmp <= 60)
                    {
                        _limitedTimeSeconds = tmp;
                    }
                }
                else if (value.Length == 0)
                {
                    _limitedTimeSeconds = 0;
                }
                OnPropertyChanged("limitedTimeSeconds");
            }
        }

        /*Added by Baptiste Germond*/
        public void stopSessionRecording()
        {
            /*Modified by Baptiste Germond 
             * -- Ensure that the actions are synchronised with the current thread to avoid conflict*/
            if (!object.ReferenceEquals(System.Windows.Threading.Dispatcher.CurrentDispatcher, Application.Current.Dispatcher))
            {
                Application.Current.Dispatcher.Invoke(new Action(() => invokStop()));
            }
            else
            {
                invokStop();
            }
        }

        public void invokStop()
        {
            ButtonAutomationPeer peer = new ButtonAutomationPeer(TrainingSideTool.Get().StopRecordingButton);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }


        #endregion

        #region videoAndAudioRecordingFunctions
        

        private void backgroundStreamVideoRecording(object sender, Bitmap e)
        {
            SavingTools.EnqueueVideoStream(e);

        }
        
        private void backgroundAvatarVideoRecording(object sender, Bitmap e)
        {
            SavingTools.EnqueueAvatarVideoStream(e);
        }

        private void backgroundAvatarXMLRecording(object sender, Skeleton sk)
        {
            SavingTools.EnqueueXMLSkeleton(sk);
        }

        private void backgroundFaceXMLRecording(object sender, FaceDataWrapper fdw)
        {
            SavingTools.EnqueueXMLFace(fdw);
        }

        // begins the video recording and set it up 
        private void BeginVideoAndAudioRecording()
        {
            Console.WriteLine("Start");
            if (Main.session.Exists())
            {
                string combine;
                if (SessionRecordingViewModel.inRecord == true)
                {
                    combine = System.IO.Path.Combine(Path.GetDirectoryName(Main.session.sessionPath), "SessionRecording");
                    if (!Directory.Exists(combine))
                    {
                        Directory.CreateDirectory(combine);
                    }
                    storingFeedbackThreadData = new StoringFeedbackThreadData("feedback", SavingTools.nameFolder(combine, "SessionRecording"));
                }
                else
                {
                    combine = System.IO.Path.Combine(Path.GetDirectoryName(Main.session.sessionPath), "FreeRecording");
                    if (!Directory.Exists(combine))
                    {
                        Directory.CreateDirectory(combine);
                    }
                    storingFeedbackThreadData = new StoringFeedbackThreadData("feedback", SavingTools.nameFolder(combine, "FreeRecording"));
                }
            }
            else
            {
                string newPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\"));
                string combine = System.IO.Path.Combine(newPath, "PublicRecord");

                if (!Directory.Exists(combine))
                {
                    Directory.CreateDirectory(combine);
                }
                storingFeedbackThreadData = new StoringFeedbackThreadData("feedback", SavingTools.nameFolder(combine, "PublicRecord"));

            }

            storingFeedbackThread = new Thread(storingFeedbackThreadData.threadProcess);
            storingFeedbackThread.Start();

            // we get width and height of the drawing sheet at this time 
            videoWidth = (int)MainWindow.drawingSheet.ActualWidth;
            videoHeight = (int)MainWindow.drawingSheet.ActualHeight;

            // width/height have to have a pair size 
            if (videoWidth % 2 != 0)
                videoWidth += 1;
            if (videoHeight % 2 != 0)
                videoHeight += 1;

            TrainingSideTool.Get().toggleAllCheckboxes(false);
            if (ToggleAvatarOpenGLRecording)
            {
                DrawingSheetAvatarViewModel.Get().isRecording = true;

                DrawingSheetAvatarViewModel.Get().IsOpenGLRecording = true;
                DrawingSheetAvatarViewModel.backgroundXMLRecordingEventStream += backgroundAvatarXMLRecording;
                SavingTools.StartSavingXMLSkeleton();

                if (TrackingSideToolViewModel.get().FaceTracking)
                {
                    DrawingSheetAvatarViewModel.backgroundXMLFaceRecordingEventStream += backgroundFaceXMLRecording;
                    SavingTools.StartSavingXMLFace();
                    //Binary change
                    //SavingTools.StartSavingBinaryFace();
                }

            }
            if (_ToggleAvatarVideoRecording)
            {
                //DrawingSheetAvatarViewModel.backgroundRecordingEventStream += backgroundAvatarVideoRecording;
                //DrawingSheetAvatarViewModel.Get().IsVideoRecording = true;
                //SavingTools.StartSavingAvatarVideoRecording();
            }
            if (_ToggleStreamRecording)
            {
                DrawingSheetStreamViewModel.backgroundDrawEventStream += backgroundStreamVideoRecording;
                SavingTools.StartSavingStreamRecording();
            }
            if (_ToggleAudioRecording)
            {
                MainWindow.main.audioProvider.startRecording(SavingTools.pathFolder + '/', "audio");
            }
            // Timothée
            if (TrackingSideToolViewModel.get().ShowTextOnScreen)
                MainWindow.main.audioProvider.startRecText();
            Console.WriteLine("EndStart");
        }

        // stops video recording and creates a test.avi file 
        private void StopVideoAndAudioRecording()
        {
            TrainingSideTool.Get().toggleAllCheckboxes(true);
            // removing the eventhandlers 
            if (_ToggleStreamRecording)
            {
                DrawingSheetStreamViewModel.backgroundDrawEventStream -= backgroundStreamVideoRecording;

                
                SavingTools.StreamDispose();
            }
            if (_ToggleAvatarVideoRecording)
            {
                DrawingSheetAvatarViewModel.Get().IsVideoRecording = false;
                DrawingSheetAvatarViewModel.backgroundRecordingEventStream -= backgroundAvatarVideoRecording;
                SavingTools.AvatarVideoDispose();
            }
            if (_ToggleAvatarOpenGLRecording)
            {
                DrawingSheetAvatarViewModel.Get().IsOpenGLRecording = false;
                DrawingSheetAvatarViewModel.backgroundXMLRecordingEventStream -= backgroundAvatarXMLRecording;
                SavingTools.XMLSkeletonDispose();

                if (TrackingSideToolViewModel.get().FaceTracking)
                {
                    DrawingSheetAvatarViewModel.backgroundXMLFaceRecordingEventStream -= backgroundFaceXMLRecording;
                    SavingTools.XMLFaceDispose();
                }

            }
            if (_ToggleAudioRecording)
            {
                MainWindow.main.audioProvider.stopRecording();
                MainWindow.main.audioProvider.stopSpeechRateDetection();
                MainWindow.main.audioProvider.stopPeakDetection();
            }
            if (storingFeedbackThreadData != null)
            {
                storingFeedbackThreadData.Processing = false;
            }
            if (TrackingSideToolViewModel.get().ShowTextOnScreen)
            {
                //Timothée
                saveTextInFile(MainWindow.main.audioProvider.stopRecText(), SavingTools.pathFolder + "\\text.txt");
            }
            nbVideos++;

            //Florian Bechu Summer 2016

            ResultsViewModel ResViewMod = ResultsViewModel.Get();
            ResViewMod.checkBoxSingleUpdate(9, EmotionRecognition.detect);
            ResViewMod.checkBoxSingleUpdate(10, lookingDirection.detect);

            ResViewMod.getAgitationStatistics(Agitation.getAgitationStats());
            List<IGraph> temp = new List<IGraph>();
            temp.AddRange(HandsJoined.getHandStatistics());
            temp.AddRange(ArmsCrossed.getArmsStatistics());
            ResViewMod.getArmsMotion(temp);
            if (EmotionRecognition.detect && lookingDirection.detect)
            {
                ResViewMod.getFaceStatistics(EmotionRecognition.getStatistics(new PieGraph()), lookingDirection.getStatistics(new CartesianGraph()));
            }
            else if (EmotionRecognition.detect)
            {
                ResViewMod.getFaceStatistics(EmotionRecognition.getStatistics(new PieGraph()), null);
            }
            else if (lookingDirection.detect)
            {
                ResViewMod.getFaceStatistics(null, lookingDirection.getStatistics(new PieGraph()));
            }

            if (TrackingSideToolViewModel.get().SpeedRate)
            {
                ResViewMod.getVoiceStatistics(AudioProvider.getVoicetatistics());
            }
            ResViewMod.SaveGraph(SavingTools.pathFolder + '/');
        }
        #endregion

        #region miscellaneous methods 

        // Timothée
        /// <summary>
        /// Save a list of string in a file
        /// </summary>
        /// <param name="textToSave"> The list string to save in file </param>
        /// <param name="pathfield"> The path of the file </param>
        private void saveTextInFile(List<String> textToSave, string pathfield)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathfield))
            {
                foreach (string line in textToSave)
                {
                    file.WriteLine(line);
                }
            }
        }

        public void startStopwatch()
        {
            this.stopwatch.Reset();
            this.stopwatch.Start();
            state = IRecordingState.Recording;
            UpdateChrono(null, null);
        }

        public void stopStopwatch()
        {
            this.stopwatch.Stop();
            state = IRecordingState.Stopped;
            UpdateChrono(null, null);
        }

        // launch the recording session 
        // manages the chronometre and other stuff 
        // launch and initialize video recording, binds with events in DrawingSheet_xxx_ViewModel
        private void BeginRecording()
        {
            isRecording = true;
            //Tools.createAndStartTimer();
            startStopwatch();
            DrawingSheetAvatarViewModel.Get().nbFrames = 0;
            Agitation.record = true;
            HandsJoined.record = true;
            ArmsCrossed.record = true;
            if (TrackingSideToolViewModel.get().FaceTracking)
            {
                lookingDirection.record = true;
                EmotionRecognition.record = true;
            }

            // Prevents the user from changing tracking configs while recording
            MainWindow.main.audioProvider.resetTmpCount();
            HandsRaised.resetCounters();
            SideToolsViewModel.Get().disableTrackingTab();
            if (isTimeLimited)
            {
                _limitedTimeSum = (_limitedTimeHours * 60 * 60) + (_limitedTimeMinutes * 60) + _limitedTimeSeconds;
                TrainerStopwatchViewModel.Get().reset();
                if (_limitedTimeSum <= 0)
                    isTimeLimited = false;
            }
            // starting video recording by creating video files 
            BeginVideoAndAudioRecording();
        }

        // stop video recording, recording session and cancels the bindings with drawing events 
        private void Stop()
        {
            isRecording = false;
            stopStopwatch();
            SideToolsViewModel.Get().allTabsSelectable();
            Agitation.record = false;
            HandsJoined.record = false;
            ArmsCrossed.record = false;

            //stop the video recording by closing each file 
            if (TrackingSideToolViewModel.get().FaceTracking)
            {
                //TrackingSideToolViewModel.get().FaceTracking = false;
                lookingDirection.record = false;
                EmotionRecognition.record = false;
            }
            //Tools.stopTimer();
            StopVideoAndAudioRecording();
        }

        /// <summary>
        /// Choose the replay the user want to use
        /// Modified by Baptiste Germond
        /// </summary>
        public void ChoosePerfToReplay()
        {
            if (replayViewModel != null)
            {
                replayViewModel.resetInstance();
                replayViewModel = null;
            }
            System.Windows.Forms.OpenFileDialog fbd = new System.Windows.Forms.OpenFileDialog();
            fbd.Filter = "Performance File (.avi,.skd)|*.avi;*.skd"; // Filter files by extension
            fbd.Title = "Select the file of the performance you want to replay";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ReplayViewModel.Set(fbd.FileName);
                replayViewModel = ReplayViewModel.Get();
                replayMode();
            }
        }

        public void replayMode()
        {
            if (replayViewModel != null)
            {
                isReplayingMode = true;
                ReplayView.Get().DataContext = replayViewModel;

                DrawingSheetView.Get().replayMode();
                MainWindow.main.audioProvider.replayMode = true;
                MainWindow.main.audioProvider.stopFFTDetection();
                (TrainingSideTool.Get().FindResource("ReplayButtonAction") as Storyboard).Begin();
                IconViewModel.get().clearAll();
            }
        }

        public void recordingMode()
        {
            DrawingSheetAvatarViewModel.Get().skToDrawInReplay = null;

            // Display setting
            MainWindow.drawingSheet.ChangeMode(SheetMode.AvatarMode);

            isReplayingMode = false;

            // Reset of the replay instance
            if (replayViewModel != null)
            {
                replayViewModel.resetInstance();
                replayViewModel = null;
            }
            // Reset icons opacity
            IconViewModel.get().clearAll();

            DrawingSheetView.Get().normalMode();
            MainWindow.main.audioProvider.replayMode = false;
            MainWindow.main.audioProvider.startFFTDetection();
        }

        public void speechImport()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "File"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text files (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                SpeechPath = dlg.FileName;
            }
        }

        private bool CanImportSpeech()
        {
            return true;
        }

        private void ShowResults()
        {
            //var results = new ResultsView();
            var results = new ChoiceResultView();
            ((ChoiceResultViewModel)results.DataContext).isLoad = false;
            ((ChoiceResultViewModel)results.DataContext).enableCheckBox();
            results.Show();
        }      

        public void UpdateChrono(object source, ElapsedEventArgs e)
        {
            Chrono = stopwatch.ToString();
            if (_isTimeLimited && _limitedTimeSum > 0)
            {
                int residualH, residualM, residualS;
                string residualTimeString = "";
                // remaining time 
                Double floatting = 360 - ((360 * (_limitedTimeSum - stopwatch.NowSeconds())) / (float)_limitedTimeSum);

                residualH = (_limitedTimeSum - stopwatch.NowSeconds()) / 3600;
                if (residualH < 10)
                    residualTimeString += "0";
                residualTimeString += residualH + ":";

                residualM = ((_limitedTimeSum - stopwatch.NowSeconds()) % 3600) / 60;
                if (residualM < 10)
                    residualTimeString += "0";
                residualTimeString += residualM + ":";

                residualS = ((_limitedTimeSum - stopwatch.NowSeconds()) % 3600) % 60;
                if (residualS < 10)
                    residualTimeString += "0";
                residualTimeString += residualS;

                IconViewModel.get().ResidualTimeText = residualTimeString;

                TrainerStopwatchViewModel.Get().update((int)floatting);
                if (floatting >= 360)
                    isTimeLimited = false;
            }
        }

        /// <summary>
        /// method called when we want to update the pools 
        /// this method is called when we update the chrono, so it's a simple way to know when we have to update
        /// </summary>
        private void UpdatePools(object sender, EventArgs e)
        {
            string[] tmp = new string[3];
            tmp = VoicePool.update();
            VoiceLabel1 = tmp[0]; VoiceLabel2 = tmp[1]; VoiceLabel3 = tmp[2];

            tmp = BodyPool.update();
            BodyLabel1 = tmp[0]; BodyLabel2 = tmp[1]; BodyLabel3 = tmp[2];

            tmp = FacePool.update();
            FaceLabel1 = tmp[0]; FaceLabel2 = tmp[1]; FaceLabel3 = tmp[2];

            tmp = CommentPool.update();
            CommentLabel1 = tmp[0]; CommentLabel2 = tmp[1]; CommentLabel3 = tmp[2];
        }

        /// <summary>
        /// Add a feedback to an existing pool specified by the string pool 
        /// If the specified pool does not exist false is returned
        /// False is returned if the feedback cannot be displayed yet (pool is full for example)
        /// True is returned if the feedback has been added to the pool
        /// Please refer to FeedbackPool adding function for more information 
        /// </summary>
        /// <param name="fb">the feedback we want to display</param>
        /// <param name="pool">the name of the pool </param>
        /// <returns></returns>
        public bool addFeedbackInPool(Feedback fb, String pool)
        {
            switch (pool)
            {
                case TrainingSideToolViewModel.voicepool:
                    return VoicePool.addFeedback(fb);
                case TrainingSideToolViewModel.facepool:
                    return FacePool.addFeedback(fb);
                case TrainingSideToolViewModel.bodypool:
                    return BodyPool.addFeedback(fb);
                case TrainingSideToolViewModel.commentpool:
                    return CommentPool.addFeedback(fb);
                default:
                    return false;
            }
        }

        public bool addFeedbackInPoolAndRemoveOne(Feedback fb, String toRemove, String pool)
        {
            switch (pool)
            {
                case TrainingSideToolViewModel.voicepool:
                    return VoicePool.addFeedbackAndRemoveOne(fb, toRemove);
                case TrainingSideToolViewModel.facepool:
                    return FacePool.addFeedbackAndRemoveOne(fb, toRemove);
                case TrainingSideToolViewModel.bodypool:
                    return BodyPool.addFeedbackAndRemoveOne(fb, toRemove);
                case TrainingSideToolViewModel.commentpool:
                    return CommentPool.addFeedbackAndRemoveOne(fb, toRemove);
                default:
                    return false;
            }
        }


        #endregion

        #region Video Training       
        public void LaunchTraining()
        {
            (TrainingSideTool.Get().FindResource("StartTraining") as Storyboard).Begin();
            SideToolsViewModel.Get().disableTrackingAndTrainingTab();
            trainingWAVM = TrainingWithAvatarViewModel.Get();
            trainingWAVM.initialize();
            DrawingSheetAvatarViewModel.Get().isTraining = true;          
        }
        private bool CanLaunchTraining()
        {
            return true;
        }
        #endregion
    }


    #region feedbacksRecording

    public class StoringFeedbackThreadData
    {
        private StreamWriter sw = null;
        private Queue<string> feedbacksText = null;
        public bool Processing { get; set; }

        public StoringFeedbackThreadData(string name, string path)
        {
            feedbacksText = new Queue<string>();
            sw = new StreamWriter(path + '/' + name + ".txt");
            Processing = true;
        }

        public void addTextFeedbackInQueue(String stringFb)
        {
            if (stringFb != null)
            {
                lock (feedbacksText)
                {
                    feedbacksText.Enqueue(stringFb);
                }
            }
        }

        public void threadProcess()
        {

            while (true)
            {
                while (feedbacksText.Count > 0)
                {
                    string tmpFb = null;
                    lock (feedbacksText)
                    {
                        tmpFb = feedbacksText.Dequeue();
                    }
                    lock (sw)
                    {
                        sw.WriteLine(tmpFb);
                    }
                }
                if (!Processing)
                {
                    lock (sw)
                    {
                        sw.Close();
                        return;
                    }
                }
            }
        }

        public static string FeedbackToText(Feedback fb, string feedbackName)
        {
            string toRet;
            int previousFrame = -20;
            if (fb is ValuedFeedback)
            {
                ValuedFeedback tmp = (ValuedFeedback)fb;
                toRet = tmp.value.ToString() + '@' + feedbackName + '@' + Tools.getStopWatch().ToString() + '@' + tmp.display;

            }
            else if (fb is LongFeedback)
            {
                LongFeedback tmp = (LongFeedback)fb;
                toRet = tmp.feedback + '@' + feedbackName + '@' + Tools.getStopWatch().ToString() + '@' + tmp.display;
            }
            else
            {
                // instant feedback 
                toRet = "";
                if (fb.feedback == "Too agitated!")
                {
                    if (previousFrame != (int)Tools.getTimer())
                    {
                        toRet = fb.feedback + '@' + feedbackName + '@' + Tools.getStopWatch().ToString() + "@True";
                        previousFrame = (int)Tools.getTimer();
                    }
                }
                else
                    toRet = fb.feedback + '@' + feedbackName + '@' + Tools.getStopWatch().ToString() + "@True";
            }
            return toRet;
        }



    }

    #endregion

}
