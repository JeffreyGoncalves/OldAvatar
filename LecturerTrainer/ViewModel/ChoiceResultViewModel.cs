using LecturerTrainer.Model;
using LecturerTrainer.Model.AudioAnalysis;
using LecturerTrainer.Model.BodyAnalysis;
using LecturerTrainer.Model.EmotionRecognizer;
using LecturerTrainer.View;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// Class managing the choice of each charts displaing
    /// </summary>
    /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    class ChoiceResultViewModel : INotifyPropertyChanged
    {
        #region Fields
        private ICommand goToResultCommandOK;
        private ICommand cancelCommandChoice;
        private DateTime minDate;
        private DateTime maxDate;
        private HashSet<string> listpath;
        private bool lastRecord;

        private ChoiceResultView choiceResultView;
        
        /// <summary>
        /// To know if the Window is called with "Open Charts Analysis" or with the "Display my result" Button
        /// </summary>
        public bool isLoad {get; set;}

        public string path { get; set; }
        #endregion

        #region Constructor
        public ChoiceResultViewModel()
        {
            choiceResultView = ChoiceResultView.Get();
            choiceResultView.cmbDate.ItemsSource = getDate();
            choiceResultView.cmbDate.SelectedIndex = 0;
            minDate = DateTime.Today;
            maxDate = DateTime.Today;
            IsLoading = false;
            lastRecord = true;
            listpath = new HashSet<string>();
        }

        #endregion

        #region Command
        public ICommand GoToResultCommandOK
        {
            get
            {
                if(this.goToResultCommandOK == null)
                {
                    this.goToResultCommandOK = new RelayCommand(() => this.ShowResults());
                }
                return this.goToResultCommandOK;
            }
        }

        public ICommand CancelCommandChoice
        {
            get
            {
                if (this.cancelCommandChoice == null)
                {
                    this.cancelCommandChoice = new RelayCommand(() => LaunchCancel());
                }
                return this.cancelCommandChoice;
            }
        }
        #endregion

        #region Property
        private int _nbRecording;
        /// <summary>
        /// object to do the link between code and XAML
        /// </summary>
        public int NbRecording
        {
            get { return _nbRecording; }
            set
            {
                _nbRecording = value;
                NotifyPropertyChanged("NbRecording");
            }
        }

        private bool _isLoading;
        /// <summary>
        /// Use to show the ComboBox with the date choice and the recording number
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Function called when the user clicked on the OK button of the window
        /// </summary>
        private void ShowResults()
        {
            if((isLoad && NbRecording>0) || IsAtLeastOneCheckBoxIsChecked(choiceResultView.stkPanel))
            {
                choiceResultView.Close();
                List<bool> lbool = new List<bool>();
                lbool.Add(choiceResultView.chkAgitationHips.IsChecked.HasValue && choiceResultView.chkAgitationHips.IsChecked.Value); // this is a way to convert a bool? to a bool
                lbool.Add(choiceResultView.chkAgitationLHand.IsChecked.HasValue && choiceResultView.chkAgitationLHand.IsChecked.Value);
                lbool.Add(choiceResultView.chkAgitationLKnee.IsChecked.HasValue && choiceResultView.chkAgitationLKnee.IsChecked.Value);
                lbool.Add(choiceResultView.chkAgitationLShoulder.IsChecked.HasValue && choiceResultView.chkAgitationLShoulder.IsChecked.Value);
                lbool.Add(choiceResultView.chkAgitationRHand.IsChecked.HasValue && choiceResultView.chkAgitationRHand.IsChecked.Value);
                lbool.Add(choiceResultView.chkAgitationRKnee.IsChecked.HasValue && choiceResultView.chkAgitationRKnee.IsChecked.Value);
                lbool.Add(choiceResultView.chkAgitationRShoulder.IsChecked.HasValue && choiceResultView.chkAgitationRShoulder.IsChecked.Value);
                lbool.Add(choiceResultView.chkHandsJoined.IsChecked.HasValue && choiceResultView.chkHandsJoined.IsChecked.Value);
                lbool.Add(choiceResultView.chkArmsCrossed.IsChecked.HasValue && choiceResultView.chkArmsCrossed.IsChecked.Value);
                lbool.Add(choiceResultView.chkEmotion.IsChecked.HasValue && choiceResultView.chkEmotion.IsChecked.Value);
                lbool.Add(choiceResultView.chkLookDirec.IsChecked.HasValue && choiceResultView.chkLookDirec.IsChecked.Value);
                lbool.Add(choiceResultView.chkNumberSyllables.IsChecked.HasValue && choiceResultView.chkNumberSyllables.IsChecked.Value);
                var results = new ResultsView(lbool);
                if (isLoad) //if the windowis called after a user click on the "Open charts analysis"
                {
                    List<string> listpathdate = new List<string>();

                    if(lastRecord) // to know if the "Last Record" choice is selected in the comboBox
                    {
                        listpathdate.Add(listpath.ElementAt(0));
                    }
                    else
                    {
                        foreach (string s in listpath)
                        {
                            DateTime date = Tools.getDateFromPath(s);
                            if (date.CompareTo(maxDate) <= 0 && date.CompareTo(minDate) >= 0) // we compare the minimum date and the maximum date
                            {
                                listpathdate.Add(s);
                            }
                        }
                    }
                    ((ResultsViewModel)results.DataContext).loadManyCharts(listpathdate); // we called the function to load files
                }
                else //if the window is called after the user clicked on the button "Display my results"
                {
                    ((ResultsViewModel)results.DataContext).getAgitationStatistics(Agitation.getAgitationStats());
                    List<IGraph> temp = new List<IGraph>();
                    temp.AddRange(HandsJoined.getHandStatistics());
                    temp.AddRange(ArmsCrossed.getArmsStatistics());
                    ((ResultsViewModel)results.DataContext).getArmsMotion(temp); //temp is a union between HandsJoined.getHandStatistics() and ArmsCrossed.getArmsStatistics()
                    if (TrackingSideToolViewModel.get().FaceTracking)
                    {
                        List<IGraph> listGraphFace = new List<IGraph>();
                        listGraphFace.AddRange(EmotionRecognition.getEmotionsStatistics());
                        listGraphFace.AddRange(lookingDirection.getLookingStatistics());

                        ((ResultsViewModel)results.DataContext).getFaceStatistics(listGraphFace);
                    }
                    if (TrackingSideToolViewModel.get().SpeedRate)
                    {
                        List<IGraph> listGraphVoice = new List<IGraph>();
                        listGraphVoice.AddRange(AudioProvider.getVoiceStatistics());
                        ((ResultsViewModel)results.DataContext).getVoiceStatistics(listGraphVoice);
                    }
                }
                ((ResultsViewModel)results.DataContext).addResultsPartToView();
                results.Show();
            }
        }

        public void LaunchCancel()
        {
            choiceResultView.Close();
        }

        /// <summary>
        /// Function that allows to enable checkboxes just after a recording, in function of the elements caught
        /// </summary>
        public void enableCheckBox()
        {
            foreach (KeyValuePair<JointType, bool> key in Agitation.getCatchedJoin())
            {
                if (key.Key == JointType.HipCenter)
                {
                    choiceResultView.chkAgitationHips.IsEnabled = true;
                }
                else if (key.Key == JointType.HandLeft)
                {
                    choiceResultView.chkAgitationLHand.IsEnabled = true;
                }
                else if (key.Key == JointType.HandRight)
                {
                    choiceResultView.chkAgitationRHand.IsEnabled = true;
                }
                else if (key.Key == JointType.KneeLeft)
                {
                    choiceResultView.chkAgitationLKnee.IsEnabled = true;
                }
                else if (key.Key == JointType.KneeRight)
                {
                    choiceResultView.chkAgitationRKnee.IsEnabled = true;
                }
                else if (key.Key == JointType.ShoulderLeft)
                {
                    choiceResultView.chkAgitationLShoulder.IsEnabled = true;
                }
                else if (key.Key == JointType.ShoulderRight)
                {
                    choiceResultView.chkAgitationRShoulder.IsEnabled = true;
                }
            }
            choiceResultView.chkArmsMotion.IsEnabled = true;
            choiceResultView.chkArmsCrossed.IsEnabled = true;
            choiceResultView.chkHandsJoined.IsEnabled = true;

            if (TrackingSideToolViewModel.get().FaceTracking)
            {
                choiceResultView.chkFace.IsEnabled = true;
                if (TrackingSideToolViewModel.get().emo)
                {
                    choiceResultView.chkEmotion.IsEnabled = true;
                }
                if (lookingDirection.detect)
                {
                    choiceResultView.chkLookDirec.IsEnabled = true;
                }
            }
            
            if(TrackingSideToolViewModel.get().SpeedRate)
            {
                choiceResultView.chkAudio.IsEnabled = true;
                choiceResultView.chkNumberSyllables.IsEnabled = true;
            }
        }



        /// <summary>
        /// Create all elements in the comboBox
        /// </summary>
        private List<KeyValuePair<string, string>> getDate()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            list.Add(new KeyValuePair<string, string>("Name", "Last Record"));
            list.Add(new KeyValuePair<string, string>("Name", "Today"));
            list.Add(new KeyValuePair<string, string>("Name", "Yesterday"));
            list.Add(new KeyValuePair<string, string>("Name", "7 last days"));
            list.Add(new KeyValuePair<string, string>("Name", "15 last days"));
            list.Add(new KeyValuePair<string, string>("Name", "Last month"));
            list.Add(new KeyValuePair<string, string>("Name", "Personalized"));

            return list;
        }

        /// <summary>
        /// Function that allows to enable checboxes in function of the files in the folders included in the path gave in parameters or in function of the variable "path"
        /// </summary>
        /// <param name="parametersPath"></param>
        public void enableSomeCheckBox(string parametersPath)
        {
            NbRecording = 0;
            HashSet<string> list = new HashSet<string>();
            listpath.Clear();
            IsEnableCheckBox(false); // we put all checkboxes at false
            if(parametersPath != null) // allow to load only one file included in the parametersPath
            {
                try
                {
                    list.UnionWith(ResultsViewModel.Get().getNamesGraphs(parametersPath + Path.DirectorySeparatorChar)); // we obtain the name of every chart in the file charts.xml
                    listpath.Add(parametersPath);
                    NbRecording++;
                }
                catch (IOException)
                {
                    MessageBox.Show("An error occured when loading directory : " + Path.GetFileName(parametersPath), "Import Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                if (Directory.Exists(path))
                {
                    if (lastRecord) // if the choice "Last Record" is selected, we should load only one file
                    {
                        List<string> temp = new List<string>();
                        temp.AddRange(Directory.EnumerateDirectories(path));
                        if (temp.Count > 0)
                        {
                            try
                            {
                                list.UnionWith(ResultsViewModel.Get().getNamesGraphs(temp[temp.Count - 1] + Path.DirectorySeparatorChar)); //we retrieve the last record
                                listpath.Add(temp[temp.Count - 1]);
                                NbRecording++;
                            }
                            catch (IOException)
                            {
                                MessageBox.Show("An error occured when loading directory : " + Path.GetFileName(temp[temp.Count - 1]), "Import Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                        }

                    }
                    else
                    {
                        foreach (string s in Directory.EnumerateDirectories(path)) //foreach folder in session folder
                        {
                            DateTime date = Tools.getDateFromPath(s);
                            if (date.CompareTo(maxDate) <= 0 && date.CompareTo(minDate) >= 0) // if the date is between the minimum date and the maximum date
                            {
                                try
                                {
                                    list.UnionWith(ResultsViewModel.Get().getNamesGraphs(s + Path.DirectorySeparatorChar));
                                    listpath.Add(s);
                                    NbRecording++;
                                }
                                catch (IOException)
                                {
                                    MessageBox.Show("An error occured when loading directory : " + Path.GetFileName(s), "Import Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                }

                            }
                        }
                    }
                }
            }

            if(NbRecording<1) // it is necessary to have at least 1 file
            {
                choiceResultView.buttonOK.IsEnabled = false;
            }
            bool agit=false,armsmot = false, face=false,audio=false;
            // it checks in the "charts.xml" file the kinf of charts that it can display
            foreach (string word in list)
            {
                if (word.ToLower().Contains("hips"))
                {
                    agit = true;
                    choiceResultView.chkAgitationHips.IsEnabled = true;
                }
                else if (word.ToLower().Contains("knees"))
                {
                    agit = true;
                    choiceResultView.chkAgitationLKnee.IsEnabled = true;
                    choiceResultView.chkAgitationRKnee.IsEnabled = true;
                }
                else if (word.ToLower().Contains("right knee"))
                {
                    agit = true;
                    choiceResultView.chkAgitationRKnee.IsEnabled = true;
                }
                else if (word.ToLower().Contains("left knee"))
                {
                    agit = true;
                    choiceResultView.chkAgitationLKnee.IsEnabled = true;
                }
                else if (word.ToLower().Contains("shoulders"))
                {
                    agit = true;
                    choiceResultView.chkAgitationLShoulder.IsEnabled = true;
                    choiceResultView.chkAgitationRShoulder.IsEnabled = true;
                }
                else if (word.ToLower().Contains("right shoulder"))
                {
                    agit = true;
                    choiceResultView.chkAgitationRShoulder.IsEnabled = true;
                }
                else if (word.ToLower().Contains("left shoulder"))
                {
                    agit = true;
                    choiceResultView.chkAgitationLShoulder.IsEnabled = true;
                }
                else if (word.ToLower().Contains("arms") && word.ToLower().Contains("crossed"))
                {
                    armsmot = true;
                    choiceResultView.chkArmsCrossed.IsEnabled = true;
                }
                else if (word.ToLower().Contains("hands were not joined") || word.ToLower().Contains("hands joined counter")
                    || word.ToLower().Contains("hands joined duration"))
                {
                    armsmot = true;
                    choiceResultView.chkHandsJoined.IsEnabled = true;
                }
                else if (word.ToLower().Contains("hands"))
                {
                    agit = true;
                    choiceResultView.chkAgitationLHand.IsEnabled = true;
                    choiceResultView.chkAgitationRHand.IsEnabled = true;
                }
                else if (word.ToLower().Contains("right hand"))
                {
                    agit = true;
                    choiceResultView.chkAgitationRHand.IsEnabled = true;
                }
                else if (word.ToLower().Contains("left hand"))
                {
                    agit = true;
                    choiceResultView.chkAgitationLHand.IsEnabled = true;
                }
                else if (word.ToLower().Contains("emotions") || word.ToLower().Contains("faces"))
                {
                    face = true;
                    choiceResultView.chkEmotion.IsEnabled = true;
                }
                else if (word.ToLower().Contains("look"))
                {
                    face = true;
                    choiceResultView.chkLookDirec.IsEnabled = true;
                }
                else if (word.ToLower().Contains("syllable") )
                {
                    audio = true;
                    choiceResultView.chkNumberSyllables.IsEnabled = true;
                }
                
            }

            if(agit)
                choiceResultView.chkAgitation.IsEnabled = true;

            if (armsmot)
                choiceResultView.chkArmsMotion.IsEnabled = true;
            
            if(face)
                choiceResultView.chkFace.IsEnabled = true;
            
            if(audio)
                choiceResultView.chkAudio.IsEnabled = true;
            
        }

        /// <summary>
        /// Initialize all checkbox with the parameters
        /// </summary>
        public void IsEnableCheckBox(bool value)
        {
            choiceResultView.chkAgitation.IsEnabled = value;
            choiceResultView.chkAgitationHips.IsEnabled = value;
            choiceResultView.chkAgitationLHand.IsEnabled = value;
            choiceResultView.chkAgitationRHand.IsEnabled = value;
            choiceResultView.chkAgitationLKnee.IsEnabled = value;
            choiceResultView.chkAgitationRKnee.IsEnabled = value;
            choiceResultView.chkAgitationLShoulder.IsEnabled = value;
            choiceResultView.chkAgitationRShoulder.IsEnabled = value;

            choiceResultView.chkArmsMotion.IsEnabled = value;
            choiceResultView.chkArmsCrossed.IsEnabled = value;
            choiceResultView.chkHandsJoined.IsEnabled = value;

            choiceResultView.chkFace.IsEnabled = value;
            choiceResultView.chkEmotion.IsEnabled = value;
            choiceResultView.chkLookDirec.IsEnabled = value;

            choiceResultView.chkAudio.IsEnabled = value;
            choiceResultView.chkNumberSyllables.IsEnabled = value;
        }

        public void ValueOfComboBoxChanged(string newValue)
        {
            var item = choiceResultView.cmbDate.SelectedValue as KeyValuePair<string, string>?;
            if (item != null)
                SetDate(item?.Value);
        }

        /// <summary>
        /// Allow to initialize the minDate and the maxDate in function of the user choice in the comboBox
        /// </summary>
        /// <param name="date">the choice of the user in the comboBox</param>
        public void SetDate(string date)
        {
            lastRecord = false;
            switch(date)
            {
                case "Today":
                    maxDate = minDate = DateTime.Today;
                    break;
                case "Yesterday":
                    minDate = DateTime.Today.AddDays(-1);
                    maxDate = minDate;
                    break;
                case "7 last days":
                    minDate = DateTime.Today.AddDays(-7);
                    maxDate = DateTime.Today;
                    break;
                case "15 last days":
                    minDate = DateTime.Today.AddDays(-15);
                    maxDate = DateTime.Today;
                    break;
                case "Last month":
                    minDate = DateTime.Today.AddMonths(-1);
                    maxDate = DateTime.Today;
                    break;
                case "Personalized":
                    ChoiceDate cd = new ChoiceDate();
                    cd.ShowDialog();
                    minDate = cd._BeginDate;
                    maxDate = cd._EndDate;
                    break;
                case "Last Record":
                    lastRecord = true;
                    break;
            }
        }

        /// <summary>
        /// verifiy if in a panel of a checkboxes, there is at least one checkbox which is checked
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsAtLeastOneCheckBoxIsChecked(Panel panel)
        {
            foreach (object child in panel.Children)
            {
                if( child is Panel)
                {
                    var temp = IsAtLeastOneCheckBoxIsChecked((Panel)child);
                    if (temp == true)
                        return true;
                }
                if (child is CheckBox)
                {
                    if(((CheckBox)child).IsChecked==true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
