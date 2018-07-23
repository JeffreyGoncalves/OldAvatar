using AForge.Video.FFMPEG;
using LecturerTrainer.Model;
using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Interaction logic for ExportAvatarView.xaml
    /// </summary>
    /// <author> Alban Descottes 2018 </author>
    public partial class ExportAvatarVideoView : Window
    {

        private static VideoFileWriter avatarVideoStreamWriter;

        private int lengthSkeletonList;

        public ExportAvatarVideoView()
        {
            // initializes the view
            InitializeComponent();
            lengthSkeletonList = ReplayAvatar.SkeletonList.Count - 1;
            ExportProgressBar.Maximum = lengthSkeletonList;
            ExportProgressBar.Value = 0;

            // it opens the videoFileWriter on the current replay folder and get the width and height of the current screen
            avatarVideoStreamWriter = new VideoFileWriter();
            var tuple = DrawingSheetAvatarViewModel.Get().getWidthAndHeight();
            avatarVideoStreamWriter.Open(ReplayViewModel.Get().folderPath + "avatar.avi", tuple.Item1, tuple.Item2, 30, VideoCodec.MPEG4, 1000000);

        }

        /* the next lines are inspired by http://www.wpf-tutorial.com/misc-controls/the-progressbar-control/ */
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= lengthSkeletonList ; i++)
            {
                (sender as BackgroundWorker).ReportProgress(i);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // if the replay has faces, it displays the face plus the skeleton
            // and it takes two screenshot because during a recording with facetracking on it records two time less skeletons 
            if (ReplayAvatar.faceTracking)
            {
                DrawingSheetAvatarViewModel.Get().drawFaceInReplay = true;
                DrawingSheetAvatarViewModel.Get().drawFace(ReplayAvatar.SkeletonList[e.ProgressPercentage].Item3.depthPointsList,
                    ReplayAvatar.SkeletonList[e.ProgressPercentage].Item3.colorPointsList,
                    ReplayAvatar.SkeletonList[e.ProgressPercentage].Item3.faceTriangles);
                DrawingSheetAvatarViewModel.Get().skToDrawInReplay = ReplayAvatar.SkeletonList[e.ProgressPercentage].Item2;
                ReplayViewModel.Get().currentFeedbackList = ReplayViewModel.listlistString[e.ProgressPercentage];
                DrawingSheetAvatarViewModel.Get().draw(this, new EventArgs());
                //ReplayViewModel.Get().nextFeedbackList(this, new EventArgs());
                avatarVideoStreamWriter.WriteVideoFrame(DrawingSheetAvatarViewModel.Get().GrabScreenshot());
                avatarVideoStreamWriter.WriteVideoFrame(DrawingSheetAvatarViewModel.Get().GrabScreenshot());
            }
            // else it displays just the skeleton and takes one screenshot
            else
            {
                // it changes the current skeleton to draw, next it draws, grabs a screenshot and writes on the video
                DrawingSheetAvatarViewModel.Get().skToDrawInReplay = ReplayAvatar.SkeletonList[e.ProgressPercentage].Item2;
                ReplayViewModel.Get().currentFeedbackList = ReplayViewModel.listlistString[e.ProgressPercentage];
                DrawingSheetAvatarViewModel.Get().draw(this, new EventArgs());
                avatarVideoStreamWriter.WriteVideoFrame(DrawingSheetAvatarViewModel.Get().GrabScreenshot());
            }
            
            // it changes the value for the progressBar
            ExportProgressBar.Value = e.ProgressPercentage;

            //if it's the last one it closes the video, the view and opens a messageBox to inform the user that is complete and the localization of the file
            if(e.ProgressPercentage == lengthSkeletonList)
            {
                avatarVideoStreamWriter.Close();
                Close();
                new MessageBoxPerso("Export avatar video", "Export complete\nThe video is located in " + ReplayViewModel.Get().folderPath).ShowDialog();

            }
        }


    }
}
