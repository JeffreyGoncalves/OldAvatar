using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LecturerTrainer.ViewModel
{
    class TrainerStopwatchViewModel : ViewModelBase
    {
        private static TrainerStopwatchViewModel tsvm = null;

        private Double _trainerStopwatchAngle;
        private SolidColorBrush _trainerStopwatchColour;
        private Visibility _trainerStopwatchVisibility;
        private bool alreadyLaunched = false;

        private TrainerStopwatchViewModel()
        {
            tsvm = this;              
            trainerStopwatchVisibility = Visibility.Hidden;
            trainerStopwatchAngle = 0;
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.A = 255;
            color.R = 203;
            color.G = 243;
            color.B = 163;
            trainerStopwatchColour = new SolidColorBrush(color);
            trainerStopwatchColour.Freeze();

        }

        public static TrainerStopwatchViewModel Get()
        {
            if (tsvm == null)
                tsvm = new TrainerStopwatchViewModel();
            return tsvm; 
        }


        #region properties
        public Visibility trainerStopwatchVisibility
        {
            get { return _trainerStopwatchVisibility; }
            set
            {
                _trainerStopwatchVisibility = value;
                OnPropertyChanged("trainerStopwatchVisibility");
            }
        }

        public SolidColorBrush trainerStopwatchColour
        {
            get { return _trainerStopwatchColour; }
            set
            {
                _trainerStopwatchColour = value;
                OnPropertyChanged("trainerStopwatchColour");
            }
        }

        public Double trainerStopwatchAngle
        {
            get { return _trainerStopwatchAngle; }
            set
            {
                if (value > 0 && value <= 360)
                {
                    alreadyLaunched = true;
                    _trainerStopwatchAngle = value;
                    CheckAngleAndChangeColor(value);
                    OnPropertyChanged("trainerStopwatchAngle");
                }
                /*Added by Baptiste Germond
                 Use for recording a session, it will access only when it is a record and when the timer is finished
                 It controlled the end of the record*/
                else if ((value % 360) == 0 && alreadyLaunched == true && TrainingSideToolViewModel.Get().isRecording == true)
                {
                    alreadyLaunched = false;
                    SessionRecordingViewModel.inRecord = false;
                    TrainingSideToolViewModel.Get().isRecording = false;
                    TrainingSideToolViewModel.Get().stopSessionRecording();
                    TrainingSideToolViewModel.Get().stopStopwatch();
                }
            }
        }
        #endregion

        #region methods 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"> value is >= 0 and <= 360 </param>
        private void CheckAngleAndChangeColor(double value)
        {
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.A = 255;
            if (value < 90)
            {
                color.R = 203;
                color.G = 243;
                color.B = 163;
            }
            else if (value < 180)
            {
                color.R = 243;
                color.G = 227;
                color.B = 121;
            }
            else if (value < 270)
            {
                color.R = 232;
                color.G = 110;
                color.B = 53;
            }
            else
            {
                color.R = 141;
                color.G = 19;
                color.B = 19;
            }
            trainerStopwatchColour = new SolidColorBrush(color);
            trainerStopwatchColour.Freeze(); 
        }

        /// <summary>
        /// will change the starting angle of the chart if needed
        /// </summary>
        public void update(int newAngle){
            this.trainerStopwatchAngle = newAngle; 
        }

        /// <summary>
        /// reset the counter
        /// </summary>
        public void reset()
        {
            this.trainerStopwatchAngle = 0;  
        }

        public void enable()
        {
            this.trainerStopwatchVisibility = Visibility.Visible;
        }

        public void disable()
        {
            this.trainerStopwatchVisibility = Visibility.Hidden; 
        }

        public double getTrainerWatchAngle()
        {
            return this.trainerStopwatchAngle;
        }
        public SolidColorBrush getTrainerWatchColor()
        {
            return this.trainerStopwatchColour;
        }
        public Visibility getTrainerWatchVisibility()
        {
            return this.trainerStopwatchVisibility;
        }
        #endregion 
    }
}
