using LecturerTrainer.Model;
using LecturerTrainer.View;
using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LecturerTrainer.ViewModel
{
    class DrawingSheetStreamViewModel : DrawingSheetViewModel
    {
        #region fields 
        /// <summary>
        /// The only instance of the class 
        /// </summary>
        private static DrawingSheetStreamViewModel dssvm = null;

        // allow us to raise an event when we want to transfer a frame to the recorder 
        public static EventHandler<Bitmap> backgroundDrawEventStream; 

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Link between name of the picture of feedbacks and its number in the dictionnary
        /// </summary>
        private Dictionary<string, int> correspondIndiceName;

        public List<ImageFeedbacksPerso> listImg;

        /// <summary>
        /// Skeleton used in the live videostream to display feedbacks next to the body user
        /// </summary>
        public static SkeletonEventArgs skt;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor because we want to have a singleton 
        /// </summary>
        /// <param name="dsv"></param>
        private DrawingSheetStreamViewModel(DrawingSheetView dsv)
        {
            this.dsv = dsv;
            listImg = new List<ImageFeedbacksPerso>();
            loadPictureInResource();
            loadImageFromRessource();
            AddImageOnCanvas();

            KinectDevice.KinectChanged += kinectChanged;
        }
        #endregion

        #region methods

        protected override void Main_isReady(object sender, EventArgs args)
        {
            if (sender != this)
            {
                Main.isReady -= Main_isReady;
            }
        }

        /// <summary>
        /// Changes video source when a new kinect is plugged in.
        /// </summary>
        /// <param name="sender">KinectDevice</param>
        /// <param name="args">EventArgs.Empty</param>
        private void kinectChanged(object sender, EventArgs args)
        {
            if (KinectDevice.oldSensor != null)
            {
                KinectDevice.oldSensor.ColorFrameReady -= draw;
            }
            if (KinectDevice.sensor != null)
            {
                this.colorPixels = new byte[KinectDevice.sensor.ColorStream.FramePixelDataLength];
                this.colorBitmap = new WriteableBitmap(KinectDevice.sensor.ColorStream.FrameWidth, KinectDevice.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                KinectDevice.sensor.ColorFrameReady += draw;
            }
        }

        /// <summary>
        /// Allow to get / create an instance of a drawingsheetstreamviewmodel with safety 
        /// </summary>
        /// <param name="drawingSheetView"></param>
        /// <returns></returns>
        public static DrawingSheetStreamViewModel Get(DrawingSheetView drawingSheetView)
        {
            if (dssvm == null)
                dssvm = new DrawingSheetStreamViewModel(drawingSheetView); 
            return dssvm; 
        }

        /// <summary>
        /// A dangerous way to use the Getter. The return value can be null, make sure to control its value 
        /// </summary>
        /// <returns></returns>
        public static DrawingSheetStreamViewModel Get()
        {
            return dssvm;
        }

        /// <summary>
        /// Event raised at each frame sent by the kinect 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="evt">ColorImageFrame we will display </param>
        public override void draw(object sender, EventArgs evt)
        {
            if (this.dsv.getMode().CompareTo(SheetMode.StreamMode)==0 || (TrainingSideToolViewModel.Get().isRecording && TrainingSideTool.Get().StreamRecordingCheckbox.IsChecked.Value))
            {
                Skeleton avatar = null;
                if (skt != null)
                {
                    avatar = skt.skeleton;
                }
                ColorImageFrameReadyEventArgs e = (ColorImageFrameReadyEventArgs)evt;
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (colorFrame != null)
                    {
                        // Copy the pixel data from the image to a temporary array
                        colorFrame.CopyPixelDataTo(this.colorPixels);

                        // Write the pixel data into our bitmap
                        this.colorBitmap.WritePixels(
                            new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                            this.colorPixels,
                            this.colorBitmap.PixelWidth * sizeof(int),
                            0);

                        using (DrawingContext dc = drawingGroup.Open())
                        {
                            dc.DrawImage(this.colorBitmap, new Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }
                        // if the eventHandler is not null it means that we are recording the session, so we raise an event including a bitmap of our rendering 
                        if (backgroundDrawEventStream != null)
                            backgroundDrawEventStream(null, ImageToBitmap(colorFrame));
                        ShowFeedbacksOnVideoStream(avatar);
                    }
                }
            }
        }

        /// <summary>
        /// Return a bitmap version of the ColorImageFrame
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap ImageToBitmap(ColorImageFrame Image)
        {
            int width = Image.Width;
            int height = Image.Height;
            if (width % 2 == 1)
                width += 1;
            if (height % 2 == 1)
                height += 1; 

            byte[] pixeldata = new byte[Image.PixelDataLength];
            Image.CopyPixelDataTo(pixeldata);
            System.Drawing.Bitmap bmap = new System.Drawing.Bitmap(width, height , System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            System.Drawing.Imaging.BitmapData bmapdata = bmap.LockBits( new Rectangle(0, 0, width, height),ImageLockMode.WriteOnly, bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(pixeldata, 0, ptr, Image.PixelDataLength);
            bmap.UnlockBits(bmapdata);
            return bmap;
        }

        /// <summary>
        /// converts a writeable bitmap into a bitmap 
        /// make sure that width and height are > 0 to prevent exceptions 
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>A bitmap instance</returns>
        public System.Drawing.Bitmap WriteableBitmapToBitmap(WriteableBitmap drawing, int width, int height)
        {
            // creates an image with drawing
            System.Windows.Controls.Image drawingImage = new System.Windows.Controls.Image { Source = drawing };
            drawingImage.Arrange(new System.Windows.Rect(0, 0, width, height));
            // converts into rendertargetbitmap
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingImage);
            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(stream);

            return new Bitmap(stream);
        }

        Bitmap DepthToBitmap(DepthImageFrame imageFrame)
        {
            short[] pixelData = new short[imageFrame.PixelDataLength];
            imageFrame.CopyPixelDataTo(pixelData);
            Bitmap bmap = new Bitmap(imageFrame.Width,imageFrame.Height,System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            BitmapData bmapdata = bmap.LockBits(new Rectangle(0, 0, imageFrame.Width, imageFrame.Height), ImageLockMode.WriteOnly, bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(pixelData, 0, ptr, imageFrame.Width * imageFrame.Height);
            bmap.UnlockBits(bmapdata);
            return bmap;
        }

        /// <summary>
        /// All feedbacks in the canvas are hidden.
        /// </summary>
        public void hideFeedbacks()
        {
            foreach (UIElement d in dsv.CanvasFeedback.Children)
            {
                d.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Show and place feedbacks in the canvas for the live videostream and the replay videostream
        /// </summary>
        /// <param name="avatar">use only in the live videostream</param>
        public void ShowFeedbacksOnVideoStream(Skeleton avatar = null)
        {
            hideFeedbacks();

            if (TrainingSideToolViewModel.Get().BodyPool.isPresent(new Feedback("Too agitated!")))
            {
                ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Agitation"]]).Height = dsv.CanvasFeedback.ActualHeight / 3.5;
                dsv.CanvasFeedback.Children[correspondIndiceName["Agitation"]].Visibility = Visibility.Visible;
            }

            if (TrainingSideToolViewModel.Get().BodyPool.isPresent(new Feedback("Hands are joined")))
            {
                if(avatar != null && !ReplayViewModel.Get().isReplaying)
                {
                    avatarJointsPosition("Hand_Joined", avatar, true, 2.5, 0, -1, 0, 6);
                }
                else
                {
                    Canvas.SetTop(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Hand_Joined"]]), dsv.CanvasFeedback.ActualHeight / 3);
                    Canvas.SetLeft(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Hand_Joined"]]), 0);
                    ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Hand_Joined"]]).Height = dsv.CanvasFeedback.ActualHeight / 3.5;
                }
                dsv.CanvasFeedback.Children[correspondIndiceName["Hand_Joined"]].Visibility = Visibility.Visible;
            }

            if (TrainingSideToolViewModel.Get().BodyPool.isPresent(new Feedback("Arms Crossed")))
            {
                if (avatar != null && !ReplayViewModel.Get().isReplaying)
                {
                    avatarJointsPosition("Arms_Crossed", avatar, true, 2.5, 50, -1, -50, 5);
                }
                else
                {
                    Canvas.SetTop(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Arms_Crossed"]]), 2 * dsv.CanvasFeedback.ActualHeight / 3);
                    Canvas.SetLeft(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Arms_Crossed"]]), 0);
                    ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Arms_Crossed"]]).Height = dsv.CanvasFeedback.ActualHeight / 3.5;
                }
                dsv.CanvasFeedback.Children[correspondIndiceName["Arms_Crossed"]].Visibility = Visibility.Visible;
            }

            if (TrainingSideToolViewModel.Get().FacePool.isPresent(new Feedback("Look to the center")))
            {
                if (avatar != null && !ReplayViewModel.Get().isReplaying)
                {
                    avatarJointsPosition("Center_Arrow", avatar, false, 2.3, 0, -0.3, 0, 5);
                }
                else
                {
                    Canvas.SetRight(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Center_Arrow"]]), 0);
                    ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Center_Arrow"]]).Height = dsv.CanvasFeedback.ActualHeight / 3.5;
                }
                dsv.CanvasFeedback.Children[correspondIndiceName["Center_Arrow"]].Visibility = Visibility.Visible;
            }

            if (TrainingSideToolViewModel.Get().FacePool.isPresent(new Feedback("Look to the left")))
            {
                if (avatar != null && !ReplayViewModel.Get().isReplaying)
                {
                    avatarJointsPosition("Left_Arrow", avatar, false, 1.8, 0, -1, -50, 5);
                }
                else
                {
                    Canvas.SetRight(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Left_Arrow"]]), 0);
                    ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Left_Arrow"]]).Height = dsv.CanvasFeedback.ActualHeight / 3.5;
                }
                dsv.CanvasFeedback.Children[correspondIndiceName["Left_Arrow"]].Visibility = Visibility.Visible;
            }

            if (TrainingSideToolViewModel.Get().FacePool.isPresent(new Feedback("Look to the right")))
            {
                if (avatar != null && !ReplayViewModel.Get().isReplaying)
                {
                    avatarJointsPosition("Right_Arrow", avatar, false, 2.9, 0, -1, -50, 5);
                }
                else
                {
                    Canvas.SetRight(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Right_Arrow"]]), 0);
                    ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Right_Arrow"]]).Height = dsv.CanvasFeedback.ActualHeight / 3.5;
                }
                dsv.CanvasFeedback.Children[correspondIndiceName["Right_Arrow"]].Visibility = Visibility.Visible;
            }

            if (TrainingSideToolViewModel.Get().FacePool.isPresent(new Feedback("Happy")))
            {
                if (avatar != null && !ReplayViewModel.Get().isReplaying)
                {
                    avatarJointsPosition("Happy", avatar, false, 1.8, 0, -1, -50, 5);
                }
                else
                {
                    Canvas.SetRight(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Happy"]]), 0);
                    Canvas.SetTop(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Happy"]]), dsv.CanvasFeedback.ActualHeight / 3);
                    ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Happy"]]).Height = dsv.CanvasFeedback.ActualHeight / 3.5;
                }

                dsv.CanvasFeedback.Children[correspondIndiceName["Happy"]].Visibility = Visibility.Visible;
            }

            if (TrainingSideToolViewModel.Get().FacePool.isPresent(new Feedback("Suprised")))
            {
                if (avatar != null && !ReplayViewModel.Get().isReplaying)
                {
                    avatarJointsPosition("Surprised", avatar, false, 1.8, 0, -1, -50, 5);
                }
                else
                {
                    Canvas.SetRight(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Surprised"]]), 0);
                    Canvas.SetTop(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Surprised"]]), dsv.CanvasFeedback.ActualHeight / 3);
                    ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName["Surprised"]]).Height = dsv.CanvasFeedback.ActualHeight / 3.5;
                }
                dsv.CanvasFeedback.Children[correspondIndiceName["Surprised"]].Visibility = Visibility.Visible;
            }

            dsv.CanvasFeedback.UpdateLayout();
        }

        /// <summary>
        /// Fonction to automate the joints positions/resets
        /// </summary>
        /// <param name="imgNameReaction"></param>
        /// <param name="avatar"></param>
        /// <param name="joint_type_hand_left"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="h"></param>
        /// <author>Charles BIDAUT</author>
        private void avatarJointsPosition(String imgNameReaction, Skeleton avatar, Boolean joint_type_hand_left, double x1, double x2, double y1, double y2, double h)
        {
            double x, y, z;

            if (joint_type_hand_left == true)
            {
                x = avatar.Joints[JointType.HandLeft].Position.X;
                y = avatar.Joints[JointType.HandLeft].Position.Y;
                z = avatar.Joints[JointType.HandLeft].Position.Z;
            }
            else
            {
                x = avatar.Joints[JointType.Head].Position.X;
                y = avatar.Joints[JointType.Head].Position.Y;
                z = avatar.Joints[JointType.Head].Position.Z;
            }

            x = (x + x1) * (dsv.CanvasFeedback.ActualWidth / 5);
            y = -(y + y1) * (dsv.CanvasFeedback.ActualHeight / 2);
            Canvas.SetTop(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName[imgNameReaction]]), y + y2);
            Canvas.SetLeft(((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName[imgNameReaction]]), x + x2);
            ((System.Windows.Controls.Image)dsv.CanvasFeedback.Children[correspondIndiceName[imgNameReaction]]).Height = dsv.CanvasFeedback.ActualHeight / h;
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Convert a Bitmap in a Image. Use the dll : gdi32.dll
        /// </summary>
        private System.Windows.Controls.Image Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            System.Windows.Controls.Image retval;
            retval = new System.Windows.Controls.Image();

            try
            {
                retval.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
            return retval;
        }

        /// <summary>
        /// Allow to load pictures in the folder View/Icons in a ressource file
        /// </summary>
        public void loadPictureInResource()
        {
            ResXResourceWriter rw = new ResXResourceWriter("Resources.resx");

            foreach (string s in Directory.EnumerateFiles(@"..\..\View\Icons"))
            {
                FileStream byteStream = new FileStream(s, FileMode.Open);
                byte[] bytes = new byte[(int)byteStream.Length];
                byteStream.Read(bytes, 0, (int)byteStream.Length);
                Bitmap bp = new Bitmap(byteStream);
                rw.AddResource(Path.GetFileNameWithoutExtension(s), bp);
                byteStream.Close();
            }
            foreach (string s in Directory.EnumerateFiles(@"..\..\View\Icons\soundTexture"))
            {
                FileStream byteStream = new FileStream(s, FileMode.Open);
                byte[] bytes = new byte[(int)byteStream.Length];
                byteStream.Read(bytes, 0, (int)byteStream.Length);
                Bitmap bp = new Bitmap(byteStream);
                rw.AddResource(Path.GetFileNameWithoutExtension(s), bp);
                byteStream.Close();
            }
            rw.Generate();
            rw.Close();
        }

        /// <summary>
        /// Allow to create Image objects from the ressource file create in loadPictureInResource
        /// </summary>
        public void loadImageFromRessource()
        {
            ResXResourceSet resxSet = new ResXResourceSet("Resources.resx");
            correspondIndiceName = new Dictionary<string, int>();

            foreach (DictionaryEntry d  in resxSet)
            {
                ImageFeedbacksPerso img = new ImageFeedbacksPerso();
                img.chooseCorrectName((string)d.Key);
                img.bitmapOpenGL = (Bitmap)d.Value;
                img.image = Bitmap2BitmapImage(img.bitmapOpenGL);
                listImg.Add(img);
            }
            resxSet.Close();
        }

        /// <summary>
        /// Allow to change the colour of each feedbacks and reload the image in the canvas
        /// </summary>
        public void changeColorFeedbacks()
        {
            foreach (ImageFeedbacksPerso ifp in listImg)
            {
                System.Windows.Media.Color temp = (System.Windows.Media.Color)App.Current.Resources["FeedbackStreamColor"];
                ifp.changeColor(System.Drawing.Color.FromArgb(temp.A, temp.R, temp.G, temp.B), false, ifp.bitmapOpenGL);
                ifp.image = Bitmap2BitmapImage(ifp.bitmapOpenGL);
                dsv.CanvasFeedback.Children.Clear();
                correspondIndiceName.Clear();
                AddImageOnCanvas();
            }
        }

        /// <summary>
        /// Allow to add image created in the function loadImageFromRessource to the canvas
        /// </summary>
        public void AddImageOnCanvas()
        {
            foreach (ImageFeedbacksPerso ifp in listImg )
            {
                switch (ifp.name)
                {
                    case "Agitation":
                    case "Hand_Joined":
                    case "Arms_Crossed":
                    case "Happy":
                    case "Surprised":
                    case "Left_Arrow":
                    case "Right_Arrow":
                    case "Center_Arrow":
                        dsv.CanvasFeedback.Children.Add(ifp.image);
                        correspondIndiceName.Add(ifp.name, dsv.CanvasFeedback.Children.IndexOf(ifp.image));
                        dsv.CanvasFeedback.Children[dsv.CanvasFeedback.Children.IndexOf(ifp.image)].Visibility = Visibility.Hidden;
                        break;
                }
            }
        }
        #endregion
    }
}
