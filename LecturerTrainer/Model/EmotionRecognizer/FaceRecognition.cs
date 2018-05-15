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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using EmotionRecognizer;
using Microsoft.Kinect;
using System.IO;

namespace LecturerTrainer.Model.EmotionRecognizer
{
    /// <summary>
    /// 
    /// This class gathers all information about the user's face and provide the algorithm which find the nearest face in database with the current face.
    /// <remarks>
    /// Author : Florian ELMALEK
    /// </remarks>
    /// </summary>
    /// 
    public partial class FaceRecognition
    {
        //Temporary variable which will contain data of the current face : obtained by the kinect each second
        private static EigenMethod eigen { get; set; }
        //All data of the user's database. Loaded and parsed from a file
        private static Parsing dataParsed { get; set; }
        //Choose the dimension you want to use for the face analysis - 3 or 2
        public static int dimension = 3;
        //Indicate the number of points : depending on the set used (2D or 3D points)
        public static int numberOfPoints = 121;
        //Variable containing the emotion of the nearest faces
        private static string nearest = "";
        //Boolean which indicates if the emotionRecognition is activated by the user
        public static bool compare = false;
        //Path of the file for the user's database
        private static string nameForDb = @"..\..\Model\EmotionRecognizer\allFrames.txt";

        public static EventHandler<EmotionEventArgs> emotionEvent;

        /// <summary>
        /// Function of initialisation : launch every functions to have all needed data for emotionRecognition
        /// </summary>
        public static void init()
        {           
            StreamReader sr = new StreamReader(nameForDb, Encoding.Default);
            //Count the exact number of lines the database
            int i = 0;
            string buff = "a";
            while (buff != null)
            {
                buff = sr.ReadLine();
                if (buff == null) { 
                    break;
                }
                else {
                    i++;
                }
            }

            //Try to load files with data (path, numberOfLines)          
            dataParsed = new Parsing(nameForDb, i);
                
            //Catch results of eigenFace
            if (dimension == 2)
                eigen = new EigenFace(dataParsed.data);

            else if (dimension == 3)
                eigen = new EigenFace3D(dataParsed.data); 
                               
            else
                throw new System.InvalidOperationException("Problem of dimension in FaceRecognizer");
        }

        public static void recognizer(KinectSensor sensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton watchedSkeletonAbs)
        {   

            SkeletonFaceTracker skeletonFaceTracker = new SkeletonFaceTracker();
            /*skeletonFaceTracker.OnFrameReady(sensor, colorImageFormat, colorImage, depthImageFormat, depthImage, watchedSkeletonAbs);*/
            recognizer(skeletonFaceTracker);
        }

        /// <summary>
        /// Provide the emotion of the nearest face of the current face get with the kinect
        /// </summary>
        /// 
        /// <param name="skeletonFaceTracker">SkeletonFaceTracker</param>
        public static void recognizer(SkeletonFaceTracker skeletonFaceTracker)
        {
            EigenMethod eigenToRec = null;
            if (dimension == 3)
            {
                //Get all needed date from the 3D set of points : eigenValues and distances between each faces
                eigenToRec = new EigenFace3D(skeletonFaceTracker.facePoints3D);
            }
            else if (dimension == 2)
            {
                //Get all needed date from the 2D set of points : eigenValues and distances between each faces
                eigenToRec = new EigenFace(skeletonFaceTracker.facePoints);
            }

            //For each variable whose name contains "espilon" we work on [i,j] with 0 <= i,j <= 3 corresponding to the coordinates of the 3 eigenVectors we chose to keep

            //Calculate the nearest face
            double minI = -1;
            int indice = -1;
            for (int i = 0; i < eigen.epsilon.GetLength(1); i++)
            {
                if (minI == -1)
                {
                    minI = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilon[0, 0] - eigen.epsilon[0, 0], 2) + Math.Pow(eigenToRec.vEpsilon[1, 0] - eigen.epsilon[1, 0], 2) + Math.Pow(eigenToRec.vEpsilon[2, 0] - eigen.epsilon[2, 0], 2)));
                    indice = i;
                }
                else
                {
                    if (minI > (Math.Sqrt(Math.Pow(eigenToRec.vEpsilon[0, 0] - eigen.epsilon[0, i], 2) + Math.Pow(eigenToRec.vEpsilon[1, 0] - eigen.epsilon[1, i], 2) + Math.Pow(eigenToRec.vEpsilon[2, 0] - eigen.epsilon[2, i], 2))))
                    {
                        minI = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilon[0, 0] - eigen.epsilon[0, i], 2) + Math.Pow(eigenToRec.vEpsilon[1, 0] - eigen.epsilon[1, i], 2) + Math.Pow(eigenToRec.vEpsilon[2, 0] - eigen.epsilon[2, i], 2)));
                        indice = i;
                    }
                }
            }
            //calculate the nearest face according only to the mouth
            double min = -1;
            int indiceM = -1;
            for (int i = 1; i < eigen.epsilonM.GetLength(1); i++)
            {
                if (min == -1)
                {
                    min = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonM[0, 0] - eigen.epsilonM[0, 0], 2) + Math.Pow(eigenToRec.vEpsilonM[1, 0] - eigen.epsilonM[1, 0], 2) + Math.Pow(eigenToRec.vEpsilonM[2, 0] - eigen.epsilonM[2, 0], 2)));
                    indiceM = i;
                }
                else
                {
                    if (min > (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonM[0, 0] - eigen.epsilonM[0, i], 2) + Math.Pow(eigenToRec.vEpsilonM[1, 0] - eigen.epsilonM[1, i], 2) + Math.Pow(eigenToRec.vEpsilonM[2, 0] - eigen.epsilonM[2, i], 2))))
                    {
                        min = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonM[0, 0] - eigen.epsilonM[0, i], 2) + Math.Pow(eigenToRec.vEpsilonM[1, 0] - eigen.epsilonM[1, i], 2) + Math.Pow(eigenToRec.vEpsilonM[2, 0] - eigen.epsilonM[2, i], 2)));
                        indiceM = i;
                    }
                }
            }
            //calculate the nearest face according only to the left eye
            min = -1;
            int indiceEyeL = -1;
            for (int i = 1; i < eigen.epsilonEyeL.GetLength(1); i++)
            {
                if (min == -1)
                {
                    min = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeL[0, 0] - eigen.epsilonEyeL[0, 0], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[1, 0] - eigen.epsilonEyeL[1, 0], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[2, 0] - eigen.epsilonEyeL[2, 0], 2)));
                    indiceEyeL = i;
                }
                else
                {
                    if (min > (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeL[0, 0] - eigen.epsilonEyeL[0, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[1, 0] - eigen.epsilonEyeL[1, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[2, 0] - eigen.epsilonEyeL[2, i], 2))))
                    {
                        min = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeL[0, 0] - eigen.epsilonEyeL[0, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[1, 0] - eigen.epsilonEyeL[1, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[2, 0] - eigen.epsilonEyeL[2, i], 2)));
                        indiceEyeL = i;
                    }
                }
            }
            //calculate the nearest face according only to the right eye
            min = -1;
            int indiceEyeR = -1;
            for (int i = 1; i < eigen.epsilonEyeR.GetLength(1); i++)
            {
                if (min == -1)
                {
                    min = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeR[0, 0] - eigen.epsilonEyeR[0, 0], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[1, 0] - eigen.epsilonEyeR[1, 0], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[2, 0] - eigen.epsilonEyeR[2, 0], 2)));
                    indiceEyeR = i;
                }
                else
                {
                    if (min > (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeR[0, 0] - eigen.epsilonEyeR[0, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[1, 0] - eigen.epsilonEyeR[1, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[2, 0] - eigen.epsilonEyeR[2, i], 2))))
                    {
                        min = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeR[0, 0] - eigen.epsilonEyeR[0, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[1, 0] - eigen.epsilonEyeR[1, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[2, 0] - eigen.epsilonEyeR[2, i], 2)));
                        indiceEyeR = i;
                    }
                }
            }
            //calculate the nearest face according to all calculations above
            min = -1;
            int indiceAll = -1;
            for (int i = 1; i < eigen.epsilon.GetLength(1); i++)
            {
                if (min == -1)
                {
                    min = (Math.Sqrt(Math.Pow(eigenToRec.vEpsilonM[0, 0] - eigen.epsilonM[0, 0], 2) + Math.Pow(eigenToRec.vEpsilonM[1, 0] - eigen.epsilonM[1, 0], 2) + Math.Pow(eigenToRec.vEpsilonM[2, 0] - eigen.epsilonM[2, 0], 2)));
                    indiceAll = i;
                }
                else
                {
                    if (min > (Math.Sqrt(Math.Pow(eigenToRec.vEpsilon[0, 0] - eigen.epsilonM[0, i], 2) + Math.Pow(eigenToRec.vEpsilon[1, 0] - eigen.epsilonM[1, i], 2) + Math.Pow(eigenToRec.vEpsilon[2, 0] - eigen.epsilonM[2, i], 2))) + Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeL[0, 0] - eigen.epsilonEyeL[0, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[1, 0] - eigen.epsilonEyeL[1, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[2, 0] - eigen.epsilonEyeL[2, i], 2)) + Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeR[0, 0] - eigen.epsilonEyeR[0, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[1, 0] - eigen.epsilonEyeR[1, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[2, 0] - eigen.epsilonEyeR[2, i], 2)))
                    {
                        min = Math.Sqrt(Math.Pow(eigenToRec.vEpsilon[0, 0] - eigen.epsilonM[0, i], 2) + Math.Pow(eigenToRec.vEpsilon[1, 0] - eigen.epsilonM[1, i], 2) + Math.Pow(eigenToRec.vEpsilon[2, 0] - eigen.epsilonM[2, i], 2)) + Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeL[0, 0] - eigen.epsilonEyeL[0, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[1, 0] - eigen.epsilonEyeL[1, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeL[2, 0] - eigen.epsilonEyeL[2, i], 2)) + Math.Sqrt(Math.Pow(eigenToRec.vEpsilonEyeR[0, 0] - eigen.epsilonEyeR[0, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[1, 0] - eigen.epsilonEyeR[1, i], 2) + Math.Pow(eigenToRec.vEpsilonEyeR[2, 0] - eigen.epsilonEyeR[2, i], 2));
                        indiceAll = i;
                    }
                }
            }
            int bestIndice = indice;
            if (min < minI)
                bestIndice = indiceAll;

            //the nearest emotion is updated
            nearest = dataParsed.emotions[bestIndice];
            emotionEvent(null, new EmotionEventArgs(nearest));
        }        
    }

    public class EmotionEventArgs : EventArgs
    {
        public string nearest;
        public EmotionEventArgs(string n)
        {
            nearest = n;
        }
    }
}
