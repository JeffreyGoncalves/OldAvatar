using LecturerTrainer.ViewModel;
using LecturerTrainer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour ReplayView.xaml
    /// </summary>
    public partial class ReplayView : UserControl
    {
        private static ReplayView instance = null;

        private bool dragStarted = false;

        private bool isPlayed = false;

        public ReplayView()
        {
            InitializeComponent();
            instance = this;
            //this.DataContext = ReplayViewModel.Get();
            this.FeedbackLabel1.DataContext = TrainingSideToolViewModel.Get();
            this.FeedbackLabel2.DataContext = TrainingSideToolViewModel.Get();
            this.FeedbackLabel3.DataContext = TrainingSideToolViewModel.Get();
            this.FeedbackLabel4.DataContext = TrainingSideToolViewModel.Get();
            this.FeedbackLabel5.DataContext = TrainingSideToolViewModel.Get();
            this.FeedbackLabel6.DataContext = TrainingSideToolViewModel.Get();
            this.FeedbackLabel7.DataContext = TrainingSideToolViewModel.Get();
            this.FeedbackLabel8.DataContext = TrainingSideToolViewModel.Get();
            this.FeedbackLabel9.DataContext = TrainingSideToolViewModel.Get();
        }

        public static ReplayView Get()
        {
            if (instance == null)
                instance = new ReplayView();
            return instance;
        }

        /// <summary>
        /// it displays the right value on the slider on replay mode
        /// </summary>
        /// <param name="time"></param>
        /// <author> Alban Descottes 2018 </author>
        public void changeValueOfSlider(int time)
        {
            LenghtVideo.Value = (double)time / ReplayViewModel.timeEnd * 100;
        }

        /// <summary>
        /// When the user strats to drag the slider the avatar/stream is paused
        /// </summary>
        /// <author> Alban Descottes 2018 </author>
        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.dragStarted = true;
            ReplayViewModel.initTime = ReplayAvatar.SkeletonList[ReplayAvatar.CurrentSkeletonNumber].Item1;
            if (Stream.IsChecked == true)
            {
                DrawingSheetView.Get().ReplayVideo.Pause();
                DrawingSheetView.Get().ReplayAudio.Pause();
            }
            if (ReplayViewModel.played)
            {
                isPlayed = true;
                ReplayViewModel.PauseReplay();
            }

        }

        /// <summary>
        /// this method is called everytime, when the user drags the slider it changes the current avatar 
        /// it selects the right avatar proportionally of the total length of the replay
        /// </summary>
        /// <author> Alban Descottes 2018 </author>
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (dragStarted)
            {
                var newTime = e.NewValue / 100 * ReplayViewModel.timeEnd;
                ReplayViewModel.changeCurrentAvatar((int)newTime);
            }
        }

        /// <summary>
        /// this method computes the offset created by the translation of the slider and replays or keeps paused the stream/avatar
        /// </summary>
        /// <author> Alban Descottes 2018 </author>
        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ReplayAvatar.offset += ReplayViewModel.localOffset;
			ReplayViewModel.localOffset = 0;
            if(Stream.IsChecked == true)
            {
                if(isPlayed)
                    DrawingSheetView.Get().ReplayVideo.Play();
                DrawingSheetView.Get().ReplayVideo.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch() - ReplayAvatar.offset);
            }

            if (isPlayed)
            {
                if(DrawingSheetView.Get().ReplayAudio.Source != null)
                {
                    DrawingSheetView.Get().ReplayAudio.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch() - ReplayAvatar.offset);
                    DrawingSheetView.Get().ReplayAudio.Play();
                }
                ReplayViewModel.PlayReplay();
                isPlayed = false;
            }
            this.dragStarted = false;
        }

        
    }
}
