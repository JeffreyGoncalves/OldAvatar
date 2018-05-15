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
using Microsoft.Kinect.Toolkit;

namespace LecturerTrainer.ViewModel
{
    public class GeneralSideToolViewModel : ViewModelBase
    {

        #region fields
        /// <summary>
        /// Command applied when avatar display is selected
        /// </summary>
        private ICommand selectAvatar;

        /// <summary>
        /// Command applied when video stream display is selected
        /// </summary>
        private ICommand selectStream;

        /// <summary>
        /// Kinect elevation value
        /// </summary>
        private double kinectElevation = 50;

        /// <summary>
        /// List of Kinect elevation possible values 
        /// </summary>
        private ObservableCollection<String> elevationList;

        /// <summary>
        /// selected elevation index in the list
        /// </summary>
        private int selectedElevationNumber = 0;

        /// <summary>
        /// Is the elevation menu visible.
        /// </summary>
        private bool elevationVisible = false;

        #endregion

        #region constructor 

        public GeneralSideToolViewModel()
        {
            KinectDevice.KinectChanged += kinectChanged;

            // Initialisation of the elevation list
            elevationList = new ObservableCollection<string>();
            elevationList.Add("Elevation choices");
            elevationList.Add("0%");
            elevationList.Add("10%");
            elevationList.Add("20%");
            elevationList.Add("30%");
            elevationList.Add("40%");
            elevationList.Add("50%");
            elevationList.Add("60%");
            elevationList.Add("70%");
            elevationList.Add("80%");
            elevationList.Add("90%");
            elevationList.Add("100%");
        }

        #endregion

        #region properties
        /// <summary>
        /// Command applied when avatar display is selected
        /// </summary>
        public ICommand SelectAvatar
        {
            get
            {
                if (this.selectAvatar == null)
                    this.selectAvatar = new RelayCommand(() => this.useAvatar(), () => this.CanSelectAvatar());

                return this.selectAvatar;
            }
        }

        /// <summary>
        /// Command applied when video stream display is selected
        /// </summary>
        public ICommand SelectStream
        {
            get
            {
                if (this.selectStream == null)
                    this.selectStream = new RelayCommand(() => this.useStream(), () => true);

                return this.selectStream;
            }
        }

        /// <summary>
        /// List of connected microphones
        /// </summary>
        public ObservableCollection<String> MicrophoneList
        {
            get
            {
                return MainWindow.main.audioProvider.ListDevices;
            }
        }

        public bool ElevationVisible
        {
            get { return elevationVisible; }
            set
            {
                elevationVisible = value;
                OnPropertyChanged("ElevationVisible");
            }
        }

        /// <summary>
        /// List of possible elevations for the Kinect
        /// </summary>
        public ObservableCollection<String> ElevationList
        {
            get
            {
                return elevationList;
            }
        }

        /// <summary>
        /// Selected microphone index in the list
        /// </summary>
        public int SelectedDevice
        {
            get
            {
                return MainWindow.main.audioProvider.DeviceNumber;
            }
            set
            {
                MainWindow.main.audioProvider.DeviceNumber = value;
            }
        }

        


        /// <summary>
        /// Selected Kinect elevation index in the list
        /// </summary>
        public int SelectedElevation
        {
            get
            {
                return selectedElevationNumber;
            }
            set
            {
                selectedElevationNumber = value;
                if (selectedElevationNumber != 0)
                {
                    float step = KinectDevice.sensor.MaxElevationAngle - KinectDevice.sensor.MinElevationAngle;
                    step = step / (elevationList.Count - 1);
                    kinectElevation = KinectDevice.sensor.MinElevationAngle + selectedElevationNumber * step;
                    KinectDevice.changeKinectElevation(kinectElevation);
                }
                OnPropertyChanged("SelectedElevation");
            }
        }

        /// <summary>
        /// Return true if Kinect elevation is automatic
        /// </summary>
        public bool UseAutoElevation
        {
            get
            {
                return KinectDevice.useAutoElevation;
            }
            set
            {
                KinectDevice.useAutoElevation = value;
                OnPropertyChanged("UseAutoElevation");
            }
        }

        /// <summary>
        /// Value of the volume slider
        /// </summary>
        public double SliderVolume
        {
            get
            {
                return MainWindow.main.audioProvider.MicrophoneLevel;
            }
            set
            {
                MainWindow.main.audioProvider.MicrophoneLevel = value;
                OnPropertyChanged("SliderVolume");
            }
        }

        /// <summary>
        /// Value of the volume slider
        /// </summary>
        public double SliderElevation
        {
            get
            {
                if (KinectDevice.sensor != null)
                    return KinectDevice.sensor.ElevationAngle;
                else
                    return 50; 
            }
            set
            {
                KinectDevice.changeKinectElevation(value);
                OnPropertyChanged("SliderElevation");
            }
        }

        /// <summary>
        /// Minimum Kinect elevation value
        /// </summary>
        public double MinKinectElevation
        {
            get
            {
                if (KinectDevice.sensor != null)
                    return KinectDevice.sensor.MinElevationAngle;
                else
                    return 100;
            }
        }

        /// <summary>
        /// Maximum Kinect elevation value
        /// </summary>
        public double MaxKinectElevation
        {
            get
            {
                if (KinectDevice.sensor != null)
                    return KinectDevice.sensor.MaxElevationAngle;
                else
                    return 100; 
            }
        }
        #endregion

        #region methods
        /// <summary>
        /// Change options according to the status of the current connected Kinect.
        /// </summary>
        /// <param name="sender">KinectDevice</param>
        /// <param name="args">EventArgs.Empty</param>
        private void kinectChanged(object sender, EventArgs args)
        {
            if (KinectDevice.status == ChooserStatus.SensorStarted)
            {
                ElevationVisible = true;
            }
            else
            {
                ElevationVisible = false;
            }
        }

        /// <summary>
        /// Return true if the avatar display can be selected
        /// </summary>
        private bool CanSelectAvatar()
        {
            return true;
        }

        /// <summary>
        /// Return true if auto elevation can be selected
        /// </summary>
        private bool canSetAutoElevation()
        {
            return true;
        }

        /// <summary>
        /// Increase Kinect elevation. It will have the value of the next elevation in the list.
        /// If the max value is already reached, it does nothing.
        /// </summary>
        public void upElevation()
        {
            if (SelectedElevation < elevationList.Count - 1)
            {
                GeneralSideTool.Get().ManualElevationCB.SelectedIndex = SelectedElevation + 1;
            }
        }

        /// <summary>
        /// Decrease Kinect elevation. It will have the value of the previous elevation in the list.
        /// If the min value is already reached, it does nothing.
        /// </summary>
        public void downElevation()
        {
            if (SelectedElevation > 1)
            {
                GeneralSideTool.Get().ManualElevationCB.SelectedIndex = SelectedElevation - 1;
            }
        }
        /// <summary>
        /// Set the display to show an avatar
        /// </summary>
        private void useAvatar()
        {
            MainWindow.drawingSheet.ChangeMode(SheetMode.AvatarMode);
        }

        /// <summary>
        /// Set the display to show video stream
        /// </summary>
        private void useStream()
        {
            MainWindow.drawingSheet.ChangeMode(SheetMode.StreamMode);
        }
        #endregion
    }
}
