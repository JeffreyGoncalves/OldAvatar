﻿using LecturerTrainer.Model;
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
using LecturerTrainer.Model.Exceptions;

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
		public String faceData;
		public String voiceData;

        /// <summary>
        /// the forder path is needed for the export of the avatar video
        /// </summary>
        public String folderPath;

        /// <summary>
        /// State of the replay mode
        /// </summary>
        // Indicates if an audio source is detected
        private bool audioSource = false;
        // Indicates if the sound will be played or not
        private bool mute = true;
        // Indicates if the video is played
        public static bool played = false;
        // Indicates if the statistics source is detected
        private string statisticsPath = "";

        private ReplayAvatar skeletonScrolling;

        public ReplayAvatar SkeletonScrolling
        {
            get
            {
                return skeletonScrolling;
            }
        }

        /// <summary>
        /// those int are used for the offset created with the slider
        /// </summary>
        public static int timeEnd;
        public static int localOffset = 0;
        public static int initTime = 0;

        /// <summary>
        /// it's for raising a exception if the skd file is not read
        /// </summary>
        private static bool skdRead = false;

        /// <summary>
        /// when the user selects a replay, the program keeps in memory if the facetracking and the speedrate are activated 
        /// </summary>
        private bool faceTrack = KinectDevice.faceTracking;
        public bool speedRateActive;

        /// <summary>
        /// Time elapsed in the video, textual version
        /// </summary>
        private String elapsedTime = "00:00:00";

        /// <summary>
        /// This list is as long as the list of skeletons
        /// each List<String> contains all the feedbacks raised during the corresponding avatar
        /// </summary>
        public static List<List<String>> ListFeedbacks;

        /// <summary>
        /// it's the current list corresponding of the current avatar displayed for the replay
        /// </summary>
        public List<String> currentFeedbackList;

        /// <summary>
        /// it's for reset the replay when the user press stop or if the replay ends
        /// </summary>
        public int currentListNumber = 0;

        /// <summary>
        /// Number of frame to display per seconds in a normal speed
        /// </summary>
        public static double normalSpeed = 30;

        /// <summary>
        /// true if it's replating
        /// </summary>
        public static bool isReplaying = false;

        #endregion

        #region commands
        // Commands linked to view controls
        private ICommand playPerformanceCommand;
        private ICommand performanceSoundCommand;
        private ICommand pausePerformanceCommand;
        private ICommand stopPerformanceCommand;
        private ICommand videoAvatarDisplayCommand;
        private ICommand avatarDisplayCommand;
        private ICommand streamDisplayCommand;
        private ICommand quitCommand;
        private ICommand otherReplayCommand;
        public ICommand PlayPerformanceCommand { get { return playPerformanceCommand; } }
        public ICommand PerformanceSoundCommand { get { return performanceSoundCommand; } }
        public ICommand PausePerformanceCommand { get { return pausePerformanceCommand; } }
        public ICommand StopPerformanceCommand { get { return stopPerformanceCommand; } }
        public ICommand VideoAvatarDisplayCommand { get { return videoAvatarDisplayCommand; } }
        public ICommand AvatarDisplayCommand { get { return avatarDisplayCommand; } }
        public ICommand StreamDisplayCommand { get { return streamDisplayCommand; } }
        public ICommand QuitCommand { get { return quitCommand; } }
        public ICommand OtherReplayCommand { get { return otherReplayCommand; } }

        #endregion

        #region export and results commands
        private ICommand resultsCommand;
        private ChoiceResultView resultsPerformance = null;

        public ICommand ResultsCommand
        {
            get
            {
                if (this.resultsCommand == null)
                    this.resultsCommand = new RelayCommand(() => this.displayResults(), () => CanDisplayResults());

                return this.resultsCommand;
            }
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
            if (statisticsPath != "")
            {
                return true;
            }
            return false;
        }

        private ICommand exportAvatarVideoCommand;
        private ExportAvatarVideoView exportAvatarVideoView = null;
        public ICommand ExportAvatarVideoCommand
        {
            get
            {
                if (this.exportAvatarVideoCommand == null)
                    this.exportAvatarVideoCommand = new RelayCommand(() => this.exportVideoAvatar());

                return this.exportAvatarVideoCommand;
            }
        }

        /// <summary>
        /// Window opened when the user wants to export 
        /// </summary>
        /// <author> Alban Descottes </author>
        private void exportVideoAvatar()
        {
            if (!File.Exists(folderPath + "avatar.avi"))
            {
                exportAvatarVideoView = new ExportAvatarVideoView();
                Application curApp = Application.Current;
                Window mainWindow = curApp.MainWindow;
                exportAvatarVideoView.Left = mainWindow.Left + (mainWindow.Width - exportAvatarVideoView.ActualWidth) / 2;
                exportAvatarVideoView.Top = mainWindow.Top + (mainWindow.Height - exportAvatarVideoView.ActualHeight) / 2;
                exportAvatarVideoView.ShowDialog();
            }
            else
            {
                new ErrorMessageBox("Export impossible", "The video of the avatar is already exported").ShowDialog();
            }
        }
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
            pausePerformanceCommand = new RelayCommand(Pause);
            stopPerformanceCommand = new RelayCommand(Stop);
            videoAvatarDisplayCommand = new RelayCommand(videoAvatarDisplay);
            avatarDisplayCommand = new RelayCommand(avatarDisplay);
            streamDisplayCommand = new RelayCommand(videoStreamDisplay);
            quitCommand = new RelayCommand(quit);
            otherReplayCommand = new RelayCommand(otherReplay);
            Tools.initStopWatch();
    
            DrawingSheetView.Get().ReplayVideo.MediaEnded += videoEnded;

            SideToolsViewModel.Get().disableTrackingAndTrainingTab();
            ManagePerformanceFiles();
            //ManageSpeedElements();
            Mute();
            pauseButtonCommand();
            if (!skdRead)
                throw new Exception(".skd file is not correct\nPlease try with one avatarSkeletonData.skd file correct");
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
            try
            {
                instance = new ReplayViewModel(file);
            }
            catch (Exception e)
            {
                throw e;
            }
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
            ListFeedbacks = FeedbacksInList(fileName);
            currentFeedbackList = ListFeedbacks.ElementAt(currentListNumber);
        }

        /// <summary>
        /// This method needs the feedback.txt file input
        /// And it returns a list of feedbacks of the same size of the list of skeleton
        /// </summary>
        /// <author> Alban Descottes 2018 </author>
        public List<List<String>> FeedbacksInList(String fileName)
        {
            var listFeedback = new List<List<String>>();
            string tempLine;
            // each list contains the feedbacks that are present in an interval of time
            // this interval is the time between two skeletons in the ReplayAvatar.SkeletonList
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
                        listTemp = new List<String>();
                    }
                    // else we read the end of the feedback.txt
                    else
                    {
                        tempLine = file.ReadLine();
                    }
                }
                file.Close();
                // if the number of lists is less than the number of skeleton, we fill with empty lists
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
        /// <author> Alban Descottes 2018 </author>
        public void nextFeedbackList(object sender, EventArgs evt)
        {
            if(ReplayAvatar.CurrentSkeletonNumber < ListFeedbacks.Count)
            {
                currentFeedbackList = ListFeedbacks.ElementAt(ReplayAvatar.CurrentSkeletonNumber);
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
                if (Path.GetFileName(filePath) == "stream.avi")
                {
                    folderPath = filePath.Remove(filePath.Length - 10);
                    DrawingSheetView.Get().ShowReplayVideoSheet();
                    filePathVideoStream = filePath;
                    activate(ReplayView.Get().Stream, GeneralSideTool.Get().Stream);
                    DrawingSheetView.Get().ReplayVideo.Source = new Uri(filePathVideoStream, UriKind.Relative);
                    skeletonScrolling = null;
                    DrawingSheetView.Get().CanvasFeedback.Visibility = Visibility.Visible;
                    tryAddOtherSources("stream.avi");
                    isReplaying = true;
                }
                else if (Path.GetFileName(filePath) == "avatarSkeletonData.skd")
                {
                    try
                    {
                        folderPath = filePath.Remove(filePath.Length - 22);
                        skdRead = true;
                        DrawingSheetView.Get().Show3DSheet();
                        filePathAvatar = filePath;
                        activate(ReplayView.Get().Avatar, GeneralSideTool.Get().Avatar);
						voiceData = filePath.Replace("avatarSkeletonData.skd", "tonePeakData.xml");
						if (!File.Exists(voiceData)) voiceData = "";
                        faceData = filePath.Replace("avatarSkeletonData.skd", "faceData.xml");
						if (!File.Exists(faceData)) faceData = "";
						skeletonScrolling = new ReplayAvatar(filePathAvatar, faceData, voiceData, this);
						tryAddOtherSources("avatarSkeletonData.skd");
                        isReplaying = true;
                    }
                    catch (XmlLoadingException)
                    {
                        throw;
                    }
                }
                if (isReplaying)
                    ReplayView.Get().TitleReplay.Content = Path.GetFileName((Path.GetDirectoryName(filePath)));
            }
        }

        /// <summary>
        /// Activate the useful buttons
        /// Added by Baptiste Germond
        /// </summary>
        private void activate(RadioButton replayViewRadio, RadioButton generalSideToolButton)
        {
            replayViewRadio.IsChecked = true;
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
                        skdRead = true;
                        skeletonScrolling = new ReplayAvatar(s, this);
                        addOtherVideoSources(ReplayView.Get().Avatar);
                        filePathAvatar = s;
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
        /// It changes the current avatar after drag the slider during the replay
        /// it changes also the offset compared to the stopwatch lanched during the replay
        /// </summary>
        /// <author > Alban Descottes 2018 </author>
        public static void changeCurrentAvatar(int newTime)
        {
            var timeDown = 0;
            var timeUp = ReplayAvatar.SkeletonList[0].Item1;
            for(int i = 0; i < ReplayAvatar.SkeletonList.Count; i++)
            {
                if (newTime <= timeUp && newTime >= timeDown)
                {
                    ReplayAvatar.CurrentSkeletonNumber = i;
                    ReplayAvatar.realTime = false;
                    localOffset = initTime - ReplayAvatar.SkeletonList[ReplayAvatar.CurrentSkeletonNumber].Item1;
                    return; 
                }
                else
                {
                    timeDown = timeUp;
                    timeUp = ReplayAvatar.SkeletonList[i + 1].Item1;
                }
            }
            return;
        }

        // REMOVE ?
        /// <summary>
        /// Displays the skeleton view
        /// </summary>
        public void videoAvatarDisplay()
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
                    DrawingSheetView.Get().ReplayVideo.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch() - ReplayAvatar.offset);
                    DrawingSheetView.Get().ReplayAudio.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch() - ReplayAvatar.offset);
                }
                filePath = filePathVideoAvatar;
                DrawingSheetView.Get().ShowReplayVideoSheet();
                PlayOrStop();
            }
        }

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
                    DrawingSheetView.Get().ReplayVideo.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch() - ReplayAvatar.offset);
                    DrawingSheetView.Get().ReplayAudio.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch() - ReplayAvatar.offset);
                }
                filePath = filePathVideoStream;
                DrawingSheetView.Get().ShowReplayVideoSheet();
                PlayOrStop();
            }
        }

        public static void PlayReplay()
        {
            played = true;
            Get().SkeletonScrolling.Start();
        }

        public static void PauseReplay()
        {
            played = false;
            Get().SkeletonScrolling.Pause();
        }


        // These functions are invoked when the user manipulate the media player
        /// <summary>
        /// Plays the performance
        /// </summary>
        public void Play()
        {
            played = true;

            if (ReplayView.Get().Avatar.IsEnabled && skeletonScrolling != null)
            {
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
            ReplayView.Get().PauseButton.IsEnabled = true;
        }

        /// <summary>
        /// Pauses the performance
        /// </summary>
        public void Pause()
        {
            played = false;
            if (ReplayView.Get().Avatar.IsEnabled && skeletonScrolling != null)
            {
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

            ReplayView.Get().PauseButton.IsEnabled = true;
        }

        /// <summary>
        /// Stops the performance
        /// </summary>
        public void Stop()
        {
            played = false;
            if (ReplayView.Get().Avatar.IsEnabled && skeletonScrolling != null)
            {
                skeletonScrolling.Stop();
            }
            if (DrawingSheetView.Get().ReplayVideo.Source != null)
            {
                DrawingSheetView.Get().ReplayVideo.Position = new TimeSpan(0,0,0,0,5);
                DrawingSheetView.Get().ReplayVideo.Stop();
            }
            if (DrawingSheetView.Get().ReplayAudio.Source != null)
            {
                DrawingSheetView.Get().ReplayAudio.Position = new TimeSpan(0, 0, 0, 0, 5);
                DrawingSheetView.Get().ReplayAudio.Stop();
            }
            Tools.resetStopWatch();
            ReplayAvatar.offset = 0;
            ReplayAvatar.realTime = true;
            currentListNumber = 0;
            ReplayView.Get().PauseButton.IsEnabled = false;
  
            // Icons cleaning and initialization of the feedback queue thanks to the save
            IconViewModel.get().clearAll();
            if (filePath != null)
                DrawingSheetView.Get().ReplayVideo.Source = new Uri(filePath, UriKind.Relative);
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
            ReplayView.Get().SoundCheckbox.IsChecked = false;
            SideToolsViewModel.Get().enableTrackingAndTrainingTab();
            TrainingSideToolViewModel.Get().recordingMode();
            DrawingSheetAvatarViewModel.Get().normalMode();

            // reactivate the sensors
            KinectDevice.sensor.SkeletonStream.Enable();
            if (faceTrack)
                KinectDevice.faceTracking = true;
            TrackingSideToolViewModel.get().SpeedRate = speedRateActive;
            
            // reactivate the audience
            if (TrainingSideToolViewModel.audienceOn)
            {
                TrainingSideToolViewModel.audienceOn = false;
                GeneralSideTool.Get().AudienceControlCheckBox.IsChecked = true;
            }
            
        }

        /// <summary>
        /// Command linked to the button to let the user chose an other replay in the folders
        /// </summary>
        private void otherReplay()
        {
            TrainingSideToolViewModel.Get().ChoosePerfToReplay();
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
