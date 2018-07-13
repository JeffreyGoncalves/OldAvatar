using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Accord.Math;
using Accord.Statistics;
using Accord.Math.Decompositions;
using LecturerTrainer.Model.EmotionRecognizer;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace EmotionRecognizer
{
    /// <summary>
    /// Equivalent to the EigenFace3D but this class works only on 2D points
    /// <remarks>
    /// BECAUSE OF LACKING TIME THIS CLASS HAS NEVER BEEN DEBUGGED - THIS CLASS CAN NOT BE USED FOR THE MOMENT (need update)
    /// Author : Florian ELMALEK
    /// </remarks>
    /// </summary>
    public class EigenFace : EigenMethod
    {
        //All we need to know about points : those useful for distance calculations
        static int[,] coupleDistanceMouth = { { 95, 107 }, {127, 119}, {125, 129}, {123, 131}, {121, 133}, {105, 109}, {103, 111}, {101, 113}, {99, 115}, {97,117}, 
                    {85, 103}, {85, 105}, {83, 103}, {83, 101}, {81, 101}, {81, 99}, {79, 99}, {79, 97}, {85, 107}, {79, 95}, {83, 111}, {81, 115}, {85, 109}, {79, 117}};
        static int[,] coupledistanceEyeLeft = { { 21, 25 }, { 19, 27 }, { 17, 29 }, { 21, 29 }, { 17, 25 }, { 61, 23 }, { 63, 21 }, { 65, 19 }, { 67, 17 }, { 69, 15 } };
        static int[,] coupledistanceEyeRight = { { 1, 13 }, { 3, 11 }, { 5, 9 }, { 1, 9 }, { 5, 13 }, { 49, 1 }, { 47, 3 }, { 45, 5 }, { 43, 7 } };

        //Number of all distances we get
        int n = coupleDistanceMouth.Length + coupledistanceEyeLeft.Length + coupledistanceEyeRight.Length;

        //function used for the faces' database : called just one time
        public EigenFace(double[,] data1)
        {
            //
            //INPUT : data -> matrix of all face's coordinates
            //Using the ACCORD framework. For all matrix operations
            //           
            data = data1;
            distanceData = new double[data.GetLongLength(1), n];
            distanceMouth = new double[data.GetLongLength(1), coupleDistanceMouth.Length];
            distanceEyeLeft = new double[data.GetLongLength(1), coupledistanceEyeLeft.Length];
            distanceEyeRight = new double[data.GetLongLength(1), coupledistanceEyeRight.Length];

            double[,] mReduce = new double[data.GetLowerBound(1), data.GetLowerBound(2)];
            double[,] mReduceM = new double[distanceMouth.GetLowerBound(1), distanceMouth.GetLowerBound(2)];
            double[,] mReduceEyeL = new double[distanceEyeLeft.GetLowerBound(1), distanceEyeLeft.GetLowerBound(2)];
            double[,] mReduceEyeR = new double[distanceEyeRight.GetLowerBound(1), distanceEyeRight.GetLowerBound(2)];

            vectorMean = new double[data.GetLowerBound(2)];
            vectorMeanM = new double[distanceMouth.GetLowerBound(2)];
            vectorMeanEyeL = new double[distanceEyeLeft.GetLowerBound(2)];
            vectorMeanEyeR = new double[distanceEyeRight.GetLowerBound(2)];

            Cov = new double[data.GetLowerBound(2), data.GetLowerBound(2)];
            CovM = new double[distanceMouth.GetLowerBound(2), distanceMouth.GetLowerBound(2)];
            CovEyeL = new double[distanceEyeLeft.GetLowerBound(2), distanceEyeLeft.GetLowerBound(2)];
            CovEyeR = new double[distanceEyeRight.GetLowerBound(2), distanceEyeRight.GetLowerBound(2)];

            eigenVectors = new double[Cov.GetLowerBound(1), Cov.GetLowerBound(2)];
            eigenVectorsM = new double[CovM.GetLowerBound(1), CovM.GetLowerBound(2)];
            eigenVectorsEyeL = new double[CovEyeL.GetLowerBound(1), CovEyeL.GetLowerBound(2)];
            eigenVectorsEyeR = new double[CovEyeR.GetLowerBound(1), CovEyeR.GetLowerBound(2)];

            epsilon = new double[nEigenValue, data.GetLowerBound(1)];
            epsilonM = new double[nEigenValue, data.GetLowerBound(1)];
            epsilonEyeL = new double[nEigenValue, data.GetLowerBound(1)];
            epsilonEyeR = new double[nEigenValue, data.GetLowerBound(1)];

            for (int j = 0; j < data.GetLongLength(1); j++)
            {
                for (int i = 0; i < coupleDistanceMouth.Length; i++)
                {
                    xa = data[j, coupleDistanceMouth[i, 0]];
                    xb = data[j, coupleDistanceMouth[i, 1]];
                    ya = data[j, (coupleDistanceMouth[i, 0]) + 1];
                    yb = data[j, (coupleDistanceMouth[i, 1]) + 1];
                    distanceData[j, i] = distance(xa, xb, ya, yb, 0, 0);
                    distanceMouth[j, i] = distance(xa, xb, ya, yb, 0, 0);
                }
                for (int i = 0; i < coupledistanceEyeLeft.Length; i++)
                {
                    xa = data[j, coupledistanceEyeLeft[i, 0]];
                    xb = data[j, coupledistanceEyeLeft[i, 1]];
                    ya = data[j, (coupledistanceEyeLeft[i, 0]) + 1];
                    yb = data[j, (coupledistanceEyeLeft[i, 1]) + 1];
                    distanceData[j, i] = distance(xa, xb, ya, yb, 0, 0);
                    distanceEyeLeft[j, i] = distance(xa, xb, ya, yb, 0, 0);
                }
                for (int i = 0; i < coupledistanceEyeRight.Length; i++)
                {
                    xa = data[j, coupledistanceEyeRight[i, 0]];
                    xb = data[j, coupledistanceEyeRight[i, 1]];
                    ya = data[j, (coupledistanceEyeRight[i, 0]) + 1];
                    yb = data[j, (coupledistanceEyeRight[i, 1]) + 1];
                    distanceData[j, i] = distance(xa, xb, ya, yb, 0, 0);
                    distanceEyeRight[j, i] = distance(xa, xb, ya, yb, 0, 0);
                }
            }

            //calculations of mean for each data matrix
            vectorMean = data.Mean(2);
            vectorMeanM = distanceMouth.Mean(2);
            vectorMeanEyeL = distanceEyeLeft.Mean(2);
            vectorMeanEyeR = distanceEyeRight.Mean(2);

            //calculations of reduced matrix
            mReduce = data.Subtract(vectorMean, 1);
            mReduceM = distanceMouth.Subtract(vectorMeanM, 1);
            mReduceEyeL = distanceEyeLeft.Subtract(vectorMeanEyeL, 1);
            mReduceEyeR = distanceEyeRight.Subtract(vectorMeanEyeR, 1);

            //calculations of covariance matrix
            Cov = mReduce.Covariance();
            CovM = mReduceM.Covariance();
            CovEyeL = mReduceEyeL.Covariance();
            CovEyeR = mReduceEyeR.Covariance();

            //calculations of eigenvectors : is equivalent to the matlab function [V,D] = eig(A)            
            eigenVectors = new EigenvalueDecomposition(Cov).Eigenvectors;
            eigenVectorsM = new EigenvalueDecomposition(CovM).Eigenvectors;
            eigenVectorsEyeL = new EigenvalueDecomposition(CovEyeL).Eigenvectors;
            eigenVectorsEyeR = new EigenvalueDecomposition(CovEyeR).Eigenvectors;

            //with the three last eigenvalues 
            indexOflastEigenValues[0] = eigenVectors.GetLowerBound(2) - 2; indexOflastEigenValues[1] = eigenVectors.GetLowerBound(2) - 1; indexOflastEigenValues[2] = eigenVectors.GetLowerBound(2);
            indexOflastEigenValuesM[0] = eigenVectorsM.GetLowerBound(2) - 2; indexOflastEigenValuesM[1] = eigenVectorsM.GetLowerBound(2) - 1; indexOflastEigenValuesM[2] = eigenVectorsM.GetLowerBound(2);
            indexOflastEigenValuesEyeL[0] = eigenVectorsEyeL.GetLowerBound(2) - 2; indexOflastEigenValuesEyeL[1] = eigenVectorsEyeL.GetLowerBound(2) - 1; indexOflastEigenValuesEyeL[2] = eigenVectorsEyeL.GetLowerBound(2);
            indexOflastEigenValuesEyeR[0] = eigenVectorsEyeR.GetLowerBound(2) - 2; indexOflastEigenValuesEyeR[1] = eigenVectorsEyeR.GetLowerBound(2) - 1; indexOflastEigenValuesEyeR[2] = eigenVectorsEyeR.GetLowerBound(2);

            epsilon = eigenVectors.GetColumns(indexOflastEigenValues).Transpose().Multiply(mReduce.Transpose());
            epsilonM = eigenVectorsM.GetColumns(indexOflastEigenValuesM).Transpose().Multiply(mReduceM.Transpose());
            epsilonEyeL = eigenVectorsEyeL.GetColumns(indexOflastEigenValuesEyeL).Transpose().Multiply(mReduceEyeL.Transpose());
            epsilonEyeR = eigenVectorsEyeR.GetColumns(indexOflastEigenValuesEyeR).Transpose().Multiply(mReduceEyeR.Transpose());
        }

        //function used for the face toRecognize : called for each frame sent by the kinect
        public EigenFace(EnumIndexableCollection<FeaturePoint, PointF> toRec) {

            double[,] vReduce = new double[1, data.GetLowerBound(1)];
            double[,] vReduceM = new double[1, distanceMouth.GetLowerBound(1)];
            double[,] vReduceEyeL = new double[1, distanceEyeLeft.GetLowerBound(1)];
            double[,] vReduceEyeR = new double[1, distanceEyeRight.GetLowerBound(1)];

            double[,] distanceToRec = new double[n, 0];
            double[,] distanceMouthToRec = new double[coupleDistanceMouth.Length, 0];
            double[,] distanceEyeLToRec = new double[coupledistanceEyeLeft.Length, 0];
            double[,] distanceEyeRToRec = new double[coupledistanceEyeRight.Length, 0];

            
            //Calculate distances for the face to recognize
            for (int i = 0; i < coupleDistanceMouth.Length; i++)
            {
                xa = toRec.ElementAt(coupleDistanceMouth[i, 0]).X;
                xb = toRec.ElementAt(coupleDistanceMouth[i, 1]).X;
                ya = toRec.ElementAt(coupleDistanceMouth[i, 0] + 1).Y;
                yb = toRec.ElementAt(coupleDistanceMouth[i, 1] + 1).Y;
                distanceToRec[i, 0] = distance(xa, xb, ya, yb, 0, 0);
                distanceMouthToRec[i, 0] = distance(xa, xb, ya, yb, 0, 0);
            }
            for (int i = 0; i < coupledistanceEyeLeft.Length; i++)
            {
                xa = toRec.ElementAt(coupledistanceEyeLeft[i, 0]).X;
                xb = toRec.ElementAt(coupledistanceEyeLeft[i, 1]).X;
                ya = toRec.ElementAt(coupledistanceEyeLeft[i, 0] + 1).Y;
                yb = toRec.ElementAt(coupledistanceEyeLeft[i, 1] + 1).Y;
                distanceToRec[i, 0] = distance(xa, xb, ya, yb, 0, 0);
                distanceMouthToRec[i, 0] = distance(xa, xb, ya, yb, 0, 0);
            }
            for (int i = 0; i < coupledistanceEyeRight.Length; i++)
            {
                xa = toRec.ElementAt(coupledistanceEyeRight[i, 0]).X;
                xb = toRec.ElementAt(coupledistanceEyeRight[i, 1]).X;
                ya = toRec.ElementAt(coupledistanceEyeRight[i, 0] + 1).Y;
                yb = toRec.ElementAt(coupledistanceEyeRight[i, 1] + 1).Y;
                distanceToRec[i, 0] = distance(xa, xb, ya, yb, 0, 0);
                distanceMouthToRec[i, 0] = distance(xa, xb, ya, yb, 0, 0);
            }

            //calculations for the face to recognize
            vReduce = distanceToRec.Subtract(vectorMean, 1);
            vReduceM = distanceMouthToRec.Subtract(vectorMeanM, 1);
            vReduceEyeL = distanceEyeLeft.Subtract(vectorMeanEyeL, 1);
            vReduceEyeR = distanceEyeRight.Subtract(vectorMeanEyeR, 1);

            vEpsilon = eigenVectors.GetColumns(indexOflastEigenValues).Transpose().Multiply(vReduce.Transpose());
            vEpsilonM = eigenVectorsM.GetColumns(indexOflastEigenValuesM).Transpose().Multiply(vReduceM.Transpose());
            vEpsilonEyeL = eigenVectorsEyeL.GetColumns(indexOflastEigenValuesEyeL).Transpose().Multiply(vReduceEyeL.Transpose());
            vEpsilonEyeR = eigenVectorsEyeR.GetColumns(indexOflastEigenValuesEyeR).Transpose().Multiply(vReduceEyeR.Transpose());
        }        
    }
}
