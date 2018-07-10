using LecturerTrainer.Model.Exceptions;
using LecturerTrainer.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Forms;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// The session currently open
    /// Added by Baptiste Germond
    /// </summary>
    public class Session
    {
        #region attributs
        /// <summary>
        /// Return the full name for the session
        /// </summary>
        public string sessionName
        {
            get
            {
                return userFirstName + " " + userLastName;
            }
        }

        /// <summary>
        /// Return the message when there is a session connected
        /// Use to display the message on the software window
        /// </summary>
        public string sessionLaunchMessage
        {
            get
            {
                if (this.Exists())
                    return "Session launched as " + userFirstName + " " + userLastName;
                else return "";
            }
        }
        public string userFirstName;
        public string userLastName;
        public string sessionPath;
        public String emotionsSamplesPath;
        public Dictionary<string, byte[]> personalization;
        public string themeName;
        #endregion

        #region constructor
        /// <summary>
        /// Constructor
        /// Instantiate new Session with default params
        /// </summary>
        public Session()
        {
            userFirstName = "";
            userLastName = "";
            sessionPath = null;
            personalization = new Dictionary<string, byte[]>();
            themeName = "Default";
        }
        #endregion

        #region methods
        public void eraseName()
        {
            userFirstName = "";
            userLastName = "";
        }

        public bool Exists()
        {
            if (userFirstName != "" || userLastName!="")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fill the dictionnary of the personalization with the actual color on the software
        /// </summary>
        public void fillPersoWithActual()
        {
            fillPersoWithSpecial("GeneralTextColor");
            fillPersoWithSpecial("SelectedTabColor");
            fillPersoWithSpecial("UnselectedTabColor");
            fillPersoWithSpecial("FeedbackStreamColor");   

        }

        /// <summary>
        /// Fill the personalization of a resource with the actual color of the software
        /// </summary>
        public void fillPersoWithSpecial(string resource)
        {
            if (!personalization.ContainsKey(resource))
            {
                personalization[resource] = new byte[4];
            }
           personalization[resource][0] = ((System.Windows.Media.Color)App.Current.Resources[resource]).R;
           personalization[resource][1] = ((System.Windows.Media.Color)App.Current.Resources[resource]).G;
           personalization[resource][2] = ((System.Windows.Media.Color)App.Current.Resources[resource]).B;
           personalization[resource][3] = ((System.Windows.Media.Color)App.Current.Resources[resource]).A;
        }

        /// <summary>
        /// Serialization of the session
        /// It is called when the user close his session or when he close the software when logged ins
        /// </summary>
        public void serializeSession(string pathString)
        {
            if (!System.IO.File.Exists(pathString))
            {
                using (System.IO.StreamWriter projectFile = new System.IO.StreamWriter(pathString + ".ltf"))
                {
                    projectFile.WriteLine(JsonConvert.SerializeObject(userFirstName));
                    projectFile.WriteLine(JsonConvert.SerializeObject(userLastName));
                    projectFile.WriteLine(JsonConvert.SerializeObject(sessionPath));
                    projectFile.WriteLine(JsonConvert.SerializeObject(themeName));


                    foreach (KeyValuePair<string,byte[]> Pair in personalization)
                    {
                        string temp = Pair.Key.ToString();
                        foreach (byte b in Pair.Value)
                        {
                            temp += '_' + b.ToString();
                        }
                        projectFile.WriteLine(JsonConvert.SerializeObject(temp));
                    }

                }
            }
        }

        /// <summary>
        /// Deserialize/read the session file and use the loading of the personalization
        /// Called when a user open his session
        /// </summary>
        public void deserializeSession(string pathString)
        {
            using (System.IO.StreamReader projectFile = new System.IO.StreamReader(pathString))
            {
                Main.session.userFirstName = JsonConvert.DeserializeObject<string>(projectFile.ReadLine());
                Main.session.userLastName = JsonConvert.DeserializeObject<string>(projectFile.ReadLine());
                Main.session.sessionPath = JsonConvert.DeserializeObject<string>(projectFile.ReadLine());
                Main.session.themeName = JsonConvert.DeserializeObject<string>(projectFile.ReadLine());
                DrawingSheetAvatarViewModel.Get().modifColorOpenGL(Main.session.themeName);

                while (!projectFile.EndOfStream)
                {
                    string temp = JsonConvert.DeserializeObject<string>(projectFile.ReadLine());
                    string[] tabTemp = temp.Split('_');
                    if (!personalization.ContainsKey(tabTemp[0]))
                    {
                        personalization[tabTemp[0]] = new byte[4];
                    }
                    personalization[tabTemp[0]][0] = (byte)Int32.Parse(tabTemp[1]);
                    personalization[tabTemp[0]][1] = (byte)Int32.Parse(tabTemp[2]);
                    personalization[tabTemp[0]][2] = (byte)Int32.Parse(tabTemp[3]);
                    personalization[tabTemp[0]][3] = (byte)Int32.Parse(tabTemp[4]);
                    loadPerso();
                }
            }
        }

        /// <summary>
        /// Load the personalization (color of the software) from the session file
        /// </summary>
        public void loadPerso()
        {
            foreach(KeyValuePair<string,byte[]> pair in personalization)
            {
                App.Current.Resources[pair.Key] = (Color.FromArgb(pair.Value[3], pair.Value[0], pair.Value[1], pair.Value[2]));
                if (pair.Key == "FeedbackStreamColor")
                {
                    DrawingSheetStreamViewModel.Get().changeColorFeedbacks();
                }
            }
            IconViewModel.get().setFFT((Color)App.Current.Resources["GeneralTextColor"], (Color)App.Current.Resources["UnselectedTabColor"]);
        }
       

        /// <summary>
        /// Create a new Project file at the indicated path
        /// It also creates the session's file and call the serialization method to create it
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="session"></param>
        /// Modified by Baptiste Germond
        public static void CreateSessionFolder(string path, string name, LecturerTrainer.Model.Session session)
        {
            try
            {
                string folderName = name;

                string pathString = System.IO.Path.Combine(path, folderName);

                foreach (string s in System.IO.Directory.EnumerateDirectories(path))
                {
                    if (s == pathString)
                    {
                        throw new CantCreateFileException("File already exists", "The session you want to create already exists\nPlease choose another name for your session\n folder or change the source folder");
                    }
                }

                System.IO.Directory.CreateDirectory(pathString);

                string fileName = name;

                pathString = System.IO.Path.Combine(pathString, fileName);

                session.sessionPath = pathString;
                Main.session.serializeSession(pathString);
            } catch (ArgumentException) {
                throw new CantCreateFileException("Cannot create session file.", "Some characters are not allowed in file path.");
            }

}

        /// <summary>
        /// Method to open a session
        /// </summary>
        /// <param name="pathString"></param>
        public static void openSession(string pathString)
        {
            Main.session.deserializeSession(pathString);
        }
        #endregion
    }
}
