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
using System.Windows.Shapes;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour ErrorMessageBox.xaml
    /// </summary>
    public partial class ErrorMessageBox : Window
    {

        public ErrorMessageBox(string errorTitle, string errorMsg)
        {
            InitializeComponent();
            TitleErrorBox.Title = errorTitle;
            TextError.Text = errorMsg;
        }

        private void closeWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
