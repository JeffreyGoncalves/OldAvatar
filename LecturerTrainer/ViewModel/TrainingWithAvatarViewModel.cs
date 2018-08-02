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
        private List<VideosList> videosNameList;
        private String textFaceTrackingDisable;

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
                SkeletonList = ReplayAvatar.LoadSkeletonsFromXML(pathFile, "");
                skeletonNumber = 0;
                faceNumber = 0;
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

        private int skeletonNumber;
        private int faceNumber;
        public SortedList<int, Tuple<int, Skeleton, FaceDataWrapper>> SkeletonList { get; private set; }

        public static WaveGesture _gesture = new WaveGesture();
        public static HandTraining _handgesture = new HandTraining();
        public static PowerTraining _powergesture = new PowerTraining();
        public static WelcomeTraining _welcomegesture = new WelcomeTraining();
        public static SaluteTraining _salutegesture = new SaluteTraining();
        public static HypeTraining _hypegesture = new HypeTraining();
        public static FaceTraining _facegesture = new FaceTraining();

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
            _hypegesture.GestureRecognized += HypeTraining_GestureRecognized;
            _facegesture.GestureRecognized += FaceTraining_GestureRecognized;
        }

        public void initialize()
        {
            initializeVideosMap();
            twav.VideosList.SelectedItem = -1;
            twav.VideosList.SelectionChanged += VideoListSelectedIndexChanged;
            pathFile = "";
        }

        public void End()
        {
            _gesture.GestureRecognized -= Gesture_GestureRecognized;
            _handgesture.GestureRecognized -= HandTraining_GestureRecognized;
            _powergesture.GestureRecognized -= PowerTraining_GestureRecognized;
            _welcomegesture.GestureRecognized -= WelcomeTraining_GestureRecognized;
            _salutegesture.GestureRecognized -= SaluteTraining_GestureRecognized;
            _hypegesture.GestureRecognized -= HypeTraining_GestureRecognized;
            _facegesture.GestureRecognized -= FaceTraining_GestureRecognized;
        }

        public static TrainingWithAvatarViewModel Get()
        {
            if (instance == null)
                instance = new TrainingWithAvatarViewModel();
            return instance;
        }

        public List<VideosList> VideosNameList
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
            string curItem = ((VideosList)twav.VideosList.SelectedItem).Name;
            //AvatarGesture = curItem;
            SkeletonListUpdate(curItem);
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
            VideosNameList = new List<VideosList>();
            DrawingSheetAvatarViewModel.Get().isTraining = false;
            SkeletonList = null;
        }
        /// <summary>
        /// Create the video dictionary with readable files
        /// Load all the movement into the software
        /// </summary>
        public void initializeVideosMap()
        {
            bool enable = KinectDevice.faceTracking;

            //Contains the names of the movements and the paths to the folders
            videosMap = new Dictionary<string, string>();
            videosNameList = new List<VideosList>();

            if (Directory.Exists(folderPath))
            {
                string[] folderName = Directory.GetDirectories(folderPath);
                for (int i = 0; i < folderName.Length; i++)
                {
                    string name = Path.GetFileName(folderName[i]);
                    videosMap.Add(name, folderName[i]);

                    string tmp = folderPath + name + "/" + name + ".xml";
                    bool isSelectable = (!File.Exists(tmp) || File.Exists(tmp) && enable) ? true : false;
                    string color = isSelectable ? "WhiteSmoke" : "Gray";
                    VideosNameList.Add(new VideosList { Name = name, IsSelectable = isSelectable, Color = color });
                }

                twav.VideosList.ItemsSource = VideosNameList;
            }

            textFaceTrackingDisable = enable ? String.Empty : "To have acces to all trainings, please activate the face tracking.";
            TrainingWithAvatarView.Get().FaceTrackingDisable.Text = textFaceTrackingDisable;

            if (!enable)
            {
                TrainingWithAvatarView.Get().FaceTrackingDisable.MouseLeftButtonDown += new MouseButtonEventHandler(FaceTrackingDisable_MouseLeftButtonDown);
            }
        }

        void FaceTrackingDisable_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TrackingSideTool.Get().ActivateFaceTrackingCheckBox.IsChecked = true;
            initializeVideosMap();
        }

        private void VideoListSelectedIndexChanged(object sender, System.EventArgs e)
        {           
            if (twav.VideosList.SelectedItem != null)
            {
                string curItem = ((VideosList)twav.VideosList.SelectedItem).Name;
                twav.VideoName.Text = curItem;
                AvatarGesture = curItem;
                SkeletonListUpdate(curItem);
                launchTraining();
            } 
        }

        //Select the skeleton to be played by the coach's avatar
        public Skeleton chooseSkeletonToDisplay()
        {           
            Skeleton skToReturn = null;
            if (SkeletonList != null)
            {
                if (playMode)
                {
                    skToReturn = SkeletonList[skeletonNumber].Item2;
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
                    skToReturn = SkeletonList[skeletonNumber].Item2;
                }
                else if (stopMode)
                {
                    skeletonNumber = 0;
                    skToReturn = SkeletonList[skeletonNumber].Item2;
                }
            }
            return skToReturn;
        }

        //Select the face to be played by the coach's avatar
        public FaceDataWrapper ChooseFaceToDisplay()
        {
            FaceDataWrapper fcToReturn = new FaceDataWrapper(null, null, null);

            if(SkeletonList != null)
            {
                if (playMode)
                {
                    fcToReturn = SkeletonList[faceNumber].Item3;
                    if (faceNumber != SkeletonList.Count - 1)
                        faceNumber++;
                    else
                    {
                        canBeInterrupted = true;
                        StopTraining();
                    }
                }
                else if (pauseMode)
                {
                    fcToReturn = SkeletonList[faceNumber].Item3;
                }
                else if (stopMode)
                {
                    faceNumber = 0;
                    fcToReturn = SkeletonList[faceNumber].Item3;
                }
            }

            return fcToReturn;
        }

        /// <summary>
        /// Update the skeleton list with a new path.
        /// </summary>
        /// <param name="curItem"></param>
        private void SkeletonListUpdate(String curItem)
        {
            pathFile = videosMap[curItem] + "/" + curItem + ".skd";
            string pathFace = pathFile.Replace("skd", "xml");
            if (File.Exists(pathFace))
            {
                SkeletonList = ReplayAvatar.LoadSkeletonsFromXML(pathFile, pathFace);
            }
            else
            {
                SkeletonList = ReplayAvatar.LoadSkeletonsFromXML(pathFile, "");
            }
            skeletonNumber = 0;
            faceNumber = 0;
        }

        /// <summary>
        /// Update the skeleton list depending on the user movement
        /// </summary>
        /// <param name="curItem"></param>
        private void SkeletonListUpdateInter(String curItem)
        {
            string pathFace = pathFile.Replace("skd", "xml");
            if (File.Exists(pathFace))
            {
                SkeletonList = ReplayAvatar.LoadSkeletonsFromXML(pathFile, pathFace);
            }
            else
            {
                SkeletonList = ReplayAvatar.LoadSkeletonsFromXML(pathFile, "");
            }
            skeletonNumber = 0;
            faceNumber = 0;
        }

        #region audio

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        public void PlayWAVFile(String path)
        {
            if (path.Contains(".wav"))
            {
                String str = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), path);
                if (File.Exists(str))
                {
                    playerAudio.Stop();
                    playerAudio.SoundLocation = str;
                    playerAudio.Play();
                }
            }
        }

        #endregion

        //When the waving gesture is recognized, we determine what file should be played to respond to the user
        public static void Gesture_GestureRecognized(object sender, EventArgs e)
        {
            string curItem = ((VideosList)TrainingWithAvatarView.Get().VideosList.SelectedItem).Name;
            int nbFrame = ((WaveGesture)sender).FrameGesture;
            if (nbFrame > 40 && nbFrame <60)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (nbFrame > 60)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Do_It_Quicker.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (nbFrame < 40)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Do_It_Slower.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }

        }

        public static void HandTraining_GestureRecognized(object sender, EventArgs e)
        {
            string curItem = ((VideosList)TrainingWithAvatarView.Get().VideosList.SelectedItem).Name;
            bool complete = ((HandTraining)sender).Complete;
            bool slow = ((HandTraining)sender).Slow;
            int count = ((HandTraining)sender).Count;

            //test audio
            /*
            switch (count)
            {
                case 1:
                    TrainingWithAvatarViewModel.Get().PlayWAVFile("intro.wav");
                    break;
                case 2:
                    TrainingWithAvatarViewModel.Get().PlayWAVFile("Two.wav");
                    break;
                case 3:
                    TrainingWithAvatarViewModel.Get().PlayWAVFile("Three.wav");
                    break;
            }
            */

            if (complete)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (slow)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Too_Slow.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }

        public static void PowerTraining_GestureRecognized(object sender, EventArgs e)
        {
            string curItem = ((VideosList)TrainingWithAvatarView.Get().VideosList.SelectedItem).Name;
            double distance = ((PowerTraining)sender).Distance;

            if (distance > 0.01)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Lean_Back.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }

        public static void WelcomeTraining_GestureRecognized(object sender, EventArgs e)
        {
            string curItem = ((VideosList)TrainingWithAvatarView.Get().VideosList.SelectedItem).Name;
            bool complete = ((WelcomeTraining)sender).Complete;
            bool slow = ((WelcomeTraining)sender).Slow;
            bool dropped = ((WelcomeTraining)sender).Dropped;

            if (complete)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (slow)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Too_Slow.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (dropped)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Raise_your_elbows.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }

        public static void SaluteTraining_GestureRecognized(object sender, EventArgs e)
        {
            string curItem = ((VideosList)TrainingWithAvatarView.Get().VideosList.SelectedItem).Name;
            bool complete = ((SaluteTraining)sender).Complete;
            bool slow = ((SaluteTraining)sender).Slow;
            bool stay = ((SaluteTraining)sender).Stay;
            bool angle = ((SaluteTraining)sender).AngleB;
            bool alignment = ((SaluteTraining)sender).Alignment;
            bool hand = ((SaluteTraining)sender).LeftHand;

            if (complete)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if(slow)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Too_Slow.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (hand)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Keep_your_left_hand_close_to_the_hip.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (stay)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Stay_2_seconds.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (angle)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_The_arm_angle_must_be_45_degrees.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if (alignment)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_The_arm_must_be_aligned_with_the_shoulders.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }

        public static void HypeTraining_GestureRecognized(object sender, EventArgs e)
        {
            string curItem = ((VideosList)TrainingWithAvatarView.Get().VideosList.SelectedItem).Name;
            bool complete = ((HypeTraining)sender).Complete;
            bool spread = ((HypeTraining)sender).Spread;
            bool stretch = ((HypeTraining)sender).Stretch;
            bool up = ((HypeTraining)sender).Up;

            if (complete)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if(spread)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Spread_your_arms.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if(stretch)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Stretch_your_arms.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
            else if(up)
            {
                string newPathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Raise_your_arms.skd");
                TrainingWithAvatarViewModel.Get().PathFile = newPathFile;
            }
        }

        public static void FaceTraining_GestureRecognized(object sender, EventArgs e)
        {
            string curItem = ((VideosList)TrainingWithAvatarView.Get().VideosList.SelectedItem).Name;
            bool complete = ((FaceTraining)sender).Complete;
            bool lookingDir = ((FaceTraining)sender).LookingDir;
            bool up = ((FaceTraining)sender).Up;

            if (complete)
            {
                TrainingWithAvatarViewModel.Get().PathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Good_Job.skd");
                TrainingWithAvatarViewModel.Get().SkeletonListUpdateInter(curItem);
            }
            else if(lookingDir)
            {
                TrainingWithAvatarViewModel.Get().PathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Look_in_the_direction_of_the_arm.skd");
                TrainingWithAvatarViewModel.Get().SkeletonListUpdateInter(curItem);
            }
            else if(up)
            {
                TrainingWithAvatarViewModel.Get().PathFile = Path.Combine(Path.GetDirectoryName(TrainingWithAvatarViewModel.Get().PathFile), curItem + "_Raise_your_arm_above_your_shoulder.skd");
                TrainingWithAvatarViewModel.Get().SkeletonListUpdateInter(curItem);
            }
        }
    }

    public class VideosList
    {
        public string Name { get; set; }
        public bool IsSelectable { get; set; }
        public string Color { get; set; }
    }
}


