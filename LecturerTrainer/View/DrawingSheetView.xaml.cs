using LecturerTrainer.ViewModel;
using LecturerTrainer.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using OpenTK;

namespace LecturerTrainer.View
{
    public enum SheetMode {AvatarMode, StreamMode }

    /// <summary>
    /// Logique d'interaction pour DrawingSheetView.xaml
    /// </summary>
    public partial class DrawingSheetView : UserControl
    {
        public DrawingSheetViewModel sheet { get; private set; }

        private SheetMode mode;

        /// <summary>
        /// Indicate if the glcontrol is loaded or not
        /// </summary>
        public bool loaded = false;

        /// <summary>
        /// Thanks to GLControl, the 3D avatar can be drawn
        /// </summary>
        public GLControl glControl = null;

        private static DrawingSheetView instance = null;

        public DrawingSheetView()
        {
            InitializeComponent();
            initializeGLControl();

            DrawingSheetStreamViewModel.Get(this);
            DrawingSheetAvatarViewModel.Get(this);
            sheet = DrawingSheetAvatarViewModel.Get(this); 
            this.DataContext = sheet;
            MyImage.Source = sheet.getImage();
            this.mode = SheetMode.AvatarMode;
            ChangeMode(SheetMode.StreamMode);
            ChangeMode(SheetMode.AvatarMode);
            instance = this;
        }


        /// <summary>
        /// Change the view according to the selected mode
        /// </summary>
        /// <param name="mode"></param>
        public void ChangeMode(SheetMode mode)
        {
            switch (mode)
            {
                case SheetMode.AvatarMode:
                    this.Show3DSheet();
                    sheet = DrawingSheetAvatarViewModel.Get(this); 
                    MyImage.Source = sheet.getImage();
                    this.mode = SheetMode.AvatarMode;
                    break;
                case SheetMode.StreamMode:
                    this.Show2DSheet();
                    sheet = DrawingSheetStreamViewModel.Get(this); 
                    MyImage.Source = sheet.getImage();
                    this.mode = SheetMode.StreamMode;
                    break;
                default:
                    //TODO
                    break;
            }
            this.DataContext = sheet;
        }

        public static DrawingSheetView Get()
        {
            if (instance == null)
                instance = new DrawingSheetView();
            return instance;
        }

        // Main form load event handler
        private void glControl_Load(object sender, EventArgs e)
        {
            DrawingSheetAvatarViewModel.Get(this).init3DScene();
            loaded = true;
        }

        /// <summary>
        /// GLControl's initialisation function
        /// </summary>
        public void initializeGLControl()
        {
            // Create the GLControl.
            glControl = new GLControl();

            //Position and configure the controls
            glControl.Top = 0;
            glControl.Left = 0;
            glControl.Width = (int)MyImage.Width;
            glControl.Height = (int)MyImage.Height;

            glControl.Load += glControl_Load;

            // Assign the GLControl as the host control's child.
            MyHost.Child = glControl;
        }

        /// <summary>
        /// Disables the Kinect's sensor while replaying
        /// </summary>
        public void replayMode()
        {
            if(KinectDevice.sensor != null)
                KinectDevice.sensor.AllFramesReady -= Main.kinect.OnAllFramesReady;
        }

        /// <summary>
        /// Enables the Kinect's sensor when the replay mode is closed
        /// </summary>
        public void normalMode()
        {
            if (KinectDevice.sensor != null)
                KinectDevice.sensor.AllFramesReady += Main.kinect.OnAllFramesReady;
        }

        /// <summary>
        /// Return the mode of the current sheet displayed
        /// </summary>
        /// <returns></returns>
        public SheetMode activeSheet()
        {
            return mode;
        }

        /// <summary>
        /// Function displaying the 2D sheet (stream view)
        /// </summary>
        public void Show2DSheet()
        {
            MyImage.Visibility = Visibility.Visible;
            MyHost.Visibility = Visibility.Hidden;
            ReplayVideo.Visibility = Visibility.Hidden;
            CanvasFeedback.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Function displaying the 3D sheet (avatar view)
        /// </summary>
        public void Show3DSheet()
        {
            MyImage.Visibility = Visibility.Hidden;
            MyHost.Visibility = Visibility.Visible;
            ReplayVideo.Visibility = Visibility.Hidden;
            CanvasFeedback.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Function displaying the video sheet (video replay view)
        /// </summary>
        public void ShowReplayVideoSheet()
        {
            MyImage.Visibility = Visibility.Hidden;
            MyHost.Visibility = Visibility.Hidden;
            ReplayVideo.Visibility = Visibility.Visible;
            if(ReplayView.Get().Stream.IsChecked.Value)
            {
                CanvasFeedback.Visibility = Visibility.Visible;
            }
            else
            {
                CanvasFeedback.Visibility = Visibility.Hidden;
            }
            
        }

        public SheetMode getMode()
        {
            return this.mode;
        }
    }
}
