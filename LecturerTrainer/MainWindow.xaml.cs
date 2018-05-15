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

using LecturerTrainer.Model;
using LecturerTrainer.ViewModel;
using LecturerTrainer.View;
using Microsoft.Speech.Recognition;
using System.IO;

namespace LecturerTrainer
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static DrawingSheetView drawingSheet { get; private set; }
        public static Main main { get; set; }

        public static MainWindow lastInstance;

        public MainWindow()
        {
            main = new Main();
            lastInstance = this;
            InitializeComponent();
            this.DataContext = TrainingSideToolViewModel.Get();
            drawingSheet = this.DrawingSheetView;
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        public void setResizable(bool resizable)
        {
            if (resizable)
                this.ResizeMode = ResizeMode.CanResize;
            else
                this.ResizeMode = ResizeMode.NoResize;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //SideToolsViewModel.FFTThread.Join();
           /* MainWindow.lastInstance.SideToolsView.
            CompositionTarget.Rendering +=  UpdateFFT;
            AudioAnalysis.FFT.FFTEvent += FFTEvent;*/

            if (Main.session.Exists())
            {
                Main.session.serializeSession(Main.session.sessionPath);
            }
            // Stopping the recording
            if (TrainingSideToolViewModel.Get().isRecording)
            {
                TrainingSideToolViewModel.Get().StopRecordingCommand.Execute(null);
            }

            // Removing of the temp AVI file if closing the application
            if (File.Exists(@"test.avi"))
                File.Delete(@"test.avi");
            System.Environment.Exit(0);
        }

        private void IconView_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
