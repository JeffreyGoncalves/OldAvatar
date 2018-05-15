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
using LecturerTrainer.ViewModel;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour TrainingWithAvatarView.xaml
    /// </summary>
    public partial class TrainingWithAvatarView : UserControl
    {
        private static TrainingWithAvatarView instance = null;

        public TrainingWithAvatarView()
        {
            InitializeComponent();
            instance = this;
            this.DataContext = TrainingWithAvatarViewModel.Get();
        }

        public static TrainingWithAvatarView Get()
        {
            if (instance == null)
                instance = new TrainingWithAvatarView();
            return instance;
        }
    }
}
