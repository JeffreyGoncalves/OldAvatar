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
using System.Windows.Shapes;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour SessionRecording.xaml
    /// </summary>
    public partial class SessionRecording
    {
        public SessionRecording()
        {
            this.DataContext = new LecturerTrainer.ViewModel.SessionRecordingViewModel(this);
            this.ResizeMode = ResizeMode.NoResize;
            InitializeComponent();
        }
    }
}
