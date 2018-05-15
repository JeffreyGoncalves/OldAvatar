using LecturerTrainer.ViewModel;
using Microsoft.Expression.Shapes;
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
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class HelpWindowView : Window
    {
        private HelpWindowViewModel myViewModel;

        /// <summary>
        /// constructor that initializes the component and keeps a reference to the viewModel class 
        /// </summary>
        public HelpWindowView()
        {
            InitializeComponent();
            myViewModel = new HelpWindowViewModel();
            this.DataContext = myViewModel;
        }

        /// <summary>
        /// called when the user clicks on the left arrow 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecreasePage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // all verifications are made in HelpWindowViewModel at currentPage binding
            myViewModel.currentPage--;
            // soft cast 
            RegularPolygon myPol = sender as RegularPolygon;
            if (myViewModel.currentPage == 1)
            {
                // if we are at the first page we have to hide the arrow 
                myPol.Visibility = Visibility.Hidden;
                // if we have more than one page we can increase with the right arrow
                if (myViewModel.nbPages > 1)
                    (this.FindName("IncreasePage") as RegularPolygon).Visibility = Visibility.Visible;
            }
        }

        private void IncreasePage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // all verifications are made in HelpWindowViewModel at currentPage binding
            myViewModel.currentPage++;
            // soft cast 
            RegularPolygon myPol = sender as RegularPolygon;
            if (myViewModel.currentPage == myViewModel.nbPages)
            {
                // if we are at the end we have to hide the arrow 
                myPol.Visibility = Visibility.Hidden;
                // we can decrease if we have more than one page 
                if (myViewModel.nbPages > 1)
                    (this.FindName("DecreasePage") as RegularPolygon).Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// allow us to change the page dynamically by typing a number in the bottom of the helping window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string newTxt = "";
            if (tb != null)
            {
                newTxt = tb.Text;
            }
            // we verify that we have a number and not an empty textbox because it would crash otherwise
            if (newTxt.Length > 0 && newTxt.All(char.IsDigit))
            {
                myViewModel.currentPage = Convert.ToInt32(newTxt);
                // if we are at first page we have to hide the decreasing arrow
                if (myViewModel.currentPage == 1)
                {
                    (this.FindName("DecreasePage") as RegularPolygon).Visibility = Visibility.Hidden;
                    // if we have more than one page we have to make sure that the increasingPage arrow is visible
                    if (myViewModel.nbPages > 1)
                        (this.FindName("IncreasePage") as RegularPolygon).Visibility = Visibility.Visible;
                    else
                        // else we have to hide both arrows
                        (this.FindName("IncreasePage") as RegularPolygon).Visibility = Visibility.Hidden;
                }
                // same case at the end : if we reached the end of the manual, no more increasing 
                else if (myViewModel.currentPage == myViewModel.nbPages)
                {
                    (this.FindName("IncreasePage") as RegularPolygon).Visibility = Visibility.Hidden;
                    if (myViewModel.nbPages == 1)
                        (this.FindName("DecreasePage") as RegularPolygon).Visibility = Visibility.Hidden;
                    else
                        (this.FindName("DecreasePage") as RegularPolygon).Visibility = Visibility.Visible;
                }
                else
                {
                    // last case : the most common one. We make sure that we display both arrows 
                    (this.FindName("DecreasePage") as RegularPolygon).Visibility = Visibility.Visible;
                    (this.FindName("IncreasePage") as RegularPolygon).Visibility = Visibility.Visible;
                }
            }
            else if (newTxt.Length > 0)
            // we ensure ourselves that we won't be able to write "abcdefg..." in the box
            {
                myViewModel.currentPage = myViewModel.currentPage;
            }
        }
    }
}
