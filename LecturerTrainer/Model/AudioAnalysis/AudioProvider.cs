using LecturerTrainer.Model;
using LecturerTrainer.View;
using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Globalization;
using NAudio.Mixer;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Text.RegularExpressions;
using LiveCharts.Wpf;
using System.Windows.Threading;
using System.Diagnostics;

namespace LecturerTrainer.Model.AudioAnalysis
{

    /// <summary>
    /// Class handling all of the voice analysis
    /// </summary>
    /// <remarks>
    /// Author: Valentin Roux
    /// Update by : Timothée Perrin
    /// </remarks>
    public class AudioProvider
    {
        #region Attributes

        /// <summary>
        /// You dont need to use the Kinect Microphone, if you have a better one, use it !
        /// </summary>
        private int _deviceNumber;

        /// <summary>
        /// Boolean used to know if we have to start the Bad reflex recognition.
        /// </summary>
        private bool _badReflex;

        /// <summary>
        /// Volume of the microphone.
        /// </summary>
        private double desiredVolume = 100;

        /// <summary>
        /// Class used for recording the audio.
        /// </summary>
        private WaveIn _waveIn;

        public int counterTicTmp = 0;
        //private int speedCounter = 0; //added by ASu 3Nov 2015
        public static int WordSum = 0;
        private int[] WordBuffer = new int[5] { 0, 0, 0, 0, 0 };


        //private string pathFile = "";

        /// <summary>
        /// Class used for recording the audio.
        /// </summary>
        //private WaveFileWriter _waveWriter;

        /// <summary>
        /// Format of the wave.
        /// </summary>
        private WaveFormat _recordingFormat;

        /// <summary>
        /// Sensor used for the speech recognition and speed detection.
        /// </summary>
        private KinectSensor _sensor;

        /// <summary>
        /// Class used to recognize the words.
        /// </summary>
        private SpeechRecognitionEngine speechEngine;

        private bool voiceCommand;

        /// <summary>
        /// Short list of recognized words.
        /// </summary>
        private List<RecognizedWordUnit> WordsL;

        /// <summary>
        /// Used to check each # seconds the number of words used and determine if the speaker
        /// speak too fast or not.
        /// </summary>
        private DispatcherTimer timer;


        /// <summary>
        /// Keeped the number of word update all # seconds
        /// Used to choose the speech rate feedback 
        /// </summary>
        private double[] nbWordByHypo;

        /// <summary>
        /// Used to know in witch case of nbWord and nbWordByHypo we are
        /// </summary>
        private int indNbWord;

        /// <summary>
        /// Use for the speed Detection;
        /// Used for the size of nbWordByHypo
        /// </summary>
        private const int sizeNbWord = 8;

        /// <summary>
        /// number of millisecond before refresh the timer name timer
        /// Used for the speed Detection
        /// </summary>
        private const int refreshTimer = 1000;

        /// <summary>
        /// use to recognize the keyword 
        /// and keep the number of times you say a keyword
        /// </summary>
        private Dictionary<String, int> keyword;

        /// <summary>
        /// use to recognize the tics
        /// and keep the number of tics said
        /// </summary>
        private Dictionary<String, int> tics;

        public static Dictionary<int, double> dicWpm = null;

        /// <summary>
        /// Used when we record a session to keep the text recognized
        /// </summary>
        private List<string> textRecorded;

        /// <summary>
        /// Used to take control of the microphone volume control
        /// </summary>
        private UnsignedMixerControl volumeControl;


        /// Feedback to display when the lecturer speak to fast.
        /// </summary>
        private static string tooFastText = "Too fast!";

        /// <summary>
        /// Used to know if we must record the data
        /// </summary>
        private bool isRecording;

        /// <summary>
        /// Used for sound recording
        /// </summary>
        private AudioRecorder recorder;

        /// <summary>
        /// If the replay mode is activated, no real time events are raised
        /// </summary>
        public bool replayMode = false;

        private ViewModel.SideToolsViewModel sideToolViewModel;

        /// <summary>
        /// Used when you listen with the kinect microphone
        /// </summary>
        private Stream kinectStream;

        /// <summary>
        /// Get the audio from the kinect microphone
        /// </summary>
        private KinectAudioSource source;

        /// <summary>
        /// Text get in the file lecturertrainer\LecturerTrainer\Prompter\Speech.txt
        /// </summary>
        List<String> textTeleprompter;

        /// <summary>
        /// First line display by the prompter 
        /// corresponding on the element of the list textTeleprompter
        /// </summary>
        int linePrompter;

        /// <summary>
        /// Contain the number of word recognize in the text to prompt
        /// </summary>
        int[] nbWordLinePrompter = { 0, 0, 0, 0, 0};

        /// <summary>
        /// Speech rate
        /// </summary>
        public static Dictionary<int, double> syllablesInTime = null;
        public static Dictionary<int, double> levelOfSpeech = null;

        int nbSyllables = 0;
        int numberOfPeaks = 0;

        #endregion

        #region Accesors

        /// <summary>
        /// Used to choose the microphone used
        /// </summary>
        public int DeviceNumber
        {
            get { return _deviceNumber; }
            set
            {
                _deviceNumber = value;
                this._waveIn.StopRecording();
                this._waveIn.DeviceNumber = value;
                this._waveIn.StartRecording();
            }
        }


        public FFT _fft { get; private set; }
        public Pitch _pitch { get; private set; }
        public List<RecognizedWordUnit> WordsRecognized
        {
            get
            {
                return WordsL;
            }
        }

        public double MicrophoneLevel
        {
            get
            {
                return desiredVolume;
            }
            set
            {
                desiredVolume = value;
                if (volumeControl != null)
                {
                    volumeControl.Percent = value;
                }
            }
        }

        /// <summary>
        /// Used for the selection of the microphone
        /// </summary>
        private ObservableCollection<String> _ListDevices;
        public ObservableCollection<String> ListDevices
        {
            get { return _ListDevices; }
            set { _ListDevices = value; }
        }

        /// <summary>
        /// Used to choose the language use for the voice recognition
        /// </summary>
        private int _SelectedLanguage;

        /// <summary>
        /// Used to change or get the value of _SelectedLanguage
        /// </summary>
        public int SelectedLanguage
        {
            get { return _SelectedLanguage; }
            set
            {
                _SelectedLanguage = value;

                // To change the recognised language, we have to dispose the Speech Recognition Engine
                // and initialise it again
                speechEngine.Dispose();

                initialiseMicrophone();

                // Because we reinitialise speechEngine, we have to reactivate all the events.

                // Reactive the speedRate
                if (TrackingSideToolViewModel.get().SpeedRate)
                {
                    speechEngine.SpeechHypothesized += SpeechHypothesized;
                }

                // Show the text on the screen if activated
                if (TrackingSideToolViewModel.get().ShowTextOnScreen)
                {
                    showText();
                }

                // Continue to use the prompter
                if (TrackingSideToolViewModel.get().TeleprompterUsed)
                {
                    speechEngine.SpeechRecognized += nextLinePrompteur;
                    speechEngine.SpeechRecognitionRejected += nextLinePrompteur;
                }

                speechEngine.SpeechRecognized += SpeechRecognizedTics;
                speechEngine.SpeechRecognized += SpeechRecognizedKeyword;
            }
        }

        /// <summary>
        /// Used to choose the language used in the recognition
        /// </summary>
        private ObservableCollection<String> _PossibleLanguage;

        /// <summary>
        /// Used to modify _SelectedLanguage
        /// </summary>
        public ObservableCollection<String> PossibleLanguage
        {
            get { return _PossibleLanguage; }
        }

        #endregion

        #region Event

        /// <summary>
        /// Event raised when the lecturer speaks too fast
        /// </summary>
        public static event EventHandler<LongFeedback> tooFastEvent;

        /// <summary>
        /// Event raised every 5min, when the "speedDetection" is activated
        /// </summary>
        public static event EventHandler<ValuedFeedback> speedEvent;

        /// <summary>
        /// Event raised every when a voice tic is detected
        /// </summary>
        public static event EventHandler<InstantFeedback> ticEvent;

        /// <summary>
        /// Event raised every when a key word is pronounced
        /// </summary>
        public static event EventHandler<InstantFeedback> keyWordEvent;

        /// <summary>
        /// Event raised every time a word is recognized.
        /// </summary>
        public static event EventHandler<Feedback> textToShowEvent;

        /// <summary>
        /// Event raised when the prompter has to change line
        /// </summary>
        public static event EventHandler<IntFeedback> linePrompterEvent;

        #endregion

        #region Contructors

        /// <summary>
        /// Instantiate an AudioProvider Class to start voice analysis
        /// </summary>
        /// <param name="kd">Informations about the kinect</param>
        /// <param name="dn">Device numver (if you want to use a different microphone for some analysis)</param>
        public AudioProvider(KinectDevice kd, int dn) : this(kd)
        {
            this.sideToolViewModel = SideToolsViewModel.Get();
            this._deviceNumber = dn;
        }
        /// <summary>
        /// Value to know when the timer starts and use it to get a more accurate value of the duration
        /// of the snippet we are analyzing
        /// </summary>
        private DateTime TimerStart { get; set; }

        /// <summary>
        /// Instantiate an AudioProvider Class to start voice analysis
        /// </summary>
        /// <param name="kd">Informations about the kinect</param>
        public AudioProvider(KinectDevice kd)
        {
            this.sideToolViewModel = SideToolsViewModel.Get();
            this._fft = new FFT();
            this._pitch = new Pitch(kd);
            this.WordsL = new List<RecognizedWordUnit>();

            this._badReflex = false;
            this.isRecording = false;

            this._waveIn = new WaveIn();
            this._recordingFormat = new WaveFormat(44100, 1);
            this._waveIn.WaveFormat = this._recordingFormat;

            this._waveIn.StartRecording();
            //pathFile = @"C:\Users\Public\TestFolder\test";

            // use every 5 sec to find the wpm (Word Per Minute)
            this.TimerStart = DateTime.Now;
            this.timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(refreshTimer);
            timer.Tick += new EventHandler(speechRateDetection);
            // initialize the array used for the wpm
            nbWordByHypo = new double[sizeNbWord];
            for (int i = 0; i < sizeNbWord; i++)
            {
                nbWordByHypo[i] = 0;
            }


            this._waveIn.DataAvailable += FFTCallback;
            this.TryGetVolumeControl();

            // Collect all the microphone connected to your computer
            ListDevices = new ObservableCollection<string>();
            this.ListDevices.Clear();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                this.ListDevices.Add(WaveIn.GetCapabilities(n).ProductName);
            }


            // load the keywords from the file and initialize counter
            keyword = new Dictionary<string, int>();
            String[] tabKeyword = takeWordFile(@"../../Grammar/keywords.txt");
            foreach (String s in tabKeyword)
                keyword.Add(s.ToUpper(), 0);
            // load the tics from the file and initialize counter
            tics = new Dictionary<string, int>();
            String[] tabTics = takeWordFile(@"../../Grammar/tics.txt");
            foreach (String s in tabTics)
                tics.Add(s.ToUpper(), 0);
            // Get all the dictionaries used on your computer
            _PossibleLanguage = new ObservableCollection<string>();
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                _PossibleLanguage.Add(recognizer.Culture.Name);
            }

            // Select the default language of your computer
            _SelectedLanguage = _PossibleLanguage.IndexOf(Thread.CurrentThread.CurrentCulture.Name);

            // Initialise SpeechEngine
            initialiseMicrophone();

            // Allow to get the keywords and the tics said
            speechEngine.SpeechRecognized += SpeechRecognizedTics;
            speechEngine.SpeechRecognized += SpeechRecognizedKeyword;

            dicWpm = new Dictionary<int, double>();
            syllablesInTime = new Dictionary<int, double>();
            levelOfSpeech = new Dictionary<int, double>();
        }


        #endregion

        #region Methods

        /// <summary>
        /// Get a text in a file
        /// </summary>
        /// <param name="path"> The path of the file </param>
        /// <returns> The text of the file </returns>
        private String[] loadTextFile(string path)
        {
            return File.ReadAllLines(@path);
        }

        /// <summary>
        /// Use to get the word to recognize in a file
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>list of the words</returns>
        public string[] takeWordFile(string path)
        {
            HashSet<string> result = new HashSet<string>();
            string test;
            string[] lines = File.ReadAllLines(@path);

            foreach (string s in lines)
            {
                // suppress all special characters
                test = Regex.Replace(s, "[^a-zA-Z '0-9]+", "", RegexOptions.Compiled);

                if (test != "")
                    result.Add(test.ToUpper());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Enable the voice command.
        /// </summary>
        public void enableVoiceCommand()
        {
            if (!voiceCommand)
            {
                speechEngine.SpeechRecognized -= SpeechRecognizedTics;
                speechEngine.SpeechRecognized += SpeechRecognized;
                voiceCommand = true;
            }
        }

        /// <summary>
        /// Disable the voice command.
        /// </summary>
        public void disableVoiceCommand()
        {
            if (voiceCommand)
            {
                speechEngine.SpeechRecognized -= SpeechRecognized;
                speechEngine.SpeechRecognized += SpeechRecognizedTics;
                voiceCommand = false;
            }
        }

        /// <summary>
        /// Used for recognized the voice command words
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;
            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                sideToolViewModel.speechRecognized(e.Result.Text);
            }
        }

        /// <summary>
        /// Used for recognized the tics
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognizedTics(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            double ConfidenceThreshold = 0.1;
            if (e.Result.Confidence >= ConfidenceThreshold)
            {

                // Search all tics said
                for (int ind = 0; ind < tics.Count; ind++)
                {
                    if (e.Result.Text.ToUpper().IndexOf(tics.ElementAt(ind).Key) >= 0)
                    {
                        // increase the value of the tic found
                        int currentCount;
                        tics.TryGetValue(tics.ElementAt(ind).Key, out currentCount);
                        currentCount++;
                        tics[tics.ElementAt(ind).Key] = currentCount;

                        // make a feedback
                        counterTicTmp++;
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\testTics.txt", true))
                        {
                            file.WriteLine("{0} detected, actual number of \" {0} \" : {1} ", tics.ElementAt(ind).Key, currentCount);
                        }
                        ticEvent(null, new InstantFeedback("\" " + tics.ElementAt(ind).Key + " \" said " + currentCount + " time(s)"));
                    }
                }
            }
        }

        /// <summary>
        /// Used to start the recording in a list of the text recognized
        /// </summary>
        public void startRecText()
        {
            textRecorded = new List<string>();
            speechEngine.SpeechRecognized += saveWordRecognized;
            speechEngine.SpeechRecognitionRejected += saveWordRecognized;
        }

        /// <summary>
        /// Used to stop the recording th word recognized
        /// </summary>
        /// <returns>All the words recognized during the recording</returns>
        public List<string> stopRecText()
        {
            speechEngine.SpeechRecognized -= saveWordRecognized;
            speechEngine.SpeechRecognitionRejected -= saveWordRecognized;

            return textRecorded;
        }

        /// <summary>
        /// Used to put the text recognized in a list to save it
        /// </summary>
        /// <param name="even">Source of the caller.</param>
        /// <param name="e">Used to get the text recognized.</param>
        private void saveWordRecognized(object even, SpeechRecognizedEventArgs e)
        {
            textRecorded.Add(e.Result.Text);
        }

        /// <summary>
        /// Used to put texte in the list of the recognized word even if it's not recognized
        /// </summary>
        /// <param name="even">Source of the caller.</param>
        /// <param name="e">Used to get the text unrecognized.</param>
        private void saveWordRecognized(object even, SpeechRecognitionRejectedEventArgs e)
        {
            textRecorded.Add("Approximate  recognized text : " + e.Result.Text);
        }

        /// <summary>
        /// Used to recognized the keyword like: "I have a dream"
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognizedKeyword(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            double ConfidenceThreshold = 0.1;
            if (e.Result.Confidence >= ConfidenceThreshold)
            {

                // Search all keywords said
                for (int ind = 0; ind < keyword.Count; ind++)
                {
                    if (e.Result.Text.ToUpper().IndexOf(keyword.ElementAt(ind).Key) >= 0)
                    {
                        // increase the value of the Keyword found
                        int currentCount;
                        keyword.TryGetValue(keyword.ElementAt(ind).Key, out currentCount);
                        currentCount++;
                        keyword[keyword.ElementAt(ind).Key] = currentCount;

                        // create a feedback
                        keyWordEvent(null, new InstantFeedback(keyword.ElementAt(ind).Key));
                    }
                }
            }
        }

        /// <summary>
        /// Used to configure speechEngine
        /// </summary>
        private void initialiseMicrophone()
        {
            // initialise with the language choosen
            Trace.WriteLine(SelectedLanguage.ToString());
            speechEngine = new SpeechRecognitionEngine(new CultureInfo(_PossibleLanguage.ElementAt(SelectedLanguage > 0 ? SelectedLanguage : 0)));
            speechEngine.LoadGrammar(new DictationGrammar());

            // We can't use speechEngine.SetInputToDefaultAudioDevice();
            // with the kinect otherwise the recognition will be very bad
            // For now the kinect isn't seen fast enough to go through that condition
            // See KinectDevice events to trigger it when a kinect is connected
            if (WaveIn.GetCapabilities(0).ProductName.Contains("Kinect") && KinectDevice.sensor != null)
            {
                // Obtain a KinectSensor if any are available
                this._sensor = KinectDevice.sensor;

                _sensor.Start();
                source = _sensor.AudioSource;
                source.EchoCancellationMode = EchoCancellationMode.None; // No AEC for this sample
                source.AutomaticGainControlEnabled = false; // Important to turn this off for speech recognition

                kinectStream = source.Start();

                speechEngine.SetInputToAudioStream(kinectStream, new System.Speech.AudioFormat.SpeechAudioFormatInfo
                    (System.Speech.AudioFormat.EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            }
            else
            {
                speechEngine.SetInputToDefaultAudioDevice(); // set the input of the speech recognizer to the default audio device

                speechEngine.InitialSilenceTimeout = TimeSpan.FromMilliseconds(1);
                speechEngine.BabbleTimeout = TimeSpan.FromMilliseconds(1);
                speechEngine.EndSilenceTimeout = TimeSpan.FromMilliseconds(1);
                speechEngine.EndSilenceTimeoutAmbiguous = TimeSpan.FromMilliseconds(1);
            }

            speechEngine.RecognizeAsync(RecognizeMode.Multiple); // recognize speech asynchronous

        }


        public static void raiseTicEvent(ServerFeedback feedback)
        {
            ticEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }

        public static void raiseTooFastEvent(ServerFeedback feedback)
        {
            tooFastEvent(null, new LongFeedback(feedback.feedbackMessage, feedback.display));
        }

        public static void raiseSpeedEvent(ServerFeedback feedback)
        {
            // The speed value is stored in the message
            int iValue;
            int.TryParse(feedback.feedbackMessage, out iValue);
            Console.Out.WriteLine("Feedback message : " + feedback.feedbackMessage);
            speedEvent(null, new ValuedFeedback(feedback.feedbackMessage, feedback.display, iValue));
        }

        public static void raiseKeyWordEvent(ServerFeedback feedback)
        {
            keyWordEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }

        /// <summary>
        /// Start the Speed detection.
        /// </summary>
        /// <returns>True if the service started well.</returns>
        public bool startSpeechSpeedDetection()
        {

            try
            {
                this.speechEngine.SpeechHypothesized += SpeechHypothesized;
                timer.Start();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Stop the speed detection.
        /// </summary>
        /// <returns>True if the service stopped well.</returns>
        public bool stopSpeechSpeedDetection()
        {
            try
            {
                this.speechEngine.SpeechHypothesized -= SpeechHypothesized;
                timer.Stop();

                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Stop the fft detection.
        /// </summary>
        /// <returns>True if the service stopped well.</returns>
        public bool stopFFTDetection()
        {
            try
            {
                this._waveIn.DataAvailable -= FFTCallback;
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Start the fft detection.
        /// </summary>
        /// <returns>True if the service stopped well.</returns>
        public bool startFFTDetection()
        {
            try
            {
                this._waveIn.DataAvailable += FFTCallback;
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Start the Monotony detection.
        /// </summary>
        /// <returns>True if the service started well.</returns>
        public bool startPitchDetection()
        {
            try
            {
                this._waveIn.DataAvailable += PitchCallback;
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Stop the Monotony detection.
        /// </summary>
        /// <returns>True if the service stopped well.</returns>
        public bool stopPitchDetection()
        {
            try
            {
                this._waveIn.DataAvailable -= PitchCallback;
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Start the Peak detection.
        /// </summary>
        /// <returns>True if the service started well.</returns>
        public bool startPeakDetection()
        {
            try
            {
                //Console.Out.WriteLine("Peak Detection starting !");
                this._waveIn.DataAvailable += PitchCallbackPeak;
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Stop the Peak detection.
        /// </summary>
        /// <returns>True if the service stopped well.</returns>
        public bool stopPeakDetection()
        {
            try
            {
                this._waveIn.DataAvailable -= PitchCallbackPeak;
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Start the "Hummmm and "Ehhhhhh" recognition.
        /// </summary>
        /// <returns>True if the service started well.</returns>
        public bool startBadReflexRecognition()
        {
            try
            {
                this._badReflex = true;
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Stop the "Hummmm and "Ehhhhhh" recognition.
        /// </summary>
        /// <returns>True if the service stopped well.</returns>
        public bool stopBadReflexRecognition()
        {
            try
            {
                this._badReflex = false;
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Called when new data arrives and start the pitch detection.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Data of the signal.</param>
        private void PitchCallback(object sender, WaveInEventArgs e)
        {
            new Thread(() => this._pitch.pitchCompute(e.Buffer, e.BytesRecorded)).Start();
            //new Thread(() => this.speechRate(e.Buffer, e.BytesRecorded)).Start();
        }

        /// <summary>
        /// Called when new data arrives and start the pitch detection.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Data of the signal.</param>
        private void PitchCallbackPeak(object sender, WaveInEventArgs e)
        {
            new Thread(() => this._pitch.pitchComputePeak(e.Buffer, e.BytesRecorded)).Start();
        }

        /// <summary>
        /// Called when new data arrives and start the FFT.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Data of the signal.</param>
        private void FFTCallback(object sender, WaveInEventArgs e)
        {
            this._fft.getFFT(e.Buffer, e.BytesRecorded);
            if (e.BytesRecorded > 0 && this._badReflex)
                this._fft.BadReflexSpeaking();
        }

        /// <summary>
        /// Take the control of the microphone volume system.
        /// </summary>
        private void TryGetVolumeControl()
        {
            int waveInDeviceNumber = _waveIn.DeviceNumber;
            if (Environment.OSVersion.Version.Major >= 6) // Vista and over
            {
                var mixerLine = _waveIn.GetMixerLine();
                foreach (var control in mixerLine.Controls)
                {
                    if (control.ControlType == MixerControlType.Volume)
                    {
                        this.volumeControl = control as UnsignedMixerControl;
                        MicrophoneLevel = desiredVolume;
                        break;
                    }
                }
            }
            else
            {
                var mixer = new Mixer(waveInDeviceNumber);
                foreach (var destination in mixer.Destinations
                    .Where(d => d.ComponentType == MixerLineComponentType.DestinationWaveIn))
                {
                    foreach (var source in destination.Sources
                        .Where(source => source.ComponentType == MixerLineComponentType.SourceMicrophone))
                    {
                        foreach (var control in source.Controls
                            .Where(control => control.ControlType == MixerControlType.Volume))
                        {
                            volumeControl = control as UnsignedMixerControl;
                            MicrophoneLevel = desiredVolume;
                            break;
                        }
                    }
                }
            }
        }

        private int stopSpeaking = 0;
        /// <summary>
        /// Called each # seconds to check if the user speak too fast or not.
        /// </summary>
        /// <param name="source">Source of the caller.</param>
        /// <param name="e">Information about the ellapsed time.</param>
        private void TimeElapsed(object source, EventArgs e)
        {

            // If the user stop speaking, we count it.
            // It's <= 2  because the microphone can get some noise.
            if (nbWordByHypo[indNbWord] <= 2)
            {
                stopSpeaking++;
            }
            else
            {
                stopSpeaking = 0;
            }


            int mult = 60000 / (refreshTimer * sizeNbWord); // used to convert the result in Word per Minute (wpm)
            int secondMult = 60000 / refreshTimer; // used to convert the bonus in WPM

            double averageWpm = 0;

            // Count all the words recorded in the array
            foreach (double i in nbWordByHypo)
            {
                averageWpm += i;
            }

            // convert in WPM
            averageWpm *= mult;
            dicWpm.Add(refreshTimer * dicWpm.Count, averageWpm);
            // If you stop speaking more than 1.5 seconds,
            // we don't look at the WPM. me just put the feedback at 0
            if (stopSpeaking * refreshTimer >= 1500)
            {
                speedEvent(this, new ValuedFeedback("", true, 0));
            }
            else
            {
                if (TrackingSideToolViewModel.get().SpeedRate && !replayMode)
                {

                    double bonusSpeed = nbWordByHypo[indNbWord] * secondMult - averageWpm;
                    if (averageWpm < 60)//Word by minute (wpm) < 60, equal to 2 words per 2 seconds
                    {
                        // The difference between the entire array and the last element is bigger than 150
                        // It's mean in this case you spoke very slow and just after you speak really fast
                        if (bonusSpeed > 200)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 3));
                        }
                        else if (bonusSpeed > 150)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 2));
                        }
                        // You speak a little bit faster
                        else if (bonusSpeed > 60)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 1));
                        }
                        // Case where you speak in a constant speed or the difference is too small.
                        else
                        {
                            speedEvent(this, new ValuedFeedback("", true, 0));
                        }
                    }
                    else if (averageWpm < 120) // 2 < words per 2 seconds < 4
                    {
                        // You spoke very faster than previously
                        if (bonusSpeed > 180)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 4));
                        }
                        else if (bonusSpeed > 120)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 3));
                        }
                        // You spoke a little bit faster tahn previously
                        else if (bonusSpeed > 60)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 2));
                        }
                        // Case where you speak in a constant speed or the difference is too small.
                        else
                        {
                            speedEvent(this, new ValuedFeedback("", true, 1));
                        }
                    }
                    else if (averageWpm < 200) // 4 < words per 2 seconds < 7
                    {
                        // You spoke a lot faster than previously
                        if (bonusSpeed > 120)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 4));
                        }
                        // You spoke a bit faster than previously
                        else if (bonusSpeed > 50)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 3));
                        }
                        // you spoke a little bit slower than previously
                        else if (bonusSpeed < -150)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 1));
                        }
                        // Case where you speak in a constant speed or the difference is too small
                        else
                        {
                            speedEvent(this, new ValuedFeedback("", true, 2));
                        }

                    }
                    else if (averageWpm < 250) // 7 < Words per 2 seconds < 10
                    {
                        // You spoke very slower than previously
                        if (bonusSpeed < -240)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 1));
                        }
                        // You spoke a little bit slower than previously
                        else if (bonusSpeed < -180)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 2));
                        }
                        else if (bonusSpeed > 60)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 4));
                        }
                        // Case where you speak in a constant speed or the difference is too small
                        else
                        {
                            speedEvent(this, new ValuedFeedback("", true, 3));
                        }

                    }
                    else // wpm > 300  = more than 10 words per 2 seconds
                    {
                        // You spoke very slower than previously
                        if (bonusSpeed < -180)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 2));
                        }
                        // You spoke a little bit slower than previously
                        else if (bonusSpeed < -90)
                        {
                            speedEvent(this, new ValuedFeedback("", true, 3));
                        }
                        // Case where you speak in a constant speed or the difference is too small
                        else
                        {
                            speedEvent(this, new ValuedFeedback("", true, 4));
                            // Make a feedback
                            tooFastEvent(this, new LongFeedback(tooFastText, true));
                        }

                    }

                    // make a feedback if you say too many tics
                    if (counterTicTmp >= 2)
                    {
                        ticEvent(null, new InstantFeedback("Too many tics!"));
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\testTicsDetected.txt", true))
                        {
                            file.WriteLine("You said too many tic words in a little amount of time -- " + counterTicTmp);
                        }
                    }
                }
            }

            // go to the next element of the array
            indNbWord++;
            // Just to stay in the array
            if (indNbWord >= sizeNbWord)
            {
                indNbWord = 0;
            }
            // reset the last element used
            nbWordByHypo[indNbWord] = 0;
        }
        private void speechRateDetection(object source, EventArgs e)
        {
            getSpeedRate();
        }
        public bool stopSpeechRateDetection()
        {
            try
            {
                this._waveIn.DataAvailable -= speechRateCallback;
                timer.Stop();
                return true;
            }
            catch (Exception) { }
            return false;
        }
        public bool startSpeechRateDetection()
        {
            try
            {
                this._waveIn.DataAvailable += speechRateCallback;
                timer.Start();
                return true;
            }
            catch (Exception) { }
            return false;
        }
        private void speechRateCallback(object sender, WaveInEventArgs e)
        {
            new Thread(() => this.speechRate(e.Buffer, e.BytesRecorded)).Start();

        }
        ///<summary>
        /// The speechRate function is called every 100ms and calculates the number of syllables using the praat way of calculating the syllables
        /// The result is stored in a variable that is initialized every 1 second
        /// </summary>
        public void speechRate(byte[] buffer, int bytesRecorded)
        {

            int silencedb = -25; // Silence threshold
            int mindip = 4; // Minimum dip between 2 peaks
            float[] intensity = this._fft.getFFT(buffer, bytesRecorded); // List of the intensities
            int nbIntensities = intensity.Length; // Number of the intensities in the buffer
            int minint = 0; // Noise floor
            int maxint = 0; // Max noise
            int max99int = 0; // Maximum noise without influence of non - speech sound bursts
            int threshold, threshold2, threshold3; // Intensity Threshold
            int nbSilences = 0; // Number of silent parts in the buffer
            int nbVoices = 0; // Number of voiced parts in the buffer
            float timeByIntensity = (float)1.0 / nbIntensities; // Time between every intensity
            float timestampSilence = 0, timestampVoice = 0; // Temporary timestamp to get the positon of the voiced or silent part in time
            bool counted = false; // Boolean used to count the different silent andd voiced parts
            int[] speaking = new int[nbIntensities]; //Array filled with 1s and 0s, 1 = speaking , 0 = not speaking
            int nbPeaks = 0; // Number of peaks in the buffer
            int[] peaks = new int[nbIntensities]; //array filled with 1s and 0s, 1 = it's a peak, 0 = it's not a peak
            int validNbPeaks = 0; // Valid number of peaks that are considered as a syllable
            //let's find the noise floor
            for (int i = 0; i < nbIntensities - 1; i++)
            {
                if (intensity[i] < intensity[minint])
                {
                    minint = i;
                }
            }
            //let's find the max noise
            for (int i = 0; i < nbIntensities - 1; i++)
            {
                if (intensity[i] >= intensity[maxint])
                {
                    maxint = i;
                }
            }
            // let's find the 0.99 quantile to get the maximumwithout influence of of non-speech sound bursts
            float[] intensity2 = new float[nbIntensities];
            float[] sortedIntensity = new float[nbIntensities];
            for (int i = 0; i < nbIntensities; i++)
            {
                intensity2[i] = (float)(20 * Math.Log10(Math.Abs(intensity[i]) / 0.000001));

            }
            for (int i = 0; i < nbIntensities; i++)
            {
                sortedIntensity[i] = intensity2[i];
            }
            bubbleSort(sortedIntensity);
            // We estimate the intensity threshold
            max99int = (int)0.99 * (sortedIntensity.Length + 1);
            threshold = (int)((sortedIntensity[max99int] / 100000) + silencedb);
            threshold2 = (int)(intensity2[maxint] - sortedIntensity[sortedIntensity.Length - max99int - 1]);
            threshold3 = silencedb - threshold2;
            if (threshold < (int)intensity2[minint])
            {
                threshold = (int)intensity2[minint];
            }
            // Estimate where are the voiced parts and the silence parts in the buffer
            for (int i = 0; i < nbIntensities; i++)
            {
                if (intensity2[i] < threshold3)
                {
                    speaking[i] = 0;
                    timestampSilence += timeByIntensity;
                    timestampVoice = 0;
                    if (timestampSilence >= 0.3)
                    {
                        if (!counted)
                        {
                            counted = true;
                            nbSilences++;
                        }
                    }
                    else { counted = false; }
                }
                else
                {
                    speaking[i] = 1;
                    timestampSilence = 0;
                    timestampVoice += timeByIntensity;
                    if (timestampVoice >= 0.1)
                    {
                        if (!counted)
                        {
                            counted = true;
                            nbVoices++;
                        }
                    }
                    else { counted = false; }
                }
            }
            // Initialize the array
            peaks = initTable(peaks);
            // Estimate the position of the peaks
            for (int i = 0; i < nbIntensities; i++)
            {
                if (i == 0 && intensity2[i] > intensity2[i + 1])
                {
                    nbPeaks++;
                    peaks[i] = 1;
                }
                else if (i == nbIntensities - 1 && intensity2[i] > intensity2[i - 1])
                {
                    nbPeaks++;
                    peaks[i] = 1;
                }
                else if (i > 0 && i < (nbIntensities - 1))
                {
                    if (intensity2[i] > intensity2[i - 1] && intensity2[i] > intensity2[i + 1])
                    {
                        nbPeaks++;
                        peaks[i] = 1;
                    }
                }

            }
            this.numberOfPeaks += nbPeaks;
            // Choose the valid peaks to be considered as syllables
            float[] valueOfPeaks = new float[nbPeaks]; // Array filled with the intensity value of the peaks
            float[] timeOfPeaks = new float[nbPeaks]; // Array filled with the time where the intensity was picked up for each peak
            int[] idOfPeaks = new int[nbPeaks];
            for (int i = 0, j = 0; i < nbPeaks; i++, j++)
            {
                while (peaks[j] != 1)
                {
                    j++;
                }
                valueOfPeaks[i] = intensity2[j];
                idOfPeaks[i] = j;
                timeOfPeaks[i] = j * timeByIntensity;
            }
            for (int i = 0; i < nbPeaks - 1; i++)
            {
                int actualID = idOfPeaks[i];
                int nextID = idOfPeaks[i + 1];
                float dip = MinValue(intensity, actualID, nextID);
                float diffDip = Math.Abs(valueOfPeaks[i] - dip);
                if (diffDip > mindip && speaking[actualID] == 1 && valueOfPeaks[i] > 89)
                {
                    validNbPeaks++;
                }

            }
            this.nbSyllables += validNbPeaks;
            // ValidNbPeaks is the number of syllables during 
            // the timestamp which is 1 second
        }
        /// <summary>
        /// The getSpeedRate takes the incremented variable of the speechRate and chooses which case it is : Not Speaking/Low/MidLow/MidHigh/High speech rate
        /// </summary>
        public void getSpeedRate()
        {
            Console.Out.WriteLine("\nNumber of syllables in 1 second : " + nbSyllables);
            Console.Out.WriteLine("Number of peaks in general in 1 second : " + numberOfPeaks);
            int level = 0;
            if (this.nbSyllables <= 2)
            {
                level = 0;
            }
            else if (this.nbSyllables <= 4)
            {
                level = 1;
            }
            else if (this.nbSyllables <= 6)
            {
                level = 2;
            }
            else if (this.nbSyllables <= 8)
            {
                level = 3;
            }
            else
            {
                level = 4;
            }
            speedEvent(this, new ValuedFeedback("", true, level));
            if (levelOfSpeech.Count == 0)
            {
                levelOfSpeech.Add(0, level);
            }
            else
            {
                levelOfSpeech.Add(levelOfSpeech.Count * 1000, level);
            }
            if (syllablesInTime.Count == 0)
            {
                syllablesInTime.Add(0, this.nbSyllables);
            }
            else
            {
                syllablesInTime.Add(syllablesInTime.Count * 1000, this.nbSyllables);
            }
            this.nbSyllables = 0;
            this.numberOfPeaks = 0;
        }
        public int[] initTable(int[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++) { buffer[i] = 0; }
            return buffer;
        }
        public float MinValue(float[] buffer, int l, int h)
        {
            float min = 100;
            for (int i = l; i <= h; i++)
            {
                if (buffer[i] < min) { min = buffer[i]; }
            }
            return min;
        }
        public int findIDofValue(float[] buf, float x)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                if (buf[i] == x) { return i; }
            }
            return -1;
        }
        public void swap(float[] buf, int x, int y)
        {
            float temp;
            temp = buf[x];
            buf[x] = buf[y];
            buf[y] = temp;
        }
        public void bubbleSort(float[] buf)
        {
            for (int i = buf.Length - 1; i > 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (buf[j] - buf[j + 1] < 0)
                    {
                        swap(buf, j, j + 1);
                    }
                }
            }
        }
        public int averageSpeechRateInSession(List<int> levels)
        {
            int result = 0;
            for (int i = 0; i < levels.Count; i++)
            {
                result += levels.ElementAt(i);
            }
            if (result != 0)
            {
                result = result / levels.Count;
            }
            return result;
        }
        /// <summary>
        /// Called when a word has been recognized.
        /// Used to count the number of word.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Information about the speech heard.</param>
        private void SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            // Count the number of hypothesis
            // which is equal to the number of word says
            // With a small margin of error
            nbWordByHypo[indNbWord]++;
        }

        /// <summary>
        /// Used to get the recognizer of the Kinect.
        /// This is necessary for the speed detection.
        /// </summary>
        /// <returns>Returns the RecognizerInfo of the Kinect</returns>
        private static RecognizerInfo GetKinectRecognizer(String language)
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (recognizer.Culture.Name == language)
                    return recognizer;
            }

            return null;
        }

        /// <summary>
        /// Load a new grammar for the speech recognizer from a file.
        /// Necessary for the speech recognition.
        /// </summary>
        /// <param name="path">Path of the file.</param>
        /// <returns>True if the grammar loaded well.</returns>
        public bool loadGammarPath(String path)
        {
            StreamReader file = new StreamReader(path);
            if (file == null)
            {
                throw new Exception("File " + path + " not found");
            }
            string line = "";
            if (null != (line = file.ReadToEnd()))
                return loadGrammar(line);
            return false;
        }

        /// <summary>
        /// Load a new grammar for the speech recognizer from a file.
        /// Necessary for the speech recognition.
        /// </summary>
        /// <param name="speech">Speech contained in a string.</param>
        /// <returns></returns>
        public bool loadGrammar(String speech)
        {
            try
            {
                var words = new Choices();

                string[] split = speech.Split(new Char[] { '.', '\n', '?', '!', ',' });


                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i] != "")
                        words.Add(split[i]);
                }

                RecognizerInfo ri = GetKinectRecognizer(_PossibleLanguage.ElementAt(_SelectedLanguage));

                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(words);

                var g = new Grammar(gb);
                speechEngine.UnloadAllGrammars();
                speechEngine.LoadGrammarAsync(g);
                // Activate the tic recognition
                speechEngine.SpeechRecognized += SpeechRecognizedTics;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Change the listening device (microphone) if you don't want to use the Kinect microphone.
        /// </summary>
        /// <param name="n">Number of the device.</param>
        public void setDeviceNumber(int n)
        {
            this._deviceNumber = n;
        }

        /// <summary>
        /// Start the audio recording
        /// </summary>
        /// <param name="path"> Path of the file </param>
        /// <param name="name"> Name of the file </param>
        /// <remarks>Modified by Amirali Ghazi
        /// I put the recording in parallel so it doesn't overload the main thread
        /// </remarks>
        public void startRecording(String path, String name)
        {
            if (!this.isRecording)
            {
                this.isRecording = true;
                dicWpm.Clear();
                Task.Factory.StartNew(() =>
                {
                    recorder = new AudioRecorder(path, name, _waveIn);
                    recorder.startRecording();

                });
            }
        }

        /// <summary>
        /// Stop the audio recording
        /// </summary>
        public void stopRecording()
        {
            this.isRecording = false;
            Task.Factory.StartNew(() =>
            {
                if (recorder != null)
                    recorder.stopRecording();
            });
        }

        public void FFTDisplay(bool value)
        {
            if (value)
                this._waveIn.DataAvailable += FFTCallback;
            else
                this._waveIn.DataAvailable -= FFTCallback;
        }

        public void setToolViewModel(ViewModel.SideToolsViewModel stvm)
        {
            this.sideToolViewModel = stvm;
        }

        /// <summary>
        /// Reset the tics and the keywords
        /// </summary>
        public void resetTmpCount()
        {
            for (int i = 0; i < tics.Count; i++)
            {
                tics[tics.ElementAt(i).Key] = 0;
            }
            for (int i = 0; i < keyword.Count; i++)
            {
                keyword[keyword.ElementAt(i).Key] = 0;
            }
        }

        /// <summary>
        /// return an event with the last sentence said
        /// </summary>
        public void getTextToShow(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            textToShowEvent(this, new Feedback(e.Result.Text));
        }

        /// <summary>
        /// return an event with the last sentence said
        /// </summary>
        public void getTextToShow(object sender, SpeechRecognizedEventArgs e)
        {
            textToShowEvent(this, new Feedback(e.Result.Text));

        }

        /// <summary>
        /// allowed to show the text on the screen
        /// </summary>
        public bool showText()
        {
            speechEngine.SpeechRecognitionRejected += getTextToShow;
            speechEngine.SpeechRecognized += getTextToShow;
            return true;
        }

        /// <summary>
        /// Stop to show the text by ending the event.
        /// </summary>
        /// <returns></returns>
        public bool hideText()
        {
            speechEngine.SpeechRecognitionRejected -= getTextToShow;
            speechEngine.SpeechRecognized -= getTextToShow;
            return true;
        }
        #endregion

        #region statistics Methods
        /// <summary>
        /// function to obtain the count of the arms crossed
        /// </summary>
        /// <returns>the graph</returns>
        public static List<IGraph> getVoicetatistics()
        {
            List<IGraph> list = new List<IGraph>();

            var chart = new CartesianGraph();
            chart.title = "Speech rate (in syllables per second)";
            chart.subTitle = Tools.ChooseTheCorrectUnitTime();
            var chart2 = new CartesianGraph();
            chart2.title = "Speech rate (Level Of Speaking per second)";
            chart2.subTitle = Tools.ChooseTheCorrectUnitTime();
            /*
            Dictionary<int, double> dictemp = new Dictionary<int, double>();
            int max = dicWpm.Count;

            dictemp = dictemp.Concat(dicWpm).ToDictionary(s => s.Key, s => s.Value);

            for(int i=0;i<max-1;i++)
            {
                dictemp.Add(dicWpm.ElementAt(i).Key + refreshTimer/2, (dicWpm.ElementAt(i ).Value + dicWpm.ElementAt(i+1).Value) /2);
            }
            var items = from pair in dictemp orderby pair.Key ascending select pair;
            Dictionary<int, double> dicSort = new Dictionary<int, double>();
            foreach (KeyValuePair<int, double> pair in items)
            {
                dicSort.Add(pair.Key, pair.Value);
            }

            if (!Tools.addKeyValuePairSeriesToCharts(chart, new LineSeries(), "Words / minute", dicSort, "", false))
                list.Add(Tools.createEmptyGraph("No estimate of words per minute"));
            else
                list.Add(chart);*/

            if (!Tools.addKeyValuePairSeriesToCharts(chart, new LineSeries(), "Syllables per Second", syllablesInTime, "", false))
                list.Add(Tools.createEmptyGraph("No estimate of syllables per second"));
            else
                list.Add(chart);

            if (!Tools.addKeyValuePairSeriesToCharts(chart2, new LineSeries(), "Level Of Speaking per Second", levelOfSpeech, "", false))
                list.Add(Tools.createEmptyGraph("No estimate of level of speaking per second"));
            else
                list.Add(chart2);

            return list;
        }

        #endregion

        #region Teleprompter Methods

        /// <summary>
        /// Get the text for the prompter and initializes all the attributes used
        /// Start to listen the voice recognition to get the word said
        /// </summary>
        /// <returns> Retrun the text get in the file 
        /// lecturertrainer\LecturerTrainer\Prompter\Speech.txt </returns>
        public List<String> startPrompter()
        {
            String result = "";

            // Get the text in the file
            String[] text = loadTextFile(@"../../Prompter/Speech.txt");
            List<string> resultList = new List<string>();


            int start = 0;
            int stop = 0;
            string subString;

            // Transforms the text of the file in a string without double line break
            foreach (String a in text)
            {
                if (!a.Equals("\n") && !a.Equals("\r") && !a.Equals(""))
                {
                    result += a + " ";
                }
            }

            // Transforms the text in a list that could directly be displayed
            while (start < result.Length)
            {
                // For the last part of the text. Used to not go outside of the array
                if (start + 25 >= result.Length)
                {
                    resultList.Add(result.Substring(start, result.Length - start));
                    start = result.Length;
                }
                else
                {
                    subString = result.Substring(start, 25);

                    // Used to display only full word.
                    stop = subString.LastIndexOf(" ");
                    resultList.Add(result.Substring(start, stop));
                    start += stop + 1;
                }
            }

            linePrompter = 0;
            textTeleprompter = resultList;

            nbWordLinePrompter[0] = 0;
            nbWordLinePrompter[1] = 0;
            nbWordLinePrompter[2] = 0;
            nbWordLinePrompter[3] = 0;
            nbWordLinePrompter[4] = 0;

            speechEngine.SpeechRecognized += nextLinePrompteur;
            speechEngine.SpeechRecognitionRejected += nextLinePrompteur;
            return resultList;
        }

        /// <summary>
        /// Stop to get the words said for the prompter.
        /// </summary>
        public void stopPrompteur()
        {
            speechEngine.SpeechRecognized -= nextLinePrompteur;
            speechEngine.SpeechRecognitionRejected -= nextLinePrompteur;
        }

        /// <summary>
        /// Used to go to the next line in the prompter
        /// </summary>
        /// <param name="textSaid"> The last sentence recognized </param>
        public void nextLinePrompteur(String textSaid)
        {
            // Split the text in word
            textSaid = Regex.Replace(textSaid, "[^a-zA-Z0-9]+", " ");
            List<String> test = textSaid.Split(' ').ToList();

            // Used to get the number of words in the four first sentences displayed by the prompter
            int[] nbWord = { -1, -1, -1, -1, -1 };
            // Saad's update : syllables instead of word
            //int[] nbSyllables = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            String tempString;
            for (int i = 0; i < 5; i++)
            {
                if (linePrompter + i < textTeleprompter.Count)
                {
                    tempString = Regex.Replace(textTeleprompter[linePrompter + i], "[^a-zA-Z0-9]+", " ");
                    nbWord[i] = Regex.Replace(tempString, " $", "").Split(' ').Length;
                }

            }

            // For each words said, see if it's in the prompter text and in which line
            // nbWord initializes at -1. If it's > 0, mean that linePrompter + i < textTeleprompter.Count not go out of the prompter text
            foreach (String i in test)
            {
                if (nbWord[0] > 0 &&
                    textTeleprompter[linePrompter].ToUpper().IndexOf(i.ToUpper()) >= 0)
                {
                    nbWordLinePrompter[0]++;
                }

                else if (nbWord[1] > 0 &&
                    textTeleprompter[linePrompter + 1].ToUpper().IndexOf(i.ToUpper()) >= 0)
                {
                    nbWordLinePrompter[1]++;
                }
                else if (nbWord[2] > 0 &&
                    textTeleprompter[linePrompter + 2].ToUpper().IndexOf(i.ToUpper()) >= 0)
                {
                    nbWordLinePrompter[2]++;
                }
                else if (nbWord[3] > 0 &&
                    textTeleprompter[linePrompter + 3].ToUpper().IndexOf(i.ToUpper()) >= 0)
                {
                    nbWordLinePrompter[3]++;
                }
                else if (nbWord[4] > 0 &&
                    textTeleprompter[linePrompter + 4].ToUpper().IndexOf(i.ToUpper()) >= 0)
                {
                    nbWordLinePrompter[4]++;
                }
            }

            if ((nbWordLinePrompter[4] * 100) / nbWord[4] > 50)
            {
                linePrompter += 5;
                nbWordLinePrompter[0] = 0;
                nbWordLinePrompter[1] = 0;
                nbWordLinePrompter[2] = 0;
                nbWordLinePrompter[3] = 0;
                nbWordLinePrompter[4] = 0;

                linePrompterEvent(this, new IntFeedback(linePrompter));// send for DrawingSheetAvatarViewModel to change line of prompter
            }
            else if ((nbWordLinePrompter[3] * 100) / nbWord[3] > 50)
            {
                linePrompter += 4;
                nbWordLinePrompter[0] = nbWordLinePrompter[4];
                nbWordLinePrompter[1] = 0;
                nbWordLinePrompter[2] = 0;
                nbWordLinePrompter[3] = 0;
                nbWordLinePrompter[4] = 0;

                linePrompterEvent(this, new IntFeedback(linePrompter));// send for DrawingSheetAvatarViewModel to change line of prompter
            }
            else if ((nbWordLinePrompter[2] * 100) / nbWord[2] > 50)
            {
                linePrompter += 3;
                nbWordLinePrompter[0] = nbWordLinePrompter[3];
                nbWordLinePrompter[1] = nbWordLinePrompter[4];
                nbWordLinePrompter[2] = 0;
                nbWordLinePrompter[3] = 0;
                nbWordLinePrompter[4] = 0;

                linePrompterEvent(this, new IntFeedback(linePrompter));// send for DrawingSheetAvatarViewModel to change line of prompter
            }
            else if ((nbWordLinePrompter[1] * 100) / nbWord[1] > 50)
            {
                linePrompter++;
                nbWordLinePrompter[0] = nbWordLinePrompter[2];
                nbWordLinePrompter[1] = nbWordLinePrompter[3];
                nbWordLinePrompter[2] = nbWordLinePrompter[4];
                nbWordLinePrompter[3] = 0;
                nbWordLinePrompter[4] = 0;

                linePrompterEvent(this, new IntFeedback(linePrompter));// send for DrawingSheetAvatarViewModel to change line of prompter
            }
            // If more of 50 % of the words said are in a the first line display the next one
            else if ((nbWordLinePrompter[0] * 100) / nbWord[0] > 50)
            {
                linePrompter++;
                nbWordLinePrompter[0] = nbWordLinePrompter[1];
                nbWordLinePrompter[1] = nbWordLinePrompter[2];
                nbWordLinePrompter[2] = nbWordLinePrompter[3];
                nbWordLinePrompter[3] = nbWordLinePrompter[4];
                nbWordLinePrompter[4] = 0;

                linePrompterEvent(this, new IntFeedback(linePrompter));// send for DrawingSheetAvatarViewModel to change line of prompter
            }
        }

        /// <summary>
        /// Get the text rejected when trying to make voice recognition
        /// And give it to the global function
        /// </summary>
        /// <param name="sender"> Get the sender of the event </param>
        /// <param name="e"> Used to get the text rejected by the voice recognition</param>
        public void nextLinePrompteur(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            nextLinePrompteur(e.Result.Text);
        }

        /// <summary>
        /// Get the text recognize and give it to the global function
        /// </summary>
        /// <param name="sender"> Get the sender of the event </param>
        /// <param name="e"> Text recognized by the voice recognition </param>
        public void nextLinePrompteur(object sender, SpeechRecognizedEventArgs e)
        {
            nextLinePrompteur(e.Result.Text);
        }
        #endregion
    }
}
