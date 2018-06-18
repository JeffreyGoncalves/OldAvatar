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
            Console.Out.WriteLine("ReplayView");
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

        public void changeValueOfSlider(int time)
        {
            LenghtVideo.Value = (double)time / ReplayViewModel.timeEnd * 100;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (dragStarted)
            {
                var newTime = e.NewValue / 100 * ReplayViewModel.timeEnd;
                ReplayViewModel.changeCurrentAvatar((int)newTime);
            }
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ReplayAvatar.offset += ReplayViewModel.localOffset;
            if(Stream.IsChecked == true)
            {
                if(isPlayed)
                    DrawingSheetView.Get().ReplayVideo.Play();
                DrawingSheetView.Get().ReplayVideo.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch() - ReplayAvatar.offset);
                if(isPlayed)
                    DrawingSheetView.Get().ReplayAudio.Play();
                DrawingSheetView.Get().ReplayAudio.Position = new TimeSpan(0, 0, 0, 0, (int)Tools.getStopWatch() - ReplayAvatar.offset);
            }
            if (isPlayed)
            {
                ReplayViewModel.PlayReplay();
                isPlayed = false;
            }
            this.dragStarted = false;
        }

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
    }
}
