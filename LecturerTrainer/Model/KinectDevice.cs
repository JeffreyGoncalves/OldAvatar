using LecturerTrainer.Model;
using LecturerTrainer.Model.BodyAnalysis;
using LecturerTrainer.Model.EmotionRecognizer;
using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LecturerTrainer.Model
{

    public class SkeletonEventArgs : EventArgs
    {
        public Skeleton skeleton;
        public SkeletonEventArgs(Skeleton sk)
            : base()
        {
            skeleton = sk;
        }
    }
    
    public class KinectDevice : UserControl
    {
        #region static attributs
        public static KinectSensor sensor;
        public static KinectSensor oldSensor;
        public static ChooserStatus status;

        private static KinectSensorChooser chooser;

        public static bool useAutoElevation = false;
        public static int nFrame = 0;
        public static bool SwitchDraw = true;
        
        public static SkeletonFaceTracker skeletonFaceTracker;
        public static bool faceTracking = false;
        private const uint MaxMissedFrames = 100;
        private Int32 trackingID = 0;
        private int frameNumber = 0;
        private short[] depthImage;
        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;
        private byte[] colorImage;
        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;
        private readonly Dictionary<int, SkeletonFaceTracker> trackedSkeletons = new Dictionary<int, SkeletonFaceTracker>();
        //public static readonly dependencyproperty kinectproperty = dependencyproperty.register(
        //    "kinect",
        //    typeof(kinectsensor),
        //    typeof(kinectdevice),
        //    new propertymetadata(
        //        null, (o, args) => ((KinectDevice)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

        /// <summary>
        /// Clear out any trackers for skeletons we haven't heard from for a while
        /// </summary>
        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in this.trackedSkeletons)
            {
                uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value.LastTrackedFrame;
                if (missedFrames > MaxMissedFrames)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }
            

            foreach (int trackingId in trackersToRemove)
            {
                this.RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            this.trackedSkeletons[trackingId].Dispose();
            this.trackedSkeletons.Remove(trackingId);
        }

        private void ResetFaceTracking()
        {
            foreach (int trackingId in new List<int>(this.trackedSkeletons.Keys))
            {
                this.RemoveTracker(trackingId);
            }
        }

        /*************/
        #endregion
        /// <summary>
        /// Active Kinect sensor
        /// </summary>

        #region event attributs

        /// <summary>
        /// Event throw when a frame is updated
        /// </summary>
        public EventHandler skeletonUpdated;

        /// <summary>
        /// Event thrown when the current Kinect sensor has been changed.
        /// </summary>
        public static EventHandler KinectChanged;

        /// <summary>
        /// Event thrown when the current status has changed.
        /// </summary>
        public static EventHandler StatusChanged;

        #endregion

        #region Constructor and kinect connection handling

        public KinectDevice()
        {

            chooser = new KinectSensorChooser();
            sensor = null;
            oldSensor = null;
            chooser.KinectChanged += OnSensorChanged;
            chooser.PropertyChanged += OnStatusChanged;
            chooser.Start();
        }

        /// <summary>
        /// Event fired when the sensor is changed.
        /// </summary>
        /// <param name="oldSensor"></param>
        /// <param name="newSensor"></param>
        private void OnSensorChanged(object sender, KinectChangedEventArgs args)
        {
            oldSensor = args.OldSensor;
            sensor = args.NewSensor;
            status = chooser.Status;

            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= this.OnAllFramesReady;
                this.ResetFaceTracking();
            }
            if (args.NewSensor != null)
            {
                sensor.AllFramesReady += this.OnAllFramesReady;

                //sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Turn on the skeleton stream to receive skeleton frames
                sensor.SkeletonStream.Enable();
                sensor.SkeletonStream.EnableTrackingInNearRange = true;
                sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                sensor.ColorStream.Enable();
                //sensor.DepthStream.Range = DepthRange.Near;
                //Listen to the AllFramesReady event to receive KinectSensor's data
                sensor.AllFramesReady += this.OnAllFramesReady;

                // Start the sensor!
                try
                {
                    sensor.Start();
                }
                catch (IOException e)
                {
                    sensor = null;
                    //System.Console.WriteLine("IOException " + e);
                    throw new Exception();
                }
            }
            KinectChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event fired when status of the sensor is changed
        /// </summary>
        /// <param name="oldSensor"></param>
        /// <param name="newSensor"></param>
        private void OnStatusChanged(object sender, EventArgs args)
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Function which can be called to show on screen RBorder if skeleton is not completely in the Kinect field
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        private FrameEdges? RenderClippedEdges(Skeleton skeleton)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                return FrameEdges.Bottom;
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                return FrameEdges.Top;
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                return FrameEdges.Left;
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                return FrameEdges.Right;
            }
            return null;
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /// <remarks>I believe there is a issue with this function if we put 
        /// the OpenGl rendering in another thread and activate the facetracking
        /// because the variable used are not locked when read/modified
        /// </remarks>
        public void OnAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (faceTracking)
            {
                // Amirali Problem with multithreading?
                SwitchDraw = true;
                skeletonOperations(e);
                SwitchDraw = false;
                faceOperations(e);
            }
            else
            {
                SwitchDraw = true;
                skeletonOperations(e);
            }
        }
        
        private void faceOperations(AllFramesReadyEventArgs e){

            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;

            try{
                colorImageFrame = e.OpenColorImageFrame();
                depthImageFrame = e.OpenDepthImageFrame();
                
                if (colorImageFrame == null || depthImageFrame == null)
                {
                    return;
                }

                // Check for image format changes.  The FaceTracker doesn't
                // deal with that so we need to reset.
                if (this.depthImageFormat != depthImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.depthImage = null;
                    this.depthImageFormat = depthImageFrame.Format;
                }

                if (this.colorImageFormat != colorImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.colorImage = null;
                    this.colorImageFormat = colorImageFrame.Format;
                }

                // Create any buffers to store copies of the data we work with
                if (this.depthImage == null)
                {
                    this.depthImage = new short[depthImageFrame.PixelDataLength];
                }

                if (this.colorImage == null)
                {
                    this.colorImage = new byte[colorImageFrame.PixelDataLength];
                }

                colorImageFrame.CopyPixelDataTo(colorImage);
                depthImageFrame.CopyPixelDataTo(depthImage);

                if (!this.trackedSkeletons.ContainsKey(this.trackingID))
                {
                    this.trackedSkeletons.Add(this.trackingID, new SkeletonFaceTracker());
                }

                if (this.trackedSkeletons.TryGetValue(this.trackingID, out skeletonFaceTracker))
                {
                    skeletonFaceTracker.OnFrameReady(sensor, colorImageFormat, colorImage, depthImageFormat, depthImage);
                    skeletonFaceTracker.LastTrackedFrame = this.frameNumber;
                }

                this.RemoveOldTrackers(this.frameNumber);

                this.InvalidateVisual();

            } finally {
                if (colorImageFrame != null)
                {
                    colorImageFrame.Dispose();
                }

                if (depthImageFrame != null)
                {
                    depthImageFrame.Dispose();
                }
            }
        }

        private void skeletonOperations(AllFramesReadyEventArgs e){
            Skeleton[] skeletons;
            Skeleton watchedSkeleton = null;   
            SkeletonFrame skeletonFrame = null;
            try
            {
            
                skeletonFrame = e.OpenSkeletonFrame();
                if (skeletonFrame == null)
                {
                    return;
                }
                skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletons);
                watchedSkeleton = getTrackedSkeleton(skeletons, trackClosest(skeletons));

                if (watchedSkeleton != null)
                {

                    skeletonUpdated(this, new SkeletonEventArgs(watchedSkeleton));
                    // Give each tracker the updated frame.
                    if (Tools.allJointsTracked(watchedSkeleton))
                    {
                        if (Gesture.compare)
                        {
                            Gesture.testCompare(watchedSkeleton);
                        }
                        if (Posture.compare)
                        {
                            Posture.testCompare(watchedSkeleton);
                        }
                    }
                    if (Agitation.detect)
                    {
                        Agitation.testAgitation(watchedSkeleton);
                    }
                    if (HandsRaised.compare)
                    {
                        HandsRaised.testCompare(watchedSkeleton);
                    }
                    if (HandsJoined.detect)
                    {
                        HandsJoined.startDetection(watchedSkeleton);
                    }
                    if (EmotionRecognition.detect)
                    {
                        EmotionRecognition.EmotionRecognizer();
                    }
                    if (ArmsWide.compare)
                    {
                        ArmsWide.testCompare(watchedSkeleton);
                    }
                    if (ArmsCrossed.compare)
                    {
                        ArmsCrossed.testCompare(watchedSkeleton);
                    }
                    if(HandsInPocket.compare) //experimental
                    {
                        HandsInPocket.testCompare(watchedSkeleton);
                    }
                    /*Experimental movement to test the training*/
                    if (DrawingSheetAvatarViewModel.Get().isTraining)
                    {
                        if (TrainingWithAvatarViewModel.canBeInterrupted)
                        {
                            if (String.Compare(TrainingWithAvatarViewModel.AvatarGesture,"WavingTraining") == 0)
                                TrainingWithAvatarViewModel._gesture.Update(watchedSkeleton);
                            if (String.Compare(TrainingWithAvatarViewModel.AvatarGesture, "HandTraining") == 0)
                                TrainingWithAvatarViewModel._handgesture.Update(watchedSkeleton);
                            if (String.Compare(TrainingWithAvatarViewModel.AvatarGesture, "PowerTraining") == 0)
                                TrainingWithAvatarViewModel._powergesture.Update(watchedSkeleton);
                            if (String.Compare(TrainingWithAvatarViewModel.AvatarGesture, "WelcomeTraining") == 0)
                                TrainingWithAvatarViewModel._welcomegesture.Update(watchedSkeleton);
                            if(String.Compare(TrainingWithAvatarViewModel.AvatarGesture, "SaluteTraining") == 0)
                                TrainingWithAvatarViewModel._salutegesture.Update(watchedSkeleton);
                        }
                    }                    
                    if (KinectDevice.useAutoElevation)
                    {
                        ClippedEdgesElevationChange(watchedSkeleton);
                    }
                }
            }finally{
                if (skeletonFrame != null)
                {
                    skeletonFrame.Dispose();
                }
            }
        }

        /// <summary>
        /// Function which select the skeleton to track as the nearer
        /// </summary>
        /// <param name="skeletons"></param>
        /// <returns></returns>
        public static int trackClosest(Skeleton[] skeletons)
        {
            if (!sensor.SkeletonStream.AppChoosesSkeletons)
            {
                sensor.SkeletonStream.AppChoosesSkeletons = true; // Ensure AppChoosesSkeletons is set
            }

            float closestDistance = 10000f; // Start with a far enough distance
            int closestID = -1;

            foreach (Skeleton skel in skeletons.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
            {
                if (skel.Position.Z < closestDistance)
                {
                    closestID = skel.TrackingId;
                    closestDistance = skel.Position.Z;
                }
            }
            try
            {
                sensor.SkeletonStream.ChooseSkeletons(closestID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "test1");
            }
            return closestID;
        }

        /// <summary>
        /// Get the skeleton tracked.
        /// </summary>
        /// <param name="skeletons">Array of detected skeleton.</param>
        /// <param name="trackingID">Id of the tracked skeleton.</param>
        /// <returns>Tracked skeleton.</returns>
        internal static Skeleton getTrackedSkeleton(Skeleton[] skeletons, int trackingID)
        {
            if (trackingID == -1) return null;
            foreach (Skeleton sk in skeletons)
            {
                if (sk.TrackingState == SkeletonTrackingState.Tracked) return sk;
            }
            return null;
        }

        /// <summary>
        /// Find wich skeleton is watched (because there are too many skeletons in the array, we don't know why for now) [clem]
        /// </summary>
        public static Skeleton getWatchedSkeleton(Skeleton[] skeletons)
        {
            foreach (Skeleton sk in skeletons)
            {
                if (sk.Position.X != 0.0 && sk.Position.Y != 0.0)
                {
                    return sk;
                }
            }
            return null;
        }

        /// <summary>
        /// Called by OnAllFramesReady if the KinectDevice.useAutoElevation attribut is set to true
        /// </summary>
        /// <param name="skeleton"></param>
        private void ClippedEdgesElevationChange(Skeleton skeleton)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom) && !skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                changeKinectElevation(sensor, -3);
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top) && !skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                changeKinectElevation(sensor, 3);
            }
        }

        /// <summary>
        /// Change the kinect elevation.
        /// </summary>
        /// <param name="sensor">the kinect sensor</param>
        /// <param name="step">step of elevation</param>
        /// <returns>angle of the kinect after elevation</returns>
        public static int changeKinectElevation(KinectSensor sensor, int step)
        {
            int ea = sensor.ElevationAngle;
            if (sensor.MaxElevationAngle == ea || sensor.MinElevationAngle == ea)
                return ea;

            ea += step;
            if (ea < sensor.MinElevationAngle)
                ea = sensor.MinElevationAngle;
            else if (ea > sensor.MaxElevationAngle)
                ea = sensor.MaxElevationAngle;

            try
            {
                sensor.ElevationAngle = ea;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "test2");
            }
            return sensor.ElevationAngle;
        }

        public static int changeKinectElevation(double angle)
        {
            int ea = sensor.ElevationAngle;

            if (angle > sensor.MaxElevationAngle)
                angle = sensor.MaxElevationAngle;
            else if (angle < sensor.MinElevationAngle)
                angle = sensor.MinElevationAngle;

                ea = (int) angle - ea;

                return changeKinectElevation(sensor, ea);
        }

        #endregion
    }
}
