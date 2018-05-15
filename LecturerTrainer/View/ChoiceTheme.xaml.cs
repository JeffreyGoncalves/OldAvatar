using LecturerTrainer.Model;
using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Panel where the user can chose the theme he wants to use
    /// Added by Baptiste Germond
    /// </summary>
    public partial class ChoiceTheme : Window
    {
        public ChoiceTheme()
        {
            InitializeComponent();
            this.DataContext = this;
            //Ordering the themes by alphabetical order
            var temp = from element in DrawingSheetAvatarViewModel.Get().getThemeList
                       orderby element.Name
                          select element;
            foreach (ThemeOpenGL t in temp)
            {
                comboBox.Items.Add(t.Name);
            }
            ThemeOpenGL tempTheme = DrawingSheetAvatarViewModel.Get().actualTheme;
            comboBox.SelectedIndex = comboBox.Items.IndexOf(tempTheme.Name);

            this.ResizeMode = ResizeMode.NoResize;
            jointDetPanel.Width = jointInnPanel.Width = bonesDetPanel.Width = bonesInnPanel.Width = (this.Width - 15)/2; // Real size of the inner window
            soundbarColor.Width = 300;
            
        }

        private ICommand _OkCommand;
        private ICommand _CancelCommand;

        public ICommand OkCommand
        {
            get
            {
                if (this._OkCommand == null)
                {
                    this._OkCommand = new RelayCommand(() => OkTheme());
                }
                return this._OkCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this._CancelCommand == null)
                {
                    this._CancelCommand = new RelayCommand(() => CancelTheme());
                }
                return this._CancelCommand;
            }
        }

        public void OkTheme()
        {
            DrawingSheetAvatarViewModel.Get().modifColorOpenGL(comboBox.SelectedItem.ToString());
            this.Close();
        }

        public void CancelTheme()
        {
            this.Close();
        }

        /// <summary>
        /// Changing the theme displayed in this panel when the selection of the combo box change
        /// </summary>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(ThemeOpenGL t in DrawingSheetAvatarViewModel.Get().getThemeList)
            {
                if (t.Name == comboBox.SelectedItem.ToString())
                {
                    loadColor(t);
                    break;
                }
            }  
        }

        /// <summary>
        /// Changing the color displayed in he panel (and also the texture of the soundBar)
        /// </summary>
        private void loadColor(ThemeOpenGL tempTheme)
        {
            jointDetColor.BackColor = System.Drawing.Color.FromArgb((byte)(tempTheme.TJC[3] * 255), (byte)(tempTheme.TJC[0] * 255), (byte)(tempTheme.TJC[1] * 255), (byte)(tempTheme.TJC[2] * 255));
            jointInnColor.BackColor = System.Drawing.Color.FromArgb((byte)(tempTheme.IJC[3] * 255), (byte)(tempTheme.IJC[0] * 255), (byte)(tempTheme.IJC[1] * 255), (byte)(tempTheme.IJC[2] * 255));
            bonesDetColor.BackColor = System.Drawing.Color.FromArgb((byte)(tempTheme.TBC[3] * 255), (byte)(tempTheme.TBC[0] * 255), (byte)(tempTheme.TBC[1] * 255), (byte)(tempTheme.TBC[2] * 255));
            bonesInnColor.BackColor = System.Drawing.Color.FromArgb((byte)(tempTheme.IBC[3] * 255), (byte)(tempTheme.IBC[0] * 255), (byte)(tempTheme.IBC[1] * 255), (byte)(tempTheme.IBC[2] * 255));
            backgroundColor.BackColor = tempTheme.BC;
            feedbackColor.BackColor = tempTheme.PFC;
            soundbarColor.Image = new Bitmap(Bitmap.FromFile(@"..\..\View\Icons\soundTexture\" + tempTheme.SN));
        }
    }
}
