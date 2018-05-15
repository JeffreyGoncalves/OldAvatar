using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmotionRecognizer
{
    /// <summary>
    /// EigenMethod class 
    /// </summary>
    /// <remarks>
    /// Author: Florian ELMALEK - Library: 
    /// </remarks>
    public class EigenMethod
    {
                
        //Temporary variable for coordinates
        protected double xa, xb, ya, yb;
        //Number of eigenvalues we want to use (only the last values are meaningful)
        protected static int nEigenValue = 3;

        //all information we need in both case : database for face to recognize
        protected static double[,] data;
        //Mean vectors
        protected static double[] vectorMean;
        protected static double[] vectorMeanM;
        protected static double[] vectorMeanEyeL;
        protected static double[] vectorMeanEyeR;
        //data for the face to recognize, we work only with ditances
        protected static double[,] distanceData;
        protected static double[,] distanceMouth;
        protected static double[,] distanceEyeLeft;
        protected static double[,] distanceEyeRight;
        //eigenVectors 
        protected static double[,] eigenVectors;
        protected static double[,] eigenVectorsM;
        protected static double[,] eigenVectorsEyeL;
        protected static double[,] eigenVectorsEyeR;
        //Index which will allow us to extract only eigenValues we want
        protected static int[] indexOflastEigenValues = new int[nEigenValue];
        protected static int[] indexOflastEigenValuesM = new int[nEigenValue];
        protected static int[] indexOflastEigenValuesEyeL = new int[nEigenValue];
        protected static int[] indexOflastEigenValuesEyeR = new int[nEigenValue];
        //Covariance Matrix
        protected static double[,] Cov;
        protected static double[,] CovM;
        protected static double[,] CovEyeL;
        protected static double[,] CovEyeR;
        //EigenVectors extracted
        protected static double[,] lastEigenVectors;
        protected static double[,] lastEigenVectorsM;
        protected static double[,] lastEigenVectorsEyeL;
        protected static double[,] lastEigenVectorsEyeR;
        //All matrix of calculated distances for each face and each case
        public double[,] epsilon { get; protected set; }
        public double[,] epsilonM { get; protected set; }
        public double[,] epsilonEyeL { get; protected set; }
        public double[,] epsilonEyeR { get; protected set; }
        public double[,] vEpsilon { get; protected set; }
        public double[,] vEpsilonM { get; protected set; }
        public double[,] vEpsilonEyeL { get; protected set; }
        public double[,] vEpsilonEyeR { get; protected set; }
        
        /// <summary>
        /// Calculate the distance between two points
        /// </summary>
        /// <param name="xa">double</param>
        /// <param name="xb">double</param>
        /// <param name="ya">double</param>
        /// <param name="yb">double</param>
        /// <param name="za">double</param>
        /// <param name="zb">double</param>
        /// <returns>double result</returns>
        protected double distance(double xa, double xb, double ya, double yb, double za, double zb)
        {
            return Math.Sqrt(Math.Pow((xa - xb), 2) + Math.Pow((ya - yb), 2) + Math.Pow((za - zb), 2));
        }

        /// <summary>
        /// Allow to print vectors = useful for debug 
        /// </summary>
        /// <param name="vector">double[] vector</param>
        protected void printMatrix(double[] vector)
        {
            System.Console.WriteLine("Display vector");
            for (int i = 0; i < vector.GetLength(0); i++)
            {
                System.Console.Write(" - " + vector[i]);
            }
            System.Console.WriteLine("End of the display");
        }

        /// <summary>
        /// Print matrix =   useful for debug
        /// </summary>
        /// <param name="matrix">double[,] matrix</param>
        protected void printMatrix(double[,] matrix)
        {
            System.Console.WriteLine("Display Matrix");
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    System.Console.Write("/" + matrix[i, j]);
                }
                System.Console.WriteLine(" ligne : " + i);
            }
            System.Console.WriteLine("End of the display");
        }
        /// <summary>
        /// Subtract : Matrix - Vector
        /// </summary>
        /// <param name="data">double[,] matrix</param>
        /// <param name="vectorMean">double[] vectorMean</param>
        /// <returns>double[,]</returns>
        protected double[,] sub(double[,] data, double[] vectorMean)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    data[i, j] -= vectorMean[j];
                }
            }
            return data;
        }
        /// <summary>
        /// Create a matrix whose columns vectors are the three parameters vectors
        /// </summary>
        /// <param name="column1">double[] vector1</param>
        /// <param name="column2">double[] vector2</param>
        /// <param name="column3">double[] vector3</param>
        /// <returns>double[,] matrix</returns>
        protected double[,] concat(double[] column1, double[] column2, double[] column3)
        {
            double[,] res = new double[column1.GetLength(0), 3];
            for (int i = 0; i < 3; i++)
            {
                res[0, i] = column1[i];
                res[1, i] = column2[i];
                res[2, i] = column3[i];
            }
            return res;
        }
    }
}
