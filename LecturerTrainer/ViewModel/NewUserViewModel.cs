using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LecturerTrainer.Model;
using LecturerTrainer.View;
using LecturerTrainer.Model.Exceptions;
using System.Windows;
using System.Windows.Media;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// Class used when we want to create a new session
    /// </summary>
    class NewUserViewModel : ViewModelBase
    {
        #region fields
        private ICommand browseCommand;
        private ICommand okCommand;
        private ICommand cancelCommand;
        private string baseName = "New_Session";
        private string fileName = "New_Session";
        private string pathName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private NewUserView newUserView;
        private ErrorMessageBox errorBox = null;
        #endregion

        #region constructor
        public NewUserViewModel(NewUserView newUserView)
        {
            this.newUserView = newUserView;
        }
        #endregion

        #region properties
        public ICommand BrowseCommand
        {
            get
            {
                if (this.browseCommand==null)
                {
                    this.browseCommand = new RelayCommand(() => LaunchBrowser(), () => CanLaunchBrowser());
                }
                return this.browseCommand;
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (this.okCommand==null)
                {
                    this.okCommand = new RelayCommand(() => LaunchOk(), () => CanLaunchOk());
                }
                return this.okCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new RelayCommand(() => LaunchCancel(), () => CanLaunchCancel());
                }
                return this.cancelCommand;
            }
        }

        public string FirstName
        {
            get
            {
                return Main.session.userFirstName;
            }
            set
            {
                Main.session.userFirstName = value;
                OnPropertyChanged("FirstName");
            }
        }

        public string LastName
        {
            get
            {
                return Main.session.userLastName;
            }
            set
            {
                NameText = baseName +"_"+ value;
                Main.session.userLastName = value;
                OnPropertyChanged("LastName");
            }
        }

        public string NameText
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                OnPropertyChanged("NameText");
            }
        }

        public string PathText
        {
            get
            {
                return pathName;
            }
            set
            {
                pathName = value;
                OnPropertyChanged("PathText");
            }
        }
        #endregion

        #region methods
        public bool CanLaunchBrowser()
        {
            return true;
        }

        public bool CanLaunchOk()
        {
            return true;
        }

        public bool CanLaunchCancel()
        {
            return true;
        }

        /// <summary>
        /// Modified by Baptiste Germond
        /// </summary>
        public void LaunchOk()
        {
            try
            {
                Session.CreateSessionFolder(pathName, fileName, Main.session);
                ToolBarView tbv = ToolBarView.Get();
                //Enabling the menu item if the user is connected
                tbv.RecordingSession.IsEnabled = true;
                tbv.closeSession.IsEnabled = true;
                tbv.createSession.IsEnabled = false;
                tbv.openSession.IsEnabled = false;
                tbv.openChartsAnalysis.IsEnabled = true;

            }
            catch (CantCreateFileException e)
            {
                Main.session.eraseName();
                errorBox = new ErrorMessageBox(e.Message, e.textError);
                errorBox.Show();
            }
            
           
            this.newUserView.Close();
        }

        public void LaunchCancel()
        {
            Main.session.eraseName();
            this.newUserView.Close();
        }

        public void LaunchBrowser()
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
            folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog1.SelectedPath = PathText;
            folderBrowserDialog1.ShowDialog();
            PathText = folderBrowserDialog1.SelectedPath;
        }
        #endregion
    }
}
