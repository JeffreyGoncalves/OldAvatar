using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Globalization;


namespace LecturerTrainer.Model.EmotionRecognizer
{
    /// <summary>
    /// Parsing of the database's file
    /// <remarks>
    /// Author : Florian ELMALEK
    /// </remarks>
    /// </summary>
   
    class Parsing
    {
        public double[,] data;
        public string[] emotions;
        //number of points in 3D or 2D multiply by the dimension
        private int numberOfPointsPerFace = FaceRecognition.numberOfPoints * FaceRecognition.dimension; 

        /// <summary>
        /// Parsing of the data's file
        /// </summary>
        /// <param name="pathForDb">String</param>
        /// <param name="numberOfFaces">int</param>
        public Parsing(string pathForDb, int numberOfFaces){

            data = new double[numberOfFaces, numberOfPointsPerFace];
            emotions = new string[numberOfFaces];

            //read each line : 1 line = 1 face
            if (!File.Exists(pathForDb))
                throw new System.InvalidOperationException("Problem with the path of the database");
            //Catch all lines of the file
            string[] lines = System.IO.File.ReadAllLines(@pathForDb);
            
            string[] temp;
            //'i' will represent the id and finally the number of faces
            int i = 0;
            //Read each line
            foreach (string s in lines) {
                
                if (!s.Equals('\t') || !s.Equals("\r\n") || !s.Equals('\n') || s != null ){
                    
                    temp = s.Split(new char[] { ' ', '\t' });
                    this.emotions[i] = temp.First();
                    
                    for (int j = 1; j < temp.Length-1; j++){
                        data[i, j - 1] = str2Double(temp[j]);
                    }
                    i++;
                }
            } 
        }
        /// <summary>
        /// Return the double contained in a string
        /// </summary>
        /// <param name="s">string</param>
        /// <returns>double</returns>
        public double str2Double(string s)
        {
            return Convert.ToDouble(s, new CultureInfo("en-US"));
        }
    }
}
