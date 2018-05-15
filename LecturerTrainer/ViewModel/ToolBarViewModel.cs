using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using LecturerTrainer.Model;
using System.Windows.Controls;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using LecturerTrainer.View;
using System.IO;
using System.Collections.Generic;
using Microsoft.Kinect.Toolkit;

namespace LecturerTrainer.ViewModel
{
    public class ToolBarViewModel : ViewModelBase
    {
        #region fields
        private ICommand newCommand;
        private ICommand infosCommand;
        private ICommand openCommand;
        private ICommand closeCommand;
        private ICommand quitCommand;
        private ICommand recordSessionCommand;
        private ICommand _HelpCommand;
        private ICommand _FontColorChangingCommand;
        private ICommand _TabsColorChangingCommand;
        private ICommand _BgColorChangingCommand;
        private ICommand _VSFeedbackColorChangingCommand;
        private ICommand _ResetColorsCommand;
        private ICommand _OpenGLThemeChangingCommand;
        private ICommand _openChartsAnalysisCommand;

        // Main color theme
        private Color mainFontColor;
        private Color mainBackgroundColor;
        private Color mainTabColor;

        private String kinectConnectionStatus = "NONE";

        #endregion

        #region Constructor
        // we set the main color settings in order to be able to reset colors if desired 
        public ToolBarViewModel()
        {
            mainFontColor = (Color)App.Current.Resources["GeneralTextColor"];
            mainBackgroundColor = (Color)App.Current.Resources["UnselectedTabColor"];
            mainTabColor = (Color)App.Current.Resources["SelectedTabColor"];

            KinectDevice.KinectChanged += kinectChanged;
        }
        #endregion

        #region properties

        public ICommand ResetColorsCommand
        {
            get
            {
                if (this._ResetColorsCommand == null)
                {
                    this._ResetColorsCommand = new RelayCommand(() => this.ResetColorRessources(), () => this.canChangeCommand());
                }
                return this._ResetColorsCommand;
            }
        }

        public ICommand BgColorChangingCommand
        {
            get
            {
                if (this._BgColorChangingCommand == null)
                {
                    this._BgColorChangingCommand = new RelayCommand(() => this.ChangeResourceColor("UnselectedTabColor"), () => this.canChangeCommand()); 
                }
                return this._BgColorChangingCommand; 
            }
        }

        public ICommand VSFeedbackColorChangingCommand
        {
            get
            {
                if (this._VSFeedbackColorChangingCommand == null)
                {
                    this._VSFeedbackColorChangingCommand = new RelayCommand(() => this.ChangeResourceColor("FeedbackStreamColor"), () => this.canChangeCommand());
                }
                return this._VSFeedbackColorChangingCommand;
            }
        }

        public ICommand TabsColorChangingCommand
        {
            get
            {
                if(this._TabsColorChangingCommand == null)
                {
                    this._TabsColorChangingCommand = new RelayCommand(() => this.ChangeResourceColor("SelectedTabColor"), () => this.canChangeCommand()); 
                }
                return this._TabsColorChangingCommand; 
            }
        }

        public ICommand FontColorChangingCommand
        {
            get
            {
                if(this._FontColorChangingCommand == null)
                {
                    this._FontColorChangingCommand = new RelayCommand(() => this.ChangeResourceColor("GeneralTextColor"), () => this.canChangeCommand()); 
                }
                return this._FontColorChangingCommand; 
            }
        }

        public ICommand OpenGLThemeChangingCommand
        {
            get
            {
                if (this._OpenGLThemeChangingCommand == null)
                {
                    this._OpenGLThemeChangingCommand = new RelayCommand(() => this.ChangeResourceColorOpenGL(), () => this.canChangeCommand());
                }
                return this._OpenGLThemeChangingCommand;
            }
        }

        public ICommand NewCommand
        {
            get
            {
                if (this.newCommand == null)
                {
                    this.newCommand = new RelayCommand(() => this.CreateUser(), () => this.CanCreateUser());
                }
                return this.newCommand;
            }
        }

        public ICommand RecordSessionCommand
        {
            get
            {
                if (this.recordSessionCommand == null)
                {
                    this.recordSessionCommand = new RelayCommand(() => this.OpenRecordingPage(), () => this.canRecordSession());
                }
                return this.recordSessionCommand;
            }
        }

        public ICommand InfosCommand
        {
            get
            {
                if (this.infosCommand == null)
                    this.infosCommand = new RelayCommand(() => this.ShowInfos(), () => this.CanShowInfos());

                return this.infosCommand;
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                if (this.openCommand == null)
                    this.openCommand = new RelayCommand(() => this.open(), () => this.CanOpen());

                return this.openCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                    this.closeCommand = new RelayCommand(() => this.close(), () => this.CanClose());

                return this.closeCommand;
            }
        }

        public ICommand QuitCommand
        {
            get
            {
                if (this.quitCommand == null)
                    this.quitCommand = new RelayCommand(() => this.quit(), () => this.CanQuit());

                return this.quitCommand;
            }
        }

        public ICommand HelpCommand
        {
            get
            {
                if( this._HelpCommand == null )
                {
                    this._HelpCommand = new RelayCommand(() => this.showHelp(), () => this.canShowHelp()); 
                } 
                return this._HelpCommand ; 
            }
        }

        public ICommand OpenChartsAnalysisCommand
        {
            get
            {
                if (this._openChartsAnalysisCommand == null)
                {
                    this._openChartsAnalysisCommand = new RelayCommand(() => this.OpenChartsAnalysis(), () => this.canOpenChartsAnalysis());
                }
                return this._openChartsAnalysisCommand;
            }
        }

        #endregion

        #region methods

        private bool canOpenChartsAnalysis()
        {
            return true;
        }

        private bool canRecordSession()
        {
            return true;
        }

        private bool canChangeCommand()
        {
            return true; 
        }

        private bool canShowHelp()
        {
            return true; 
        }

        private bool CanCreateUser()
        {
            return true;
        }

        private bool CanClose()
        {
            return true;
        }

        private bool CanQuit()
        {
            return true;
        }

        private bool CanOpen()
        {
            return true;
        }

        private void OpenChartsAnalysis()
        {
            if(Main.session.Exists())
            {
                string path = Path.Combine(Directory.GetParent(Main.session.sessionPath).ToString(), "SessionRecording");
                path = path + Path.DirectorySeparatorChar;
                if(Directory.Exists(path))
                {
                    HashSet<string> set = new HashSet<string>();

                    ChoiceResultView crv = new ChoiceResultView();
                    ((ChoiceResultViewModel)crv.DataContext).path = path;
                    ((ChoiceResultViewModel)crv.DataContext).enableSomeCheckBox(null);
                    ((ChoiceResultViewModel)crv.DataContext).isLoad = true;
                    ((ChoiceResultViewModel)crv.DataContext).IsLoading = true;
                    crv.Show();
                }
                else
                {
                    System.Windows.MessageBox.Show("No session recorded", "Open charts analysis error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CreateUser()
        {
            Main.session.fillPersoWithActual();
            LecturerTrainer.View.NewUserView newUV = new LecturerTrainer.View.NewUserView();
            newUV.ShowDialog();
            SessionName = Main.session.sessionName;
            SessionLaunchedMessage = Main.session.sessionLaunchMessage;            
        }

        /// <summary>
        /// Open the recording session panel (recordingSession.xaml)
        /// Added by Baptiste Germond
        /// </summary>
        private void OpenRecordingPage()
        {
            LecturerTrainer.View.SessionRecording sessionRecordingPanel = new LecturerTrainer.View.SessionRecording();
            sessionRecordingPanel.ShowDialog();
        }

        private bool CanShowInfos()
        {
            return true;
        }

        private void ShowInfos()
        {
            InformationView info = new InformationView();
            info.ShowDialog();
        }

        private void showHelp()
        {
            new LecturerTrainer.View.HelpWindowView().Show();
        }

        private void ChangeResourceColor(string resource)
        {
            ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                App.Current.Resources[resource] = (System.Windows.Media.Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B));
                if (resource == "FeedbackStreamColor")
                {
                    DrawingSheetStreamViewModel.Get().changeColorFeedbacks();
                }
                IconViewModel.get().setFFT((Color)App.Current.Resources["GeneralTextColor"], (Color)App.Current.Resources["UnselectedTabColor"]);

                Main.session.fillPersoWithSpecial(resource);
            }      
           
        }

        private void ChangeResourceColorOpenGL()
        {
            ChoiceTheme choiceT = new ChoiceTheme();
            choiceT.ShowDialog();
        }

        private void ResetColorRessources()
        {
            DrawingSheetAvatarViewModel.Get().modifColorOpenGL("Default");
            App.Current.Resources["UnselectedTabColor"] = mainBackgroundColor;
            App.Current.Resources["SelectedTabColor"] = mainTabColor;
            App.Current.Resources["GeneralTextColor"] = mainFontColor;
            App.Current.Resources["FeedbackStreamColor"] = Color.FromArgb(255, 128, 128, 128);
            IconViewModel.get().setFFT((Color)App.Current.Resources["GeneralTextColor"], (Color)App.Current.Resources["UnselectedTabColor"]);
            DrawingSheetStreamViewModel.Get().changeColorFeedbacks();
            Main.session.fillPersoWithActual();
        }

        private void close()
        {
            ToolBarView tbv = ToolBarView.Get();
            tbv.closeSession.IsEnabled = false;
            tbv.createSession.IsEnabled = true;
            tbv.openSession.IsEnabled = true;
            tbv.RecordingSession.IsEnabled = false;
            tbv.openChartsAnalysis.IsEnabled = false;
            Main.session.serializeSession(Main.session.sessionPath);
            Main.session = new Session();
            SessionName = Main.session.sessionName;
            SessionLaunchedMessage = Main.session.sessionLaunchMessage;   
        }

        private void quit()
        {
            System.Environment.Exit(0);
        }


        private void open()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "File"; // Default file name
            dlg.DefaultExt = ".ltf"; // Default file extension
            dlg.Filter = "Lecturer Trainer File (.ltf)|*.ltf"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                ToolBarView tbv = ToolBarView.Get();
                tbv.closeSession.IsEnabled = true;
                tbv.openChartsAnalysis.IsEnabled = true;
                tbv.createSession.IsEnabled = false;
                tbv.openSession.IsEnabled = false;
                tbv.RecordingSession.IsEnabled = true;
                Session.openSession(dlg.FileName);
                SessionName = Main.session.sessionName;
                SessionLaunchedMessage = Main.session.sessionLaunchMessage;
            }
        }

        public string SessionName
        {
            get
            {
                return Main.session.sessionName;
            }
            set
            {
                OnPropertyChanged("SessionName");
            }
        }

        public string SessionLaunchedMessage
        {
            get
            {
                return Main.session.sessionLaunchMessage;
            }
            set
            {
                OnPropertyChanged("SessionLaunchedMessage");
            }
        }

        public string KinectConnectionStatus
        {
            get { return kinectConnectionStatus; }
            set
            {
                kinectConnectionStatus = value;
                OnPropertyChanged("KinectConnectionStatus");
            }
        }
        

        /// <summary>
        /// Change options according to the status of the current connected Kinect.
        /// </summary>
        /// <param name="sender">KinectDevice</param>
        /// <param name="args">EventArgs.Empty</param>
        private void kinectChanged(object sender, EventArgs args)
        {
            if(KinectDevice.status == ChooserStatus.SensorStarted)
            {
                KinectConnectionStatus = "YES";
            }
            else if(KinectDevice.status == ChooserStatus.NoAvailableSensors)
            {
                KinectConnectionStatus = "NO";
            }
            else
            {
                KinectConnectionStatus = "ERR";
            }
        }

        #endregion
    }
}
