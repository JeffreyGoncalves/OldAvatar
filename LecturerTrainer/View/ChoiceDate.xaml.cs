using LecturerTrainer.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour ChoiceDate.xaml
    /// </summary>
    public partial class ChoiceDate : Window
    {
        private ICommand goToResultCommandOK;
        private ICommand cancelCommandChoice;
        public DateTime _BeginDate { get; set; }
        public DateTime _EndDate { get; set; }

        public ChoiceDate()
        {
            InitializeComponent();
            this.DataContext = this;

        }

        public ICommand GoToResultCommandOK
        {
            get
            {
                if (this.goToResultCommandOK == null)
                {
                    this.goToResultCommandOK = new RelayCommand(() => this.ValidDate());
                }
                return this.goToResultCommandOK;
            }
        }

        public ICommand CancelCommandChoice
        {
            get
            {
                if (cancelCommandChoice == null)
                {
                    cancelCommandChoice = new RelayCommand(() => LaunchCancel());
                }
                return cancelCommandChoice;
            }
        }

        public void LaunchCancel()
        {
            Close();
        }
        
        /// <summary>
        /// Methods called to verifiy the choice of the user
        /// </summary>
        public void ValidDate()
        {
            _BeginDate = BeginDate.SelectedDate ?? DateTime.Now;
            _EndDate = EndDate.SelectedDate ?? DateTime.Now;

            if(_BeginDate<=_EndDate)
            {
                Close();
            }
            else
            {
                MessageBox.Show("The date of beginning should be befor the date of ending", "Error Date", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            
        }
    }
}
