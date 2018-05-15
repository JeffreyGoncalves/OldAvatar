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
    /// Logique d'interaction pour InformatonView.xaml
    /// </summary>
    public partial class InformationView : Window
    {
        public InformationView()
        {
            InitializeComponent();
            text.Text = "Lecturer Trainer is a DCU's School of Computing projet\n" +
                "Created and initiated by Mrs. Fiona DERMODY\n" +
                "Under the supervision of Dr. Alistair SUTHERLAND\n" +
                "Developped by Polytech Paris-Sud's students\n" +
                "In 2014: \n" +
                "In 2015: \n" +
                "In 2016: Florian BECHU, Baptiste GERMOND, Amirali GHAZI and Timothée PERRIN";
        }

        private void closeWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
