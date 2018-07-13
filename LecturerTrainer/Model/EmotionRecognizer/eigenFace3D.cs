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
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Windows.Forms;

namespace EmotionRecognizer
{
    /// <summary>
    /// EigenFace3D class contains all the necessary for eigenface with 3D points : inherits EigenMethod
    /// </summary>
    /// <remarks>
    /// Author: Florian ELMALEK - Library: Accord is required
    /// </remarks>
    class EigenFace3D : EigenMethod
    {
        //All variables "couple" represent couple of points for meaningful distance of the face. All information available in the documentation ( '/3' because each point has "x,y,z" coordinates )
        static int[,] coupleDistanceMouth3D = { { 94 / 3, 193 / 3 }, { 265 / 3, 268 / 3 }, { 244 / 3, 250 / 3 }, { 247 / 3, 253 / 3 }, { 262 / 3, 127 / 3 }, { 22 / 3, 28 / 3 }, { 241 / 3, 259 / 3 }, { 247 / 3, 253 / 3 }, { 118 / 3, 28 / 3 }, { 256 / 3, 334 / 3 }, { 259 / 3, 337 / 3 } };
        static int[,] coupleDistanceEyeLeft3D = { { 286/3 , 304 /3}, { 292/3 , 288 /3}, { 202/3 , 205 /3}, { 64/3 , 67 /3}, { 58/3 , 73 /3}, { 214/3 , 217 /3}, { 310/3 , 328 /3}, { 316/3 , 322 /3}};
        static int[,] coupleDistanceEyeRight3D = { { 313 / 3, 331 /3}, { 319/3 , 325 /3}, { 220/3 , 223 /3}, { 157/3 , 172 /3}, { 163/3 , 166 /3}, { 208/3 , 211 /3}, { 289/3 , 307 /3}, { 295/3 , 301 /3}};
        
        //number of data available after gathering all eigenfeatures mouth + eyes left + eyes right
        static int n3D = coupleDistanceMouth3D.GetLength(0) + coupleDistanceEyeLeft3D.GetLength(0) + coupleDistanceEyeRight3D.GetLength(0);

        //Z coordinates
        double za, zb;
        
        /// <summary>
        /// This method initialize all needed information about the user's face.
        /// Picked coordinates of the user's face recorded during the training session at the creation of his session
        /// All initialized variables available in the statics fields of eigenMethod
        /// </summary>
        /// <remarks>
        /// Called just one time, at the beginning of the session
        /// </remarks>
        /// <param name="data1">All information available at initialization : corresponding to the data stored about the user's face</param>
        public EigenFace3D(double[,] data1)
        {
            data = data1;     
            //Total of all distances
            distanceData = new double[data.GetLength(0), n3D];
            //Distances by eigenfeature
            distanceMouth = new double[data.GetLength(0), coupleDistanceMouth3D.GetLength(0)];
            distanceEyeLeft = new double[data.GetLength(0), coupleDistanceEyeLeft3D.GetLength(0)];
            distanceEyeRight = new double[data.GetLength(0), coupleDistanceEyeRight3D.GetLength(0)];
            //Declaration and initialization of reduced matrix
            double[,] mReduce = new double[data.GetLength(0), data.GetLength(1)];
            double[,] mReduceM = new double[distanceMouth.GetLength(0), distanceMouth.GetLength(1)];
            double[,] mReduceEyeL = new double[distanceEyeLeft.GetLength(0), distanceEyeLeft.GetLength(1)];
            double[,] mReduceEyeR = new double[distanceEyeRight.GetLength(0), distanceEyeRight.GetLength(1)];
            //Declaration and initialization of mean vectors
            vectorMean = new double[data.GetLength(1)];
            vectorMeanM = new double[distanceMouth.GetLength(1)];
            vectorMeanEyeL = new double[distanceEyeLeft.GetLength(1)];
            vectorMeanEyeR = new double[distanceEyeRight.GetLength(1)];
            //Declaration and initialization of covariance matrix
            Cov = new double[data.GetLength(1), data.GetLength(1)];
            CovM = new double[distanceMouth.GetLength(1), distanceMouth.GetLength(1)];
            CovEyeL = new double[distanceEyeLeft.GetLength(1), distanceEyeLeft.GetLength(1)];
            CovEyeR = new double[distanceEyeRight.GetLength(1), distanceEyeRight.GetLength(1)];
            //Declaration and initialization of all eigenVectors 
            eigenVectors = new double[Cov.GetLength(0), Cov.GetLength(1)];
            eigenVectorsM = new double[CovM.GetLength(0), CovM.GetLength(1)];
            eigenVectorsEyeL = new double[CovEyeL.GetLength(0), CovEyeL.GetLength(1)];
            eigenVectorsEyeR = new double[CovEyeR.GetLength(0), CovEyeR.GetLength(1)];
            //Declaration and initialization for epsilon variables = mean distance for each face
            epsilon = new double[nEigenValue, data.GetLength(0)];
            epsilonM = new double[nEigenValue, data.GetLength(0)];
            epsilonEyeL = new double[nEigenValue, data.GetLength(0)];
            epsilonEyeR = new double[nEigenValue, data.GetLength(0)];
            //int n = coupleDistanceMouth3D.GetLength(0);

            //For each couple of points indicated by "couple" variables, we stored the distance between the two points in "distance" variables
            //DistanceDate stored all distances, contrary to others distance variables which contains the distances for one eigenfeature.
            for (int j = 0; j < data.GetLength(0); j++)
            {
                for (int i = 0; i < coupleDistanceMouth3D.GetLength(0); i++)
                {                      
                    xa = data[j, coupleDistanceMouth3D[i, 0]];
                    xb = data[j, coupleDistanceMouth3D[i, 1]];
                    ya = data[j, (coupleDistanceMouth3D[i, 0]) + 1];
                    yb = data[j, (coupleDistanceMouth3D[i, 1]) + 1];
                    za = data[j, (coupleDistanceMouth3D[i, 0]) + 2];
                    zb = data[j, (coupleDistanceMouth3D[i, 1]) + 2];
                    distanceData[j, i] = distance(xa, xb, ya, yb, za, zb);
                    distanceMouth[j, i] = distance(xa, xb, ya, yb, za, zb);
                }
                for (int i = 0; i < coupleDistanceEyeLeft3D.GetLength(0); i++)
                {
                    xa = data[j, coupleDistanceEyeLeft3D[i, 0]];
                    xb = data[j, coupleDistanceEyeLeft3D[i, 1]];
                    ya = data[j, (coupleDistanceEyeLeft3D[i, 0]) + 1];
                    yb = data[j, (coupleDistanceEyeLeft3D[i, 1]) + 1];
                    za = data[j, (coupleDistanceEyeLeft3D[i, 0]) + 2];
                    zb = data[j, (coupleDistanceEyeLeft3D[i, 1]) + 2];
                    distanceData[j, i] = distance(xa, xb, ya, yb, za, zb);
                    distanceEyeLeft[j, i] = distance(xa, xb, ya, yb, za, zb);
                }
                //n = coupleDistanceEyeLeft3D.GetLength(0) + coupleDistanceMouth3D.GetLength(0);
                for (int i = 0; i < coupleDistanceEyeRight3D.GetLength(0); i++)
                {
                    xa = data[j, coupleDistanceEyeRight3D[i, 0]];
                    xb = data[j, coupleDistanceEyeRight3D[i, 1]];
                    ya = data[j, (coupleDistanceEyeRight3D[i, 0]) + 1];
                    yb = data[j, (coupleDistanceEyeRight3D[i, 1]) + 1];
                    za = data[j, (coupleDistanceEyeRight3D[i, 0]) + 2];
                    zb = data[j, (coupleDistanceEyeRight3D[i, 1]) + 2];
                    distanceData[j, i] = distance(xa, xb, ya, yb, za, zb);
                    distanceEyeRight[j, i] = distance(xa, xb, ya, yb, za, zb);
                }
            }

            //calculations of mean for each data matrix
            vectorMean = distanceData.Mean(3);
            vectorMeanM = distanceMouth.Mean(3);
            vectorMeanEyeL = distanceEyeLeft.Mean(3);
            vectorMeanEyeR = distanceEyeRight.Mean(3);
            
            //calculations of reduced matrix. REMARK : "sub()" is a method created by myself you can find at the end of this class.
            mReduce = sub(distanceData, vectorMean);
            mReduceM = sub(distanceMouth, vectorMeanM);
            mReduceEyeL = sub(distanceEyeLeft, vectorMeanEyeL);
            mReduceEyeR = sub(distanceEyeRight,vectorMeanEyeR);
            
            //calculations of covariance matrix
            Cov = mReduce.Covariance();
            CovM = mReduceM.Covariance();
            CovEyeL = mReduceEyeL.Covariance();
            CovEyeR = mReduceEyeR.Covariance();

            //calculations of eigenvectors : EigenvalueDecomposition(Cov) is equivalent to the matlab function [EigenVectors,DiagonalMatrix] = eig(Matrix)
            eigenVectors = new EigenvalueDecomposition(Cov).Eigenvectors;
            eigenVectorsM = new EigenvalueDecomposition(CovM).Eigenvectors;
            eigenVectorsEyeL = new EigenvalueDecomposition(CovEyeL).Eigenvectors;
            eigenVectorsEyeR = new EigenvalueDecomposition(CovEyeR).Eigenvectors;
            
            //Recording the 'n' last eigenvalues 
            indexOflastEigenValues[0] = eigenVectors.GetLength(1) - 3 ; indexOflastEigenValues[1] = eigenVectors.GetLength(1) - 2; indexOflastEigenValues[2] = eigenVectors.GetLength(1) - 1;
            indexOflastEigenValuesM[0] = eigenVectorsM.GetLength(1) - 3 ; indexOflastEigenValuesM[1] = eigenVectorsM.GetLength(1) - 2; indexOflastEigenValuesM[2] = eigenVectorsM.GetLength(1) - 1;
            indexOflastEigenValuesEyeL[0] = eigenVectorsEyeL.GetLength(1) - 3 ; indexOflastEigenValuesEyeL[1] = eigenVectorsEyeL.GetLength(1) - 2; indexOflastEigenValuesEyeL[2] = eigenVectorsEyeL.GetLength(1) - 1;
            indexOflastEigenValuesEyeR[0] = eigenVectorsEyeR.GetLength(1) - 3 ; indexOflastEigenValuesEyeR[1] = eigenVectorsEyeR.GetLength(1) - 2; indexOflastEigenValuesEyeR[2] = eigenVectorsEyeR.GetLength(1) - 1;
            
            //Catch every 'n' last eigenVectors in a submatrix
            lastEigenVectors = new double[eigenVectors.GetLength(0), nEigenValue];
            lastEigenVectors = concat(eigenVectors.ToArray().GetColumn(indexOflastEigenValues[0]), eigenVectors.ToArray().GetColumn(indexOflastEigenValues[1]), eigenVectors.ToArray().GetColumn(indexOflastEigenValues[2]));
            //Calculate the distance of faces using the all eigenFeatures
            epsilon = lastEigenVectors.Transpose().Multiply(mReduce.Transpose()); 
            
            lastEigenVectorsM = new double[eigenVectorsM.GetLength(0), nEigenValue];
            lastEigenVectorsM = concat(eigenVectorsM.ToArray().GetColumn(indexOflastEigenValuesM[0]), eigenVectorsM.ToArray().GetColumn(indexOflastEigenValuesM[1]), eigenVectorsM.ToArray().GetColumn(indexOflastEigenValuesM[2]));
            //Calculate the distance of faces using the eigenMouth
            epsilonM = lastEigenVectorsM.Transpose().Multiply(mReduceM.Transpose());

            lastEigenVectorsEyeL = new double[eigenVectorsEyeL.GetLength(0), nEigenValue];
            lastEigenVectorsEyeL = concat(eigenVectorsEyeL.ToArray().GetColumn(indexOflastEigenValuesEyeL[0]), eigenVectorsEyeL.ToArray().GetColumn(indexOflastEigenValuesEyeL[1]), eigenVectorsEyeL.ToArray().GetColumn(indexOflastEigenValuesEyeL[2]));
            //Calculate the distance of faces using the eigenEyeL
            epsilonEyeL = lastEigenVectorsEyeL.Transpose().Multiply(mReduceEyeL.Transpose());

            lastEigenVectorsEyeR = new double[eigenVectorsEyeR.GetLength(0), nEigenValue];
            lastEigenVectorsEyeR = concat(eigenVectorsEyeR.ToArray().GetColumn(indexOflastEigenValuesEyeR[0]), eigenVectorsEyeR.ToArray().GetColumn(indexOflastEigenValuesEyeR[1]), eigenVectorsEyeR.ToArray().GetColumn(indexOflastEigenValuesEyeR[2]));
            //Calculate the distance of faces using the eigenEyeR
            epsilonEyeR = lastEigenVectorsEyeR.Transpose().Multiply(mReduceEyeR.Transpose());
            
        }

        /// <summary>
        /// This method compare the face obtained by the kinect to the database of the user to display the current facial emotion
        /// </summary>
        /// <remarks>
        /// Called each second
        /// </remarks>
        /// <param name="toRec">All data about the face to recognize = coordinates of all 3D points of the face</param>
        public EigenFace3D(EnumIndexableCollection<FeaturePoint, Vector3DF> toRec)
        {
            //it is required with the library and functions we use to work on matrix and not vectors ... [n,1] is a solution of this "problem"
            double[,] distanceToRec = new double[n3D, 1];
            double[,] distanceMouthToRec = new double[coupleDistanceMouth3D.Length, 1];
            double[,] distanceEyeLToRec = new double[coupleDistanceEyeLeft3D.Length, 1];
            double[,] distanceEyeRToRec = new double[coupleDistanceEyeRight3D.Length, 1];
            
            double[,] vReduce = new double[1, data.GetLength(0)];
            double[,] vReduceM = new double[1, distanceMouth.GetLength(0)];
            double[,] vReduceEyeL = new double[1, distanceEyeLeft.GetLength(0)];
            double[,] vReduceEyeR = new double[1, distanceEyeRight.GetLength(0)];
            int n = coupleDistanceMouth3D.GetLength(0);           
            //Calculate distances for the face to recognize
            for (int i = 0; i < coupleDistanceMouth3D.GetLength(0); i++)
            {
                xa = toRec.ElementAt(coupleDistanceMouth3D[i, 0]).X;
                xb = toRec.ElementAt(coupleDistanceMouth3D[i, 1]).X;
                ya = toRec.ElementAt(coupleDistanceMouth3D[i, 0] + 1).Y;
                yb = toRec.ElementAt(coupleDistanceMouth3D[i, 1] + 1).Y;
                za = toRec.ElementAt(coupleDistanceMouth3D[i, 0] + 2).Z;
                zb = toRec.ElementAt(coupleDistanceMouth3D[i, 1] + 2).Z;
                distanceToRec[i, 0] = distance(xa, xb, ya, yb, za, zb);
                distanceMouthToRec[i, 0] = distance(xa, xb, ya, yb, za, zb);
            }
            for (int i = 0; i < coupleDistanceEyeLeft3D.GetLength(0); i++)
            {
                xa = toRec.ElementAt(coupleDistanceEyeLeft3D[i, 0]).X;
                xb = toRec.ElementAt(coupleDistanceEyeLeft3D[i, 1]).X;
                ya = toRec.ElementAt(coupleDistanceEyeLeft3D[i, 0] + 1).Y;
                yb = toRec.ElementAt(coupleDistanceEyeLeft3D[i, 1] + 1).Y;
                za = toRec.ElementAt(coupleDistanceEyeLeft3D[i, 0] + 2).Z;
                zb = toRec.ElementAt(coupleDistanceEyeLeft3D[i, 1] + 2).Z;
                distanceToRec[n+i, 0] = distance(xa, xb, ya, yb, za, zb);
                distanceEyeLToRec[i, 0] = distance(xa, xb, ya, yb, za, zb);
            }
            n = coupleDistanceMouth3D.GetLength(0) + coupleDistanceEyeLeft3D.GetLength(0);
            for (int i = 0; i < coupleDistanceEyeRight3D.GetLength(0); i++)
            {
                xa = toRec.ElementAt(coupleDistanceEyeRight3D[i, 0]).X;
                xb = toRec.ElementAt(coupleDistanceEyeRight3D[i, 1]).X;
                ya = toRec.ElementAt(coupleDistanceEyeRight3D[i, 0] + 1).Y;
                yb = toRec.ElementAt(coupleDistanceEyeRight3D[i, 1] + 1).Y;
                za = toRec.ElementAt(coupleDistanceEyeRight3D[i, 0] + 2).Z;
                zb = toRec.ElementAt(coupleDistanceEyeRight3D[i, 1] + 2).Z;
                distanceToRec[i+n, 0] = distance(xa, xb, ya, yb, za, zb);
                distanceEyeRToRec[i, 0] = distance(xa, xb, ya, yb, za, zb);
            }
            
            //calculations for the face to recognize
            vReduce = sub(distanceToRec,vectorMean);
            vReduceM = sub(distanceMouthToRec,vectorMeanM);
            vReduceEyeL = sub(distanceEyeLeft,vectorMeanEyeL);
            vReduceEyeR = sub(distanceEyeRight,vectorMeanEyeR);
            //calculations of distances between each faces
            vEpsilon = lastEigenVectors.Transpose().Multiply(vReduce);
            vEpsilonM = lastEigenVectorsM.Transpose().Multiply(vReduceM);
            vEpsilonEyeL = lastEigenVectorsEyeL.Transpose().Multiply(vReduceEyeL);
            vEpsilonEyeR = lastEigenVectorsEyeR.Transpose().Multiply(vReduceEyeR);            
        }       
    }
}
