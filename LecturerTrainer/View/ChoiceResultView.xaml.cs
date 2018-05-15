using LecturerTrainer.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour ChoiceResultView.xaml
    /// </summary>
    /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    public partial class ChoiceResultView : Window
    {
        private static ChoiceResultView instance = null;

        #region Contructor and Getter
        public ChoiceResultView()
        {
            instance = this;
            InitializeComponent();
            ChoiceResultViewModel crvm = new ChoiceResultViewModel() ;
            this.DataContext = crvm;
        }

        public static ChoiceResultView Get()
        {
            if (instance == null)
                instance = new ChoiceResultView();
            return instance;
        }
        #endregion

        #region Methods linked XAML objects
        /// <summary>
        /// Allow to check and uncheck all checkbox for the agitation
        /// </summary>
        private void chkAgitation_Checked(object sender, RoutedEventArgs e)
        {
            bool newval = (chkAgitation.IsChecked == true);
            foreach (object child in wrpPanelAgit.Children)
            {
                if (((CheckBox)child).IsEnabled == true)
                    ((CheckBox)child).IsChecked = newval;
            }
        }

        /// <summary>
        /// Allow to check and uncheck all checkbox for the arms motion
        /// </summary>
        private void chkArmsMotion_Checked(object sender, RoutedEventArgs e)
        {
            bool newval = (chkArmsMotion.IsChecked == true);
            foreach (object child in wrpPanelArmsMot.Children)
            {
                if (((CheckBox)child).IsEnabled == true)
                    ((CheckBox)child).IsChecked = newval;
            }
        }

        /// <summary>
        /// Allow to check and uncheck all checkbox for the face
        /// </summary>
        private void chkFace_Checked(object sender, RoutedEventArgs e)
        {
            bool newval = (chkFace.IsChecked == true);
            foreach (object child in wrpPanelFace.Children)
            {
                if (((CheckBox)child).IsEnabled == true)
                    ((CheckBox)child).IsChecked = newval;
            }
        }

        /// <summary>
        /// Allow to check and uncheck all checkbox for the audio
        /// </summary>
        private void chkAudio_Checked(object sender, RoutedEventArgs e)
        {
            bool newval = (chkAudio.IsChecked == true);
            foreach (object child in wrpPanelAudio.Children)
            {
                if (((CheckBox)child).IsEnabled == true)
                    ((CheckBox)child).IsChecked = newval;
            }
        }

        /// <summary>
        /// Method link to the combobox and proceed to the change in function of the selection
        /// </summary>
        private void cmbDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.DataContext != null)
            {
                var comboBox = sender as ComboBox;
                string value = comboBox.SelectedItem as string;
                ((ChoiceResultViewModel)DataContext).ValueOfComboBoxChanged(value);
                ((ChoiceResultViewModel)DataContext).enableSomeCheckBox(null);
                if(((ChoiceResultViewModel)DataContext).NbRecording<=0)
                    buttonOK.IsEnabled = false;
                else
                    buttonOK.IsEnabled = true;
            }
            
        }
        #endregion
    }
}
