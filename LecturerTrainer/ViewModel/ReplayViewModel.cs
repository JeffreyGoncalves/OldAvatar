using LecturerTrainer.Model;
using LecturerTrainer.Model.AudioAnalysis;
using LecturerTrainer.Model.BodyAnalysis;
using LecturerTrainer.Model.EmotionRecognizer;
using LecturerTrainer.Model.EventsAnalysis;
using LecturerTrainer.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Threading;
using Microsoft.Kinect;
using System.Windows.Threading;

namespace LecturerTrainer.ViewModel
{
    class ReplayViewModel : ViewModelBase
    {
        #region fields
        /// <summary>
        /// INstance of the class (singleton)
        /// </summary>
        private static ReplayViewModel instance = null;

        /// <summary>
        /// The directory in which the performance has been saved
        /// Modified by Baptiste Germond
        /// </summary>
        public String filePathVideoAvatar;
        public String filePathAvatar;
        public String filePathVideoStream;
        public String filePath;

        /// <summary>
        /// State of the replay mode
        /// </summary>
        // Indicates if an audio source is detected
        private bool audioSource = false;
        // Indicates if the sound will be played or not
        private bool mute = true;
        // Indicates if the video is stopped
        private bool stopped = true;
        // Indicates if the video is paused
        private bool paused = true;
        // Indicates if the video is played
        private static bool played = false;
        // Indicates if the statistics source is detected
        private string statisticsPath = "";

        private ReplayAvatar skeletonScrolling;


        /// <summary>
        /// Time elapsed in the video, textual version
        /// </summary>
        private String elapsedTime = "00:00:00";

        /// <summary>
        /// Queue containing all the feedbacks
        /// </summary>
        private Queue<ServerFeedback> feedbacksQueue;

        /// <summary>
        /// Queue saved to replay the performance many times
        /// </summary>
        private Queue<ServerFeedback> savedFeedbacksQueue;

        public static List<List<String>> listlistString;

        public List<String> currentFeedbackList;
        public int currentListNumber = 0;

        /// <summary>
        /// Static array storing the different speed ratios allowed
        /// And below, the index selected
        /// </summary>
        private static double[] speedRatios = { 0.25, 0.5, 1, 1.5, 2 };
        private int speedRatioIndex = 2;

        /// <summary>
        /// Number of frame to display per seconds in a normal speed
        /// </summary>
        public static double normalSpeed = 30;

        private double timeRecord = 0;

        public double TimeRecord
        {
            get
            {
                return timeRecord;
            }
            set
            {
                timeRecord = value;
            }
        }

        public static bool isReplaying = false;

        private ICommand resultsCommand;
        private ChoiceResultView resultsPerformance = null;

        private static int currentAvatarNumber = 0;
        public static int CurrentAvatarNumber
        {
            get
            {
                return currentAvatarNumber;
            }
            set
            {
                currentAvatarNumber = value;
            }
        }

        private static Tuple<int, Skeleton> currentAvatar;
        public static Tuple<int, Skeleton> CurrentAvatar
        {
            get
            {
                return currentAvatar;
            }
            set
            {
                currentAvatar = value;
            }
        }
        
        #endregion

        #region commands
        // Commands linked to view controls
        private ICommand playPerformanceCommand;
        private ICommand performanceSoundCommand;
        private ICommand speedUpPerformanceCommand;
        private ICommand slowDownPerformanceCommand;
        private ICommand pausePerformanceCommand;
        private ICommand stopPerformanceCommand;
        private ICommand videoAvatarDisplayCommand;
        private ICommand avatarDisplayCommand;
        private ICommand streamDisplayCommand;
        private ICommand quitCommand;
        private ICommand otherReplayCommand;
        public ICommand PlayPerformanceCommand { get { return playPerformanceCommand; } }
        public ICommand PerformanceSoundCommand { get { return performanceSoundCommand; } }
        public ICommand SpeedUpPerformanceCommand { get { return speedUpPerformanceCommand; } }
        public ICommand SlowDownPerformanceCommand { get { return slowDownPerformanceCommand; } }
        public ICommand PausePerformanceCommand { get { return pausePerformanceCommand; } }
        public ICommand StopPerformanceCommand { get { return stopPerformanceCommand; } }
        public ICommand VideoAvatarDisplayCommand { get { return videoAvatarDisplayCommand; } }
        public ICommand AvatarDisplayCommand { get { return avatarDisplayCommand; } }
        public ICommand StreamDisplayCommand { get { return streamDisplayCommand; } }
        public ICommand QuitCommand { get { return quitCommand; } }
        public ICommand OtherReplayCommand { get { return otherReplayCommand; } }

        #endregion

        #region properties
        /// <summary>
        /// Elapsed time from the video starting
        /// </summary>
        public String ElapsedTime
        {
            get { return elapsedTime; }
            set
            {
                try
                {
                    elapsedTime = value;
                    ReplayView.Get().ReplayTimeLabel.Text = value;
                    OnPropertyChanged("ElapsedTime");
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.ToString());
                }
            }
        }

        public ICommand ResultsCommand
        {
            get
            {
                if (this.resultsCommand == null)
                    this.resultsCommand = new RelayCommand(() => this.displayResults(), () => CanDisplayResults());

                return this.resultsCommand;
            }
        }
        #endregion

        #region constructor and get()
        /// <summary>
        /// Constructor
        /// Modified by Baptiste Germond
        /// </summary>
        /// <param name="file"></param>
        private ReplayViewModel(String file)
        {
            filePath = file;
            instance = this;
            performanceSoundCommand = new RelayCommand(performanceSound);
            playPerformanceCommand = new RelayCommand(Play);
            speedUpPerformanceCommand = new RelayCommand(SpeedUp);
            slowDownPerformanceCommand = new RelayCommand(SlowDown);
            pausePerformanceCommand = new RelayCommand(Pause);
            stopPerformanceCommand = new RelayCommand(Stop);
            //videoAvatarDisplayCommand = new RelayCommand(videoAvatarDisplay);
            avatarDisplayCommand = new RelayCommand(avatarDisplay);
            streamDisplayCommand = new RelayCommand(videoStreamDisplay);
            quitCommand = new RelayCommand(quit);
            otherReplayCommand = new RelayCommand(otherReplay);
            Tools.initStopWatch();
            timeRecord = 0;
    
            DrawingSheetView.Get().ReplayVideo.MediaEnded += videoEnded;

            SideToolsViewModel.Get().disableTrackingAndTrainingTab();
            ManagePerformanceFiles();
            //ManageSpeedElements();
            Mute();
            pauseButtonCommand();

            Console.Out.WriteLine("end constr");
        }



        public static ReplayViewModel Get()
        {
            if (instance == null)
            {
                instance = new ReplayViewModel("../../bin/Debug/");
            }
            return instance;
        }

        public static void Set(String file)
        {
            instance = new ReplayViewModel(file);
        }

        /// <summary>
        /// Reset the instance of this class and the skeleton scrolling. 
        /// </summary>
        public void resetInstance()
        {
            instance = null;
        }
        #endregion

        #region methods

        #region feedback management methods

        /// <summary>
        /// Create and fill the list of feedbacks
        /// </summary>
        private void initialiseFeedbacksQueue(string fileName)
        {
            listlistString = FeedbacksInList(fileName);
            currentFeedbackList = listlistString.ElementAt(currentListNumber);
        }

        /// <summary>
        /// This method needs the feedback.txt file input
        /// And it returns a list of feedbacks of the same size of the list of skeleton
        /// </summary>
        /// <author>Alban Descottes 2018</author>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<List<String>> FeedbacksInList(String fileName)
        {
            var listFeedback = new List<List<String>>();
            string tempLine;
            // each list contains the feedbacks contain in an interval of time
            // this interval is the time during two skeleton in the ReplayAvatar.SkeletonList
            int timeDown = 0;
            int count = 0;
            int timeUp = ReplayAvatar.SkeletonList[count].Item1;
            if (File.Exists(fileName))
            {
                StreamReader file = new StreamReader(fileName);
                // we create the first sub-list and read the first line of feedback.txt
                var listTemp = new List<String>();
                tempLine = file.ReadLine();
                while (tempLine != null)
                {
                    if (tempLine.Equals(""))
                        break;
                    string[] splitedLine = tempLine.Split('@');
                    int fhf;
                    bool display;
                    Int32.TryParse(splitedLine[2], out fhf);
                    Boolean.TryParse(splitedLine[3], out display);
                    // if the feedback is in the inteval we check if it is already in the list
                    // and we read the next line of feedback.txt
                    if(fhf > timeDown && fhf <= timeUp)
                    {
                        if(!listTemp.Contains(splitedLine[0]))
                            listTemp.Add(splitedLine[0]);
                        tempLine = file.ReadLine();
                    }
                    // else if the number of list is less than the number of skeletons
                    // we change the two bounds of the interval 
                    // and we add the list in the other list and create a new one
                    else if(count < ReplayAvatar.SkeletonList.Count)
                    {
                        listFeedback.Add(listTemp);
                        timeDown = timeUp;
                        timeUp = ReplayAvatar.SkeletonList[count++].Item1;
                        Console.Out.WriteLine(" -- " + count);
                        listTemp = new List<String>();
                    }
                    // else we read the end of the feedback.txt
                    else
                    {
                        tempLine = file.ReadLine();
                    }
                }
                file.Close();
                // if the number of list is less than the number of skeleton we fill with empty lists
                if(listFeedback.Count != ReplayAvatar.SkeletonList.Count)
                {
                    int diff = ReplayAvatar.SkeletonList.Count - listFeedback.Count;
                    for (int i = 0; i < diff; i++)
                        listFeedback.Add(new List<String>());
                }
            }
            return listFeedback;
        }


        /// <summary>
        /// this method is used in the ReplayAvatar class in the DispatcherTimer 
        /// </summary>
        /// <author>Alban Descottes 2018</author>
        /// <param name="sender"></param>
        /// <param name="evt"></param>
        public void nextFeedbackList(object sender, EventArgs evt)
        {
            currentListNumber ++;
            if(currentListNumber != listlistString.Count)
            {
                currentFeedbackList = listlistString.ElementAt(currentListNumber);
            }
        }

       
        #endregion

        #region view management methods
        /// <summary>
        /// Manage the loading of the file the user choose
        /// Added by Baptiste Germond
        /// </summary>
        private void ManagePerformanceFiles()
        {
            if(File.Exists(filePath))
            {
                if (Path.GetFileName(filePath) == "avatar.avi")
                {
                    DrawingSheetView.Get().ShowReplayVideoSheet();
                    filePathVideoAvatar = filePath;
                    activate(ReplayView.Get().VideoAvatar, GeneralSideTool.Get().Avatar);
                    deactivateOther(ReplayView.Get().Stream, ReplayView.Get().Avatar);
                    DrawingSheetView.Get().ReplayVideo.Source = new Uri(filePathVideoAvatar, UriKind.Relative);
                    skeletonScrolling = null;
                    tryAddOtherSources("avatar.avi");
                    isReplaying = true;
                }
                else if (Path.GetFileName(filePath) == "stream.avi")
                {
                    DrawingSheetView.Get().ShowReplayVideoSheet();
                    filePathVideoStream = filePath;
                    activate(ReplayView.Get().Stream, GeneralSideTool.Get().Stream);
                    deactivateOther(ReplayView.Get().VideoAvatar, ReplayView.Get().Avatar);
                    DrawingSheetView.Get().ReplayVideo.Source = new Uri(filePathVideoStream, UriKind.Relative);
                    skeletonScrolling = null;
                    DrawingSheetView.Get().CanvasFeedback.Visibility = Visibility.Visible;
                    tryAddOtherSources("stream.avi");
                    isReplaying = true;
                }
                else if (Path.GetFileName(filePath) == "avatarSkeletonData.skd")
                {
                    DrawingSheetView.Get().Show3DSheet();
                    filePathAvatar = filePath;
                    activate(ReplayView.Get().Avatar, GeneralSideTool.Get().Avatar);
                    deactivateOther(ReplayView.Get().Stream, ReplayView.Get().VideoAvatar);

                    var faceData = filePath.Replace("avatarSkeletonData.skd", "faceData.xml");
                    if(File.Exists(faceData))
                    {
                        skeletonScrolling = new ReplayAvatar(filePathAvatar, faceData, this, 0);
                        tryAddOtherSources("avatarSkeletonData.skd");
                        isReplaying = true;
                    }
                    else
                    {
                        skeletonScrolling = new ReplayAvatar(filePathAvatar, this, 0);
                        tryAddOtherSources("avatarSkeletonData.skd");
                        isReplaying = true;
                    }
                }
                if (isReplaying)
                    ReplayView.Get().TitleReplay.Content = Path.GetFileName((Path.GetDirectoryName(filePath)));
            }
        }

        /// <summary>
        /// Deactivate the useless buttons
        /// </summary>
        private void deactivateOther(RadioButton replayViewRadio1, RadioButton replayViewRadio2)
        {
            replayViewRadio1.IsEnabled = false;
            replayViewRadio2.IsEnabled = false;
            replayViewRadio1.Opacity = 0.5;
            replayViewRadio2.Opacity = 0.5;
        }

        /// <summary>
        /// Activate the useful buttons
        /// Added by Baptiste Germond
        /// </summary>
        private void activate(RadioButton replayViewRadio, RadioButton generalSideToolButton)
        {
            replayViewRadio.IsChecked = true;
            replayViewRadio.Command.Execute(null);
            generalSideToolButton.IsChecked = true;
            replayViewRadio.IsEnabled = true;
            replayViewRadio.Opacity = 1;
            DrawingSheetView.Get().ReplayVideo.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Add the audio replay to the other replay
        /// Added by Baptiste Germond
        /// </summary>
        private void AddAudio(String name)
        {
                DrawingSheetView.Get().ReplayAudio.Source = new Uri(name);
                audioSource = true;
                ReplayView.Get().SoundCheckbox.IsEnabled = true;
        }
		
        /// <summary>
        /// Adding the other replay available with the replay the user chose
        /// Added by Baptiste Germond
        /// </summary>
        public void tryAddOtherSources(string fileBase)
        {
            bool audioLoad = false;
            foreach (string s in Directory.GetFiles(Path.GetDirectoryName(filePath)))
            {
                string fileName = Path.GetFileName(s);
                if (fileName != fileBase)
                {
                    if (fileName == "avatarSkeletonData.skd")
                    {
                        // the replay needs an instance of ReplayAvatar, so if the first file chose is not the .skd
                        // we have to create one
                        skeletonScrolling = new ReplayAvatar(s, this, 0);
                        addOtherVideoSources(ReplayView.Get().Avatar);
                        filePathAvatar = s;
                    }
                    else if (fileName == "avatar.avi")
                    {
                        addOtherVideoSources(ReplayView.Get().VideoAvatar);
                        filePathVideoAvatar = s;
                    }
                    else if (fileName == "stream.avi")
                    {
                        addOtherVideoSources(ReplayView.Get().Stream);
                        filePathVideoStream = s;
                    }
                    else if (fileName == "audio.wav" && !audioLoad)
                    {
                        AddAudio(s);
                        audioLoad = true;
                    }
                    else if (fileName == "feedback.txt")
                    {
                        initialiseFeedbacksQueue(s);
                    }
                    else if (fileName == "charts.xml")
                    {
                        statisticsPath = s;
                    }
                }
            }
            if (!audioLoad)
            {
                ReplayView.Get().SoundCheckbox.IsEnabled = false;
                DrawingSheetView.Get().ReplayAudio.Source = null;
                audioSource = false;
            }
        }

        /// <summary>
        /// Adding other video replay
        /// Added by Baptiste Germond
        /// </summary>
        /// <param name="replayViewRadio"></param>
        private void addOtherVideoSources(RadioButton replayViewRadio)
        {
            replayViewRadio.IsEnabled = true;
            replayViewRadio.Opacity = 1;
        }

/*
        /// <summary>
        /// Manages speed buttons and labels according to the current state
        /// Modified by Baptiste Germond
        /// </summary>
        private void ManageSpeedElements()
        {
                // If maxSpeed is reached, speeding up is not possible
                if (speedRatioIndex == speedRatios.Count() - 1)
                    ReplayView.Get().FastButton.IsEnabled = false;

                // If minSpeed is reached, slowing down is not possible
                else if (speedRatioIndex == 0)
                    ReplayView.Get().SlowButton.IsEnabled = false;

                // In other cases, speeding up and slowing down is possible
                else
                {
                    ReplayView.Get().SlowButton.IsEnabled = true;
                    ReplayView.Get().FastButton.IsEnabled = true;
                }

                if (speedRatios[speedRatioIndex]==1 && timeRecord%200 != 0)
                {
                    timeRecord -= timeRecord % 200;
                }

                // Finally, the speed can be displayed
                ReplayView.Get().SpeedLabel.Text = "Speed : " + speedRatios[speedRatioIndex];
        }
*/
        /// <summary>
        /// Detect when the video ended to change the button to stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void videoEnded(object sender, RoutedEventArgs e)
        {
            stopButtonCommand();
        }
        #endregion

        #region buttons action
        /// <summary>
        /// Execute the trainingSideToolView's stop button command
        /// </summary>
        public void stopButtonCommand()
        {
            ReplayView.Get().StopButton.IsChecked = true;
            if (ReplayView.Get().StopButton.Command != null)
                ReplayView.Get().StopButton.Command.Execute(null);
        }

        /// <summary>
        /// Execute the trainingSideToolView's pause button command
        /// </summary>
        public void pauseButtonCommand()
        {
            ReplayView.Get().PauseButton.IsChecked = true;
            if (ReplayView.Get().PauseButton.Command != null)
                ReplayView.Get().PauseButton.Command.Execute(null);
        }

        public void startButtonCommand()
        {
            ReplayView.Get().PlayButton.IsChecked = true;
            if (ReplayView.Get().PlayButton.Command != null)
                ReplayView.Get().PlayButton.Command.Execute(null);
        }

        /// <summary>
        /// Displays the skeleton view
        /// </summary>
        /*public void videoAvatarDisplay()
        {
            if (filePathVideoAvatar != null)
            {
                TimeSpan tempV = DrawingSheetView.Get().ReplayVideo.Position;
                TimeSpan tempA = DrawingSheetView.Get().ReplayAudio.Position;
                DrawingSheetView.Get().ReplayVideo.Source = new Uri(filePathVideoAvatar, UriKind.Relative);
                if (skeletonScrolling == null)
                {
                    DrawingSheetView.Get().ReplayVideo.Position = tempV;
                    DrawingSheetView.Get().ReplayAudio.Position = tempA;
                }
                else
                {
                    DrawingSheetView.Get().ReplayVideo.Position = new TimeSpan(0,0, (int)(Tools.getTimer() / 1000));
                    DrawingSheetView.Get().ReplayAudio.Position = new TimeSpan(0, 0, (int)(Tools.getTimer() / 1000));
                }
                filePath = filePathVideoAvatar;
                DrawingSheetView.Get().ShowReplayVideoSheet();
                if (skeletonScrolling != null)
                    skeletonScrolling.Stop();
                skeletonScrolling = null;
                startButtonCommand();
            }
        }*/

        /// <summary>
        /// Displays the avatar view
        /// </summary>
        public void avatarDisplay()
        {
            if (filePathAvatar != null)
            {
                filePath = filePathVideoAvatar;
                DrawingSheetView.Get().Show3DSheet();
                PlayOrStop();
            }
        }

        /// <summary>
        /// this method is required when the user change the display mode (avatar/stream)
        /// </summary>
        private void PlayOrStop()
        {
            if (!played)
            {
                startButtonCommand();
                pauseButtonCommand();
            }
            else
            {
                startButtonCommand();
            }
        }

        /// <summary>
        /// Displays the video view
        /// </summary>
        public void videoStreamDisplay()
        {
            //pauseButtonCommand();
            if (filePathVideoStream != null)
            {
                TimeSpan tempV = DrawingSheetView.Get().ReplayVideo.Position;
                TimeSpan tempA = DrawingSheetView.Get().ReplayAudio.Position;
                DrawingSheetView.Get().ReplayVideo.Source = new Uri(filePathVideoStream, UriKind.Relative);
                if (skeletonScrolling == null)
                {
                    DrawingSheetView.Get().ReplayVideo.Position = tempV;
                    DrawingSheetView.Get().ReplayAudio.Position = tempA;
                }
                else
                {
                    DrawingSheetView.Get().ReplayVideo.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch());
                    DrawingSheetView.Get().ReplayAudio.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch());
                }
                filePath = filePathVideoStream;
                DrawingSheetView.Get().ShowReplayVideoSheet();
                PlayOrStop();
            }
        }

        // These functions are invoked when the user manipulate the media player
        /// <summary>
        /// Plays the performance
        /// </summary>
        public void Play()
        {
            played = true;
            stopped = false;
            paused = false;

            if (ReplayView.Get().Avatar.IsEnabled && skeletonScrolling != null)
            {
                //Console.Out.WriteLine(" -PLA- " + Tools.getStopWatch() + " -PLA- ");
                skeletonScrolling.Start();
            }

            if (DrawingSheetView.Get().ReplayVideo.Source != null)
            {
                DrawingSheetView.Get().ReplayVideo.Play();
            }
            if (DrawingSheetView.Get().ReplayAudio.Source != null)
            {
                DrawingSheetView.Get().ReplayAudio.Play();
            }
            //ReplayView.Get().FastButton.IsEnabled = true;
            //ReplayView.Get().SlowButton.IsEnabled = true;
            ReplayView.Get().PauseButton.IsEnabled = true;
            //ManageSpeedElements();
        }

        /// <summary>
        /// Pauses the performance
        /// </summary>
        public void Pause()
        {
            paused = true;
            played = false;
            stopped = false;
            if (ReplayView.Get().Avatar.IsEnabled && skeletonScrolling != null)
            {
                //Console.Out.WriteLine(" -PAU- " + Tools.getStopWatch() + " -PAU- ");
                skeletonScrolling.Pause();
            }
            if (DrawingSheetView.Get().ReplayVideo.Source != null)
            {
                DrawingSheetView.Get().ReplayVideo.Visibility = Visibility.Visible;
                DrawingSheetView.Get().ReplayVideo.Pause();
            }
            if (DrawingSheetView.Get().ReplayAudio.Source != null)
            {
                DrawingSheetView.Get().ReplayAudio.Pause();
            }

            //ReplayView.Get().FastButton.IsEnabled = true;
            //ReplayView.Get().SlowButton.IsEnabled = true;
            ReplayView.Get().PauseButton.IsEnabled = true;
            //ManageSpeedElements();
        }

        /// <summary>
        /// Stops the performance
        /// </summary>
        public void Stop()
        {
            timeRecord = 0;
            stopped = true;
            paused = false;
            played = false;
            if (ReplayView.Get().Avatar.IsEnabled && skeletonScrolling != null)
            {
                //Console.Out.WriteLine(" -STP- " + Tools.getStopWatch() + " -STP- ");
                skeletonScrolling.Stop();
            }
            if (DrawingSheetView.Get().ReplayVideo.Source != null)
            {
                DrawingSheetView.Get().ReplayVideo.Position = new TimeSpan(0,0,0,0,5);
                DrawingSheetView.Get().ReplayVideo.Stop();
            }
            if (DrawingSheetView.Get().ReplayAudio.Source != null)
            {
                DrawingSheetView.Get().ReplayAudio.Stop();
            }
            Tools.restartStopWatch();
            currentListNumber = 0;
            //ReplayView.Get().FastButton.IsEnabled = false;
            //ReplayView.Get().SlowButton.IsEnabled = false;
            ReplayView.Get().PauseButton.IsEnabled = false;
            speedRatioIndex = 2;
            DrawingSheetView.Get().ReplayAudio.SpeedRatio = speedRatios[speedRatioIndex];
            DrawingSheetView.Get().ReplayVideo.SpeedRatio = speedRatios[speedRatioIndex];
            //if(skeletonScrolling != null)
            //    skeletonScrolling.Speed = speedRatios[speedRatioIndex];
            //ManageSpeedElements();
  
            // Icons cleaning and initialization of the feedback queue thanks to the save
            IconViewModel.get().clearAll();
            if (savedFeedbacksQueue != null)
                feedbacksQueue = new Queue<ServerFeedback>(savedFeedbacksQueue);
            if (filePath != null)
                DrawingSheetView.Get().ReplayVideo.Source = new Uri(filePath, UriKind.Relative);
        }

        /// <summary>
        /// Speeds up the performance
        /// </summary>
        public void SpeedUp()
        {
            if (speedRatioIndex < speedRatios.Count() - 1)
            {
                speedRatioIndex++;
                if (DrawingSheetView.Get().ReplayAudio.Source != null)
                    DrawingSheetView.Get().ReplayAudio.SpeedRatio = speedRatios[speedRatioIndex];
                if (DrawingSheetView.Get().ReplayVideo.Source != null)
                    DrawingSheetView.Get().ReplayVideo.SpeedRatio = speedRatios[speedRatioIndex];
                if (skeletonScrolling!= null)
                    skeletonScrolling.Speed = speedRatios[speedRatioIndex];
                //ManageSpeedElements();
            }
        }

        /// <summary>
        /// Slows down the performance
        /// </summary>
        public void SlowDown()
        {
            if (speedRatioIndex > 0)
            {
                speedRatioIndex--;
                if (DrawingSheetView.Get().ReplayAudio.Source != null)
                    DrawingSheetView.Get().ReplayAudio.SpeedRatio = speedRatios[speedRatioIndex];
                if (DrawingSheetView.Get().ReplayVideo.Source != null)
                    DrawingSheetView.Get().ReplayVideo.SpeedRatio = speedRatios[speedRatioIndex];
                if (skeletonScrolling != null)
                    skeletonScrolling.Speed = speedRatios[speedRatioIndex];
                //ManageSpeedElements();
            }
        }

        public void performanceSound()
        {
            if (instance != null)
            {
                if (ReplayView.Get().SoundCheckbox.IsChecked.Value)
                    instance.Unmute();
                else
                    instance.Mute();
            }
        }

        /// <summary>
        /// Unmutes the performance
        /// </summary>
        public void Unmute()
        {
            mute = false;
            DrawingSheetView.Get().ReplayAudio.IsMuted = false;
        }


        /// <summary>
        /// mutes the performance
        /// </summary>
        public void Mute()
        {
            mute = true;
            if (audioSource)
                DrawingSheetView.Get().ReplayAudio.IsMuted = true;
        }

        ///<summary>
        ///Releasing the media elements when closing the replay window
        ///Switch to recording mode
        ///Written by Baptiste Germond
        ///</summary>
        private void quit()
        {
            isReplaying = false;
            statisticsPath = "";
            if (skeletonScrolling != null)
            {
                skeletonScrolling.Stop();
                skeletonScrolling = null;
            }
            (TrainingSideTool.Get().FindResource("StopReplayButtonAction") as Storyboard).Begin();
            DrawingSheetView.Get().ReplayVideo.Close();
            DrawingSheetView.Get().ReplayVideo.Source = null;
            DrawingSheetView.Get().ReplayAudio.Close();
            DrawingSheetView.Get().ReplayAudio.Source = null;
            SideToolsViewModel.Get().enableTrackingAndTrainingTab();
            TrainingSideToolViewModel.Get().recordingMode();
            DrawingSheetAvatarViewModel.Get().normalMode();
        }

        /// <summary>
        /// Command linked to the button to let the user chose an other replay in the folders
        /// </summary>
        private void otherReplay()
        {
            TrainingSideToolViewModel.Get().ChoosePerfToReplay();
        }

        private void displayResults()
        {
            resultsPerformance = new ChoiceResultView();
            ((ChoiceResultViewModel)resultsPerformance.DataContext).enableSomeCheckBox(Path.GetDirectoryName(statisticsPath));
            ((ChoiceResultViewModel)resultsPerformance.DataContext).isLoad = true;
            resultsPerformance.Show();
        }

        private bool CanDisplayResults()
        {
           if (statisticsPath!= "")
            {
                return true;
            }
            return false;
        }
        #endregion

        #endregion
    }
}

#region server feedback class
/// <summary>
/// Class only used for the replay. It was implemented in the cloud version.
/// </summary>
public class ServerFeedback
{
    public ServerFeedback(string feedbackMessage, string eventName, int feedbackHappeningFrame, bool display)
    {
        this.feedbackMessage = feedbackMessage;
        this.eventName = eventName;
        this.feedbackHappeningFrame = feedbackHappeningFrame;
        this.display = display;
    }

    public ServerFeedback(string feedbackMsg)
    {
        
        switch (feedbackMsg)
        {
            case "Arms Crossed":
                this.eventName = "armsCrossedEvent";
                break;
            case "Too agitated!":
                this.eventName = "agitationEvent";
                break;
            case "Hands are joined":
                this.eventName = "handsJoinedEvent";
                break;
            /*case "tooFastEvent": <- we need to do the opposite
                this.feedbackMessage = feedbackMessage;
                break;
            case "speedEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "boringEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "reflexEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "FFTEvent":
                break;
            case "gestureEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "postureEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "emotionEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "lookEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "mouthEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "mouth2Event":
                this.feedbackMessage = feedbackMessage;
                break;
            case "pupilREvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "handsRaisedEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "ticEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "armsWideEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "enthusiasmEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "keyWordEvent":
                this.feedbackMessage = feedbackMessage;
                break;
            case "epicnessEvent":
                this.feedbackMessage = feedbackMessage;
                break;*/
            default:
                break;
        }
        this.feedbackMessage = feedbackMsg;
        this.feedbackHappeningFrame = 0;
        this.display = true;
    }

    public string feedbackMessage { get; set; }
    public string eventName { get; set; }
    public int feedbackHappeningFrame { get; set; }
    public bool display { get; set; }
}
#endregion
