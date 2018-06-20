using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using Microsoft.Kinect;
using System.Xml;
using System.Drawing;
using AForge.Video.FFMPEG;
using System.Text.RegularExpressions;
using LecturerTrainer.Model.Exceptions;
using Microsoft.Kinect.Toolkit.FaceTracking;
using LecturerTrainer.Model.EmotionRecognizer;
using System.Windows.Threading;
using System.Diagnostics;
using LecturerTrainer.View;
using LecturerTrainer.ViewModel;
using LecturerTrainer.Model.AudioAnalysis;



namespace LecturerTrainer.Model
{
    /// <summary>
    /// Gestion of everything that's need to be save
    /// Interraction between the computer and the software in term of choice
    /// </summary>
    class SavingTools
    {
        private static DateTime localDate = DateTime.Now;
        public static string pathFolder ="";

        #region PCQueueFields
        /// <summary>
        /// PCQueue for the videostream recording
        /// </summary>
        private static PCQueue<Bitmap> videoStreamQueue;
        
        /// <summary>
        /// PCQueue for the openGL video stream recording
        /// </summary>
        private static PCQueue<Bitmap> avatarStreamQueue;
        
        /// <summary>
        /// PCQueue for the openGL Avatar data recording (body)
        /// </summary>
        private static PCQueue<Skeleton> xmlSkeletonQueue;

        /// <summary>
        /// PCQueue for the openGL Avatar data recording (face)
        /// </summary>
        private static PCQueue<FaceDataWrapper> xmlFaceQueue;

        /// <summary>
        /// PCQueue for the openGL Avatar data recording (voice)
        /// </summary>		
		private static PCQueue<float> xmlVoiceQueue;
        #endregion

        /// <summary>
        /// These are the different writer classes allowing the save the recordings
        /// </summary>
        #region writerFields
        private static VideoFileWriter videoStreamWriter;
        private static VideoFileWriter avatarVideoStreamWriter;
        private static XmlWriter xmlSkeletonWriter;
        private static XmlWriter xmlFaceWriter;
		private static XmlWriter xmlVoiceWriter;
        #endregion

        //private static XmlReader xmlSkeletonReader;
        

        /// <summary>
        /// Chose the name of the folder to save in depending on the other folder name in the folder the user is using
        /// Added by Baptiste Germond
        /// </summary>
        public static string nameFolder(string path, string baseFileName)
        {
            IEnumerable<String> listFolder = Directory.EnumerateDirectories(path);
            String folderName;
            if (listFolder.Any<String>()) // Go in if listFolder is not empty
            {
                int numRecord =-1;
                string[] finalFolderName = null;
                //Check each folder in the recording folder
                foreach (string pathFile in listFolder)
                {
                    string[] generalFolderName = pathFile.Split('\\');

                    //Format used to name the folder after
                    string monthTemp = String.Format("{0:MM}", localDate);
                    string dayTemp = String.Format("{0:dd}", localDate);

                    //Pattern to recognize the date of the file
                    string pattern = "[0-9]{4}_[0-9]{2}_[0-9]{2}_*[0-9]*";
                    Regex searchRegEx = new Regex(pattern);
                    Match correspond = searchRegEx.Match(generalFolderName[generalFolderName.Count<string>() - 1]);
                    //Split the recognized pattern using "_"
                    String[] splitFolderName = correspond.Groups[0].Value.Split('_');

                    if (splitFolderName[0] == localDate.Year.ToString() && splitFolderName[1] == monthTemp
                    && splitFolderName[2] == dayTemp) // If there is already a folder with the same date
                    {
                        if (splitFolderName.Count<string>()==4) //If there's already a folder with the same date and a number
                        {
                            int tempNumRecord = Int32.Parse(splitFolderName[3]);
                            if (tempNumRecord > numRecord)//Look for the highest folder number of the same date
                            {
                                numRecord = tempNumRecord;
                                finalFolderName = splitFolderName;
                            }
                        }
                        else// If there is not a folder with the same date and a number
                        {
                            numRecord = 0; //The number will be (0 +1)
                            finalFolderName = splitFolderName;
                        }
                    }
                }
                if (numRecord != -1) //If there was already a folder with the same date
                { 
                    //We create a new folder with the number = the highest previous number +1
                    folderName = baseFileName + "_" + finalFolderName[0] + "_" + finalFolderName[1] + "_" + finalFolderName[2] + "_" + (numRecord + 1).ToString();
                }
                else//Else we create a new folder with the date
                {
                    folderName = baseFileName + "_" + localDate.ToString("yyyy_MM_dd");
                }
            }
            else // If there was no folder, we create a new folder with the date
            {
                folderName = baseFileName + "_" + localDate.ToString("yyyy_MM_dd");
                
            }
            pathFolder = Path.Combine(path, folderName);
            Directory.CreateDirectory(pathFolder);
            return pathFolder;
        }

        /// <summary>
        /// Enqueuing functions for the different recordings
        /// </summary>
        /// <author> Amirali Ghazi </author>
        #region EnqueueItemFunctions

        /// <summary>
        /// Enqueue a Bitmap to the videoStream recording queue
        /// </summary>
        /// <param name="e"></param>     
        public static void EnqueueVideoStream(Bitmap e)
        {
            if (videoStreamQueue != null)
            {
                if (!videoStreamWriter.IsOpen)
                    videoStreamWriter.Open(SavingTools.pathFolder + '/' + "stream" + ".avi", e.Width, e.Height, 30, VideoCodec.MPEG4, 1000000);
                videoStreamQueue.EnqueueItem(e);
            }
        }

        /// <summary>
        /// Enqueue a Bitmap to the avatar videoStream recording queue
        /// </summary>
        /// <param name="e">The bitmap to enqueue</param>
        public static void EnqueueAvatarVideoStream(Bitmap e)
        {
            if (avatarStreamQueue != null)
            {
                if (!avatarVideoStreamWriter.IsOpen)
                    avatarVideoStreamWriter.Open(SavingTools.pathFolder + '/' + "avatar" + ".avi", e.Width, e.Height, 30, VideoCodec.MPEG4, 1000000);
                avatarStreamQueue.EnqueueItem(e);
            }
        }
        /// <summary>
        /// Enqueue a skeleton to the skeleton recording queue
        /// </summary>
        /// <param name="sk">The skeleton to enqueue</param>
        public static void EnqueueXMLSkeleton(Skeleton sk)
        {
            xmlSkeletonQueue.EnqueueItem(sk);
        }

        /// <summary>
        /// Enqueue a face data wrapper to the face recording queue
        /// </summary>
        /// <param name="fdw"></param>
        public static void EnqueueXMLFace(FaceDataWrapper fdw)
        {
            xmlFaceQueue.EnqueueItem(fdw);
        }

		/// <summary>
        /// Enqueue a voice data wrapper to the voice recording queue
        /// </summary>
        /// <param name="fdw"></param>
        public static void EnqueueXMLVoice(float value)
        {
            xmlVoiceQueue.EnqueueItem(value);
        }

        #endregion


        /// <summary>
        /// Functions starting the different recordings (video, openGL, face, etc)
        /// </summary>
        /// <author> Amirali Ghazi </author>
        #region StartSavingFunctions
        public static void StartSavingStreamRecording()
        {
            try
            {
                videoStreamWriter = new VideoFileWriter();
                videoStreamQueue = new PCQueue<Bitmap>(bm =>
                {
                  videoStreamWriter.WriteVideoFrame(bm);
                }, () =>
                {
                    videoStreamWriter.Close();
                }, "VideoRecordingTask");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void StartSavingAvatarVideoRecording()
        {
            try
            {
                avatarVideoStreamWriter = new VideoFileWriter();
                avatarStreamQueue = new PCQueue<Bitmap>(bm =>
                {
                    avatarVideoStreamWriter.WriteVideoFrame(bm);
                }, () =>
                {
                    avatarVideoStreamWriter.Close();
                }, "AvatarVideoRecordingTask");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public static void StartSavingXMLSkeleton()
        {
			Console.Out.WriteLine("here");
            Tools.initStopWatch();
            Tools.startStopWatch();
            int nbSkFrame = 0;
            int count = 0;

            try
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    Indent = true
                };
                xmlSkeletonWriter = XmlWriter.Create(SavingTools.pathFolder + "/" + "avatarSkeletonData.skd", settings);
                xmlSkeletonWriter.WriteStartDocument();
                xmlSkeletonWriter.WriteStartElement("Skeletons");
                xmlSkeletonQueue = new PCQueue<Skeleton>(sk =>
                {
                    nbSkFrame++;
                    if(nbSkFrame % 2 == 1 || nbSkFrame == 0)
                    {
                        xmlSkeletonWriter.WriteStartElement("Skeleton_" + count++);
						xmlSkeletonWriter.WriteAttributeString("TimeElapse", Tools.getStopWatch().ToString());
                        xmlSkeletonWriter.WriteAttributeString("TrackingState", sk.TrackingState.ToString());
						List<Joint> lJoints = sk.Joints.ToList();
                        lJoints.ForEach(joint =>
                        {
                            xmlSkeletonWriter.WriteStartElement(joint.JointType.ToString());
                            xmlSkeletonWriter.WriteAttributeString("TrackingState", joint.TrackingState.ToString());
                            xmlSkeletonWriter.WriteAttributeString("X", joint.Position.X.ToString());
                            xmlSkeletonWriter.WriteAttributeString("Y", joint.Position.Y.ToString());
                            xmlSkeletonWriter.WriteAttributeString("Z", joint.Position.Z.ToString());
                            xmlSkeletonWriter.WriteEndElement();
                        });
                        xmlSkeletonWriter.WriteEndElement();
                    }
                    

                }, () =>
                {
                    xmlSkeletonWriter.WriteEndElement();
                    xmlSkeletonWriter.WriteEndDocument();
                    xmlSkeletonWriter.Flush();
                    xmlSkeletonWriter.Close();
                }, "XMLSkeletonRecordingTask");
            }
            catch (Exception ex)
            {
                if (Tools.getStateStopWatch())
                    Tools.stopStopWatch();
                Console.WriteLine(ex.ToString());
            }
		}

        public static void StartSavingXMLFace()
        {
            int nbFaceFrame = 0;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    Indent = true
                };
                xmlFaceWriter = XmlWriter.Create(SavingTools.pathFolder + "/" + "faceData.xml", settings);
                xmlFaceWriter.WriteStartDocument();
                xmlFaceWriter.WriteStartElement("Faces");
                
                xmlFaceQueue = new PCQueue<FaceDataWrapper>( fdw =>
               {
                   xmlFaceWriter.WriteStartElement("Face_" + nbFaceFrame++);
                   xmlFaceWriter.WriteStartElement("FacePoints3D");
                   for(int i = 0; i < fdw.depthPointsList.Count; ++i)
                   {
                       switch(i)
                       {
                           case 7: case 8: case 15: case 16: case 17: case 18: 
                           case 19: case 20: case 21: case 22: case 23: case 24:
                           case 31: case 40: case 41: case 48: case 49: case 50:
                           case 51: case 52: case 53: case 54: case 55: case 56:
                           case 57: case 64: case 87:
                               {
                                   xmlFaceWriter.WriteStartElement("Point3D_" + i);
                                   xmlFaceWriter.WriteAttributeString("X", fdw.depthPointsList.ElementAt(i).X.ToString());
                                   xmlFaceWriter.WriteAttributeString("Y", fdw.depthPointsList.ElementAt(i).Y.ToString());
                                   xmlFaceWriter.WriteAttributeString("Z", fdw.depthPointsList.ElementAt(i).Z.ToString());
                                   xmlFaceWriter.WriteEndElement();
                               }
                               break;
                       }
                   }
                   xmlFaceWriter.WriteEndElement();
                   xmlFaceWriter.WriteStartElement("FacePoints");
                   for(int i = 0; i < fdw.colorPointsList.Count; ++i)
                   {
                       xmlFaceWriter.WriteStartElement("Point_" + i);
                       xmlFaceWriter.WriteAttributeString("X", fdw.colorPointsList.ElementAt(i).X.ToString());
                       xmlFaceWriter.WriteAttributeString("Y", fdw.colorPointsList.ElementAt(i).Y.ToString());
                       xmlFaceWriter.WriteEndElement();
                   }
                   xmlFaceWriter.WriteEndElement();

                   xmlFaceWriter.WriteStartElement("FaceTriangles");
                   for(int i = 0; i < fdw.faceTriangles.Count(); ++i)
                   {
                       // Amirali
                       xmlFaceWriter.WriteStartElement("FaceTriangle_" + i);
                       xmlFaceWriter.WriteAttributeString("Vertex_1", fdw.faceTriangles.ElementAt(i).First.ToString());
                       xmlFaceWriter.WriteAttributeString("Vertex_2", fdw.faceTriangles.ElementAt(i).Second.ToString());
                       xmlFaceWriter.WriteAttributeString("Vertex_3", fdw.faceTriangles.ElementAt(i).Third.ToString());
                       xmlFaceWriter.WriteEndElement();
                   }
                   xmlFaceWriter.WriteEndElement();
                   xmlFaceWriter.WriteEndElement();

               }, () =>
               {
                   xmlFaceWriter.Flush();
                   xmlFaceWriter.WriteEndElement();
                   xmlFaceWriter.WriteEndDocument();
                   xmlFaceWriter.Close();

               }, "XMLFaceRecordingTask");
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

		public static void startSavingTonePeak()
		{
			int valueIndex = 0;
			try
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    Indent = true
                };
                xmlVoiceWriter = XmlWriter.Create(SavingTools.pathFolder + "/" + "tonePeakData.xml", settings);
                xmlVoiceWriter.WriteStartDocument();
                xmlVoiceWriter.WriteStartElement("PeakValues");
                xmlVoiceQueue = new PCQueue<float>(value =>
                {
                        xmlVoiceWriter.WriteStartElement("PeakValue_" + valueIndex++);
                        xmlVoiceWriter.WriteAttributeString("Value", value.ToString());
						xmlVoiceWriter.WriteAttributeString("TimeElapse", Tools.getStopWatch().ToString());
						xmlVoiceWriter.WriteEndElement();
                }, () =>
                {
                    xmlVoiceWriter.WriteEndElement();
                    xmlVoiceWriter.WriteEndDocument();
                    xmlVoiceWriter.Flush();
                    xmlVoiceWriter.Close();
					System.Diagnostics.Debug.WriteLine("close");
                }, "XMLVoiceRecordingTask");
            }
            catch (Exception ex)
            {
                if (Tools.getStateStopWatch())
                    Tools.stopStopWatch();
                Console.WriteLine(ex.ToString());
            }
		}
		


        #endregion

        public static void StartSavingBinaryFace()
        {
            string fileName = SavingTools.pathFolder + "/" + "faceData.dat";

            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                PCQueue<FaceDataWrapper> binaryFaceQueue = new PCQueue<FaceDataWrapper>(fdw =>
                {
                    //writer.Write("Face_" + nbFaceFrame++);
                    //writer.Write("FacePoints3D");
                    for (int i = 0; i < fdw.depthPointsList.Count; ++i)
                    {
                        switch (i)
                        {
                            case 7:
                            case 8:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 31:
                            case 40:
                            case 41:
                            case 48:
                            case 49:
                            case 50:
                            case 51:
                            case 52:
                            case 53:
                            case 54:
                            case 55:
                            case 56:
                            case 57:
                            case 64:
                            case 87:
                                {
                                    writer.Write(i);
                                    writer.Write(fdw.depthPointsList.ElementAt(i).X);
                                    writer.Write(fdw.depthPointsList.ElementAt(i).Y);
                                    writer.Write(fdw.depthPointsList.ElementAt(i).Z);
                                }
                                break;
                        }
                    }

                    //writer.Write("FacePoints");

                    for (int i = 0; i < fdw.colorPointsList.Count; ++i)
                    {
                        writer.Write(i);
                        writer.Write(fdw.colorPointsList.ElementAt(i).X);
                        writer.Write(fdw.colorPointsList.ElementAt(i).Y);
                    }

                    //writer.Write("FaceTriangles");

                    for (int i = 0; i < fdw.faceTriangles.Count(); ++i)
                    {
                        writer.Write(i);
                        writer.Write(fdw.faceTriangles.ElementAt(i).First);
                        writer.Write(fdw.faceTriangles.ElementAt(i).Second);
                        writer.Write(fdw.faceTriangles.ElementAt(i).Third);
                    }

                }, "BinaryFaceRecordingTask");
            }
        }

        /// <summary>
        /// Functions disposing the different queue when their recordings ended 
        /// </summary>
        /// <author> Amirali Ghazi </author>
        #region QueueDisposeFunctions
        public static void StreamDispose()
        {
            videoStreamQueue?.Dispose();
        }

        public static void AvatarVideoDispose()
        {
            avatarStreamQueue?.Dispose();
        }

        public static void XMLSkeletonDispose()
        {
            xmlSkeletonQueue?.Dispose();
        }

        public static void XMLFaceDispose()
        {
            xmlFaceQueue?.Dispose();
        }

		public static void XMLVoiceDispose()
        {
            xmlVoiceQueue?.Dispose();
        }

        #endregion
        
    }

    /// <summary>
    /// Data wrapper class to ease the writing/ loading of the face data
    /// </summary>
    /// <author> Amirali Ghazi </author>
    public struct FaceDataWrapper
    {
        public List<Vector3DF> depthPointsList;
        public List<Microsoft.Kinect.Toolkit.FaceTracking.PointF> colorPointsList;
        public FaceTriangle[] faceTriangles;


        public FaceDataWrapper(List<Vector3DF> depthPointsList,
            List<Microsoft.Kinect.Toolkit.FaceTracking.PointF> colorPointsList,
            FaceTriangle[] faceTriangles)
        {
            this.depthPointsList = depthPointsList;
            this.colorPointsList = colorPointsList;
            this.faceTriangles = faceTriangles;
        }
    }
}
