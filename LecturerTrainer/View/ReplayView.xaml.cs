using LecturerTrainer.ViewModel;
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

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour ReplayView.xaml
    /// </summary>
    public partial class ReplayView : UserControl
    {
        private static ReplayView instance = null;

        public ReplayView()
        {
            InitializeComponent();
            instance = this;

            this.DataContext = ReplayViewModel.Get();
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
    }
}
