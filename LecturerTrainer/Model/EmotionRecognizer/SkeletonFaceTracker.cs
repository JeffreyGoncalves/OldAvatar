using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace LecturerTrainer.Model.EmotionRecognizer
{
    /// <summary>
    /// All we need about the skeleton
    /// <remarks>
    /// Author : Microsoft (available in the faceTracking sample)
    /// </remarks>
    /// </summary>
    [Serializable]
    public class SkeletonFaceTracker : IDisposable
    {
        public int LastTrackedFrame { get; set; }
        public static EventHandler faceUpdated;
        private static FaceTriangle[] faceTriangles;
        public EnumIndexableCollection<FeaturePoint, PointF> facePoints {get; private set;}
        public EnumIndexableCollection<FeaturePoint, Vector3DF> facePoints3D {get; private set;}
        //X-man modification
        public  EnumIndexableCollection<AnimationUnit, float> AU { get; private set;}
        [NonSerialized] private FaceTracker faceTracker;
        // properties to toggle rendering 3D mesh, shape points
        public bool DrawFaceMesh { get; set; }
        public bool DrawShapePoints { get; set; }
        private bool lastFaceTrackSucceeded;
        //Lock variable 
        private static Object _lock = new Object();

        // array for Points to be used in shape points rendering
        //private PointF[] shapePoints;

        // defined array for the feature points
        //private Array featurePoints;

        public void Dispose()
        {   
            if (this.faceTracker != null)
            {
                this.faceTracker.Dispose();
                this.faceTracker = null;
            }
        }


        /// <summary>
        /// Updates the face tracking information for this skeleton
        /// </summary>
        internal void OnFrameReady(KinectSensor sensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage)
        {
            if (this.faceTracker == null)
            {
                try
                {
                    this.faceTracker = new FaceTracker(sensor);
                }
                catch (InvalidOperationException)
                {
                    // During some shutdown scenarios the FaceTracker
                    // is unable to be instantiated.  Catch that exception
                    // and don't track a face.
                    Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                    this.faceTracker = null;
                }
            }

            if (this.faceTracker != null)
            {
                FaceTrackFrame frame = this.faceTracker.Track(colorImageFormat, colorImage, depthImageFormat, depthImage);

                this.lastFaceTrackSucceeded = frame.TrackSuccessful;
                if (this.lastFaceTrackSucceeded)
                {
                    if (faceTriangles == null)
                    {
                        // only need to get this once.  It doesn't change.
                        faceTriangles = frame.GetTriangles();
                    }

                    this.facePoints = frame.GetProjected3DShape();
                    this.facePoints3D = frame.Get3DShape();
                    //X-man modification
                    this.AU = frame.GetAnimationUnitCoefficients();
                }
                if (faceTriangles != null && facePoints != null)
                {
                    faceUpdated(this, new FaceTrackerEventArgs(facePoints, faceTriangles));
                }
                if (FaceRecognition.compare)
                {
                    FaceRecognition.recognizer(this);
                }
                if (mouthOpened.detect)
                {
                    mouthOpened.mouthRecognizer();
                }
                if (mouthShut.detect)
                {
                    mouthShut.mouth2Recognizer();
                }
                if (lookingDirection.detect)
                {
                    lookingDirection.lookRecognizer();
                }
                if (pupilRight.detect)
                {
                    pupilRight.pupilRecognizer();
                }
            }
        }
        public void DrawFaceModel(DrawingContext drawingContext)
        {
            if (!this.lastFaceTrackSucceeded /* || this.skeletonTrackingState != SkeletonTrackingState.Tracked*/)
            {
                return;
            }

            var faceModelPts = new List<Point>();
            var faceModel = new List<FaceModelTriangle>();

            for (int i = 0; i < this.facePoints.Count; i++)
            {
                faceModelPts.Add(new Point(this.facePoints[i].X + 0.5f, this.facePoints[i].Y + 0.5f));
            }

            foreach (var t in faceTriangles)
            {
                var triangle = new FaceModelTriangle();
                triangle.P1 = faceModelPts[t.First];
                triangle.P2 = faceModelPts[t.Second];
                triangle.P3 = faceModelPts[t.Third];
                faceModel.Add(triangle);
            }

            var faceModelGroup = new GeometryGroup();
            for (int i = 0; i < faceModel.Count; i++)
            {
                var faceTriangle = new GeometryGroup();
                faceTriangle.Children.Add(new LineGeometry(faceModel[i].P1, faceModel[i].P2));
                faceTriangle.Children.Add(new LineGeometry(faceModel[i].P2, faceModel[i].P3));
                faceTriangle.Children.Add(new LineGeometry(faceModel[i].P3, faceModel[i].P1));
                faceModelGroup.Children.Add(faceTriangle);
            }

            drawingContext.DrawGeometry(Brushes.LightYellow, new Pen(Brushes.LightYellow, 1.0), faceModelGroup);
        }
        private struct FaceModelTriangle
        {
            public Point P1;
            public Point P2;
            public Point P3;
        }

    }

    public class FaceTrackerEventArgs : EventArgs
    {
        public EnumIndexableCollection<FeaturePoint, PointF> facePoints;
        public FaceTriangle[] faceTriangles;
        public FaceTrackerEventArgs(EnumIndexableCollection<FeaturePoint, PointF> facePoints, FaceTriangle[] faceTriangles) : base()
        {
            this.facePoints = facePoints;
            this.faceTriangles = faceTriangles;
        }
    }
}
