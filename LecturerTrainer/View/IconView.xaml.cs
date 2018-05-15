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
using LecturerTrainer.ViewModel;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour IconView.xaml
    /// </summary>
    public partial class IconView : UserControl
    {
        public IconView()
        {
            InitializeComponent();
            this.DataContext = IconViewModel.get();
        }

        private void SpeechRecognitionCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
