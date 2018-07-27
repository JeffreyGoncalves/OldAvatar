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
    /// Interaction logic for MessageBoxPerso.xaml
    /// </summary>
    public partial class MessageBoxPerso : Window
    {

        public MessageBoxPerso(string title, string msg)
        {
            InitializeComponent();
            TitleBox.Title = title;
            TextMessage.Text = msg;
        }

        private void closeWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
