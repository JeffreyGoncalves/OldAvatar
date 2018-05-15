using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Geometry tools used in other classes.
    /// </summary>
    /// <remarks>Author: Clement Michard</remarks>
    class Geometry
    {

        #region Skeleton infos getters

        /// <summary>
        /// Get the origin of the skeleton reference.
        /// </summary>
        /// <param name="sk">Skeleton observed.</param>
        /// <returns>Point3D representing the origin in the skeleton reference.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static Point3D getOrigin(Skeleton sk)
        {
            return new Point3D(sk.Joints[JointType.ShoulderCenter].Position.X, sk.Joints[JointType.ShoulderCenter].Position.Y, sk.Joints[JointType.ShoulderCenter].Position.Z);
        }

        /// <summary>
        /// Compute the scale of the skeleton. The Y distance between shoulder center and spine is equivalent to 1
        /// </summary>
        /// <param name="skeleton">Skeleton concerned.</param>
        /// <returns>The computed ratio.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static double getRatio(Skeleton skeleton)
        {
            return 1 / (skeleton.Joints[JointType.ShoulderCenter].Position.Y - skeleton.Joints[JointType.Spine].Position.Y);
        }

        #endregion getters

        #region References

        /// <summary>
        /// Convert a point from skeleton reference to screen reference.
        /// </summary>
        /// <param name="pSkeleton">The point to convert.</param>
        /// <param name="sk">The skeleton.</param>
        /// <returns>The point in screen reference.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static Point refSkeletonToScreen(Point3D pSkeleton, Skeleton sk)
        {
            Point3D refKinect = refSkeletonToKinect(pSkeleton, sk);
            return refKinectToScreen(refKinect);
        }

        /// <summary>
        /// Convert a point from kinect reference to skeleton reference.
        /// </summary>
        /// <param name="pKinect">The point to convert.</param>
        /// <param name="sk">The skeleton.</param>
        /// <returns>The point in skeleton reference.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static Point3D refKinectToSkeleton(Point3D pKinect, Skeleton sk)
        {
            double ratio = getRatio(sk);
            Point3D origin = getOrigin(sk);
            /*Point3D ptRef = getPtRef(sk);
            Point3D R1 = moveRef(ptRef, origin);
            Point3D R2 = new Point3D(Math.Sqrt(Tools.distanceSquare), R1.Y, 0);
            return rotateZ(homoth(moveRef(pKinect,origin), ratio), angle(R1, R2));
            */
            return new Point3D(ratio * (pKinect.X - origin.X), ratio * (pKinect.Y - origin.Y), ratio * (pKinect.Z - origin.Z));
        }

        /// <summary>
        /// Convert a point from skeleton reference to kinect reference.
        /// </summary>
        /// <param name="pSkeleton">The point to convert.</param>
        /// <param name="actualSkeleton">The skeleton observed.</param>
        /// <returns>The point in kinect reference.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static Point3D refSkeletonToKinect(Point3D pSkeleton, Skeleton sk)
        {
            double ratio = getRatio(sk);
            Point3D origin = getOrigin(sk);
            return new Point3D(pSkeleton.X / ratio + origin.X, pSkeleton.Y / ratio + origin.Y, pSkeleton.Z / ratio + origin.Z);
        }

        /// <summary>
        /// Convert a point from kinect reference to screen reference.
        /// </summary>
        /// <param name="p">The point to convert.</param>
        /// <returns>The point in screen reference.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static Point refKinectToScreen(Point3D p)
        {
            return new Point((p.X + 1) * MainWindow.drawingSheet.ActualWidth / 2, MainWindow.drawingSheet.ActualHeight - (p.Y + 1) * MainWindow.drawingSheet.ActualHeight / 2);
        }

        #endregion

        #region Tools

        /// <summary>
        /// Return the square distance between two points.
        /// </summary>
        /// <param name="p1"¨>First point.</param>
        /// <param name="p2">Second point.</param>
        /// <returns>The square distance.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static double distanceSquare(Point3D p1, Point3D p2)
        {
            return ((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z));
        }

        #endregion

    }

    /// <summary>
    /// Describe a point.
    /// </summary>
    /// <remarks>Author: Clement Michard</remarks>
    public class Point3D
    {

        #region PROPERTIES

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Construct the Point3D from its coordinates.
        /// </summary>
        /// <remarks>Author: Clement Michard</remarks>
        public Point3D(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        /// <summary>
        /// Construct a Point3D from a SkeletonPoint (Microsoft.Kinect)
        /// </summary>
        /// <param name="p">The SkeletonPoint to convert as a Point3D.</param>
        /// <remarks>Author: Clement Michard</remarks>
        public Point3D(SkeletonPoint p)
        {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
        }

        #endregion

        #region CONVERTERS

        /// <summary>
        /// Convert the point to string.
        /// </summary>
        /// <returns>String describing the point.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public override string ToString()
        {
            return "" + Tools.format(X) + " " + Tools.format(Y) + " " + Tools.format(Z);
        }

        /// <summary>
        /// Convert a Point3D to Point.
        /// </summary>
        /// <returns>The converted Point.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public Point toPoint()
        {
            return new Point(X, Y);
        }

        /// <summary>
        /// Convert a Point3D to SkeletonPoint (Microsoft.Kinect)
        /// </summary>
        /// <returns>The SkeletonPoint.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public SkeletonPoint toSkPoint()
        {
            SkeletonPoint p = new SkeletonPoint();
            p.X = (float)this.X; p.Y = (float)this.Y; p.Z = (float)this.Z;
            return p;
        }

        #endregion
    }
}
