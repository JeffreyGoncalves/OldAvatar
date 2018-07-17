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
using LecturerTrainer.Model.BodyAnalysis;

namespace LecturerTrainer.ViewModel
{
    public class TrackingSideToolViewModel : ViewModelBase
    {

        #region fields
        /// <summary>
        /// Supposed to be a singleton of the class
        /// </summary>
        private static TrackingSideToolViewModel trackingSideToolViewModel = null;

        /// <summary>
        /// Indicates if body feedbacks are enabled
        /// </summary>
        private bool useFeedback = true;

        /// <summary>
        /// The emotion detected
        /// </summary>
        private string emotion = "";

        /// <summary>
        /// Indicates if bad reflex feedbacks are enabled
        /// </summary>
        private bool badVoiceReflex = false;

        /// <summary>
        /// Indicates if bad voice monotony feedbacks are enabled
        /// </summary>
        private bool voiceMonotony = false;

        /// <summary>
        /// Indicates if speed rate feedbacks are enabled
        /// </summary>
        private bool speedRate = false;
        
        /// <summary>
        /// Indicates if peak feedbacks are enabled
        /// </summary>
        private bool peakDetection = false;

        /// <summary>
        /// Used to allow DrawingSheetAvatarViewModel to get the text
        /// recognized in AudioProvider
        /// </summary>
        private bool showTextOnScreen = false;

        /// <summary>
        /// Used to start to use the teleprompter
        /// </summary>
        private bool teleprompterUsed = false;

        #endregion

        #region constructor and get
        private TrackingSideToolViewModel()
        {
            UseFeedback = true;
            FaceRecognition.emotionEvent += emotionEvent;
        }        
        
        public static TrackingSideToolViewModel get()
        {
            if (trackingSideToolViewModel == null)
            {
                trackingSideToolViewModel = new TrackingSideToolViewModel();
            }
            return trackingSideToolViewModel;
        }
        #endregion

        #region customize feedbacks
        private ICommand feedbackCommand;
        private ChoiceFeedbackView choiceFeedback = null;
        public ICommand FeedbackCommand
        {
            get
            {
                if (this.feedbackCommand == null)
                    this.feedbackCommand = new RelayCommand(() => this.displayFeedbackChoice());

                return this.feedbackCommand;
            }
        }

        private void displayFeedbackChoice()
        {
            choiceFeedback = new ChoiceFeedbackView();
            //((ChoiceResultViewModel)choiceFeedback.DataContext).enableSomeCheckBox(Path.GetDirectoryName(statisticsPath));
            //((ChoiceResultViewModel)choiceFeedback.DataContext).isLoad = true;
            choiceFeedback.ShowDialog();
        }

        #endregion

        #region bindings == properties
        /// <summary>
        /// Voice monotony length of samples
        /// </summary>
        public int MonotonySampleLength
        {
            get { return MainWindow.main.audioProvider._pitch.Length; }
            set
            {
                MainWindow.main.audioProvider._pitch.Length = value;
                OnPropertyChanged("MonotonySampleLength");
            }
        }

        /// <summary>
        /// Voice monotony (boring event) threshold
        /// </summary>
        public int MonotonySampleThreshold
        {
            get { return (int)MainWindow.main.audioProvider._pitch.Threshold; }
            set
            {
                MainWindow.main.audioProvider._pitch.Threshold = (double)value;
                OnPropertyChanged("MonotonySampleThreshold");
            }
        }

        /// <summary>
        /// Hesitation (stress / bad reflex) length of samples
        /// </summary>
        public int HesitationSampleLength
        {
            get
            {
                return MainWindow.main.audioProvider._fft.Length;
            }
            set
            {
                MainWindow.main.audioProvider._fft.Length = value;
                OnPropertyChanged("HesitationSampleLength");
            }
        }

        /// <summary>
        /// Mouth opened feedbacks detection
        /// Feature not working
        /// </summary>
        /*public bool Mouth
        {
            get
            {
                return mouthOpened.detect;
            }
            set
            {
                mouthOpened.detect = value;
                OnPropertyChanged("Mouth");
            }
        }*/

        /// <summary>
        /// Mouth shut feedbacks detection
        /// Feature not working
        /// </summary>
        /*public bool Mouth2
        {
            get
            {
                return mouthShut.detect;
            }
            set
            {
                mouthShut.detect = value;
                OnPropertyChanged("Mouth2");
            }
        }*/

        /// <summary>
        /// Looking direction feedbacks detection
        /// </summary>
        public bool LookR
        {
            get
            {
                return lookingDirection.detect;
            }
            set
            {
                lookingDirection.detect = value;
                if (value==false)
                {
                    lookingDirection.feedC = false;
                    lookingDirection.feedL = false;
                    lookingDirection.feedR = false;
                }
                
                OnPropertyChanged("LookR");
            }
        }

        /// <summary>
        /// Pupil right feedbacks detection
        /// Feature not working
        /// </summary>
        /*public bool pupilR
        {
            get
            {
                return pupilRight.detect;
            }
            set
            {
                pupilRight.detect = value;
                OnPropertyChanged("pupilR");
            }
        }*/

        /// <summary>
        /// Emotion feedbacks detection
        /// </summary>
        public bool emo
        {
            get
            {
                return EmotionRecognition.detect;
            }
            set
            {
                EmotionRecognition.detect = value;
                if (value == false)
                {
                    EmotionRecognition.happy = false;
                    EmotionRecognition.surprised = false;
                }              
                IconViewModel.get().clearEmotion();

                OnPropertyChanged("emo");
            }
        }

        /// <summary>
        /// ?
        /// </summary>
        public bool UseFeedback
        {
            get
            {
                return useFeedback;
            }
            set
            {
                try
                {
                    this.useFeedback = value;
                    if (useFeedback)
                    {
                        Gesture.init();
                        Posture.init();
                    }
                    else
                    {
                        IconViewModel.get().clearBody();
                    }
                    Gesture.compare = useFeedback;
                    Posture.compare = useFeedback;
                    Agitation.detect = useFeedback;
                    HandsJoined.detect = useFeedback;
                    HandsRaised.compare = useFeedback;
                    OnPropertyChanged("UseFeedback");
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Bad reflex feedbacks detection
        /// </summary>
        public bool BadVoiceReflex
        {
            get
            {
                return badVoiceReflex;
            }
            set
            {
                if (value)
                {
                    MainWindow.main.audioProvider.startBadReflexRecognition();
                }
                else
                {
                    MainWindow.main.audioProvider.stopBadReflexRecognition();
                    IconViewModel.get().clearBadVoiceReflex();
                }
                badVoiceReflex = value;
                OnPropertyChanged("BadVoiceReflex");
            }
        }

        /// <summary>
        /// Voice monotony (boring) feedbacks detection
        /// </summary>
        public bool VoiceMonotony
        {
            get
            {
                return voiceMonotony;
            }
            set
            {
                if (value)
                {
                    MainWindow.main.audioProvider.startPitchDetection();
                }
                else
                {
                    MainWindow.main.audioProvider.stopPitchDetection();
                    IconViewModel.get().clearBoring();
                }
                voiceMonotony = value;
                OnPropertyChanged("VoiceMonotony");
            }
        }

        /// <summary>
        /// Speed feedbacks detection
        /// </summary>
        public bool SpeedRate
        {
            get
            {
                return speedRate;
            }
            set
            {
                if (value)
                {
                    MainWindow.main.audioProvider.startSpeechRateDetection();
                }
                else
                {
                    MainWindow.main.audioProvider.stopSpeechRateDetection();
                    IconViewModel.get().clearSpeed();
                }
                speedRate = value;
                OnPropertyChanged("SpeedRate");
            }
        }

        /// <summary>
        /// Peak detection
        /// </summary>
        public bool PeakDetection
        {
            get
            {
                return peakDetection;
            }
            set
            {
                if (value)
                {
                    MainWindow.main.audioProvider.startPeakDetection();
                }
                else
                {
                    MainWindow.main.audioProvider.stopPeakDetection();
                }
                peakDetection = value;
                OnPropertyChanged("PeakDetection");
            }
        }

        /// <summary>
        /// Used to allow DrawingSheetAvatarViewModel to get the text
        /// recognized in AudioProvider
        /// </summary>
        public bool ShowTextOnScreen
        {
            get
            {
                return showTextOnScreen;
            }
            set
            {
                if (value)
                {
                    MainWindow.main.audioProvider.showText();
                    DrawingSheetAvatarViewModel.Get().startDisplayTxtSaid();
                }
                else
                {
                    MainWindow.main.audioProvider.hideText();
                    DrawingSheetAvatarViewModel.Get().stopDisplayTxtSaid();
                }
                showTextOnScreen = value;
                OnPropertyChanged("ShowTextOnScreen");
            }
        }

        /// <summary>
        /// Allow to use the teleprompter or to shut down it.
        /// </summary>
        public bool TeleprompterUsed
        {
            get { return teleprompterUsed; }
            set
            {
                if (value)
                {
                    DrawingSheetAvatarViewModel.Get().startTelePrompter();
                }
                else
                {
                    DrawingSheetAvatarViewModel.Get().stopTeleprompter();
                }
                teleprompterUsed = value;
                OnPropertyChanged("TeleprompterUsed");
            }
        }

        /// <summary>
        /// Max agitation sensitivity value
        /// Modified by Baptiste Germond
        /// </summary>
        public double MaxAgitationSensitivity { get { return 150; } }
        public double MinAgitationSensitivity { get { return 50; } }
        public double SliderAgitation
        {
            get
            {
                return MaxAgitationSensitivity - Agitation.RelevantTreshold + MinAgitationSensitivity;

            }
            set
            {
                Agitation.RelevantTreshold = MaxAgitationSensitivity - value +MinAgitationSensitivity;
                OnPropertyChanged("SliderAgitation");
            }
        }

        /// <summary>
        /// Emotion text
        /// </summary>
        public string Emotion
        {
            get
            {
                return emotion;
            }
            set
            {
                emotion = value;
                OnPropertyChanged("Emotion");
            }
        }

        /// <summary>
        /// Emotion detection enabling
        /// </summary>
        public bool UseEmotion
        {
            get
            {
                return FaceRecognition.compare;
            }
            set
            {
                FaceRecognition.compare = value;
                if (!value)
                {
                    Emotion = "";
                }
                OnPropertyChanged("UseEmotion");
            }
        }

        /// <summary>
        /// Face tracking enabling
        /// </summary>
        public bool FaceTracking
        {
            get
            {
                return KinectDevice.faceTracking;
            }
            set
            {
                if (value)
                {
                    KinectDevice.faceTracking = true;
                }
                else
                {
                    KinectDevice.faceTracking = false;
                    this.UseEmotion = false;
                    emo = false;
                    //Mouth = false;
                    //Mouth2 = false;
                    LookR = false;
                    //pupilR = false;
                    voice(true);
                }
                OnPropertyChanged("FaceTracking");
            }
        }

        public void emotionEvent(object source, EmotionEventArgs e)
        {
            Emotion = e.nearest;
        }


        /// <summary>
        /// List of the differents language you can recognize
        /// </summary>
        public ObservableCollection<String> LanguageList
        {
            get
            {
                return MainWindow.main.audioProvider.PossibleLanguage;
            }
        }

        /// <summary>
        /// Select the language recognized
        /// </summary>
        public int SelectedLanguage
        {
            get
            {
                return MainWindow.main.audioProvider.SelectedLanguage;
            }
            set
            {
                MainWindow.main.audioProvider.SelectedLanguage = value;
            }
        }

        #endregion

        #region methods

        private void voice(bool enable)
        {
            if (!enable)
            {
                BadVoiceReflex = false;
                SpeedRate = false;
                VoiceMonotony = false;
            }
            
        }

        private bool CanUseFeedback()
        {
            return true;
        }
        #endregion
    }
}
