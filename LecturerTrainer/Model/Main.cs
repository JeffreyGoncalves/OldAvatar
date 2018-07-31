using LecturerTrainer.Model.EmotionRecognizer;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LecturerTrainer.Model;
using LecturerTrainer.View;

namespace LecturerTrainer.Model
{
    public class Main
    {
        #region static attributs
        public static event EventHandler isReady;
        public static KinectDevice kinect { get; set; }
        public static bool[] jointsToObserve = new bool[20];
        public static Session session;
        #endregion

        #region attributs
        public AudioAnalysis.AudioProvider audioProvider;
        public EmotionRecognizer.FaceRecognition faceRecognition;
        #endregion

        #region constructor
        public Main()
        {
            session = new Session();
            
            for (int i = 0; i < 20; i++)
            {
                /*Binding in sideToolsViewModel in comments*/
                jointsToObserve[i] = true;
            }
            try
            {
                //Initiate the FaceRecognition
                //FaceRecognition.init();
                kinect = new KinectDevice();

                if (isReady != null)
                {
                    isReady(this, EventArgs.Empty);
                }   
                faceRecognition = new EmotionRecognizer.FaceRecognition();
                audioProvider = new AudioAnalysis.AudioProvider(kinect);
                
                

            }
            catch (System.InvalidOperationException e)
            {
                if (e.Source != null)
                {
                    MessageBox.Show("aqui " + e.Message + " in :\n" + e.StackTrace + "\n\n", "System.InvalidOperationException");
                }
                System.Environment.Exit(0);
            }
            catch (Exception)
            {
                new ErrorMessageBox("KinectError", "No Kinect plugged-in").ShowDialog();
                System.Environment.Exit(0);
                
            }

        }
        #endregion    
    }
}
