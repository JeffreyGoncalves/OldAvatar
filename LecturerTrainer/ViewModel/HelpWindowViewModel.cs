using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// This class provides some features to allow the user to have a in-app helping system 
    /// The idea is to write small texts and titles in a central file that will be red when desired and that 
    /// will display tooltips for the user. Optionnal pictures are also displayable in order to give visual 
    /// information in addition of texts
    /// </summary>
    class HelpWindowViewModel : ViewModelBase
    {
        #region fields
        private const string path = @"..\..\help\helpData.txt";
        private const string picturesPath = @"..\..\help\";
        private const string extension = ".jpg";

        /// <summary>
        /// data base of the help module 
        /// </summary>
        private string[] helpTitlePages;
        private string[] helpSubtitlePages;
        private string[] helpPages;
        private string[] helpPicturePages;

        // number of pages in the help module 
        private int _nbPages;
        // current page displayed 
        private int _currentPage;
        // title of the current page 
        private string _currentTitle;
        // subtitle of the current page 
        private string _currentSubtitle;
        // picture displayed on the current page
        private string _currentPicture;
        // text displayed on the current page 
        private string _currentText;
        #endregion

        #region constructor
        public HelpWindowViewModel()
        {
            // we read a file included in the project folder
            string[] lines = System.IO.File.ReadAllLines(path);
            string[] split = null;

            // we instantiate each array 
            helpPages = new string[lines.Length];
            helpTitlePages = new string[lines.Length];
            helpSubtitlePages = new string[lines.Length];
            helpPicturePages = new string[lines.Length];
            _nbPages = lines.Length;
            _currentPage = 0;
            // we split around a sharp character, so we won't accept any unexpected sharp in helpData.txt
            // 0 index = title 
            // 1 = subtitle 
            // 2 = text 
            // 3 = picture name 
            for (int i = 0; i < lines.Length; i++)
            {
                split = lines[i].Split('#');
                helpTitlePages[i] = split[0];
                helpSubtitlePages[i] = split[1];
                helpPages[i] = split[2];
                helpPicturePages[i] = split[3];
            }

            // default : we display the first page 
            _currentTitle = helpTitlePages[0];
            _currentSubtitle = helpSubtitlePages[0];
            _currentText = helpPages[0];
            _currentPicture = picturesPath + helpPicturePages[0] + extension;
        }
        #endregion

        #region properties
        public int nbPages
        {
            get
            {
                return _nbPages;
            }
            set
            {
                _nbPages = value;
                OnPropertyChanged("nbPages");
            }
        }

        public int currentPage
        {
            get
            {
                // more convenient for displaying to have a value > 0 
                return _currentPage + 1;
            }
            set
            {
                // so we have to substract 1 from the value we want to set and ensure that we won't be out of bounds
                if (value - 1 < _nbPages && value - 1 >= 0)
                {
                    _currentPage = value - 1;
                    // updating data - by using the bindings and not private variables we ensure that we raise OnPropertyChanged event 
                    currentTitle = helpTitlePages[_currentPage];
                    currentSubtitle = helpSubtitlePages[_currentPage];
                    currentText = helpPages[_currentPage];
                    currentPicture = picturesPath + helpPicturePages[_currentPage] + extension;
                    OnPropertyChanged("currentPage");
                }
                else
                {
                    OnPropertyChanged("currentPage");
                }
            }
        }

        public string currentTitle
        {
            get
            {
                return _currentTitle;
            }
            set
            {
                _currentTitle = value;
                OnPropertyChanged("currentTitle");
            }
        }

        public string currentSubtitle
        {
            get
            {
                return _currentSubtitle;
            }
            set
            {
                _currentSubtitle = value;
                OnPropertyChanged("currentSubtitle");
            }
        }

        public string currentPicture
        {
            get
            {
                return _currentPicture;
            }
            set
            {
                _currentPicture = value;
                OnPropertyChanged("currentPicture");
            }
        }

        public string currentText
        {
            get
            {
                return _currentText;
            }
            set
            {
                _currentText = value;
                OnPropertyChanged("currentText");
            }
        }
        #endregion
    }
}
