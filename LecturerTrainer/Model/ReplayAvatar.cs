﻿using LecturerTrainer.Model.Exceptions;
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
using OpenTK.Graphics.OpenGL;


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
        public static bool faceTracking = false;

        public static bool voiceData = false;

        /// <summary>
        /// XmlReader for loading the skeletons data (body)
        /// </summary>
        private static XmlReader xmlSkeletonReader;

        /// <summary>
        /// The sorted list (the key being the frame number) of loaded skeletons
        /// </summary>
        private static SortedList<int, Tuple<int, Skeleton, FaceDataWrapper>> skeletonsList;

        public static SortedList<int, Tuple<int, Skeleton, FaceDataWrapper>> SkeletonList
        {
            get
            {
                return skeletonsList;
            }
        }

		private static List<float> replayWiggle = null;
		private static int wiggleIndex = 0;
		private static int indexCount = 0;
      
        #endregion

        #region constructor
        /// <summary>
        /// Time ellapsed in ms. It is not the real elapsed time, it depends on the speed
        /// </summary>

        public ReplayAvatar(string avDir, string fDir, string vDir,ReplayViewModel rvm, int num)
        {
            try
            {
                avatarDir = avDir;
                faceDir = fDir;
                replayViewModel = rvm;
                faceTracking = fDir !=  "" ? true : false;
                voiceData = vDir != "" ? true : false;
                DrawingSheetAvatarViewModel.Get().drawFaceInReplay = false;

                //Load the list of the skeletons
                currentSkeletonNumber = num;
                skeletonsList = new SortedList<int, Tuple<int, Skeleton, FaceDataWrapper>>();
                skeletonsList = LoadSkeletonsFromXML(avatarDir, faceDir);

                //Load the voice data if it exists
                if (vDir != "")
                {
                    replayWiggle = LoadVoiceDataFromXML(vDir);
                }
				//Initilise the first skeleton to be displayed, depending if the replay is on play or stop
                if (currentSkeletonNumber >= skeletonsList.Count)
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
            }catch(ArgumentException e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Constructor for skeleton scrolling without face elements
        /// </summary>
        /// <param name="skDir"></param>
        /// <param name="rvm"></param>
        public ReplayAvatar(string skDir, ReplayViewModel rvm, int num) : this(skDir, "", "", rvm, num) { }
        #endregion

        #region replay methods

        private void changeSlider(object sender, EventArgs evt)
        {
            if (realTime)
               ReplayView.Get().changeValueOfSlider((int)Tools.getStopWatch());
            else
                ReplayView.Get().changeValueOfSlider((int)Tools.getStopWatch() - offset);
        }

        private static int replayFace = 0;
        /// <summary>
        /// update the current skeleton in the folder
        /// </summary>
        /// <returns></returns>
        private void nextSkeleton(object sender, EventArgs evt)
        {
            bool face = (faceDir == "") ? false : true;

            if ((currentSkeletonNumber < skeletonsList.Count && (replayFace % 2) == 0 && face)
                || (currentSkeletonNumber < skeletonsList.Count && !face))
            {
                currentSkeleton = skeletonsList[(int)currentSkeletonNumber].Item2;
            }
            else if (currentSkeletonNumber == skeletonsList.Count)
                currentSkeleton = null;

                if (currentSkeleton != null)
            {
                if (face)
                {
                    DrawingSheetAvatarViewModel.Get().drawFaceInReplay = true;
                    DrawingSheetAvatarViewModel.Get().drawFace(skeletonsList[(int)currentSkeletonNumber].Item3.depthPointsList,
                        skeletonsList[(int)currentSkeletonNumber].Item3.colorPointsList,
                        skeletonsList[(int)currentSkeletonNumber].Item3.faceTriangles);
                }
                // We only draw the last skeleton
                DrawingSheetAvatarViewModel.Get().skToDrawInReplay = currentSkeleton;
                setDisplayedTime();
            }
            else
            {
                replayViewModel.stopButtonCommand();
            }

			if(face)
			{
                if ((replayFace % 2) == 1)
                    currentSkeletonNumber += 1;
                replayFace++;
            }
			else
			{
				currentSkeletonNumber += 1;
			}
        }

        /// <summary>
        /// Updates the displayed time
        /// </summary>
        public static void setDisplayedTime()
        {
            ReplayViewModel.Get().ElapsedTime = Tools.FormatTime((int)Tools.getStopWatch() - offset);
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
			wiggleIndex = 0;
            currentSkeletonNumber = 0;
            currentSkeleton = skeletonsList[currentSkeletonNumber].Item2;
            DrawingSheetAvatarViewModel.Get().skToDrawInReplay = currentSkeleton;
            DrawingSheetAvatarViewModel.Get().forceDraw(currentSkeleton, false);
        }

        #endregion

        #region XmlSkeletonLoading

        /// <summary>
        /// Load all the skeletons from a file
        /// </summary>
        /// <param name="path">The path of the skeletonData</param>
        /// <returns>The sorted list of skeletons</returns>
        /// <author>Amirali Ghazi, modified by Alban Descottes</author>
        public static SortedList<int, Tuple<int, Skeleton, FaceDataWrapper>> LoadSkeletonsFromXML(String path, string pathFace)
        {
            try
            {
                int skCount = 0;
                var skeletonSortedListWithTime = new SortedList<int, Tuple<int, Skeleton, FaceDataWrapper>>();
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
                            if(pathFace != "")
                            {
                                var tu = new Tuple<int, Skeleton, FaceDataWrapper>(timeElapsed , sk, loadFaceWFrame(pathFace, skCount));
                                skeletonSortedListWithTime.Add(skCount++, tu);
                            }
                            else{
                                var tu = new Tuple<int, Skeleton, FaceDataWrapper>(timeElapsed, sk, new FaceDataWrapper(null, null, null));
                                skeletonSortedListWithTime.Add(skCount++, tu);
                            }
                        }
                    }
                }
                ReplayViewModel.timeEnd = skeletonSortedListWithTime[skeletonSortedListWithTime.Count - 1].Item1;
                return skeletonSortedListWithTime;
            }catch(Exception)
            {
                throw new ArgumentException("Impossible to read ", path);
            }

        }

        #endregion

        #region XmlfaceLoading

        /// <summary>
        /// Load the faceData of the 'frame' frame   
        /// </summary>
        /// <param name="path">The path of the faceData</param>
        /// <param name="frame">The frame number</param>
        /// <returns>The faceDataWrapper containing the data needed to display the face</returns>
        /// <author> Amirali Ghazi </author>
        public static FaceDataWrapper loadFaceWFrame(string path, int frame)
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

                                var buffer = new PointF(x, y);
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

		#region XMLTonePeakLoading
		
		private static List<float> stuffReplayWiggleList(List<float> list)
		{
			for (int i = 0; i < 300; i++)
			{
				list.Add(0);
			}
			return list;
		}

		/*private static void endReplayWiggle()
		{
			for (int i = 0; i < 300; i++)
			{
				replayWiggle.Add(0);
			}
		}*/
		
		public static void drawWiggle()
		{
			float  xw, xw1, yw, yw1;

            GL.BindTexture(TextureTarget.Texture2D, (from p in DrawingSheetStreamViewModel.Get().listImg where p.name.Count() > 0 && DrawingSheetAvatarViewModel.Get().actualTheme.SN.Contains(p.name) select p.idTextureOpenGL).First());
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			
			GL.PushAttrib(AttribMask.ColorBufferBit);
            for (int i = 0;  i < 299; i++)
            {
				yw = +0.6f + replayWiggle[i + wiggleIndex] / 500.0f; 
				yw1 = +0.6f + replayWiggle[i + 1 + wiggleIndex] / 500.0f;
				
                xw = -2.5f + i / 60.0f;
                xw1 = -2.5f + (i + 1) / 60.0f;

                GL.PushMatrix();
                GL.Begin(PrimitiveType.Lines);
                GL.Color4(0.5, 0.5, 0.5, 1.0);
                GL.Normal3(0.0f, 0.0f, 1.0f);
                GL.LineWidth(1.0f);

                GL.TexCoord2((xw + 2.5f) / 5.0, (yw - 0.6) / 1.15);
                GL.Vertex3(xw, yw, 1.0f);

                GL.TexCoord2((xw1 + 2.5f) / 5.0, (yw1 - 0.6) / 1.15);
                GL.Vertex3(xw1, yw1, 1.0f);

                GL.End();
                GL.PopMatrix();
            }
            GL.PopAttrib();
            GL.BindTexture(TextureTarget.Texture2D, 0);
			wiggleIndex+=3;
			if (indexCount % 3 == 0) wiggleIndex +=1;
			indexCount++;
		}


		/// <summary>
        /// Load the voiceData   
        /// </summary>
		public static List<float> LoadVoiceDataFromXML(String path)
        {
            try
            {
                int count = 0;
                List<float> toneList = new List<float>();
				stuffReplayWiggleList(toneList);
                XmlReaderSettings settings = new XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = true };
                XmlReader xmlVoiceReader = XmlReader.Create(path, settings);

                xmlVoiceReader.MoveToContent();
                while (xmlVoiceReader.Read())
                {
                    if (xmlVoiceReader.NodeType == XmlNodeType.Element)
                    {
                        if (xmlVoiceReader.Name == "PeakValue_" + count)
                        {
                            xmlVoiceReader.MoveToAttribute(0);
							toneList.Add(Convert.ToSingle(xmlVoiceReader.Value));
                        }
                    }
					count++;
                }
				stuffReplayWiggleList(toneList);
                return toneList;
            }catch(Exception)
            {
                throw new ArgumentException("Impossible to read voice data : ", path);
            }

        }

		#endregion

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

    }
}
