using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LecturerTrainer.Model;
using System.Timers;
using System.Windows;
using LecturerTrainer.Model.AudioAnalysis;
using System.Windows.Input;
using LecturerTrainer.View;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using LecturerTrainer.Model.EmotionRecognizer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// Class which links IconView to Events emited by Models to show icons on screen
    /// </summary>
    class IconViewModel : ViewModelBase
    {

        #region fields
        /// <summary>
        /// Unique instance of the class
        /// </summary>
        public static IconViewModel iconViewModel = null;

        /// <summary>
        /// Linked view
        /// </summary>
        public static IconView iconView = null;

        /// <summary>
        /// Maximum value for icons opacity
        /// </summary>
        public static double maxOpacity = 1;

        /// <summary>
        /// Minimum value for icons opacity
        /// </summary>
        public static double minOpacity = 0.1;

        /// <summary>
        /// Visibility of time icon
        /// </summary>
        public static Visibility timeVisibility = Visibility.Collapsed;

        /// <summary>
        /// Text displayed below the time icon. It correspond to the remainning time.
        /// </summary>
        public static String residualTimeText;

        // private variables containing a double value corresponding to the state of each Icon
        /// <summary>
        /// Boring icon's current opacity
        /// </summary>
        private double boringOpacity = minOpacity;

        /// <summary>
        /// Stressed icon's current opacity
        /// </summary>
        private double stressOpacity = minOpacity;

        /// <summary>
        /// Agitation icon's current opacity
        /// </summary>
        private double agitationOpacity = minOpacity;

        /// <summary>
        /// Speed icon's current opacity
        /// </summary>
        private double speedOpacity = minOpacity;

        /// <summary>
        /// Emotion icon's current opacity
        /// </summary>
        private double emotionOpacity = minOpacity;

        /// <summary>
        /// Emotion icon's image source
        /// </summary>
        private string emotionIconSource = "Icons/neutral.png";

        /// <summary>
        /// Emotion icon's current text (linked to the emoticon displayed)
        /// </summary>
        private string emotionIconText = "Neutral";

        /// <summary>
        /// Speed icon's image source
        /// </summary>
        private string speedIconSource = "Icons/speed_empty.png";

        /// <summary>
        /// fft variables 
        /// </summary>
        private readonly object energyLock = new object();
        public float[] FFTDisplay;
        public WriteableBitmap fftBitmap;
        private const int EnergyBitmapWidth = 128;
        private const int EnergyBitmapHeight = 150;
        private readonly Int32Rect fullEnergyRect = new Int32Rect(0, 0, EnergyBitmapWidth, EnergyBitmapHeight);
        private readonly byte[] backgroundPixels = new byte[EnergyBitmapWidth * EnergyBitmapHeight];
        const short wBinsForFFT = 512;
        private byte[] foregroundPixels;
        protected DrawingGroup fftDrawingGroup;
        protected DrawingImage fftImageSource;

        #endregion

        #region constructor and get()
        /// <summary>
        /// Constructor
        /// Bind functions to events
        /// </summary>
        private IconViewModel()
        {
            Agitation.agitationEvent += iconAgitation;
            FFT.reflexEvent += iconBadReflex;
            Pitch.BoringEvent += iconBoringVoice;
            AudioProvider.speedEvent += iconSpeed;
            EmotionRecognition.emoEvent += iconEmotion;

            fftDrawingGroup = new DrawingGroup();
            fftImageSource = new DrawingImage(fftDrawingGroup);
            this.foregroundPixels = new byte[EnergyBitmapHeight];
            this.fftBitmap = new WriteableBitmap(EnergyBitmapWidth, EnergyBitmapHeight, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Color.FromRgb(30, 31, 36), Color.FromRgb(201, 201, 201) }));
            
			for (int i = 0; i < this.foregroundPixels.Length; ++i)
            {
                this.foregroundPixels[i] = 0xff;
            }
            CompositionTarget.Rendering += UpdateFFT;
            LecturerTrainer.Model.AudioAnalysis.FFT.FFTEvent += FFTEvent;

            iconViewModel = this;
        }

        public static IconViewModel get()
        {
            if (iconViewModel == null)
            {
                iconViewModel = new IconViewModel();
            }
            return iconViewModel;
        }

        public static IconViewModel get(IconView view)
        {
            if (iconViewModel == null)
            {
                iconViewModel = new IconViewModel();
            }
            iconView = view;
            return iconViewModel;
        }

        #endregion

        #region properties
        /// <summary>
        /// Bitmap showing the current fft
        /// </summary>
        public WriteableBitmap FftBitmap
        {
            get
            {
                return fftBitmap;
            }
        }

        /// <summary>
        /// Binding Opacity of Boring's Icon
        /// </summary>
        public double BoringOpacity
        {
            get
            {
                return boringOpacity;
            }
            set
            {
                boringOpacity = value;
                OnPropertyChanged("BoringOpacity");
            }
        }

        /// <summary>
        /// Binding Opacity of Stress's Icon
        /// </summary>
        public double StressOpacity
        {
            get
            {
                return stressOpacity;
            }
            set
            {
                stressOpacity = value;
                OnPropertyChanged("StressOpacity");
            }
        }

        /// <summary>
        /// Binding Opacity of Emotion's Icon
        /// </summary>
        public double EmotionOpacity
        {
            get
            {
                return emotionOpacity;
            }
            set
            {
                emotionOpacity = value;
                OnPropertyChanged("EmotionOpacity");
            }
        }

        /// <summary>
        /// Binding Opacity of Speed's Icon
        /// </summary>
        public double SpeedOpacity
        {
            get
            {
                return speedOpacity;
            }
            set
            {
                speedOpacity = value;
                OnPropertyChanged("SpeedOpacity");
            }
        }

        /// <summary>
        /// Binding source of Emotion's Icon
        /// </summary>
        public string EmotionSource
        {
            get
            {
                return emotionIconSource;
            }
            set
            {
                emotionIconSource = value;
                OnPropertyChanged("EmotionSource");
            }
        }

        /// <summary>
        /// Binding source of Speed's Icon
        /// </summary>
        public string SpeedSource
        {
            get
            {
                return speedIconSource;
            }
            set
            {
                speedIconSource = value;
                OnPropertyChanged("SpeedSource");
            }
        }

        /// <summary>
        /// Binding text of Emotion's Icon
        /// </summary>
        public string EmotionText
        {
            get
            {
                return emotionIconText;
            }
            set
            {
                emotionIconText = value;
                OnPropertyChanged("EmotionText");
            }
        }

        /// <summary>
        /// Binding Opacity of Agitation's Icon
        /// </summary>
        public double AgitationOpacity
        {
            get
            {
                return agitationOpacity;
            }
            set
            {
                agitationOpacity = value;
                OnPropertyChanged("AgitationOpacity");
            }
        }

        /// <summary>
        /// Binding visibility of time icon
        /// </summary>
        public Visibility TimeVisibility
        {
            get
            {
                return timeVisibility;
            }
            set
            {
                timeVisibility = value;
                OnPropertyChanged("TimeVisibility");
            }
        }

        /// <summary>
        /// Binding text of residual time textBloc
        /// </summary>
        public String ResidualTimeText
        {
            get
            {
                return residualTimeText;
            }
            set
            {
                residualTimeText = value;
                OnPropertyChanged("ResidualTimeText");
            }
        }

        #endregion

        #region methods
        /// <summary>
        /// Raises an fft event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FFTEvent(object sender, LecturerTrainer.Model.AudioAnalysis.FFT.FFTEventArgs e)
        {
            FFTDisplay = e.FFTDisplay;
        }

        /// <summary>
        /// Updates the fft 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateFFT(object sender, EventArgs e)
        {
            lock (this.energyLock)
            {
                this.fftBitmap.WritePixels(fullEnergyRect, this.backgroundPixels, EnergyBitmapWidth, 0);
                int k = 0;
                if (FFTDisplay != null)
                    for (int i = 0; i < wBinsForFFT / 4; i++)
                    {
                        const int HalfImageHeight = EnergyBitmapHeight / 2;
                        if (FFTDisplay != null)
                        {
                            try
                            {
                                int barHeight = (int)Math.Max(1.0, Math.Min(1.0, this.FFTDisplay[i]) * EnergyBitmapHeight * 20);
                                if (barHeight > EnergyBitmapHeight)
                                    barHeight = EnergyBitmapHeight;
                                var barRect = new Int32Rect(k, HalfImageHeight - (barHeight / 2), 1, barHeight);
                                this.fftBitmap.WritePixels(barRect, this.foregroundPixels, 1, 0);
                            }
                            catch (Exception) { }
                        }
                        k++;
                    }
            }
        }

        /// <summary>
        /// Set the fft colors : backgroud and foreground
        /// </summary>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        public void setFFT(Color foreground, Color background)
        {
            this.fftBitmap = new WriteableBitmap(EnergyBitmapWidth, EnergyBitmapHeight, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { background, foreground }));
            OnPropertyChanged("FftBitmap");
        }

        /// <summary>
        /// Returns the fft image
        /// </summary>
        public DrawingImage getImage()
        {
            return fftImageSource;
        }

        /// <summary>
        /// Clears the body icon : set its opacity to minimum value
        /// </summary>
        public void clearBody()
        {
            AgitationOpacity = minOpacity;
        }

        /// <summary>
        /// Clears the reflex (stress) icon : set its opacity to minimum value
        /// </summary>
        public void clearBadVoiceReflex()
        {
            StressOpacity = minOpacity;
        }

        /// <summary>
        /// Clears the emotion icon : set its opacity to minimum value and shows the "Neutral" emoticoon
        /// </summary>
        public void clearEmotion()
        {
            EmotionSource = "Icons/neutral.png";
            EmotionText = "Neutral";
            EmotionOpacity = minOpacity;
        }

        /// <summary>
        /// Clears the speed icon : set its opacity to minimum value and shows the "Empty" emoticoon
        /// </summary>
        public void clearSpeed()
        {
            SpeedSource = "Icons/speed_empty.png";
            SpeedOpacity = minOpacity;
        }

        /// <summary>
        /// Clears the boring icon : set its opacity to minimum value
        /// </summary>
        public void clearBoring()
        {
            BoringOpacity = minOpacity;
        }

        /// <summary>
        /// Clears all icons
        /// </summary>
        public void clearAll()
        {
            clearBoring();
            clearSpeed();
            clearEmotion();
            clearBadVoiceReflex();
            clearBody();
        }

        /// <summary>
        /// Set or Unset Agitation's Icon function
        /// Call by an Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="f"></param>
        private void iconAgitation(object sender, InstantFeedback f)
        {
             AgitationOpacity = minOpacity;
        }

        /// <summary>
        /// Set or Unset Boring's Icon function
        /// Call by an Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="f"></param>
        private void iconBoringVoice(object sender, LongFeedback f)
        {
            if (f.display)
            {
                BoringOpacity = maxOpacity;
            }
            else
            {
                BoringOpacity = minOpacity;
            }
        }

        /// <summary>
        /// Set or Unset Stress's Icon function
        /// Call by an Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="f"></param>
        private void iconBadReflex(object sender, LongFeedback f)
        {
            if (f.display)
            {
                StressOpacity = maxOpacity;
            }
            else
            {
                StressOpacity = minOpacity;
            }
        }

        /// <summary>
        /// Set or Unset Fast's Icon function
        /// Call by an Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="f"></param>
        private void iconSpeed(object sender, ValuedFeedback f)
        {
            if (f.display)
            {
                SpeedOpacity = maxOpacity;

                if (f.value == 0)
                    SpeedSource = "Icons/speed0.png";
                else if (f.value == 1)
                    SpeedSource = "Icons/speed1.png";
                else if (f.value == 2)
                    SpeedSource = "Icons/speed2.png";
                else if (f.value == 3)
                    SpeedSource = "Icons/speed3.png";
                else
                    SpeedSource = "Icons/speed4.png";
            }
            else
            {
                SpeedOpacity = minOpacity;
                SpeedSource = "Icons/speed_empty.png";
            }
            if (TrainingSideToolViewModel.Get().State == IRecordingState.Recording)
                TrainingSideToolViewModel.Get().storingFeedbackThreadData.addTextFeedbackInQueue(StoringFeedbackThreadData.FeedbackToText(f, "speedEvent"));
        }

        /// <summary>
        /// Set Emotion's Icon function
        /// Call by an Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="f"></param>
        private void iconEmotion(object sender, LongFeedback f)
        {
            EmotionText = f.feedback;          
            if (EmotionText == "Happy")
                EmotionSource = "Icons/happy.png";

            else if (EmotionText == "Sad")
                EmotionSource = "Icons/sad.png";

            else if (EmotionText == "Surprised")
                EmotionSource = "Icons/surprised.png";

            else if (EmotionText == "Frightened")
                EmotionSource = "Icons/frightened.png";

            else if (EmotionText == "Angry")
                EmotionSource = "Icons/angry.png";

            else if (EmotionText == "Disgusted")
                EmotionSource = "Icons/disgusted.png";
            else
                EmotionSource = "Icons/neutral.png";

            if (f.display)
                EmotionOpacity = maxOpacity;
            else
                EmotionOpacity = minOpacity;
        }
        #endregion
    }
}