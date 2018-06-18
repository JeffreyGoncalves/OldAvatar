using LecturerTrainer.Model.Exceptions;
using LecturerTrainer.View;
using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace LecturerTrainer.Model
{
    class ReplayAvatar
    {
        #region fields
        /// <summary>
        /// The timer used for the scrolling
        /// </summary>
        private DispatcherTimer timeToUpdate;

        /// <summary>
        /// The replayview model instance
        /// </summary>
        private ReplayViewModel replayViewModel;

        /// <summary>
        /// Directory containing skeleton frames
        /// </summary>
        private string avatarDir;

        /// <summary>
        /// Current skeleton frame number
        /// </summary>
        private static int currentSkeletonNumber;

        public static int CurrentSkeletonNumber
        {
            get
            {
                return currentSkeletonNumber;
            }
            set
            {
                currentSkeletonNumber = value;
            }
        }

        public static bool realTime = true;

        public static int offset = 0;

        /// <summary>
        /// Current skeleton object
        /// </summary>
        private Skeleton currentSkeleton;

        public Skeleton CurrentSkeleton
        {
            get
            {
                return currentSkeleton;
            }
        }

        // Face

        /// <summary>
        /// Directory containing face tracking elements
        /// </summary>
        private string faceDir;


        /// <summary>
        /// Indicates if face tracking is enabled or not
        /// </summary>
        private bool faceTracking = false;

        /// <summary>
        /// XmlReader for loading the skeletons data (body)
        /// </summary>
        private static XmlReader xmlSkeletonReader;

        /// <summary>
        /// The sorted list (the key being the frame number) of loaded skeletons
        /// </summary>
        private static SortedList<int, Tuple<int, Skeleton>> skeletonsList;

        public static SortedList<int, Tuple<int, Skeleton>> SkeletonList
        {
            get
            {
                return skeletonsList;
            }
        }
      
        #endregion

        #region constructor
        /// <summary>
        /// Time ellapsed in ms. It is not the real elapsed time, it depends on the speed
        /// </summary>

        public ReplayAvatar(string avDir, string fDir, ReplayViewModel rvm, int num)
        {
            //Console.Out.WriteLine("replayavatar constructor");
            avatarDir = avDir;
            faceDir = fDir;
            replayViewModel = rvm;
            faceTracking = false;
            DrawingSheetAvatarViewModel.Get().drawFaceInReplay = false;

            //Load the list of the skeletons
            // TODO fix this
            currentSkeletonNumber = num;

            skeletonsList = new SortedList<int, Tuple<int, Skeleton>>();
            skeletonsList = LoadSkeletonsFromXML(avatarDir);
            
            //Initilise the first skeleton to be displayed, depending if the replay is on play or stop
            if (currentSkeletonNumber > skeletonsList.Count)
                currentSkeleton = skeletonsList[skeletonsList.Count - 1].Item2;
            else
                currentSkeleton = skeletonsList[currentSkeletonNumber].Item2;

            // draw the first avatar
            DrawingSheetAvatarViewModel.Get().skToDrawInReplay = currentSkeleton;
            DrawingSheetAvatarViewModel.Get().forceDraw(currentSkeleton, false);

            // init of the DispatcherTimer that is used for the replay
            Tools.initStopWatch();
            timeToUpdate = new DispatcherTimer();
            timeToUpdate.Interval = TimeSpan.FromMilliseconds(31.2);
            timeToUpdate.IsEnabled = true;
            timeToUpdate.Stop();
            timeToUpdate.Tick += nextSkeleton;
            timeToUpdate.Tick += ReplayViewModel.Get().nextFeedbackList;
            timeToUpdate.Tick += DrawingSheetAvatarViewModel.Get().draw;
            timeToUpdate.Tick += changeSlider;
        }
        /// <summary>
        /// Constructor for skeleton scrolling without face elements
        /// </summary>
        /// <param name="skDir"></param>
        /// <param name="rvm"></param>
        public ReplayAvatar(string skDir, ReplayViewModel rvm, int num) : this(skDir, "", rvm, num) { }
        #endregion

        #region replay methods

        private void changeSlider(object sender, EventArgs evt)
        {
            if (realTime)
               ReplayView.Get().changeValueOfSlider((int)Tools.getStopWatch());
            else
                ReplayView.Get().changeValueOfSlider((int)Tools.getStopWatch() - offset);
        }
        
        /// <summary>
        /// Deserializes an object, can return null if an exception occured 
        /// </summary>
        /// <typeparam name="T">A serializable type</typeparam>
        /// <param name="fileName"></param>
        /// <returns>An object T that can be null</returns>
        /*public static T DeserializeObject<T>(string fileName)
        {
            try
            {
                Stream stream = File.Open(fileName, FileMode.Open);
                IFormatter formatter = new BinaryFormatter();
                T temp = (T)formatter.Deserialize(stream);
                stream.Close();
                return temp;
            }
            catch (IOException ex)
            {
                Console.WriteLine("Exception thrown :" + ex.Message);
                return default(T);
            }
        }*/

        /// <summary>
        /// update the current skeleton in the folder
        /// </summary>
        /// <returns></returns>
        private void nextSkeleton(object sender, EventArgs evt)
        {
            if (currentSkeletonNumber < skeletonsList.Count)
            {
                currentSkeleton = skeletonsList[(int)currentSkeletonNumber].Item2;
            }
            else
                currentSkeleton = null;

            if (currentSkeleton != null)
            {
                if (faceDir != "")
                {
                    try
                    {
                        FaceDataWrapper fdw = loadFaceWFrame(faceDir, currentSkeletonNumber);
                        DrawingSheetAvatarViewModel.Get().drawFaceInReplay = true;
                        DrawingSheetAvatarViewModel.Get().drawFace(fdw.depthPointsList, fdw.colorPointsList, fdw.faceTriangles);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                // We only draw the last skeleton
                DrawingSheetAvatarViewModel.Get().skToDrawInReplay = currentSkeleton;
                setDisplayedTime();
            }
            else
            {
                replayViewModel.stopButtonCommand();
            }
            currentSkeletonNumber += 1;
        }
                /*
        private static int count = 0;
        /// <summary>
        /// update the current skeleton in the folder
        /// </summary>
        /// <returns></returns>
        private void nextSkeleton(object sender, EventArgs evt)
        {
            count++;

            elapsedVideoTime += (ReplayViewModel.normalSpeed);
            setDisplayedTime();

            if (faceDir != "" && count % 2 == 0)
            {
                if (currentSkeletonNumber < skeletonsList.Count)
                    currentSkeleton = skeletonsList[(int)currentSkeletonNumber];
                else
                    currentSkeleton = null;

                if (currentSkeleton != null)
                {
                    try
                    {
                        FaceDataWrapper fdw = loadFaceWFrame(faceDir, (int)currentSkeletonNumber);
                        DrawingSheetAvatarViewModel.Get().drawFaceInReplay = true;
                        DrawingSheetAvatarViewModel.Get().drawFace(fdw.depthPointsList, fdw.colorPointsList, fdw.faceTriangles);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    replayViewModel.stopButtonCommand();
                }

                currentSkeletonNumber += 1;
            }
            else if(faceDir == "") 
            {
                if (currentSkeletonNumber < skeletonsList.Count)
                    currentSkeleton = skeletonsList[(int)currentSkeletonNumber];
                else
                    currentSkeleton = null;

                if (currentSkeleton != null)
                {
                    try
                    {
                        FaceDataWrapper fdw = loadFaceWFrame(faceDir, (int)currentSkeletonNumber);
                        DrawingSheetAvatarViewModel.Get().drawFaceInReplay = true;
                        DrawingSheetAvatarViewModel.Get().drawFace(fdw.depthPointsList, fdw.colorPointsList, fdw.faceTriangles);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    replayViewModel.stopButtonCommand();
                }

                currentSkeletonNumber += 1;
            }

            // We only draw the last skeleton
            DrawingSheetAvatarViewModel.Get().skToDrawInReplay = currentSkeleton;
}*/

        /// <summary>
        /// Updates the displayed time
        /// </summary>
        public static void setDisplayedTime()
        {
            ReplayViewModel.Get().ElapsedTime = FormatTime(Tools.getStopWatch() - offset);
        }

        private static  string FormatTime(long time)
        {
            var _timeMS = (int)time;
            int h, m, s/*, ms*/;
            string stringTime = "";

            h = _timeMS / 3600000;
            if (h < 10)
                stringTime += "0";
            stringTime += h + ":";
            
            m = (_timeMS % 3600000) / 60000;
            if (m < 10)
                stringTime += "0";
            stringTime += m + ":";

            s = ((_timeMS % 3600000) % 60000) / 1000;
            if (s < 10)
                stringTime += "0";
            stringTime += s;
            // not very usefull to display on the UI
            /*stringTime += ".";
            ms = ((_timeMS % 3600000) % 60000) % 1000;
            if (ms < 100)
                stringTime += "0";
            if (ms < 10)
                stringTime += "0";
            stringTime += ms;*/
            return stringTime;
        }
        

        /// <summary>
        /// Starts the scrolling
        /// </summary>
        public void Start()
        {
            Tools.startStopWatch();
            timeToUpdate.Start();
        }

        /// <summary>
        /// Stops the scrolling
        /// </summary>
        public void Pause()
        {
            Tools.stopStopWatch();
            timeToUpdate.Stop();
        }

        /// <summary>
        /// Stops and resets the scrolling
        /// </summary>
        public void Stop()
        {
            Tools.stopStopWatch();
            timeToUpdate.Stop();
            currentSkeletonNumber = 0;
            currentSkeleton = skeletonsList[currentSkeletonNumber].Item2;
            DrawingSheetAvatarViewModel.Get().skToDrawInReplay = currentSkeleton;
            DrawingSheetAvatarViewModel.Get().forceDraw(currentSkeleton, false);
        }

        #endregion

        #region XmlSkeletonLoading

        private static int skCount = 0;

        /// <summary>
        /// Load all the skeletons from a file
        /// </summary>
        /// <param name="path">The path of the skeletonData</param>
        /// <returns>The sorted list of skeletons</returns>
        /// <author>Amirali Ghazi, modified by Alban Descottes</author>
        public static SortedList<int, Tuple<int, Skeleton>> LoadSkeletonsFromXML(String path)
        {
            skCount = 0;
            var skeletonSortedListWithTime = new SortedList<int, Tuple<int, Skeleton>>();
            SortedList<int, Skeleton> skeletonsSortedList = new SortedList<int, Skeleton>();
            XmlReaderSettings settings = new XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
            xmlSkeletonReader = XmlReader.Create(path, settings);

            xmlSkeletonReader.MoveToContent();
            while (xmlSkeletonReader.Read())
            {
                if (xmlSkeletonReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlSkeletonReader.Name == "Skeleton_" + skCount)
                    {
                        Skeleton sk = new Skeleton();
                        xmlSkeletonReader.MoveToAttribute(0);
                        int timeElapsed = Convert.ToInt32(xmlSkeletonReader.Value);
                        xmlSkeletonReader.MoveToAttribute(1);
                        SkeletonTrackingState skTrackingState = (SkeletonTrackingState)Enum.Parse(typeof(SkeletonTrackingState), xmlSkeletonReader.Value);
                        xmlSkeletonReader.MoveToContent();
                        sk.TrackingState = skTrackingState;
                        
                        Joint currentJoint = new Joint();
                        XmlReader xmlSkeletonJointsReader = xmlSkeletonReader.ReadSubtree();
                        xmlSkeletonJointsReader.MoveToContent();
                        while (xmlSkeletonJointsReader.Read())
                        {
                            if (xmlSkeletonJointsReader.IsStartElement())
                            {
                                string jointName = xmlSkeletonJointsReader.Name;
                                SkeletonPoint jointPoints = new SkeletonPoint();

                                if (xmlSkeletonJointsReader.AttributeCount == 4)
                                {
                                    JointType currentJointType = (JointType)Enum.Parse(typeof(JointType), jointName);

                                    currentJoint = sk.Joints[currentJointType];

                                    xmlSkeletonJointsReader.MoveToAttribute(0);
                                    JointTrackingState trackingState = (JointTrackingState)Enum.Parse(typeof(JointTrackingState), xmlSkeletonJointsReader.Value);
                                    currentJoint.TrackingState = trackingState;

                                    for (int attInd = 1; attInd < xmlSkeletonJointsReader.AttributeCount; ++attInd)
                                    {
                                        xmlSkeletonJointsReader.MoveToAttribute(attInd);
                                        switch (xmlSkeletonJointsReader.Name)
                                        {
                                            case "X":
                                                jointPoints.X = float.Parse(xmlSkeletonJointsReader.Value, CultureInfo.InvariantCulture);
                                                break;
                                            case "Y":
                                                jointPoints.Y = float.Parse(xmlSkeletonJointsReader.Value, CultureInfo.InvariantCulture);
                                                break;
                                            case "Z":
                                                jointPoints.Z = float.Parse(xmlSkeletonJointsReader.Value, CultureInfo.InvariantCulture);
                                                break;
                                            default:
                                                //Console.WriteLine("Error: " + xmlSkeletonJointsReader.Value, CultureInfo.InvariantCulture);
                                                break;
                                        }
                                    }
                                    currentJoint.Position = jointPoints;
                                    sk.Joints[currentJointType] = currentJoint;
                                    xmlSkeletonJointsReader.MoveToElement();
                                }
                            }
                        }
                        var tu = new Tuple<int, Skeleton>(timeElapsed , sk);
                        skeletonSortedListWithTime.Add(skCount++, tu);
                    }
                }
            }
            ReplayViewModel.timeEnd = skeletonSortedListWithTime[skeletonSortedListWithTime.Count - 1].Item1;
            return skeletonSortedListWithTime;
        }

        /// <summary>
        /// Load the skeleton 'frame' 
        /// </summary>
        /// <param name="path">The path of the skeleton data</param>
        /// <param name="frame">the frame number of the skeleton</param>
        /// <returns></returns>
        /// <author> Amirali Ghazi </author>
        public static Skeleton GetXMLSkeleton(string path, int frame)
        {
            if (frame <= 0)
                throw new XmlLoadingException("Cannot load this file", "The savefile" + path + " is incorrectly written.");
            Skeleton sk = new Skeleton();
            XmlReaderSettings settings = new XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
            xmlSkeletonReader = XmlReader.Create(path, settings);

            xmlSkeletonReader.MoveToContent();
            while (xmlSkeletonReader.Read())
            {
                if (xmlSkeletonReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlSkeletonReader.Name == "Skeleton_" + frame)
                    {
                        xmlSkeletonReader = xmlSkeletonReader.ReadSubtree();
                        xmlSkeletonReader.MoveToContent();

                        Joint currentJoint = new Joint();
                        while (xmlSkeletonReader.Read())
                        {
                            if (xmlSkeletonReader.IsStartElement())
                            {
                                string jointName = xmlSkeletonReader.Name;
                                SkeletonPoint jointPoints = new SkeletonPoint();

                                if (xmlSkeletonReader.AttributeCount == 3)
                                {
                                    JointType currentJointType = (JointType)Enum.Parse(typeof(JointType), jointName);
                                    //Console.WriteLine(currentJointType);

                                    currentJoint = sk.Joints[currentJointType];

                                    for (int attInd = 0; attInd < xmlSkeletonReader.AttributeCount; ++attInd)
                                    {
                                        xmlSkeletonReader.MoveToAttribute(attInd);
                                        switch (xmlSkeletonReader.Name)
                                        {
                                            case "X":
                                                jointPoints.X = float.Parse(xmlSkeletonReader.Value, CultureInfo.InvariantCulture);
                                                break;
                                            case "Y":
                                                jointPoints.Y = float.Parse(xmlSkeletonReader.Value, CultureInfo.InvariantCulture);
                                                break;
                                            case "Z":
                                                jointPoints.Z = float.Parse(xmlSkeletonReader.Value, CultureInfo.InvariantCulture);
                                                break;
                                            default:
                                                //Console.WriteLine("Error: " + xmlSkeletonReader.Value, CultureInfo.InvariantCulture);
                                                break;
                                        }
                                    }
                                    currentJoint.Position = jointPoints;
                                    sk.Joints[currentJointType] = currentJoint;
                                    xmlSkeletonReader.MoveToElement();
                                }
                            }
                        }
                        xmlSkeletonReader.Close();
                        return sk;
                    }
                }
            }
            throw new XmlLoadingException("Cannot load this file", "The skeleton n°" + frame + "cannot be found in " + path);
        }

        #endregion

        #region XmlfaceLoading

        #region FaceLoadingByFrame

        /// <summary>
        /// Load the faceData of the 'frame' frame   
        /// </summary>
        /// <param name="path">The path of the faceData</param>
        /// <param name="frame">The frame number</param>
        /// <returns>The faceDataWrapper containing the data needed to display the face</returns>
        /// <author> Amirali Ghazi </author>
        public FaceDataWrapper loadFaceWFrame(string path, int frame)
        {
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
            XmlReader xmlFaceReader;

            try
            {
                xmlFaceReader = System.Xml.XmlReader.Create(path, settings);

                var depthPointsList = new List<Vector3DF>(121);
                var colorPointsList = new List<Microsoft.Kinect.Toolkit.FaceTracking.PointF>(121);
                var faceTrianglesList = new List<FaceTriangle>(206);

                // This is not a pretty code but I am forced to do so 
                // because I can't instantiate the EnumIndexableCollection of Microsoft 
                for (int i = 0; i < 121; ++i)
                {
                    depthPointsList.Add(Vector3DF.Empty);
                }

                for (int i = 0; i < 121; ++i)
                {
                    colorPointsList.Add(Microsoft.Kinect.Toolkit.FaceTracking.PointF.Empty);
                }

                xmlFaceReader.MoveToContent();
                xmlFaceReader.Read();
                while (xmlFaceReader.Name != "Face_" + frame)
                {
                    xmlFaceReader.Skip();
                }
                if (xmlFaceReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlFaceReader.Name == "Face_" + frame)
                    {
                        xmlFaceReader.Read();
                        XmlReader xmlFacePoints3DReader = xmlFaceReader.ReadSubtree();
                        xmlFacePoints3DReader.MoveToContent();
                        // FacePoints3D
                        if (xmlFacePoints3DReader.Name == "FacePoints3D")
                        {

                            while (xmlFacePoints3DReader.Read() && xmlFacePoints3DReader.IsStartElement())
                            {
                                if (xmlFacePoints3DReader.Name.StartsWith("Point3D_") == false)
                                    throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");

                                int nbPoint = int.Parse(xmlFacePoints3DReader.Name.Split('_').ElementAt(1));
                                Vector3DF vector3df = new Vector3DF();
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                vector3df.X = float.Parse(xmlFacePoints3DReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                vector3df.Y = float.Parse(xmlFacePoints3DReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                vector3df.Z = float.Parse(xmlFacePoints3DReader.Value);

                                depthPointsList[nbPoint] = vector3df;
                            }
                        }
                        else throw new XmlLoadingException("Error while loading the face",
                            "Error loading the facepoints3D in the savefile.\nCannot load this file : ");

                        xmlFaceReader.Skip();
                        XmlReader xmlFacePointsReader = xmlFaceReader.ReadSubtree();
                        xmlFacePointsReader.MoveToContent();

                        // FacePoints 
                        if (xmlFacePointsReader.Name == "FacePoints")
                        {
                            while (xmlFacePointsReader.Read() && xmlFacePointsReader.IsStartElement())
                            {
                                if (xmlFacePointsReader.Name.StartsWith("Point_") == false)
                                    throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");

                                int nbPoint = int.Parse(xmlFacePointsReader.Name.Split('_').ElementAt(1));

                                xmlFacePoints3DReader.MoveToNextAttribute();
                                var x = float.Parse(xmlFacePointsReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                var y = float.Parse(xmlFacePointsReader.Value);

                                var buffer = new Microsoft.Kinect.Toolkit.FaceTracking.PointF(x, y);
                                colorPointsList[nbPoint] = buffer;

                            }
                        }
                        else throw new XmlLoadingException("Error while loading the face",
                           "Error loading the facepoints in the savefile.\nCannot load this file : ");

                        xmlFaceReader.Skip();
                        XmlReader xmlFaceTrianglesReader = xmlFaceReader.ReadSubtree();
                        xmlFaceTrianglesReader.MoveToContent();

                        // FaceTriangles
                        if (xmlFacePointsReader.Name == "FaceTriangles")
                        {

                            while (xmlFaceTrianglesReader.Read() && xmlFaceTrianglesReader.IsStartElement())
                            {
                                if (xmlFaceTrianglesReader.Name.StartsWith("FaceTriangle_") == false)
                                    throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");

                                int nbPoint = int.Parse(xmlFacePointsReader.Name.Split('_').ElementAt(1));
                                xmlFaceTrianglesReader.MoveToNextAttribute();

                                var first = int.Parse(xmlFaceTrianglesReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                var second = int.Parse(xmlFaceTrianglesReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                var third = int.Parse(xmlFaceTrianglesReader.Value);

                                FaceTriangle ft = new FaceTriangle(first, second, third);
                                faceTrianglesList.Add(ft);
                            }
                            xmlFaceReader.Read();

                        }
                        xmlFaceReader.Close();
                        return new FaceDataWrapper(depthPointsList, colorPointsList, faceTrianglesList.ToArray());
                    }
                    else
                    {
                        xmlFaceReader.Close();
                        throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");
                    }
                }
                else
                {
                    xmlFaceReader.Close();
                    throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");
        }
        #endregion

        public FaceDataWrapper LoadBinaryFaceFrame(string fileName, int frame)
        {
            if (File.Exists(fileName))
            {
                var depthPointsList = new List<Vector3DF>(121);
                var colorPointsList = new List<Microsoft.Kinect.Toolkit.FaceTracking.PointF>(121);
                var faceTrianglesList = new List<FaceTriangle>(206);

                int nbPoint;

                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    for(int i = 0; i < 121; ++i)
                    {
                        nbPoint = reader.ReadInt32();

                        Vector3DF vector3df = new Vector3DF();
                        vector3df.X = reader.ReadSingle();
                        vector3df.Y = reader.ReadSingle();
                        vector3df.Z = reader.ReadSingle();

                        depthPointsList[nbPoint] = vector3df;
                    }

                    for(int i = 0; i < 121; ++i)
                    {
                        nbPoint = reader.ReadInt32();

                        var buffer = new Microsoft.Kinect.Toolkit.FaceTracking.PointF(reader.ReadSingle(), reader.ReadSingle());

                        colorPointsList[nbPoint] = buffer;
                    }

                    while(reader.PeekChar() != -1)
                    {
                        FaceTriangle ft = new FaceTriangle
                        {
                            First = reader.ReadInt32(),
                            Second = reader.ReadInt32(),
                            Third = reader.ReadInt32()
                        };

                        faceTrianglesList.Add(ft);
                    }

                    return new FaceDataWrapper(depthPointsList, colorPointsList, faceTrianglesList.ToArray());
                }
            }         
            else
            {
                throw new Exception("Error while loading the file");
            }
        }

        //These uses in the end too much memory to be used
        #region SlowerAlternateVersionsOfFaceLoading

        #region sequentialLoadingFace

        public SortedList<int, FaceDataWrapper> ResultFaceTest(string path)
        {
            int fCount = 0;
            var faceSortedList = new SortedList<int, FaceDataWrapper>();
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
            var xmlFaceReader = System.Xml.XmlReader.Create(path, settings);

            var depthPointsList = new List<Vector3DF>(40);
            var colorPointsList = new List<Microsoft.Kinect.Toolkit.FaceTracking.PointF>(121);
            var faceTrianglesList = new List<FaceTriangle>(206);

            // This is not a pretty code but I am forced to do so 
            // because I can't instantiate the EnumIndexableCollection of Microsoft 
            for (int i = 0; i < 40; ++i)
            {
                depthPointsList[i] = Vector3DF.Empty;
            }

            for (int i = 0; i < 121; ++i)
            {
                colorPointsList[i] = Microsoft.Kinect.Toolkit.FaceTracking.PointF.Empty;
            }

            xmlFaceReader.MoveToContent();
            while (xmlFaceReader.Read())
            {
                if (xmlFaceReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlFaceReader.Name == "Face_" + fCount++)
                    {

                        xmlFaceReader.Read();
                        XmlReader xmlFacePoints3DReader = xmlFaceReader.ReadSubtree();
                        xmlFacePoints3DReader.MoveToContent();
                        // FacePoints3D
                        if (xmlFacePoints3DReader.Name == "FacePoints3D")
                        {
                            while (xmlFacePoints3DReader.Read() && xmlFacePoints3DReader.IsStartElement())
                            {
                                if (xmlFacePoints3DReader.Name.StartsWith("Point3D_") == false)
                                    throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");

                                int nbPoint = int.Parse(xmlFacePoints3DReader.Name.Split('_').ElementAt(1));
                                Vector3DF vector3df = new Vector3DF();
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                vector3df.X = float.Parse(xmlFacePoints3DReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                vector3df.Y = float.Parse(xmlFacePoints3DReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                vector3df.Z = float.Parse(xmlFacePoints3DReader.Value);

                                depthPointsList.Insert(nbPoint, vector3df);
                            }
                        }
                        else throw new XmlLoadingException("Error while loading the face",
                            "Error loading the facepoints3D in the savefile.\nCannot load this file : ");

                        xmlFaceReader.Skip();
                        XmlReader xmlFacePointsReader = xmlFaceReader.ReadSubtree();
                        xmlFacePointsReader.MoveToContent();

                        // FacePoints 
                        if (xmlFacePointsReader.Name == "FacePoints")
                        {
                            while (xmlFacePointsReader.Read() && xmlFacePointsReader.IsStartElement())
                            {
                                if (xmlFacePointsReader.Name.StartsWith("Point_") == false)
                                    throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");

                                int nbPoint = int.Parse(xmlFacePointsReader.Name.Split('_').ElementAt(1));

                                xmlFacePoints3DReader.MoveToNextAttribute();
                                var x = float.Parse(xmlFacePointsReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                var y = float.Parse(xmlFacePointsReader.Value);

                                var buffer = new Microsoft.Kinect.Toolkit.FaceTracking.PointF(x, y);
                                colorPointsList.Insert(nbPoint, buffer);

                            }
                            //parseXmlFaceSubTree<float, Microsoft.Kinect.Toolkit.FaceTracking.PointF>(xmlFacePointsReader, "Point_", 121);
                        }
                        else throw new XmlLoadingException("Error while loading the face",
                           "Error loading the facepoints in the savefile.\nCannot load this file : ");

                        xmlFaceReader.Skip();
                        XmlReader xmlFaceTrianglesReader = xmlFaceReader.ReadSubtree();
                        xmlFaceTrianglesReader.MoveToContent();

                        // FaceTriangles
                        if (xmlFacePointsReader.Name == "FaceTriangles")
                        {

                            while (xmlFaceTrianglesReader.Read() && xmlFaceTrianglesReader.IsStartElement())
                            {
                                if (xmlFaceTrianglesReader.Name.StartsWith("FaceTriangle_") == false)
                                    throw new XmlLoadingException("Error while loading the face",
                                        "The savefile is wrongly written");

                                int nbPoint = int.Parse(xmlFacePointsReader.Name.Split('_').ElementAt(1));
                                xmlFaceTrianglesReader.MoveToNextAttribute();

                                var first = int.Parse(xmlFaceTrianglesReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                var second = int.Parse(xmlFaceTrianglesReader.Value);
                                xmlFacePoints3DReader.MoveToNextAttribute();
                                var third = int.Parse(xmlFaceTrianglesReader.Value);

                                FaceTriangle ft = new FaceTriangle(first, second, third);
                                faceTrianglesList.Insert(nbPoint, ft);
                            }
                            xmlFaceReader.Read();

                        }
                        xmlFaceReader.Read();
                    }
                }
                faceSortedList.Add(fCount - 1, new FaceDataWrapper(depthPointsList, colorPointsList, faceTrianglesList.ToArray()));
            }
            return faceSortedList;
        }

        #endregion

        // ~1500 ms 1059 frames
        #region sequentialLoadingFaceWithGenericReflection

        private SortedList<int, V> parseXmlFaceSubTree<T, V>(XmlReader subtree, string treeName, int initCapacity)
        {
            SortedList<int, V> points3DList = new SortedList<int, V>(initCapacity);

            System.Reflection.MethodInfo m = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });

            while (subtree.Read() && subtree.IsStartElement())
            {
                if (subtree.Name.StartsWith(treeName) == false)
                    throw new XmlLoadingException("Error while loading the face",
                        "The savefile is wrongly written");
                object[] testf = new object[subtree.AttributeCount];

                int nbPoint = int.Parse(subtree.Name.Split('_').ElementAt(1));

                for (int i = 0; i < subtree.AttributeCount; ++i)
                {
                    if (m != null)
                    {
                        subtree.MoveToNextAttribute();
                        var parsedValue = m.Invoke(null, new object[] { subtree.Value });
                        testf[i] = (T)parsedValue;
                    }
                    else
                        throw new XmlLoadingException("Error while loading the face",
                       "The savefile is wrongly written");
                }
                V test = (V)Activator.CreateInstance(typeof(V), testf);
                points3DList.Add(nbPoint, test);
            }
            return points3DList;
        }

        private void ResultFaceTest2(string path)
        {
            int fCount = 0;
            var faceSortedList = new SortedList<int, FaceDataWrapper>();
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
            var xmlFaceReader = System.Xml.XmlReader.Create(path, settings);

            xmlFaceReader.MoveToContent();
            while (xmlFaceReader.Read())
            {
                if (xmlFaceReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlFaceReader.Name == "Face_" + fCount++)
                    {
                        xmlFaceReader.Read();
                        XmlReader xmlFacePoints3DReader = xmlFaceReader.ReadSubtree();
                        xmlFacePoints3DReader.MoveToContent();
                        // FacePoints3D
                        if (xmlFacePoints3DReader.Name == "FacePoints3D")
                        {
                            parseXmlFaceSubTree<float, Vector3DF>(xmlFacePoints3DReader, "Point3D_", 27);
                        }
                        else throw new XmlLoadingException("Error while loading the face",
                            "Error loading the facepoints3D in the savefile.\nCannot load this file : ");

                        xmlFaceReader.Skip();
                        XmlReader xmlFacePointsReader = xmlFaceReader.ReadSubtree();
                        xmlFacePointsReader.MoveToContent();

                        // FacePoints 
                        if (xmlFacePointsReader.Name == "FacePoints")
                        {

                            parseXmlFaceSubTree<float, Microsoft.Kinect.Toolkit.FaceTracking.PointF>(xmlFacePointsReader, "Point_", 121);
                        }
                        else throw new XmlLoadingException("Error while loading the face",
                           "Error loading the facepoints in the savefile.\nCannot load this file : ");

                        xmlFaceReader.Skip();
                        XmlReader xmlFaceTrianglesReader = xmlFaceReader.ReadSubtree();
                        xmlFaceTrianglesReader.MoveToContent();

                        // FaceTriangles
                        if (xmlFacePointsReader.Name == "FaceTriangles")
                        {
                            parseXmlFaceSubTree<int, FaceTriangle>(xmlFaceTrianglesReader, "FaceTriangle_", 206);
                        }
                        xmlFaceReader.Read();
                    }
                }
            }
        }
        #endregion

        // ~ 850 ms 1059 frames
        #region ParallelLoadingFaceWithGenericReflection
        private ConcurrentDictionary<int, V> parseXTree<T, V>(XElement root, string elemName, int initCapacity)
        {
            ConcurrentDictionary<int, V> points3DList = new ConcurrentDictionary<int, V>(4, initCapacity);
            int count = 0;

            System.Reflection.MethodInfo m = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });

            Parallel.ForEach(root.Descendants(), child =>
            {
                if (child.Name.LocalName.StartsWith(elemName) == false)
                    throw new XmlLoadingException("Error while loading the face",
                       "The savefile is wrongly written");

                int nbPoint = int.Parse(child.Name.LocalName.Split('_').ElementAt(1));
                object[] testf = new object[child.Attributes().Count()];
                int i = 0;

                foreach (XAttribute att in child.Attributes())
                {
                    if (m != null)
                    {
                        var parsedValue = m.Invoke(null, new object[] { att.Value });
                        testf[i++] = (T)parsedValue;
                    }
                    else
                        throw new XmlLoadingException("Error while loading the face",
                       "The savefile is wrongly written");
                }
                V test = (V)Activator.CreateInstance(typeof(V), testf);
                points3DList.TryAdd(nbPoint, test);
                count++;
            });
            return points3DList;
        }

        private void loadFacePLFWGR(XElement face)
        {


            XElement fP3DXElement = face.Element("FacePoints3D");
            XElement fPXElement = face.Element("FacePoints");
            XElement fTXElement = face.Element("FaceTriangles");

            //ConcurrentDictionary<int, Vector3DF> test;
            var t1 = Task.Factory.StartNew(() => parseXTree<float, Vector3DF>(fP3DXElement, "Point3D_", 27));

            //ConcurrentDictionary<int, Microsoft.Kinect.Toolkit.FaceTracking.PointF> test2;
            var t2 = Task.Factory.StartNew(()
               => parseXTree<float, Microsoft.Kinect.Toolkit.FaceTracking.PointF>(fPXElement, "Point_", 121));

            //ConcurrentDictionary<int, Microsoft.Kinect.Toolkit.FaceTracking.PointF> test3;
            var t3 = Task.Factory.StartNew(() => parseXTree<int, FaceTriangle>(fTXElement, "FaceTriangle_", 206));
        }


        private void ResultFaceTest3(string path)
        {
            int fCount = 0;
            var faceSortedList = new SortedList<int, FaceDataWrapper>();
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
            var xmlFaceReader = System.Xml.XmlReader.Create(path, settings);

            xmlFaceReader.MoveToContent();
            while (xmlFaceReader.EOF == false)
            {
                if (xmlFaceReader.Read() == false) return;
                if (xmlFaceReader.IsStartElement())
                {
                    if (xmlFaceReader.Name != "Face_" + fCount++)
                    {
                        throw new XmlLoadingException("Error while loading the face",
                            "Error loading the face" + --fCount + " in the savefile.\nCannot load this file : ");
                    }
                    XElement face = XElement.Load(xmlFaceReader.ReadSubtree());
                    loadFacePLFWGR(face);
                }
            }
        }

        #endregion

        // ~566 ms 1059 frames
        // ~780 ms 1059 frames _ w/ task
        #region ParallelFaceLoading

        private void loadFacePFL(XElement face)
        {

            XElement fP3DXElement = face.Element("FacePoints3D");
            XElement fPXElement = face.Element("FacePoints");
            XElement fTXElement = face.Element("FaceTriangles");

            //ConcurrentDictionary<int, Vector3DF> test;
            var t1 = Task.Factory.StartNew(() =>
            {
                ConcurrentDictionary<int, Vector3DF> points3DList = new ConcurrentDictionary<int, Vector3DF>(4, 207);
                int count = 0;
                foreach (var child in fP3DXElement.Descendants())
                {
                    if (child.Name.LocalName.StartsWith("Point3D_") == false)
                        throw new XmlLoadingException("Error while loading the face",
                           "The savefile is wrongly written");

                    int nbPoint = int.Parse(child.Name.LocalName.Split('_').ElementAt(1));
                    float[] testf = new float[child.Attributes().Count()];
                    int i = 0;

                    foreach (XAttribute att in child.Attributes())
                    {
                        testf[i++] = float.Parse(att.Value);
                    }

                    Vector3DF test = new Vector3DF(testf[0], testf[1], testf[2]);
                    points3DList.TryAdd(nbPoint, test);
                    count++;
                }
            });

            //ConcurrentDictionary<int, Microsoft.Kinect.Toolkit.FaceTracking.PointF> test2;
            var t2 = Task.Factory.StartNew(()
               =>
            {
                ConcurrentDictionary<int, Microsoft.Kinect.Toolkit.FaceTracking.PointF> pointsList
                = new ConcurrentDictionary<int, Microsoft.Kinect.Toolkit.FaceTracking.PointF>(4, 207);
                int count = 0;
                foreach (var child in fPXElement.Descendants())
                {
                    if (child.Name.LocalName.StartsWith("Point_") == false)
                        throw new XmlLoadingException("Error while loading the face",
                           "The savefile is wrongly written");

                    int nbPoint = int.Parse(child.Name.LocalName.Split('_').ElementAt(1));
                    float[] testf = new float[child.Attributes().Count()];
                    int i = 0;

                    foreach (XAttribute att in child.Attributes())
                    {
                        testf[i++] = float.Parse(att.Value);
                    }

                    Microsoft.Kinect.Toolkit.FaceTracking.PointF test = new Microsoft.Kinect.Toolkit.FaceTracking.PointF(testf[0], testf[1]);
                    pointsList.TryAdd(nbPoint, test);
                    count++;
                }
            });


            //ConcurrentDictionary<int, FaceTriangle> test3;
            var t3 = Task.Factory.StartNew(() =>
            {
                ConcurrentDictionary<int, FaceTriangle> trianglesList = new ConcurrentDictionary<int, FaceTriangle>(4, 207);
                int count = 0;
                foreach (var child in fTXElement.Descendants())
                {
                    if (child.Name.LocalName.StartsWith("FaceTriangle_") == false)
                        throw new XmlLoadingException("Error while loading the face",
                           "The savefile is wrongly written");

                    int nbPoint = int.Parse(child.Name.LocalName.Split('_').ElementAt(1));
                    int[] testf = new int[child.Attributes().Count()];
                    int i = 0;

                    foreach (XAttribute att in child.Attributes())
                    {
                        testf[i++] = int.Parse(att.Value);
                    }

                    FaceTriangle test = new FaceTriangle(testf[0], testf[1], testf[2]);
                    trianglesList.TryAdd(nbPoint, test);
                    count++;
                }
            });
        }


        private void ResultFaceTest4(string path)
        {
            int fCount = 0;
            var faceSortedList = new SortedList<int, FaceDataWrapper>();
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
            var xmlFaceReader = System.Xml.XmlReader.Create(path, settings);

            xmlFaceReader.MoveToContent();
            while (xmlFaceReader.EOF == false)
            {
                if (xmlFaceReader.Read() == false) return;
                if (xmlFaceReader.IsStartElement())
                {
                    if (xmlFaceReader.Name != "Face_" + fCount++)
                    {
                        throw new XmlLoadingException("Error while loading the face",
                            "Error loading the face" + --fCount + " in the savefile.\nCannot load this file : ");
                    }
                    XElement face = XElement.Load(xmlFaceReader.ReadSubtree());
                    Task.Factory.StartNew(() => loadFacePFL(face));
                }
            }
        }
        #endregion

        // ~940 ms 1059 frames w/ task
        #region ParallelFaceLoadingWithParallelFor
        //bool test = true;
        private void loadFacePFLWPF(XElement face)
        {


            XElement fP3DXElement = face.Element("FacePoints3D");
            XElement fPXElement = face.Element("FacePoints");
            XElement fTXElement = face.Element("FaceTriangles");

            //ConcurrentDictionary<int, Vector3DF> test;
            var t1 = Task.Factory.StartNew(() =>
            {
                ConcurrentDictionary<int, Vector3DF> points3DList = new ConcurrentDictionary<int, Vector3DF>(4, 207);
                int count = 0;
                Parallel.ForEach(fP3DXElement.Descendants(), child =>
                {
                    if (child.Name.LocalName.StartsWith("Point3D_") == false)
                        throw new XmlLoadingException("Error while loading the face",
                           "The savefile is wrongly written");

                    int nbPoint = int.Parse(child.Name.LocalName.Split('_').ElementAt(1));
                    float[] testf = new float[child.Attributes().Count()];
                    int i = 0;

                    foreach (XAttribute att in child.Attributes())
                    {
                        testf[i++] = float.Parse(att.Value);
                    }

                    Vector3DF test = new Vector3DF(testf[0], testf[1], testf[2]);
                    points3DList.TryAdd(nbPoint, test);
                    count++;
                });
            });

            //ConcurrentDictionary<int, Microsoft.Kinect.Toolkit.FaceTracking.PointF> test2;
            var t2 = Task.Factory.StartNew(()
               =>
            {
                ConcurrentDictionary<int, Microsoft.Kinect.Toolkit.FaceTracking.PointF> pointsList
                = new ConcurrentDictionary<int, Microsoft.Kinect.Toolkit.FaceTracking.PointF>(4, 207);
                int count = 0;
                Parallel.ForEach(fPXElement.Descendants(), child =>
                {
                    if (child.Name.LocalName.StartsWith("Point_") == false)
                        throw new XmlLoadingException("Error while loading the face",
                           "The savefile is wrongly written");

                    int nbPoint = int.Parse(child.Name.LocalName.Split('_').ElementAt(1));
                    float[] testf = new float[child.Attributes().Count()];
                    int i = 0;

                    foreach (XAttribute att in child.Attributes())
                    {
                        testf[i++] = float.Parse(att.Value);
                    }

                    Microsoft.Kinect.Toolkit.FaceTracking.PointF test = new Microsoft.Kinect.Toolkit.FaceTracking.PointF(testf[0], testf[1]);
                    pointsList.TryAdd(nbPoint, test);
                    count++;
                });
            });


            //ConcurrentDictionary<int, FaceTriangle> test3;
            var t3 = Task.Factory.StartNew(() =>
            {
                ConcurrentDictionary<int, FaceTriangle> trianglesList = new ConcurrentDictionary<int, FaceTriangle>(4, 207);
                int count = 0;
                Parallel.ForEach(fTXElement.Descendants(), child =>
                {
                    if (child.Name.LocalName.StartsWith("FaceTriangle_") == false)
                        throw new XmlLoadingException("Error while loading the face",
                           "The savefile is wrongly written");

                    int nbPoint = int.Parse(child.Name.LocalName.Split('_').ElementAt(1));
                    int[] testf = new int[child.Attributes().Count()];
                    int i = 0;

                    foreach (XAttribute att in child.Attributes())
                    {
                        testf[i++] = int.Parse(att.Value);
                    }

                    FaceTriangle test = new FaceTriangle(testf[0], testf[1], testf[2]);
                    trianglesList.TryAdd(nbPoint, test);
                    count++;
                });
            });
        }


        private void ResultFaceTest5(string path)
        {
            int fCount = 0;
            var faceSortedList = new SortedList<int, FaceDataWrapper>();
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
            var xmlFaceReader = System.Xml.XmlReader.Create(path, settings);

            xmlFaceReader.MoveToContent();
            while (xmlFaceReader.EOF == false)
            {
                if (xmlFaceReader.Read() == false) return;
                if (xmlFaceReader.IsStartElement())
                {
                    if (xmlFaceReader.Name != "Face_" + fCount++)
                    {
                        throw new XmlLoadingException("Error while loading the face",
                            "Error loading the face" + --fCount + " in the savefile.\nCannot load this file : ");
                    }
                    XElement face = XElement.Load(xmlFaceReader.ReadSubtree());
                    Task.Factory.StartNew(() => loadFacePFLWPF(face));
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
