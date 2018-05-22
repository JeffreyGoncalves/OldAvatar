using LecturerTrainer.Model;
using LecturerTrainer.Model.BodyAnalysis;
using LecturerTrainer.View;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LecturerTrainer.ViewModel
{
    ///<summary>
    ///The class that handle the training with an avatar
    ///Created by Baptiste Germond
    ///</summary>
    public class TrainingWithAvatarViewModel : ViewModelBase
    {
        private static TrainingWithAvatarViewModel instance = null;

        /// <summary>
        /// List of video to show in the list.
        /// </summary>
        public List<String> videosNameList;

        private string pathFile;
        public string PathFile
        {
            get
            {
                return pathFile;
            }
            set
            {
                pathFile = value;
                skeletonsList = ReplayAvatar.LoadSkeletonsFromXML(pathFile);
                skeletonNumber = 0;
                launchTraining();
            }
        }

        public string folderPath;

        /// <summary>
        /// The dictionary contains videos name in key and path in value
        /// </summary>
        private Dictionary<String, String> videosMap;
        public Dictionary<String, String> VideosMap
        {
            get
            {
                return videosMap;
            }
        }

        private ICommand playVideoCommand;
        private ICommand pauseVideoCommand;
        private ICommand stopVideoCommand;
        private ICommand stopVideoTrainingCommand;
        private TrainingWithAvatarView twav = null;

        private bool playMode = false;
        private bool stopMode = true;
        private bool pauseMode = false;

        public bool PlayMode
        {
            get
            {
                return playMode;
            }
        }

        private int skeletonNumber = 0;

        private SortedList<int, Skeleton> skeletonsList;
        public SortedList<int, Skeleton> SkeletonList
        {
            get
            {
                return skeletonsList;
            }
        }

        public static WaveGesture _gesture = new WaveGesture();
        public static HandTraining _handgesture = new HandTraining();
        public static PowerTraining _powergesture = new PowerTraining();
        public static WelcomeTraining _welcomegesture = new WelcomeTraining();
        public static SaluteTraining _salutegesture = new SaluteTraining();

        public static string AvatarGesture;

        public static bool canBeInterrupted = false;

        private TrainingWithAvatarViewModel()
        {
            instance = this;

            folderPath = "../../Training/";
            
            twav = TrainingWithAvatarView.Get();
            initialize();

            instance = this;

            _gesture.GestureRecognized += Gesture_GestureRecognized;
            _handgesture.GestureRecognized += HandTraining_GestureRecognized;
            _powergesture.GestureRecognized += PowerTraining_GestureRecognized;
            _welcomegesture.GestureRecognized += WelcomeTraining_GestureRecognized;
            _salutegesture.GestureRecognized += SaluteTraining_GestureRecognized;

        }

        public void initialize()
        {
            initializeVideosMap();
            twav.VideosList.SelectedItem = -1;
            twav.VideosList.SelectionChanged += VideoListSelectedIndexChanged;
            pathFile = "";
        }

        public static TrainingWithAvatarViewModel Get()
        {
            if (instance == null)
                instance = new TrainingWithAvatarViewModel();
            return instance;
        }

        public List<String> VideosNameList
        {
            get { return videosNameList; }
            set
            {
                videosNameList = value;
                OnPropertyChanged("VideosNameList");
            }
        }

        public ICommand PlayVideoCommand
        {
            get
            {
                if (this.playVideoCommand == null)
                    this.playVideoCommand = new RelayCommand(() => this.PlayTraining(), () => CanPlayPauseStop());

                return this.playVideoCommand;
            }
        }

        public ICommand PauseVideoCommand
        {
            get
            {
                if (this.pauseVideoCommand == null)
                    this.pauseVideoCommand = new RelayCommand(() => this.PauseTraining(), () => CanPlayPauseStop());

                return this.pauseVideoCommand;
            }
        }

        public ICommand StopVideoCommand
        {
            get
            {
                if (this.stopVideoCommand == null)
                    this.stopVideoCommand = new RelayCommand(() => this.StopTraining(), () => CanPlayPauseStop());

                return this.stopVideoCommand;
            }
        }

        public ICommand StopVideoTrainingCommand
        {
            get
            {
                if (this.stopVideoTrainingCommand == null)
                    this.stopVideoTrainingCommand = new RelayCommand(() => this.StopVideoTraining());

                return this.stopVideoTrainingCommand;
            }
        }

       private void PlayTraining()
       {
            string curItem = twav.VideosList.SelectedItem.ToString();
            //AvatarGesture = curItem;
            pathFile = videosMap[curItem] + "/" + curItem + ".skd";
            skeletonsList = ReplayAvatar.LoadSkeletonsFromXML(pathFile);
            skeletonNumber = 0;
            launchTraining();
        }

        private void launchTraining()
        {
            canBeInterrupted = false;
            twav.PlayButton.IsChecked = true;
            instance.playMode = true;
            instance.pauseMode = false;
            instance.stopMode = false;
        }

       private void PauseTraining()
       {
            twav.PauseButton.IsChecked = true;
            instance.pauseMode = true;
            instance.playMode = false;
            instance.stopMode = false;
        }

       private void StopTraining()
       {
            twav.StopButton.IsChecked = true;
            instance.stopMode = true;
            instance.pauseMode = false;
            instance.playMode = false;
        }

        private bool CanPlayPauseStop()
        {
            if (pathFile == "")
                return false;
            else
                return true;
        }

        public void StopVideoTraining()
        {
            SideToolsViewModel.Get().enableTrackingAndTrainingTab();
            (TrainingSideTool.Get().FindResource("StopVideoTraining") as Storyboard).Begin();
            VideosNameList = new List<string>();
            DrawingSheetAvatarViewModel.Get().isTraining = false;
            skeletonsList = null;
        }
        /// <summary>
        /// Create the video dictionary with readable files
        /// Load all the movement into the software
        /// </summary>
        public void initializeVideosMap()
        {
            //Contains the names of the movements and the paths to the folders
            videosMap = new Dictionary<string, string>();

            if (Directory.Exists(folderPath))
            {
                string[] folderName = Directory.GetDirectories(folderPath);
                for (int i = 0; i < folderName.Length; i++)
                {
                    string name = Path.GetFileName(folderName[i]);
                    videosMap.Add(name, folderName[i]);
                }

                VideosNameList = videosMap.Keys.ToList();

            }
        }

        private void VideoListSelectedIndexChanged(object sender, System.EventArgs e)
        {           
            if (twav.VideosList.SelectedItem != null)
            {
                string curItem = twav.VideosList.SelectedItem.ToString();
                twav.VideoName.Text = curItem;
                AvatarGesture = curItem;
                pathFile = videosMap[curItem] + "/" + curItem + ".skd";
                skeletonsList = ReplayAvatar.LoadSkeletonsFromXML(pathFile);
                skeletonNumber = 0;
                launchTraining();
            } 
        }

        //Select the skeleton to be played by the coach's avatar
        public Skeleton chooseSkeletonToDisplay()
        {           
            Skeleton skToReturn = null;
            if (skeletonsList != null)
            {
                if (playMode)
                {
                    skToReturn = SkeletonList[skeletonNumber];
                    if (skeletonNumber != SkeletonList.Count - 1)
                        skeletonNumber++;
                    else
                    {
                        canBeInterrupted = true;
                        StopTraining();
                    }
                }
                else if (pauseMode)
                {
                    skToReturn = SkeletonList[skeletonNumber];
                }
                else if (stopMode)
                {
                    skeletonNumber = 0;
                    skToReturn = SkeletonList[skeletonNumber];
                }
            }
            return skToReturn;
        }

        //When the waving gesture is recognized, we determine what file should be played to respond to the user
        public static void Gesture_GestureRecognized(object sender, EventArgs e)
        {
            int nbFrame = ((WaveGesture)sender).FrameGesture;
            if (nbFrame >40 && nbFrame <60)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile =Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (nbFrame > 60)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Do_It_Quicker.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (nbFrame < 40)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Do_It_Slower.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }

        }

        public static void HandTraining_GestureRecognized(object sender, EventArgs e)
        {

            double distance = ((HandTraining)sender).Distance;

            if(distance < 0.01)
            { 
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Move_Closer.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }

        public static void PowerTraining_GestureRecognized(object sender, EventArgs e)
        {

            double distance = ((PowerTraining)sender).Distance;

            if (distance > 0.01)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Lean_Back.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }

        public static void WelcomeTraining_GestureRecognized(object sender, EventArgs e)
        {

            bool complete = ((WelcomeTraining)sender).Complete;
            bool slow = ((WelcomeTraining)sender).Slow;
            bool dropped = ((WelcomeTraining)sender).Dropped;

            if (complete)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (slow)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Too_Slow.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (dropped)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Elbows.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }

        public static void SaluteTraining_GestureRecognized(object sender, EventArgs e)
        {
            bool complete = ((SaluteTraining)sender).Complete;
            bool slow = ((SaluteTraining)sender).Slow;
            bool stay = ((SaluteTraining)sender).Stay;

            if (complete)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if(slow)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Too_Slow.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (stay)
            {
                string curItem = TrainingWithAvatarView.Get().VideosList.SelectedItem.ToString();
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Stay_2_seconds.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }
    }
}
