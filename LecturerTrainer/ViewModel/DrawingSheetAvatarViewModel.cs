using LecturerTrainer.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LecturerTrainer.View;
using Tao.FreeGlut;
using Tao.OpenGl;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using OpenTK;
using LecturerTrainer.Model.EmotionRecognizer;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Text.RegularExpressions;
using LecturerTrainer.Model;


// TO NOTE : the z axis has been reversed for a better visibility of the avatar
namespace LecturerTrainer.Model
{
    class DrawingSheetAvatarViewModel : DrawingSheetViewModel
    {
        #region fields

        #region Mentor's variables

        public static String displayCustomText = String.Empty;

        private static int count;
        private static bool drawFM;

        private static Skeleton skToDisplay = null;
        private static FaceDataWrapper fcToDisplay = new FaceDataWrapper(null, null, null);

        private Vector3 mentorMTUL;
        private Vector3 mentorMBUL;
        private Vector3 mentorMBLL;
        private Vector3 mentorMTLL;
        private Vector3 mentorORCM;
        private Vector3 mentorOLCM;

        private Vector3 mentorface19;
        private Vector3 mentorface20;
        private Vector3 mentorface21;
        private Vector3 mentorface22;
        private Vector3 mentorface23;
        private Vector3 mentorface24;

        private Vector3 mentorface52;
        private Vector3 mentorface53;
        private Vector3 mentorface54;
        private Vector3 mentorface55;
        private Vector3 mentorface56;
        private Vector3 mentorface57;

        private Vector3 mentorface15;
        private Vector3 mentorface16;
        private Vector3 mentorface17;
        private Vector3 mentorface18;

        private Vector3 mentorface48;
        private Vector3 mentorface49;
        private Vector3 mentorface50;
        private Vector3 mentorface51;

        private Vector3 mentorheadCenterPoint;
        private Vector3 mentorheadTilt;
        #endregion

        /// <summary>
        /// the instance of the singleton class 
        /// </summary>
        private static DrawingSheetAvatarViewModel dsavm = null;


        /// <summary>
        /// Raise an event when we want to transfer a frame to the recorder
        /// </summary>
        /// <author> Amirali Ghazi</author>
        public static EventHandler<Bitmap> backgroundRecordingEventStream;

        /// <summary>
        /// Raise an event when we want to transfer a frame to the recorder
        /// </summary>
        /// <author> Amirali Ghazi</author>
        public static EventHandler<Skeleton> backgroundXMLRecordingEventStream;

        /// <summary>
        /// Raise an event when we want to transfer a frame to the recorder
        /// </summary>
        /// <author> Amirali Ghazi </author>
        public static EventHandler<FaceDataWrapper> backgroundXMLFaceRecordingEventStream;

		/// <summary>
        /// Raise an event when we want to transfer a frame to the recorder
        /// </summary>
        /// <author> Amirali Ghazi </author>
        public static EventHandler<float> backgroundXMLVoiceRecordingEventStream;

        /// <summary>
        /// Boolean representing the state of the video recording 
        /// (if the user decides to record his performance or not)
        /// </summary>
        /// <author> Amirali Ghazi</author>
        public bool IsVideoAvatarRecording { get; set; }

        ///<summary>
        /// Boolean representing the state of the skeleton recording, i.e. whether it has to record
        /// the coordinates in a xml file or not
        /// </summary>
        /// <author> Amirali Ghazi</author>
        public bool IsOpenGLRecording { get; set; }

        // for the grabscreenshot method
        private bool firstScreenshot = true;
        private Bitmap oldBmp;

        public bool diplayFeedback = true;
        public bool diplayBodyFeedback = true;
        public bool diplayFaceFeedback = true;

        public bool displayAgitationFeedback = true;
        public bool displayHandsJoinedFeedback = true;
        public bool displayArmsCrossedFeedback = true;
        public bool displayEmotionFeedback = true;
        public bool displayLookDirFeedback = true;


        /// <summary>
        /// Points situating the face
        /// </summary>
        private Vector3 MTUL;
        private Vector3 MBUL;
        private Vector3 MBLL;
        private Vector3 MTLL;
        private Vector3 ORCM;
        private Vector3 OLCM;

        private Vector3 face19;
        private Vector3 face20;
        private Vector3 face21;
        private Vector3 face22;
        private Vector3 face23;
        private Vector3 face24;

        private Vector3 face52;
        private Vector3 face53;
        private Vector3 face54;
        private Vector3 face55;
        private Vector3 face56;
        private Vector3 face57;


        private Vector3 face15;
        private Vector3 face16;
        private Vector3 face17;
        private Vector3 face18;

        private Vector3 face48;
        private Vector3 face49;
        private Vector3 face50;
        private Vector3 face51;


        /// <summary>
        /// The control allowing to create a 3D scene
        /// </summary>
        private GLControl glControl;

        /// <summary>
        /// Stacks and slices of glut elements composing the avatar
        /// </summary>
        private readonly int generalStacks = 10;
        private readonly int generalSlices = 10;

        /// <summary>
        /// Radius of the 3D elements composing the avatar
        /// </summary>
        private readonly float jointsRadius = 0.03f;
        private readonly float jointsHandRadius = 0.01f;
        private readonly float bonesRadius = 0.05f;
        private readonly float headRadius = 0.1f;
        private readonly float shoulderRadius = 0.05f;
        private readonly float upperTorsoRadius = 0.16f;
        private readonly float lowerTorsoRadius = 0.16f;
        private readonly float hipRadius = 0.07f;
        private readonly float armRadius = 0.04f;
        private readonly float legRadius = 0.07f;

        /// <summary>
        /// Avatar initial position.
        /// </summary>
        //private readonly Vector3 initialHead = new Vector3(0.135897f, 0.7635499f, 2.229455f);
        private readonly Vector3 initialHead = new Vector3(0, 0.7f, 2.229455f);
        //private readonly Vector3 initialShoulderCenter = new Vector3(0.1280521f, 0.5663089f, 2.229455f);
        private readonly Vector3 initialShoulderCenter = new Vector3(0, 0.55f, 2.229455f);
        //private readonly Vector3 initialShoulderLeft = new Vector3(-0.03456776f, 0.4672897f, 2.229455f);
        private readonly Vector3 initialShoulderLeft = new Vector3(-0.18f, 0.45f, 2.229455f);
        //private readonly Vector3 initialShoulderRight = new Vector3(0.3046574f, 0.4610354f, 2.229455f);
        private readonly Vector3 initialShoulderRight = new Vector3(0.18f, 0.45f, 2.229455f);
        //private readonly Vector3 initialElbowLeft = new Vector3(-0.1046979f, 0.2168238f, 2.21722f);
        private readonly Vector3 initialElbowLeft = new Vector3(-0.24f, 0.2f, 2.229455f);
        //private readonly Vector3 initialElbowRight = new Vector3(0.3838935f, 0.2056853f, 2.254441f);
        private readonly Vector3 initialElbowRight = new Vector3(0.24f, 0.2f, 2.229455f);
        //private readonly Vector3 initialWristLeft = new Vector3(-0.1332553f, -0.02705427f, 2.115675f);
        private readonly Vector3 initialWristLeft = new Vector3(-0.2f, -0.05f, 2.229455f);
        //private readonly Vector3 initialWristRight = new Vector3(0.4350696f, 0.007437438f, 2.160146f);
        private readonly Vector3 initialWristRight = new Vector3(0.2f, -0.05f, 2.229455f);
        //private readonly Vector3 initialHandLeft = new Vector3(-0.1316205f, -0.1162565f, 2.085958f);
        private readonly Vector3 initialHandLeft = new Vector3(-0.19f, -0.2f, 2.229455f);
        //private readonly Vector3 initialHandRight = new Vector3(0.4430848f, -0.0936875f, 2.119174f);
        private readonly Vector3 initialHandRight = new Vector3(0.19f, -0.2f, 2.229455f);
        //private readonly Vector3 initialSpine = new Vector3(0.1352139f, 0.1978379f, 2.229455f);
        private readonly Vector3 initialSpine = new Vector3(0, 0.1f, 2.229455f);
        //private readonly Vector3 initialHipCenter = new Vector3(0.1422898f, 0.1372883f, 2.229455f);
        private readonly Vector3 initialHipCenter = new Vector3(0, 0.05f, 2.229455f);
        //private readonly Vector3 initialHipLeft = new Vector3(0.06824586f, 0.06192002f, 2.204764f);
        private readonly Vector3 initialHipLeft = new Vector3(-0.075f, 0, 2.229455f);
        //private readonly Vector3 initialHipRight = new Vector3(0.2213473f, 0.06250456f, 2.221346f);
        private readonly Vector3 initialHipRight = new Vector3(0.075f, 0, 2.229455f);
        //private readonly Vector3 initialKneeLeft = new Vector3(0.002393939f, -0.4226111f, 2.23532f);
        private readonly Vector3 initialKneeLeft = new Vector3(-0.085f, -0.4f, 2.229455f);
        //private readonly Vector3 initialKneeRight = new Vector3(0.2920577f, -0.4315554f, 2.239343f);
        private readonly Vector3 initialKneeRight = new Vector3(0.085f, -0.4f, 2.229455f);
        //private readonly Vector3 initialAnkleLeft = new Vector3(-0.04282022f, -0.804864f, 2.239705f);
        private readonly Vector3 initialAnkleLeft = new Vector3(-0.075f, -0.8f, 2.229455f);
        //private readonly Vector3 initialAnkleRight = new Vector3(0.3301864f, -0.8169076f, 2.264407f);
        private readonly Vector3 initialAnkleRight = new Vector3(0.075f, -0.8f, 2.229455f);
        //private readonly Vector3 initialFootLeft = new Vector3(-0.06813762f, -0.8776183f, 2.208368f);
        private readonly Vector3 initialFootLeft = new Vector3(-0.12f, -0.8776183f, 2.2f);
        //private readonly Vector3 initialFootRight = new Vector3(0.3620958f, -0.8851607f, 2.2292f);
        private readonly Vector3 initialFootRight = new Vector3(0.12f, -0.8776183f, 2.2f);

        /// <summary>
        /// Proper dimensions of the avatar parts. 
        /// These parameters allow to keep good proportions at every moment.
        /// </summary>
        private float properHeadToShoulderCenter;
        private float properShoulderCenterToShoulderEnd;
        private float properShoulderEndToElbow;
        private float properElbowToWrist;
        private float properWristToHand;
        private float properHipCenterToHipEnd;
        private float properHipEndToKnee;
        private float properKneeToAnkle;
        private float properAnkleToFoot;
        private float properShoulderCenterToSpine;
        private float properSpineToHipCenter;

        //static float theta = (float)Math.PI / 6;

        /// <summary>
        /// Colors used to draw skeleton joints and bones and the colours of the feedbacks and the background
        /// Modified by Baptiste Germond
        /// </summary> 
        private OpenTK.Vector4 trackedJointColor = new OpenTK.Vector4(68 / 255f, 192 / 255f, 68 / 255f, 1);
        private OpenTK.Vector4 inferredJointColor = new OpenTK.Vector4(1, 1, 0, 1);
        private OpenTK.Vector4 trackedBoneColor = new OpenTK.Vector4(1, 0, 0, 1);
        private OpenTK.Vector4 inferredBoneColor = new OpenTK.Vector4(0.5f, 0.5f, 0.5f, 1);
        private OpenTK.Vector3 feedbackColor = new OpenTK.Vector3(1, 1, 1); //0011
        private System.Drawing.Color pixelFeedbackColor = System.Drawing.Color.FromArgb(255, 128, 128, 128);
        private System.Drawing.Color backgroundColor = System.Drawing.Color.FromArgb(255, 30, 31, 36);
        private OpenTK.Vector4 mentorBoneColor = new OpenTK.Vector4(0, 0, 1, 1);
        private OpenTK.Vector4 savedBoneColor = new OpenTK.Vector4(1, 0, 0, 1);
        /// <summary>
        /// List of the available themes
        /// Added by Baptiste Germond
        /// </summary>
        private List<ThemeOpenGL> themesList = new List<ThemeOpenGL>();
        /// <summary>
        /// Actual theme display by the software
        /// </summary>
        public ThemeOpenGL actualTheme;

        /// <summary>
        /// Vectors indicating the translation to apply before drawing
        /// they allow to keep good proportions. The second translation is only apply on a particular member
        /// and is reset after the drawing.
        /// </summary>
        private Vector3 generalAdjustment = new Vector3(0, 0, 0);
        private Vector3 specificAdjustment = new Vector3(0, 0, 0);

        /// <summary>
        /// Margin of error for adjustement. If the current bone is smaller than 
        /// wellPropBoneSize - adjustmentErrorMargin or bigger than wellPropBoneSize + adjustmentErrorMargin,
        /// an adjustment is made
        /// </summary>
        private readonly float adjustmentErrorMargin = 0.01f;

        /// <summary>
        /// The current center of the head. Used for face tracking.
        /// </summary>
        private Vector3 headCenterPoint;
        private Vector3 headTilt;

        /// <summary>
        /// Vector corresponding to the alignment of the eyes (headTilt.EyesAlignment is normally equal to 0)
        /// </summary>
        private Vector3 EyesAlignment;

        /// <summary>
        /// Floats corresponding to the current distance between the eyebrow and the eye and the distance between the eyebrow and the eye on the last frame
        /// </summary>
        private float currentRightEBEDist;
        private float lastRightEBEDist = 0;
        private float currentLeftEBEDist;
        private float lastLeftEBEDist = 0;

        /// <summary>
        /// Float corresponding to the difference between currentEBE and lastEBE
        /// </summary>
        private float eyebrowDifference;

        /// <summary>
        /// Float added to the eyebrows' Y coordinate to make the animation
        /// </summary>
        private float eyebrowAdjustment = 0;

        /// <summary>
        /// Float for opening/closing the mouth
        /// </summary>
        private float currentLipsDistance;
        private float lastLipsDistance = 0;
        private float LipsDistanceDiff;
        private float verticalLipsAdjustment;
        private float horizontalLipsAdjustment;

        private bool isInitialized = false, isSignalLostInitialized = false;

        public int nbFrames { get; set; }

        /// <summary>
        /// Boolean used to know if the software is in replaying mode
        /// </summary>
        public bool isRecording { get; set; }

        /// <summary>
        /// Boolean used to know if the software is in training mode
        /// </summary>
        public bool isTraining = false;
        public bool mentor = false;

        public struct FaceModelTriangle
        {
            public Vector2 P1;
            public Vector2 P2;
            public Vector2 P3;
        }

        // The skeleton to draw when no skeleton is tracked (used for the replay mode)
        public Skeleton skToDrawInReplay = null;

        // Indicate if the face has to be drawn (used for the replay mode)
        public bool drawFaceInReplay = false;

        // Timothée
        /// <summary>
        /// Contained the speech to display
        /// </summary>
        private Queue<char> txtToDisplay;

        // Timothée
        /// <summary>
        /// Used to display the text sayed by the user
        /// </summary>
        private char[] txtShowOnscreen;

        // Timothée
        /// <summary>
        /// Used to know if we show the text said
        /// </summary>
        private bool displayText = false;

        // Timothée
        /// <summary>
        /// Text used by the prompter
        /// </summary>
        private List<String> prompterText;

        // Timothée 
        /// <summary>
        /// Is the teleprompter activated
        /// </summary>
        private bool teleprompterActivated = false;

        // Timothée
        /// <summary>
        /// Used to know the first line display by th prompter in prompterText
        /// </summary>
        private int actualLine;

        /// <summary>
        /// True if legs are tracked 
        /// </summary>
        private bool legTracked = false;
        public bool LegTracked
        {
            get
            {
                return legTracked;
            }
        }

        #endregion

        #region constructor and Get()
        // private constructor allow us to make sure that we have only one instance 
        private DrawingSheetAvatarViewModel(DrawingSheetView dsv)
            : base()
        {
            this.dsv = dsv;
            initAvatarDimensions();
            this.glControl = dsv.glControl;
            glControl.Paint += glControl_Paint;
            glControl.Resize += glControl_Resize;

            isRecording = false;
            nbFrames = 0;

            createAllThemes();
        }

        /// <summary>
        /// Safe way to call the Get() function, instantiates if needed the DrawingSheetAvatarViewModel 
        /// </summary>
        /// <param name="drawingSheetView"></param>
        /// <returns></returns>
        public static DrawingSheetAvatarViewModel Get(DrawingSheetView drawingSheetView)
        {
            if (dsavm == null)
                dsavm = new DrawingSheetAvatarViewModel(drawingSheetView);
            return dsavm;
        }

        /// <summary>
        /// Wild way to call the Get() function. Please make sure that the value returned is not null. 
        /// </summary>
        /// <returns></returns>
        public static DrawingSheetAvatarViewModel Get()
        {
            return dsavm;
        }

        public OpenTK.Vector4 TrackedJointColor
        {
            get { return trackedJointColor; }
            set { trackedJointColor = value; }
        }
        public OpenTK.Vector4 InferredJointColor
        {
            get { return inferredJointColor; }
            set { inferredJointColor = value; }
        }
        public OpenTK.Vector4 TrackedBoneColor
        {
            get { return trackedBoneColor; }
            set { trackedBoneColor = value; }
        }
        public OpenTK.Vector4 InferredBoneColor
        {
            get { return inferredBoneColor; }
            set { inferredBoneColor = value; }
        }
        public System.Drawing.Color PixelFeedbackColor
        {
            get { return pixelFeedbackColor; }
            set { pixelFeedbackColor = value; }
        }
        public System.Drawing.Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        public List<ThemeOpenGL> getThemeList
        {
            get
            {
                return themesList;
            }
        }
        #endregion

        #region general methods
        protected override void Main_isReady(object sender, EventArgs args)
        {
            normalMode();
            if (sender != this)
            {
                Main.isReady -= Main_isReady;
            }
        }

        /// <summary>
        /// Draws the scene 
        /// </summary>
        private void display(EventArgs evt)
        {
            
            if (KinectDevice.faceTracking && !KinectDevice.SwitchDraw)
            {
                drawFace(evt);
            }
            else
            {
                /*If the software is in training mode, we have to display two avatar*/
                if (isTraining)
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    //CHANGE HERE
                    /*
                     * GL.PushMatrix();
                        {
                            displayTextTraining();
                            GL.Translate(-1.0f, 0, 0);
                            drawAvatar(evt);
                        }
                        GL.PopMatrix();
                    *
                    */

                    if (displayCustomText.Length == 0)
                    {
                        GL.PushMatrix();
                        {
                            /*user's avatar*/
                            displayTextTraining();
                            GL.Translate(-1.0f, 0, 0);
                            drawAvatar(evt);
                        }
                        GL.PopMatrix();
                    }
                    else
                    {
                        GL.PushMatrix();
                        {
                            /*user's avatar*/
                            DisplayTextTraining(displayCustomText);
                            GL.Translate(-1.0f, 0, 0);
                            drawAvatar(evt);
                        }
                        GL.PopMatrix();
                    }

                    /*The coach's avatar*/
                    GL.PushMatrix();
                    GL.Translate(1.0f, 0, 0);
                    /*Get the skeleton object to replay*/
                    trackedBoneColor = mentorBoneColor;
                    
                    if(drawFM)
                    {
                        count++;
                        if (count % 2 == 0)
                        {
                            fcToDisplay = TrainingWithAvatarViewModel.Get().ChooseFaceToDisplay();
                            if (fcToDisplay.depthPointsList != null && skToDisplay != null)
                            {
                                DrawMentor(skToDisplay, fcToDisplay, true);
                            }
                            else
                            {
                                drawFM = false;
                                count = 0;
                            }
                        }
                        else
                        {
                            skToDisplay = TrainingWithAvatarViewModel.Get().chooseSkeletonToDisplay();
                            if(skToDisplay != null && fcToDisplay.depthPointsList != null)
                            {
                                DrawMentor(skToDisplay, fcToDisplay, true);
                            }
                            else
                            {
                                drawFM = false;
                                count = 0;
                            }
                        }
                    }
                    else
                    {
                        skToDisplay = TrainingWithAvatarViewModel.Get().chooseSkeletonToDisplay();
                        if (skToDisplay != null)
                        {
                            fcToDisplay = TrainingWithAvatarViewModel.Get().ChooseFaceToDisplay();
                            if (fcToDisplay.depthPointsList != null)
                                drawFM = true;
                            DrawMentor(skToDisplay, fcToDisplay, drawFM);
                        }
                        else
                            drawInitialAvatar();
                    }
                    
                    GL.PopMatrix();
                    GL.Flush();
                    trackedBoneColor = savedBoneColor;

                }
                /*Normal mode*/
                else
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    GL.PushMatrix();
                    {
                        drawAvatar(evt);
                    }
                    GL.PopMatrix();
                }

            }

            if (IsVideoAvatarRecording)
            {
               // TODO record avatar video
               // backgroundRecordingEventStream?.Invoke(this, GrabScreenshot());
            }

            if (IsOpenGLRecording && evt.GetType() == typeof(SkeletonEventArgs))
            {
                SkeletonEventArgs skEvent = (SkeletonEventArgs)evt;
                Skeleton skeleton = skEvent.skeleton;
                backgroundXMLRecordingEventStream?.Invoke(this, skeleton);
            }
        }

        /// <summary>
        /// Draws the scene without any event handled
        /// </summary>
        private void display()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PushMatrix();
            
            // If no replay skeleton is detected, display of the initial avatar
            if (skToDrawInReplay == null)
            {
				DrawAxes();
                drawInitialAvatar();
			}
            else
                drawAvatar(skToDrawInReplay, drawFaceInReplay);
            GL.PopMatrix();
            GL.Flush();


        }

        /// <summary>
        /// On replay mode, the kinect's sensor is not linked to draw
        /// like that, the sheet can display replay elements
        /// </summary>
        public void replayMode()
        {
            Main.kinect.skeletonUpdated -= draw;
            SkeletonFaceTracker.faceUpdated -= draw;
        }

        /// <summary>
        /// On normal mode, the kinect's sensor is linked to draw
        /// </summary>
        public void normalMode()
        {
            Main.kinect.skeletonUpdated -= draw;
            Main.kinect.skeletonUpdated += draw;
            SkeletonFaceTracker.faceUpdated -= draw;
            SkeletonFaceTracker.faceUpdated += draw;
        }

        public override void draw(object sender, EventArgs evt)
        {
            display(evt);
            if (evt.GetType() == typeof(SkeletonEventArgs))
            {
                DrawingSheetStreamViewModel.skt = (SkeletonEventArgs)evt; //used to retrieve the skeleton in the videostream view
            }
            if (!KinectDevice.faceTracking || KinectDevice.SwitchDraw)
                glControl.SwapBuffers();
        }

        /// <summary>
        /// Function which displays the face's mesh. It calculates positions, there is no display yet.
        /// </summary>
        /// <remarks>
        /// Extract of Microsoft sample : FaceTracking
        /// (Amirali Ghazi)
        /// I did not change the names of the variables but these names are not explicit at all.
        /// </remarks>
        /// <param name="faceP3D"> Represents the DepthPoints of the face</param>
        /// <param name="faceP"> Represents the ColourPoints of the face</param>
        /// <param name="faceT"> Represents the triangles (3 vertexes/vertices) of the face</param>
        public void drawFace(
            List<Vector3DF> faceP3D,
            List<Microsoft.Kinect.Toolkit.FaceTracking.PointF> faceP,
            FaceTriangle[] faceT)
        {
            List<Vector2> faceModelPts = new List<Vector2>();
            List<Vector2> faceModelPtsDraw = new List<Vector2>();
            var faceModel = new List<FaceModelTriangle>();

            // This vector will allow to center the face on the head
            Vector3 faceAdjustment = new Vector3();

            // the center point is situated between the two eyes and the upperlip
            Vector3 faceCenterPoint = new Vector3(faceP3D.ElementAt(20).X, faceP3D.ElementAt(20).Y, faceP3D.ElementAt(20).Z);
            faceCenterPoint += new Vector3(faceP3D.ElementAt(53).X, faceP3D.ElementAt(53).Y, faceP3D.ElementAt(53).Z);
            faceCenterPoint += new Vector3(faceP3D.ElementAt(40).X, faceP3D.ElementAt(40).Y, faceP3D.ElementAt(40).Z);
            faceCenterPoint = faceCenterPoint / 3;
            //faceAdjustment = headCenterPoint + new Vector3(0, 0, -headRadius) - faceCenterPoint;

            Vector3 radial = faceCenterPoint + headTilt / 2 - headCenterPoint;
            double magnitude = Math.Sqrt(radial.X * radial.X + radial.Y * radial.Y + radial.Z * radial.Z);
            float fmagnitude = (float)magnitude;
            faceAdjustment = headCenterPoint + headRadius * radial / fmagnitude - faceCenterPoint;

            // Upper lip
            MTUL = new Vector3(faceP3D.ElementAt(7).X, faceP3D.ElementAt(7).Y, faceP3D.ElementAt(7).Z) + faceAdjustment;
            MBUL = new Vector3(faceP3D.ElementAt(87).X, faceP3D.ElementAt(87).Y, faceP3D.ElementAt(87).Z) + faceAdjustment;
            // Lower lip
            MBLL = new Vector3(faceP3D.ElementAt(41).X, faceP3D.ElementAt(41).Y, faceP3D.ElementAt(41).Z) + faceAdjustment;
            MTLL = new Vector3(faceP3D.ElementAt(40).X, faceP3D.ElementAt(40).Y, faceP3D.ElementAt(40).Z) + faceAdjustment;
            // Right mouth
            ORCM = new Vector3(faceP3D.ElementAt(31).X, faceP3D.ElementAt(31).Y, faceP3D.ElementAt(31).Z) + faceAdjustment;
            // Left mouth
            OLCM = new Vector3(faceP3D.ElementAt(64).X, faceP3D.ElementAt(64).Y, faceP3D.ElementAt(64).Z) + faceAdjustment;
            // Right eye
            face19 = new Vector3(faceP3D.ElementAt(19).X, faceP3D.ElementAt(19).Y, faceP3D.ElementAt(19).Z) + faceAdjustment;
            face20 = new Vector3(faceP3D.ElementAt(20).X, faceP3D.ElementAt(20).Y, faceP3D.ElementAt(20).Z) + faceAdjustment;
            face21 = new Vector3(faceP3D.ElementAt(21).X, faceP3D.ElementAt(21).Y, faceP3D.ElementAt(21).Z) + faceAdjustment;
            face22 = new Vector3(faceP3D.ElementAt(22).X, faceP3D.ElementAt(22).Y, faceP3D.ElementAt(22).Z) + faceAdjustment;
            face23 = new Vector3(faceP3D.ElementAt(23).X, faceP3D.ElementAt(23).Y, faceP3D.ElementAt(23).Z) + faceAdjustment;
            face24 = new Vector3(faceP3D.ElementAt(24).X, faceP3D.ElementAt(24).Y, faceP3D.ElementAt(24).Z) + faceAdjustment;
            // Right eyebrow
            face15 = new Vector3(faceP3D.ElementAt(15).X, faceP3D.ElementAt(15).Y * 1.25f, faceP3D.ElementAt(15).Z) + faceAdjustment;
            face16 = new Vector3(faceP3D.ElementAt(16).X, faceP3D.ElementAt(16).Y * 1.25f, faceP3D.ElementAt(16).Z) + faceAdjustment;
            face17 = new Vector3(faceP3D.ElementAt(17).X, faceP3D.ElementAt(17).Y * 1.25f, faceP3D.ElementAt(17).Z) + faceAdjustment;
            face18 = new Vector3(faceP3D.ElementAt(18).X, faceP3D.ElementAt(18).Y * 1.25f, faceP3D.ElementAt(18).Z) + faceAdjustment;
            // Left eye
            face52 = new Vector3(faceP3D.ElementAt(52).X, faceP3D.ElementAt(52).Y, faceP3D.ElementAt(52).Z) + faceAdjustment;
            face53 = new Vector3(faceP3D.ElementAt(53).X, faceP3D.ElementAt(53).Y, faceP3D.ElementAt(53).Z) + faceAdjustment;
            face54 = new Vector3(faceP3D.ElementAt(54).X, faceP3D.ElementAt(54).Y, faceP3D.ElementAt(54).Z) + faceAdjustment;
            face55 = new Vector3(faceP3D.ElementAt(55).X, faceP3D.ElementAt(55).Y, faceP3D.ElementAt(55).Z) + faceAdjustment;
            face56 = new Vector3(faceP3D.ElementAt(56).X, faceP3D.ElementAt(56).Y, faceP3D.ElementAt(56).Z) + faceAdjustment;
            face57 = new Vector3(faceP3D.ElementAt(57).X, faceP3D.ElementAt(57).Y, faceP3D.ElementAt(57).Z) + faceAdjustment;
            // Left eyebrow
            face48 = new Vector3(faceP3D.ElementAt(48).X, faceP3D.ElementAt(48).Y*1.25f, faceP3D.ElementAt(48).Z) + faceAdjustment;
            face49 = new Vector3(faceP3D.ElementAt(49).X, faceP3D.ElementAt(49).Y*1.25f, faceP3D.ElementAt(49).Z) + faceAdjustment;
            face50 = new Vector3(faceP3D.ElementAt(50).X, faceP3D.ElementAt(50).Y*1.25f, faceP3D.ElementAt(50).Z) + faceAdjustment;
            face51 = new Vector3(faceP3D.ElementAt(51).X, faceP3D.ElementAt(51).Y*1.25f, faceP3D.ElementAt(51).Z) + faceAdjustment;
            //Eyes alignment
            EyesAlignment = new Vector3(face53.X - face20.X, face53.Y - face20.Y, face53.Z - face20.Z);

            //Finally, we want to lengthen face elements
            float verticalFaceGap = 0.02f * 2.0f;
            float horizontalFaceGap = 0.05f * 2.0f;
            // Eyes spreading
            lengthenSegment(ref face19, ref face52, horizontalFaceGap);
            lengthenSegment(ref face24, ref face57, horizontalFaceGap);
            lengthenSegment(ref face23, ref face56, horizontalFaceGap);
            lengthenSegment(ref face20, ref face53, horizontalFaceGap);
            lengthenSegment(ref face21, ref face54, horizontalFaceGap);
            lengthenSegment(ref face22, ref face55, horizontalFaceGap);
            // Eyes enlargement
            lengthenSegment(ref face23, ref face20, horizontalFaceGap);
            lengthenSegment(ref face22, ref face21, verticalFaceGap);
            lengthenSegment(ref face19, ref face24, verticalFaceGap);
            lengthenSegment(ref face53, ref face56, horizontalFaceGap);
            lengthenSegment(ref face54, ref face55, verticalFaceGap);
            lengthenSegment(ref face52, ref face57, verticalFaceGap);
            // Eyebrows spreading
            lengthenSegment(ref face15, ref face48, horizontalFaceGap);
            lengthenSegment(ref face16, ref face49, horizontalFaceGap);
            lengthenSegment(ref face18, ref face51, horizontalFaceGap);
            lengthenSegment(ref face49, ref face51, verticalFaceGap/3);
            lengthenSegment(ref face16, ref face18, verticalFaceGap/3);
            // Mouth enlargement
            lengthenSegment(ref MTUL, ref MBLL, verticalFaceGap);
            lengthenSegment(ref ORCM, ref OLCM, horizontalFaceGap*1.5f);

            for (int i = 0; i < faceP.Count; i++)
            {
                faceModelPts.Add(new Vector2(faceP[i].X, faceP[i].Y));
                faceModelPtsDraw.Add(new Vector2(faceP[i].X, faceP[i].Y));
            }

            foreach (var t in faceT)
            {
                var triangle = new FaceModelTriangle();
                triangle.P1 = faceModelPtsDraw[t.First];
                triangle.P2 = faceModelPtsDraw[t.Second];
                triangle.P3 = faceModelPtsDraw[t.Third];
                faceModel.Add(triangle);
            }
        }



        /// <summary>
        /// Function which displays the face's mesh. It calculates positions, there is no display yet.
        /// </summary>
        /// <remarks>
        /// Extract of Microsoft sample : FaceTracking
        /// </remarks>
        /// <param name="evt"></param>
        private void drawFace(EventArgs evt)
        {
            if (evt.GetType() == typeof(FaceTrackerEventArgs))
            {
                FaceTrackerEventArgs face = (FaceTrackerEventArgs)evt;
                var temp = KinectDevice.skeletonFaceTracker.facePoints3D;
                drawFace(temp.ToList(), face.facePoints.ToList(), face.faceTriangles);

                if (IsOpenGLRecording == true)
                {
                    FaceDataWrapper fdw = new FaceDataWrapper(temp.ToList(), face.facePoints.ToList(), face.faceTriangles);
                    backgroundXMLFaceRecordingEventStream?.Invoke(this, fdw);
                }
            }
        }

        public void forceDraw(Skeleton avatar, bool faceT)
        {
            drawAvatar(avatar, faceT);
        }

        public void drawAvatarReplay(Skeleton sk)
        {
            display();
        }


        private void drawAvatar(Skeleton avatar, bool faceT)
        {
            if (isRecording)
            {
                nbFrames++;
            }
            else
                nbFrames = 0;

            if (avatar.TrackingState == SkeletonTrackingState.Tracked)
                this.DrawBonesAndJoints(avatar);

            float x, y, z;
            int i;


            if (!isInitialized) //here, we load each picture in a texture to show different feedbacks
            {
                isInitialized = true;
                foreach (ImageFeedbacksPerso img in DrawingSheetStreamViewModel.Get().listImg)
                {
                    img.initializeOpenGL();
                }
                /*Make sure that teh color of the OpenGL is good*/
                modifColorOpenGL(actualTheme.Name);
            }

            x = avatar.Joints[JointType.Head].Position.X;
            y = avatar.Joints[JointType.Head].Position.Y;
            z = avatar.Joints[JointType.Head].Position.Z;


            //OpenGL of the sound bar
            //Added by Baptiste Germond using value and code of Alistair Sutherland
            if (TrackingSideTool.Get().PeakDetectionCheckBox.IsChecked == true && !ReplayViewModel.isReplaying)
            {
				float xw, yw, yw1;

                GL.BindTexture(TextureTarget.Texture2D, (from p in DrawingSheetStreamViewModel.Get().listImg where p.name.Count() > 0 && actualTheme.SN.Contains(p.name) select p.idTextureOpenGL).First());
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				
                
				GL.PushAttrib(AttribMask.ColorBufferBit);
                for (i = 0; i < Model.AudioAnalysis.Pitch.WIGGLE_SIZE - 1; i++)
                {
                    yw = +0.6f + Model.AudioAnalysis.Pitch.wiggle[i] / 500.0f; 
                    yw1 = +0.6f + Model.AudioAnalysis.Pitch.wiggle[i+1] / 500.0f;

                    xw = -3.6f + (i+130) / 60.0f;
                    float xw1 = -3.6f + (i + 1+130) / 60.0f;

                    

                    GL.PushMatrix();
                    GL.Begin(PrimitiveType.Lines);


                    
                    //GL.Color4(0.5, 0.5, 0.5, 1.0);
                    if (AudioAnalysis.AudioProvider.currentIntensity == 0.0f)
                    {
                        GL.Color4(0.0, 0.0, 0.5, 1.0);
                    }
                    else if (AudioAnalysis.AudioProvider.currentIntensity > 650.0f)
                    {
                        GL.Color4(0.5, 0.0, 0.0, 1.0);
                    }
                    else
                        GL.Color4(0.5, 0.5, 0.5, 1.0);  //HERE
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
            }
			else if((ReplayViewModel.isReplaying && ReplayAvatar.voiceData) && ReplayViewModel.played)
            {
				ReplayAvatar.drawWiggle();
			}

            //Added by Baptiste Germond
            //OpenGL of the Trainer watch
            if (TrainerStopwatchViewModel.Get().getTrainerWatchVisibility() == Visibility.Visible)
            {
                System.Windows.Media.Color color = TrainerStopwatchViewModel.Get().getTrainerWatchColor().Color;

                float angle, raioX = 0.3f, raioY = 0.3f;
                float circle_points = 100.0f;

                GL.PushMatrix();

                GL.Translate(-1.5, -0.5, 0);
                GL.Rotate(90.0, 0.0, 0.0, 1.0);
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
                GL.Normal3(-3.0f, -1.5f, 0.0f);
                GL.Vertex3(0, 0, 0);

                for (i = 0; i < circle_points + 1; i++)
                {
                    angle = ((float)Math.PI * (float)(360 - TrainerStopwatchViewModel.Get().getTrainerWatchAngle()) / 180) * i / circle_points;

                    GL.Vertex3(Math.Cos(angle) * raioX, Math.Sin(angle) * raioY, 0);

                }
                GL.End();
                GL.PopMatrix();
            }

            HudDrawFeedback(avatar);

            // Timothée
            if (displayText)
            {
                showText();
            }

            // Timothée
            if (teleprompterActivated)
            {
                displayPrompterText();
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);

            //Updated by Jeffrey Goncalves
            if (faceT)
            {
                GL.PushMatrix();
                {
                    OpenTK.Vector4 faceColor = new OpenTK.Vector4(1.0f, 1.0f, 1.0f, 1.0f); 
                    GL.Normal3(0.0f, 0.0f, 1.0f);
                    GL.LineWidth(3.0f);

                    //Entering into the head axis system
                    Vector3 HeadX = EyesAlignment;
                    Vector3 HeadY = headTilt;
                    Vector3 HeadZ;
                    HeadZ = Vector3.Cross(HeadX, HeadY);
                    HeadX.Normalize();
                    HeadY.Normalize();
                    HeadZ.Normalize();

                    //HeadM is the projection matrix used for the head axis system
                    double[] HeadM = new double[16] { HeadX.X, HeadX.Y, HeadX.Z, 0, HeadY.X, HeadY.Y, HeadY.Z, 0, HeadZ.X, HeadZ.Y, HeadZ.Z, 0, 0, 0, 0, 1 };
                    GL.Translate(headCenterPoint);
                    GL.MultMatrix(HeadM);
                    GL.Color4(faceColor);
                   
                    
                    //Drawing of the mouth
                    Gl.glPushMatrix();
                    {
                       
                        float step = (float)Math.PI / (float)generalStacks;
                        float scale = 0.05f; //size of the mouth
                      
                        currentLipsDistance = (float)Math.Sqrt(Math.Pow(MBUL.X - MTLL.X, 2) + Math.Pow(MBUL.Y - MTLL.Y, 2) + Math.Pow(MBUL.Z - MTLL.Z, 2));
                        LipsDistanceDiff = currentLipsDistance - lastLipsDistance;
                        Gl.glTranslatef(0, -0.07f, -0.1f);
                        Vector3 leftOuterCorner = new Vector3(-scale, 0, -0.1f);
                        Vector3 rightOuterCorner = new Vector3(scale, 0, -0.1f);

                        //Calculation of lips' position at each frame
                        if (LipsDistanceDiff > 0.003f)
                        {
                            verticalLipsAdjustment = 3.0f;
                            horizontalLipsAdjustment = 0.75f;
                        }
                        else if(LipsDistanceDiff < -0.003f)
                        {
                            verticalLipsAdjustment = 1.0f;
                            horizontalLipsAdjustment = 1.0f;
                        }

                        Gl.glScalef(1.75f*horizontalLipsAdjustment , verticalLipsAdjustment * 0.5f, 1);

                        /*Drawing of an ellipse that represents the hole of the mouth
                        this ellipse looks like a line when the mouth is closed and looks like
                        a real ellipse when the mouth is open*/
                        Gl.glColor3f(1.0f, 0, 0);
                        float beta;
                        float betaStep = (float)(Math.PI) / (float)generalStacks;
                        Vector3[] innerPoints = new Vector3[20];
                        int cnt = 0;
                        float RHori = (float)Math.Sqrt(Math.Pow(leftOuterCorner.X - rightOuterCorner.X, 2) + Math.Pow(leftOuterCorner.Y - rightOuterCorner.Y, 2) + Math.Pow(leftOuterCorner.Z - rightOuterCorner.Z, 2)) / 2;
                        float RVert = verticalLipsAdjustment / 500;

                        for (beta = 0; beta < (float)2 * Math.PI; beta += betaStep)
                        {
                            innerPoints[cnt].X = (float)Math.Cos(beta) * RHori;
                            innerPoints[cnt].Y = (float)Math.Sin(beta) * RVert;
                            innerPoints[cnt].Z = -0.1f;
                            cnt++;
                        }

                        Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                        for (cnt = 0; cnt < innerPoints.Length; cnt++)
                        {
                            Gl.glVertex3f(innerPoints[cnt].X, innerPoints[cnt].Y, innerPoints[cnt].Z);
                        }
                        Gl.glEnd();

                        //Drawing of the mouth's lips
                        Gl.glColor3f(1.0f, 1.0f, 1.0f);
                        DrawMouth(leftOuterCorner, rightOuterCorner);

                        lastLipsDistance = currentLipsDistance;
                    }
                    Gl.glPopMatrix();

                    // Drawing of the right eye
                    Gl.glPushMatrix();
                    {
                        float beta;
                        float betaStep = (float)(Math.PI) / (float)generalStacks;
                        Vector3[] eyesPoints = new Vector3[20];
                        float RVert = 0.02f; // Size of the horizontal semi-axis of the ellipse
                        float RHori = 0.07f; //Size of the vertical semi-axis of the ellipse
                        int cnt = 0;

                        for (beta = 0; beta < (float)2 * Math.PI; beta += betaStep)
                        {
                            eyesPoints[cnt].X = (float)Math.Cos(beta) * RHori;
                            eyesPoints[cnt].Y = (float)Math.Sin(beta) * RVert;
                            eyesPoints[cnt].Z = -0.1f;
                            cnt++;
                        }

                      
                        Gl.glTranslatef(-RHori, 0.01f, -0.1f);
                        Gl.glScalef(0.75f, 1, 1);
                        Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                        for (cnt = 0; cnt < eyesPoints.Length; cnt++)
                        {
                            Gl.glVertex3f(eyesPoints[cnt].X, eyesPoints[cnt].Y, eyesPoints[cnt].Z);
                        }
                        Gl.glEnd();
                    }
                    Gl.glPopMatrix();


                    //Drawing of right eyebrow
                    Gl.glPushMatrix();
                    {
                        float step = (float)Math.PI / (float)generalStacks;
                        float scale = 0.05f; // size of the eyebrow
                        float fullness = -0.9999f;//value drawing a crescent when close to -1 and circle when close to 1
                        currentRightEBEDist = (float)Math.Sqrt(Math.Pow(face16.X - face21.X, 2) + Math.Pow(face16.Y - face21.Y, 2) + Math.Pow(face16.Z - face21.Z, 2));
                        Gl.glTranslatef(-0.07f, 0.07f, -0.1f);
                        Gl.glScalef(1, 0.25f, 1);
                        eyebrowDifference = currentRightEBEDist - lastRightEBEDist;

                        //This block contains the implementation of right eyebrow's animation
                        if (eyebrowDifference >= 0.002) //there is a range [-0.002, 0.002] between the 2 states of the animation to prevent the eyebrow from moving everytime
                        {
                            eyebrowAdjustment = 0.1f;
                        }
                        else if (eyebrowDifference < -0.002)
                        {
                            eyebrowAdjustment = 0;
                        }
                       
                        //Drawing of the crescent shape
                        Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                        {
                            Gl.glVertex3f(scale,eyebrowAdjustment, -0.1f);
                            float angle = step;
                           
                            while (angle < (float)Math.PI)
                            {
                                float sinAngle = (float)Math.Sin(angle);
                                float cosAngle = (float)Math.Cos(angle);
                                Gl.glVertex3f(scale * cosAngle, scale * sinAngle + eyebrowAdjustment, -0.1f);
                                Gl.glVertex3f(-fullness * scale * cosAngle, scale * sinAngle + eyebrowAdjustment, -0.1f);
                                angle += step;
                            }
                            Gl.glVertex3f(-scale, eyebrowAdjustment, -0.1f);
                        }
                        Gl.glEnd();
                        
                        lastRightEBEDist = currentRightEBEDist;
                    }
                    Gl.glPopMatrix();

                    // Drawing of the left eye
                    Gl.glPushMatrix();
                    {

                        float beta;
                        float betaStep = (float)(Math.PI) / (float)generalStacks;
                        Vector3[] eyesPoints = new Vector3[20];
                        float RVert = 0.02f; // Size of the horizontal semi-axis of the ellipse
                        float RHori = 0.07f; //Size of the vertical semi-axis of the ellipse
                        int cnt = 0;

                        //Drawing of the ellipse
                        for (beta = 0; beta < (float)2 * Math.PI; beta += betaStep)
                        {
                            eyesPoints[cnt].X = (float)Math.Cos(beta) * (RHori);
                            eyesPoints[cnt].Y = (float)Math.Sin(beta) * (RVert);
                            eyesPoints[cnt].Z = -0.1f;
                            cnt++;
                        }
                        Gl.glTranslatef(RHori, 0.01f, -0.1f);
                        Gl.glScalef(0.75f, 1, 1);
                        Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                        for (cnt = 0; cnt < eyesPoints.Length; cnt++)
                        {
                            Gl.glVertex3f(eyesPoints[cnt].X, eyesPoints[cnt].Y, eyesPoints[cnt].Z);
                        }
                        Gl.glEnd();
                    }
                    Gl.glPopMatrix();

                    // Drawing of the left eyebrow
                    Gl.glPushMatrix();
                    {
                        float step = (float)Math.PI / (float)generalStacks;
                        float scale = 0.05f; // size of the eyebrow
                        float fullness = -0.9999f;//value drawing a crescent when close to -1 and circle when close to 1
                        currentLeftEBEDist = (float)Math.Sqrt(Math.Pow(face51.X - face54.X, 2) + Math.Pow(face51.Y - face54.Y, 2) + Math.Pow(face51.Z - face54.Z, 2));
                        Gl.glTranslatef(0.07f, 0.07f, -0.1f);
                        Gl.glScalef(1, 0.25f, 1);
                        eyebrowDifference = currentLeftEBEDist - lastLeftEBEDist;
                       
                        //This block contains the implementation of left eyebrow's animation
                        if (eyebrowDifference >= 0.002) //there is a range [-0.002, 0.002] between the 2 states of the animation to prevent the eyebrow from moving everytime
                        {
                            eyebrowAdjustment = 0.1f;
                        }
                        else if (eyebrowDifference < -0.02)
                        {
                            eyebrowAdjustment = 0;
                        }

                        //Drawing of the crescent shape
                        Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                        {
                            Gl.glVertex3f(scale, eyebrowAdjustment, -0.1f);
                            float angle = step;

                            while (angle < (float)Math.PI)
                            {
                                float sinAngle = (float)Math.Sin(angle);
                                float cosAngle = (float)Math.Cos(angle);
                                Gl.glVertex3f(scale * cosAngle, scale * sinAngle + eyebrowAdjustment, -0.1f);
                                Gl.glVertex3f(-fullness * scale * cosAngle, scale * sinAngle + eyebrowAdjustment, -0.1f);
                                angle += step;
                            }
                            Gl.glVertex3f(-scale, eyebrowAdjustment, -0.1f);
                        }
                        Gl.glEnd();
                        lastLeftEBEDist = currentLeftEBEDist;
                    }
                    Gl.glPopMatrix();
                }
                GL.PopMatrix();

            }
        }

        /// <summary>
        /// Draws the avatar's body (among other things)
        /// </summary>
        /// <param name="evt"></param>
        private void drawAvatar(EventArgs evt)
        {
			if(GeneralSideTool.Get().AudienceControlCheckBox.IsChecked == true)
			{
				if (AudienceMember.WholeAudience.Count == 0) initAudience(); 
				drawAudience();
			}

            // Test if there is a replay avatar to display
            if (skToDrawInReplay == null)
            {
                SkeletonEventArgs skEvent = (SkeletonEventArgs)evt;
                Skeleton avatar = skEvent.skeleton;
                if (!Tools.MajorityJointsTracked(avatar))
                {
                    HudDrawSignalLost();
                    Agitation.removeAgitation(avatar);
                }
                else
                {
                    drawAvatar(avatar, KinectDevice.faceTracking);
                }

            }
            else
            {
                drawAvatar(skToDrawInReplay, drawFaceInReplay);
            }
        }


        /// <summary>
        /// Avatar's members dimension initialization
        /// </summary>
        private void initAvatarDimensions()
        {
            properHeadToShoulderCenter = distance2Vectors(initialHead, initialShoulderCenter);
            properShoulderCenterToShoulderEnd = distance2Vectors(initialShoulderCenter, initialShoulderLeft);
            properShoulderEndToElbow = distance2Vectors(initialShoulderLeft, initialElbowLeft);
            properElbowToWrist = distance2Vectors(initialElbowLeft, initialWristLeft);
            properWristToHand = distance2Vectors(initialWristLeft, initialHandLeft);
            properHipCenterToHipEnd = distance2Vectors(initialHipCenter, initialHipLeft);
            properHipEndToKnee = distance2Vectors(initialHipLeft, initialKneeLeft);
            properKneeToAnkle = distance2Vectors(initialKneeLeft, initialAnkleLeft);
            properAnkleToFoot = distance2Vectors(initialAnkleLeft, initialFootLeft);
            properShoulderCenterToSpine = distance2Vectors(initialShoulderCenter, initialSpine);
            properSpineToHipCenter = distance2Vectors(initialSpine, initialHipCenter);
        }

		private void initAudience()
		{
			AudienceMember.GlobalInterest = 0.5f;
			for(int i = -9; i <= 9; i= i + 3){
				new AudienceMember(1, i/10.0f, 0.3f, 0.7f);
			}
		}

		private void drawAudience(){
			float[] interest = new float[3];
			GL.Color3(0f, 0f, 0f);
            GL.Normal3(0f, 0f, 1f);

			foreach(AudienceMember mem in AudienceMember.WholeAudience)
			{
				paint(mem.currentFace, 0.1f, 0.1f, mem.horizontalPosition, -0.61f);
				paint("Audience_Body", 0.1f, 0.1f, mem.horizontalPosition, -0.8f);
				if(mem.horizontalPosition == 0.9f) interest[0] = mem.Interest;
				if(mem.horizontalPosition == 0) interest[1] = mem.Interest;
				if(mem.horizontalPosition == -0.9f) interest[2] = mem.Interest;
			}
			//System.Diagnostics.Debug.WriteLine("{0} - {1} - {2}", interest[0], interest[1], interest[2]);
			
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

        /// <summary>
        /// Return the distance separating two vectors
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        private float distance2Vectors(Vector3 v0, Vector3 v1)
        {
            
            Vector3 tempV = new Vector3();
            Vector3.Subtract(ref v0, ref v1, out tempV);
            return tempV.Length;
        }

        /// <summary>
        /// Return an adjustment vector : the bone will be reduced or elongate by this vector 
        /// and will have a "wellPropBoneSize" size
        /// </summary>
        /// <param name="currentBone"></param>
        /// <param name="wellPropBoneSize"></param>
        /// <returns></returns>
        private Vector3 calculateAdjustment(Vector3 currentBone, float wellPropBoneSize)
        {
            // If the difference is smaller than the error margin, there is no adjustment to do
            Vector3 tempAdjustment;
            if (Math.Abs(currentBone.Length - wellPropBoneSize) > adjustmentErrorMargin)
            {
                tempAdjustment = new Vector3(currentBone);

                float toMult = 1 - (wellPropBoneSize / currentBone.Length);

                tempAdjustment = Vector3.Multiply(tempAdjustment, toMult);
            }
            else
                tempAdjustment = new Vector3(0, 0, 0);
            return tempAdjustment;
        }

        /// <summary>
        /// Add the two adjustment vectors to the points
        /// </summary>
        /// <param name="point0"></param>
        /// <param name="point1"></param>
        private void addAdjustmentsToPoints(ref Vector3 point0, ref Vector3 point1)
        {
            point0 += generalAdjustment;
            point0 += specificAdjustment;
            point1 += generalAdjustment;
            point1 += specificAdjustment;
        }

        /// <summary>
        /// Lengthens or shrinks the distance between two points by adding gap at both edges
        /// </summary>
        /// <param name="point0"></param>
        /// <param name="point1"></param>
        private void lengthenSegment(ref Vector3 point0, ref Vector3 point1, float gap)
        {
            Vector3 unitarySegment = (new Vector3(point0 - point1)) / 2.0f;
            unitarySegment = unitarySegment / unitarySegment.Length;
            point0 = point0 + (unitarySegment * gap) / 2.0f;
            point1 = point1 - (unitarySegment * gap) / 2.0f;
        }

        #endregion

        #region HUD drawing methods

        /// <summary>
        /// Draws the HUD with feedback images.
        /// </summary>
        /// <param name="avatar">Skeleton of the avatar to give feedback to</param>
        /// <author>Vincent Fabioux</author>
        private void HudDrawFeedback(Skeleton avatar)
        {
            // Initialize parameters for image rendering
            GL.Color3(feedbackColor);
            GL.Normal3(0f, 0f, 1f);

            float headX = avatar.Joints[JointType.Head].Position.X,
                headY = avatar.Joints[JointType.Head].Position.Y;

			bool fb = false;

            //Test for 3D feedback by ASu 25 March 2016 ad modified by F Bechu June 2016
            /*The second part of the if (after the ||) is used to display the feedbacks when replaying an avatar, it's the same for all the feedbacks below*/
            /*OpenGL feedback of the hands crossed*/           
            
            if(!isTraining)
			{
				//The way feedback is displayed changes wether we are in normal mode or replay mode

                // Normal mode feedback display 

                if (!ReplayViewModel.isReplaying)
                {
                    if (diplayFeedback)
                    {
                        //OpenGL feedback of the hands crossed
                        if(Model.HandsJoined.hands && diplayBodyFeedback && displayHandsJoinedFeedback)
                        {
							fb = true;
                            HudDrawImage("Hand_Joined", 0.15f, 0.15f,
                                avatar.Joints[JointType.HandLeft].Position.X,
                                avatar.Joints[JointType.HandLeft].Position.Y);
                        }

                        //OpenGL feedback of agitation
                        if (Model.Agitation.feedAg && diplayBodyFeedback && displayAgitationFeedback)
                        {
							fb = true;
                            HudDrawImage("Agitation", 0.2f, 0.2f,
                                -1f,
                                0);
                        }

                        //OpenGL feedback of the arms crossed
                        if (Model.BodyAnalysis.ArmsCrossed.feedArmsCrossed && diplayBodyFeedback && displayArmsCrossedFeedback)
                        {
							fb = true;
                            HudDrawImage("Arms_Crossed", 0.2f, 0.2f,
                                0.75f,
                                0);
                        }

                        //OpenGL feedback of the look at the center
                        if (Model.EmotionRecognizer.lookingDirection.feedC && diplayFaceFeedback && displayLookDirFeedback)
                        {
							fb = true;
                            HudDrawImage("Center_Arrow", 0.2f, 0.2f,
                                headX,
                                headY + 0.5f);
                        }

                        //OpenGL feedback of the look at the left
                        if(Model.EmotionRecognizer.lookingDirection.feedL && diplayFaceFeedback && displayLookDirFeedback)
                        {
							fb = true;
                            HudDrawImage("Left_Arrow", 0.2f, 0.2f,
                                headX - 0.5f,
                                headY);
                        }

                        //OpenGL feedback of the look at the right
                        if(Model.EmotionRecognizer.lookingDirection.feedR && diplayFaceFeedback && displayLookDirFeedback)
                        {
							fb = true;
                            HudDrawImage("Right_Arrow", 0.2f, 0.2f,
                                headX + 0.5f,
                                headY);
                        }

                        //OpenGL feedback of the happy emotion
                        if(Model.EmotionRecognizer.EmotionRecognition.happy && diplayFaceFeedback && displayEmotionFeedback)
                        {
							fb = true;
                            HudDrawImage("Happy", 0.2f, 0.2f,
                                0.75f,
                                0.5f);
                        }

                        //OpenGL feedback of the surprised emotion
                        if(Model.EmotionRecognizer.EmotionRecognition.surprised && diplayFaceFeedback && displayEmotionFeedback)
                        {
							fb = true;
                            HudDrawImage("Surprised", 0.2f, 0.2f,
                                0.75f,
                                0.5f);
                        }
                        
                    }

					if (fb && AudienceMember.GlobalInterest > 0)
						AudienceMember.GlobalInterest -= 0.002f;
					else if (AudienceMember.GlobalInterest < 1)
						AudienceMember.GlobalInterest += 0.002f;
				}
                /* Replay Mode feedback display */
                else{
                    /* List containing all the feedbacks that must be displayed during the current frame */
                    List<String> feedbacksToDisplay = ReplayViewModel.Get().currentFeedbackList;

					if(feedbacksToDisplay != null)
					{

						/* Iterating through the list to see which feedback to display */
						foreach (String message in feedbacksToDisplay)
						{
							/*ServerFeedback tempFeedback = new ServerFeedback(message);
							ReplayViewModel.Get().manageFeedback(tempFeedback.eventName);*/
							switch(message)
							{
								/*OpenGL feedback of the hands crossed*/
								case("Hands are joined"):
									HudDrawImage("Hand_Joined", 0.15f, 0.15f,
										avatar.Joints[JointType.HandLeft].Position.X,
										avatar.Joints[JointType.HandLeft].Position.Y);
									break;

								/*OpenGL feedback of the look at the center*/
								case("Look to the center"):
									HudDrawImage("Center_Arrow", 0.2f, 0.2f,
										headX,
										headY + 0.5f);
									break;
                            
								/*OpenGL feedback of the look at the left*/
								case("Look to the left"):
									HudDrawImage("Left_Arrow", 0.2f, 0.2f,
										headX - 0.5f,
										headY);
									break;
                            
								/*OpenGL feedback of the look at the right*/
								case("Look to the right"):
									HudDrawImage("Right_Arrow", 0.2f, 0.2f,
										headX + 0.5f,
										headY);
									break;
                            
								/*OpenGL feedback of the happy emotion*/
								case("Happy"):
									HudDrawImage("Happy", 0.2f, 0.2f,
										0.75f,
										0.5f);
									break;
                            
								/*OpenGL feedback of agitation*/
								case("Too agitated!"):
									HudDrawImage("Agitation", 0.2f, 0.2f,
										-1f,
										0);
									break;
                            
								/*OpenGL feedback of the arms crossed*/
								case("Arms Crossed"):
									HudDrawImage("Arms_Crossed", 0.2f, 0.2f,
										0.75f,
										0);
									break;
							}
						}
					}
				}
			}
            else // Training mode
            {
                /*if (TrainingWithAvatarViewModel.Get().PlayMode & mentor)
                {
                    if (Model.BodyAnalysis.WelcomeTraining.goodjob)
                    {
                        HudDrawImage("GoodJob", 0.25f, 0.25f,
                            -1f,
                            0);
                    }
                    else if (Model.BodyAnalysis.WelcomeTraining.elbows)
                    {
                        HudDrawImage("Elbows", 0.25f, 0.25f,
                            -1f,
                            0.75f);

                        HudDrawImage("Center_Arrow", 0.2f, 0.2f,
                            avatar.Joints[JointType.ElbowLeft].Position.X,
                            avatar.Joints[JointType.ElbowLeft].Position.Y + 0.4f);
                    }
                    else if (Model.BodyAnalysis.WelcomeTraining.slow)
                    {
                        if (Math.Abs(avatar.Joints[JointType.ShoulderLeft].Position.Y - avatar.Joints[JointType.ElbowLeft].Position.Y) > 0.1 & first)
                        {
                            HudDrawImage("Slow", 0.25f, 0.25f,
                                -1f,
                                0.75f);
                        }
                        else
                        {
                            HudDrawImage("LikeThis", 0.25f, 0.25f,
                                -1f,
                                0.75f);

                            first = false; // stops the Too Slow feedback being displayed again
                        }
                    }
                }
                else if (!mentor & TrainingWithAvatarViewModel.Get().SkeletonList != null && TrainingWithAvatarViewModel.canBeInterrupted)
                {
                    HudDrawImage("YourTurn", 0.25f, 0.25f,
                        -1f,
                        0.75f);

                    first = true;  //resets the TooSlow feedback 
                }*/
            }

            // Reset the texture applied to polygons
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Draws the lost signal image on screen.
        /// </summary>
        /// <author>Vincent Fabioux</author>
        private void HudDrawSignalLost()
        {
            // Initialize parameters for image rendering
            GL.Color3(feedbackColor);
            GL.Normal3(0f, 0f, 1f);

            if (!isSignalLostInitialized)
            {
                isSignalLostInitialized = true;
                modifColorOpenGL(actualTheme.Name);
            }

            HudDrawImage("Signal_Lost", 0.5f, 0.5f);

            // Reset the texture applied to polygons
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Draws a 2d image to screen on top of everything.
        /// </summary>
        /// <param name="imgName">Name of the texture to use</param>
        /// <param name="w">Half the width on screen</param>
        /// <param name="h">Hafl the height on screen</param>
        /// <param name="x">Vertical position on screen</param>
        /// <param name="y">Horizontal position on screen</param>
        /// <author>Vincent Fabioux</author>
        private void HudDrawImage(String imgName, float w, float h, float x = 0f, float y = 0f)
        {
            try
            {
                GL.BindTexture(TextureTarget.Texture2D, (from p in DrawingSheetStreamViewModel.Get().listImg where p.name == imgName select p.idTextureOpenGL).First());
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

                GL.Begin(PrimitiveType.Polygon);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(x + w, y - h, 0);
                GL.TexCoord2(1.0, 1.0);
                GL.Vertex3(x + w, y + h, 0);
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(x - w, y + h, 0);
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(x - w, y - h, 0);
                GL.End();
			}
            catch(Exception e)
            {
				Console.Out.WriteLine(e.Message);
            }
        }

		private void paint(String imgName, float w, float h, float x = 0f, float y = 0f)
        {
            try
            {
                GL.BindTexture(TextureTarget.Texture2D, (from p in DrawingSheetStreamViewModel.Get().listImg where p.name == imgName select p.idTextureOpenGL).First());
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

                GL.Begin(PrimitiveType.Polygon);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(x + w, y - h, -5);
                GL.TexCoord2(1.0, 1.0);
                GL.Vertex3(x + w, y + h, -5);
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(x - w, y + h, -5);
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(x - w, y - h, -5);
                GL.End();
			}
            catch(Exception e)
            {
				Console.Out.WriteLine(e.Message);
            }
        }

        #endregion

        #region drawing methods
        /// <summary>
        /// Function grabbing a screenshot from the openGL current context (the 3D avatar)
        /// </summary>
        /// <returns>a Bitmap of the current 3D rendering</returns>
        /// <remarks>Excerpt from the openTK website : http://www.opentk.com/doc/graphics/save-opengl-rendering-to-disk </remarks>
        /// <author>Amirali Ghazi</author>
        /// <remarks>Find a way to prevent video recording / grabing a screenshot if the kinect does not detect a body</remarks>
        public Bitmap GrabScreenshot()
        {
            if (GraphicsContext.CurrentContext == null)
            {
                throw new GraphicsContextMissingException();
            }
            int screenshotWidth = glControl.Width;
            int screenshotHeight = glControl.Height;

            // width/height have to have a pair size 
            if (screenshotWidth % 2 != 0)
                screenshotWidth += 1;
            if (screenshotHeight % 2 != 0)
                screenshotHeight += 1;
            if (Tools.getStopWatch() % 30 > 22 || Tools.getStopWatch() % 30 < 8 || firstScreenshot)
            {
                if (firstScreenshot)
                    firstScreenshot = false;
                

                Bitmap bmp = new Bitmap(screenshotWidth, screenshotHeight);
                //bmp.SetResolution(1920, 1080);
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                GL.ReadPixels(0, 0, screenshotWidth, screenshotHeight, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, bmpData.Scan0);
                GL.Finish();
                bmp.UnlockBits(bmpData);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                oldBmp = bmp;
                return bmp;
            }
            else
            {
                return oldBmp;
            }
        }

        /// <summary>
        /// Draws the avatar in its initial position 
        /// </summary>
        private void drawInitialAvatar()
        {
            // Bones drawing
            // Head and Shoulders
            this.DrawHead(initialHead, initialShoulderCenter, trackedBoneColor);
            this.DrawShoulder(initialShoulderCenter, initialShoulderLeft, trackedBoneColor);
            this.DrawShoulder(initialShoulderCenter, initialShoulderRight, trackedBoneColor);

            // Render Torso
            this.DrawUpperTorso(initialShoulderCenter, initialSpine, trackedBoneColor);
            this.DrawLowerTorso(initialSpine, initialHipCenter, trackedBoneColor);
            this.DrawHip(initialHipCenter, initialHipLeft, trackedBoneColor);
            this.DrawHip(initialHipCenter, initialHipRight, trackedBoneColor);

            // Left Arm
            this.DrawArm(initialShoulderLeft, initialElbowLeft, trackedBoneColor);
            this.DrawForeArm(initialElbowLeft, initialWristLeft, trackedBoneColor);
            this.DrawHand(initialWristLeft, initialHandLeft, trackedBoneColor, true);

            // Right Arm
            this.DrawArm(initialShoulderRight, initialElbowRight, trackedBoneColor);
            this.DrawForeArm(initialElbowRight, initialWristRight, trackedBoneColor);
            this.DrawHand(initialWristRight, initialHandRight, trackedBoneColor, false);

            // Left Leg
            this.DrawThigh(initialHipLeft, initialKneeLeft, trackedBoneColor);
            this.DrawLeg(initialKneeLeft, initialAnkleLeft, trackedBoneColor);
            this.DrawFoot(initialAnkleLeft, initialFootLeft, trackedBoneColor, true);

            // Right Leg
            this.DrawThigh(initialHipRight, initialKneeRight, trackedBoneColor);
            this.DrawLeg(initialKneeRight, initialAnkleRight, trackedBoneColor);
            this.DrawFoot(initialAnkleRight, initialFootRight, trackedBoneColor, false);


        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="sk"></param>
        private void DrawBonesAndJoints(Skeleton sk)
        {
            // YOYO
            if (sk != null)
            {
                // Head and Shoulders
                // Here, the generalAjustment is set
                this.DrawBone(sk, JointType.Head, JointType.ShoulderCenter, trackedBoneColor);
                specificAdjustment = new Vector3(0, 0, 0);

                // Left Arm
                this.DrawBone(sk, JointType.ShoulderCenter, JointType.ShoulderLeft, trackedBoneColor);
                this.DrawBone(sk, JointType.ShoulderLeft, JointType.ElbowLeft, trackedBoneColor);
                this.DrawBone(sk, JointType.ElbowLeft, JointType.WristLeft, trackedBoneColor);
                this.DrawBone(sk, JointType.WristLeft, JointType.HandLeft, trackedBoneColor);
                specificAdjustment = new Vector3(0, 0, 0);

                // Right Arm
                this.DrawBone(sk, JointType.ShoulderCenter, JointType.ShoulderRight, trackedBoneColor);
                this.DrawBone(sk, JointType.ShoulderRight, JointType.ElbowRight, trackedBoneColor);
                this.DrawBone(sk, JointType.ElbowRight, JointType.WristRight, trackedBoneColor);
                this.DrawBone(sk, JointType.WristRight, JointType.HandRight, trackedBoneColor);
                specificAdjustment = new Vector3(0, 0, 0);

                // Render Torso
                // Here, the generalAjustment is set
                this.DrawBone(sk, JointType.ShoulderCenter, JointType.Spine, trackedBoneColor);
                this.DrawBone(sk, JointType.Spine, JointType.HipCenter, trackedBoneColor);
                specificAdjustment = new Vector3(0, 0, 0);

                legTracked = false;
                //Modified by Baptiste Germond
                //If the legs are tracked, we draw the avatar as usual
                if (sk.Joints[JointType.KneeRight].TrackingState == JointTrackingState.Tracked &&
                    sk.Joints[JointType.KneeLeft].TrackingState == JointTrackingState.Tracked)
                {
                    // Left Leg
                    legTracked = true;
                    this.DrawBone(sk, JointType.HipCenter, JointType.HipLeft, trackedBoneColor);
                    this.DrawBone(sk, JointType.HipLeft, JointType.KneeLeft, trackedBoneColor);
                    this.DrawBone(sk, JointType.KneeLeft, JointType.AnkleLeft, trackedBoneColor);
                    this.DrawBone(sk, JointType.AnkleLeft, JointType.FootLeft, trackedBoneColor);
                    specificAdjustment = new Vector3(0, 0, 0);

                    // Right Leg
                    this.DrawBone(sk, JointType.HipCenter, JointType.HipRight, trackedBoneColor);
                    this.DrawBone(sk, JointType.HipRight, JointType.KneeRight, trackedBoneColor);
                    this.DrawBone(sk, JointType.KneeRight, JointType.AnkleRight, trackedBoneColor);
                    this.DrawBone(sk, JointType.AnkleRight, JointType.FootRight, trackedBoneColor);
                }
                //If not, the legs are drawn using simulated points
                //It drawing the legs straight, like with the initial avatar
                else if (sk.Joints[JointType.HipCenter].TrackingState == JointTrackingState.Tracked)
                {
                    Joint joint0 = sk.Joints[JointType.HipCenter];
                    Vector3 tempSpec = specificAdjustment;
                    Vector3 tempGen = generalAdjustment;

                    Vector3 point0 = new Vector3(joint0.Position.X, joint0.Position.Y, joint0.Position.Z);
                    Vector3 pointTemp = drawAvatarNotTracked(JointType.HipRight, point0);
                    pointTemp = drawAvatarNotTracked(JointType.KneeRight, pointTemp);
                    pointTemp = drawAvatarNotTracked(JointType.AnkleRight, pointTemp);
                    pointTemp = drawAvatarNotTracked(JointType.FootRight, pointTemp);

                    specificAdjustment = tempSpec;
                    generalAdjustment = tempGen;

                    point0 = new Vector3(joint0.Position.X, joint0.Position.Y, joint0.Position.Z);
                    pointTemp = drawAvatarNotTracked(JointType.HipLeft, point0);
                    pointTemp = drawAvatarNotTracked(JointType.KneeLeft, pointTemp);
                    pointTemp = drawAvatarNotTracked(JointType.AnkleLeft, pointTemp);
                    pointTemp = drawAvatarNotTracked(JointType.FootLeft, pointTemp);
                }
                //Reinitialize adjustment
                specificAdjustment = new Vector3(0, 0, 0);
                generalAdjustment = new Vector3(0, 0, 0);

            }
        }
        /// <summary>
        /// Draw the legs of the avatar when they are not tracked
        /// </summary>
        /// <remarks>Added by baptiste Germond</remarks>
        private Vector3 drawAvatarNotTracked(JointType type, Vector3 point1)
        {
            Vector3 point2 = new Vector3(0, 0, 0);
            switch (type)
            {
                case JointType.HipRight:
                    point2 = new Vector3(point1.X + 0.0790575f, point1.Y - 0.07478374f, point1.Z - 0.008109f);
                    calculAdjustmentForNotTracked(properHipCenterToHipEnd, ref point1, ref point2);
                    DrawHip(point1, point2, inferredBoneColor);
                    break;
                case JointType.KneeRight:
                    point2 = new Vector3(point1.X + 0.0707104f, point1.Y - 0.49405996f, point1.Z + 0.017997f);
                    calculAdjustmentForNotTracked(properHipEndToKnee, ref point1, ref point2);
                    DrawThigh(point1, point2, inferredBoneColor);
                    break;
                case JointType.AnkleRight:
                    point2 = new Vector3(point1.X + 0.0381287f, point1.Y - 0.3853522f, point1.Z + 0.025064f);
                    calculAdjustmentForNotTracked(properKneeToAnkle, ref point1, ref point2);
                    DrawLeg(point1, point2, inferredBoneColor);
                    break;
                case JointType.FootRight:
                    point2 = new Vector3(point1.X + 0.0319094f, point1.Y - 0.0682531f, point1.Z - 0.035207f);
                    calculAdjustmentForNotTracked(properAnkleToFoot, ref point1, ref point2);
                    DrawFoot(point1, point2, inferredBoneColor, false);
                    drawJoint(point2, inferredBoneColor);
                    break;
                case JointType.HipLeft:
                    point2 = new Vector3(point1.X - 0.07404394f, point1.Y - 0.07536828f, point1.Z - 0.024695f);
                    calculAdjustmentForNotTracked(properHipCenterToHipEnd, ref point1, ref point2);
                    DrawHip(point1, point2, inferredBoneColor);
                    break;
                case JointType.KneeLeft:
                    point2 = new Vector3(point1.X - 0.065851921f, point1.Y - 0.48453112f, point1.Z + 0.030556f);
                    calculAdjustmentForNotTracked(properHipEndToKnee, ref point1, ref point2);
                    DrawThigh(point1, point2, inferredBoneColor);
                    break;
                case JointType.AnkleLeft:
                    point2 = new Vector3(point1.X - 0.045214159f, point1.Y - 0.3822529f, point1.Z + 0.004385f);
                    calculAdjustmentForNotTracked(properKneeToAnkle, ref point1, ref point2);
                    DrawLeg(point1, point2, inferredBoneColor);
                    break;
                case JointType.FootLeft:
                    point2 = new Vector3(point1.X - 0.0253174f, point1.Y - 0.0727543f, point1.Z - 0.031337f);
                    calculAdjustmentForNotTracked(properAnkleToFoot, ref point1, ref point2);
                    DrawFoot(point1, point2, inferredBoneColor, true);
                    drawJoint(point2, inferredBoneColor);
                    break;
            }
            specificAdjustment = new Vector3(0, 0, 0);
            generalAdjustment = new Vector3(0, 0, 0);
            return point2;

        }
        /// <summary>
        /// Calculate the adjustment for the bones when the legs are not tracked
        /// </summary>
        /// <remarks>Added by Baptiste Germond</remarks>
        private void calculAdjustmentForNotTracked(float proper, ref Vector3 point0, ref Vector3 point1)
        {
            Vector3 currentBone = new Vector3();
            Vector3.Subtract(ref point0, ref point1, out currentBone);

            // Points are moved according to the old adjustment
            addAdjustmentsToPoints(ref point0, ref point1);
            // adjustment applicated on the edge Point
            Vector3 currentAdjustment;

            // Drawing of the top joint
            drawJoint(point0, inferredBoneColor);
            currentAdjustment = calculateAdjustment(currentBone, proper);
            specificAdjustment += currentAdjustment;
            point1 += specificAdjustment;
        }

        /// <summary>
        /// Test if the bone can be drawn or not.
        /// </summary>
        /// <param name="joint0"></param>
        /// <param name="joint1"></param>
        /// <returns></returns>
        private bool IsBoneDrawable(Joint joint0, Joint joint1)
        {
            bool toRet = true;

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
                toRet = false;

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
                toRet = false;

            return toRet;
        }

        /// <summary>
        /// Draw a joint with a simple sphere.
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="position"></param>
        private void drawJoint(Joint joint, Vector3 position)
        {
            OpenTK.Vector4 drawColor = new OpenTK.Vector4();
            bool colorSelected = false;
            if (joint.TrackingState == JointTrackingState.Tracked)
            {
                drawColor = this.trackedJointColor;
                colorSelected = true;
            }
            else if (joint.TrackingState == JointTrackingState.Inferred)
            {
                drawColor = this.inferredJointColor;
                colorSelected = true;
            }
            if (colorSelected)
            {
                if(joint.JointType == JointType.HandLeft || joint.JointType == JointType.HandRight)
                    DrawSphere1P(position.X, position.Y, position.Z, jointsHandRadius, drawColor);
                else
                    DrawSphere1P(position.X, position.Y, position.Z, jointsRadius, drawColor);
                
            }
        }
      
        private void drawJoint(Vector3 position, OpenTK.Vector4 color)
        {
            DrawSphere1P(position.X, position.Y, position.Z, jointsRadius, color);
        }

        /// <summary>
        /// Draws a bone between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, JointType jointType0, JointType jointType1, OpenTK.Vector4 color)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];
            Vector3 point0 = new Vector3(joint0.Position.X, joint0.Position.Y, joint0.Position.Z);
            Vector3 point1 = new Vector3(joint1.Position.X, joint1.Position.Y, joint1.Position.Z);

            Vector3 currentBone = new Vector3();
            Vector3.Subtract(ref point0, ref point1, out currentBone);

            // Points are moved according to the old adjustment
            addAdjustmentsToPoints(ref point0, ref point1);

            // adjustment applicated on the edge Point
            Vector3 currentAdjustment;

            // Drawing of the top joint
            drawJoint(joint0, point0);
            // Alban's idea
            /*
            if (joint0.JointType == JointType.ShoulderCenter && joint1.JointType == JointType.Spine)
            {
                Console.Out.WriteLine("-- " + joint0.JointType.ToString() + " -- " + joint1.JointType.ToString());
                Console.Out.WriteLine(Math.Sqrt((point0.X - point1.X) * (point0.X - point1.X) +
                    (point0.Y - point1.Y) * (point0.Y - point1.Y) +
                    (point0.Z - point1.Z) * (point0.Z - point1.Z)) + "\n--");
            }*/
            if (IsBoneDrawable(joint0, joint1))
            {
                // We assume all drawn bones are inferred unless BOTH joints are tracked
                OpenTK.Vector4 drawColor = this.inferredBoneColor;
                if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
                {
                    drawColor = color;
                }
                // Draw the bone according to the concerned body part
                GL.PushMatrix();
                {
                    if ((jointType0 == JointType.ShoulderLeft && jointType1 == JointType.ElbowLeft) ||
                        (jointType0 == JointType.ShoulderRight && jointType1 == JointType.ElbowRight))
                    {
                        // Then, the edge point is moved according to the new adjustment
                        currentAdjustment = calculateAdjustment(currentBone, properShoulderEndToElbow);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawArm(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.AnkleLeft && jointType1 == JointType.FootLeft) ||
                             (jointType0 == JointType.AnkleRight && jointType1 == JointType.FootRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properAnkleToFoot);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        if (jointType1 == JointType.FootLeft)
                            DrawFoot(point0, point1, drawColor, true);
                        else
                            DrawFoot(point0, point1, drawColor, false);

                        // Drawing of the second joint (foot is an extremity)
                        drawJoint(joint1, point1);
                    }

                    else if ((jointType0 == JointType.ElbowLeft && jointType1 == JointType.WristLeft) ||
                             (jointType0 == JointType.ElbowRight && jointType1 == JointType.WristRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properElbowToWrist);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawForeArm(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.WristLeft && jointType1 == JointType.HandLeft) ||
                             (jointType0 == JointType.WristRight && jointType1 == JointType.HandRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properWristToHand);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        if(jointType1 == JointType.HandLeft)
                            DrawHand(point0, point1, drawColor, true);
                        else
                            DrawHand(point0, point1, drawColor, false);

                        // Drawing of the second joint (hand is an extremity)
                        drawJoint(joint1, point1);
                    }

                    else if ((jointType0 == JointType.Head && jointType1 == JointType.ShoulderCenter))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properHeadToShoulderCenter);
                        generalAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        headCenterPoint = new Vector3(point0);
                        headTilt = point0 - point1;
                        DrawHead(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.ShoulderCenter && jointType1 == JointType.Spine))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properShoulderCenterToSpine);
                        generalAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawUpperTorso(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.Spine && jointType1 == JointType.HipCenter))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properSpineToHipCenter);
                        generalAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        // we must align the hipCenter with the spine, else it dispalys the avatar twisted
                        point1.X = point0.X; point1.Z = point0.Z;
                        DrawLowerTorso(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.HipCenter) &&
                             (jointType1 == JointType.HipLeft || jointType1 == JointType.HipRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properHipCenterToHipEnd);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawHip(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.KneeLeft && jointType1 == JointType.AnkleLeft) ||
                             (jointType0 == JointType.KneeRight && jointType1 == JointType.AnkleRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properKneeToAnkle);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawLeg(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.ShoulderCenter) &&
                             (jointType1 == JointType.ShoulderLeft || jointType1 == JointType.ShoulderRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properShoulderCenterToShoulderEnd);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawShoulder(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.HipLeft && jointType1 == JointType.KneeLeft) ||
                             (jointType0 == JointType.HipRight && jointType1 == JointType.KneeRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properHipEndToKnee);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawThigh(point0, point1, drawColor);
                    }
                }
                GL.PopMatrix();
            }
        }

        /// <summary>
        /// Draws a solid cylinder between the neck base and the center of the head
        /// and a solid sphere which the center is placed in the "headCenter" joint
        /// </summary>
        /// <param name="shoulderCenter"></param>
        /// <param name="headCenter"></param>
        /// <param name="color"></param>
        private void DrawHead(Vector3 headCenter, Vector3 shoulderCenter, OpenTK.Vector4 color)
        {
            DrawCylinder2P(shoulderCenter.X, shoulderCenter.Y, shoulderCenter.Z,
                            headCenter.X, headCenter.Y, headCenter.Z, bonesRadius, color);
            DrawSphere1P(shoulderCenter.X, shoulderCenter.Y, shoulderCenter.Z, bonesRadius, color);

            DrawSphere1P(headCenter.X, headCenter.Y, headCenter.Z, headRadius*1.5f, color);
        }

        /// <summary>
        /// The shoulder is represented by a large cone
        /// </summary>
        /// <param name="shoulderCenter"></param>
        /// <param name="spine"></param>
        /// <param name="color"></param>
        private void DrawShoulder(Vector3 shoulderCenter, Vector3 shoulderEnd, OpenTK.Vector4 color)
        {
            // Calculation of the point vertically alligned with shoulderCenter and
            // horizontally alligned with the shoulderEnd point
            DrawSphere1P(shoulderEnd.X, shoulderEnd.Y, shoulderEnd.Z, shoulderRadius, color);
            DrawCylinderWithTwoRadius(shoulderEnd.X, shoulderEnd.Y, shoulderEnd.Z,
                       shoulderCenter.X, shoulderCenter.Y, shoulderCenter.Z, shoulderRadius, shoulderRadius - 0.03f, color);
        }

        /// <summary>
        /// the arm is composed of three cylinders to look like more human
        /// </summary>
        /// <param name="shoulderEnd"></param>
        /// <param name="elbow"></param>
        /// <param name="color"></param>
        private void DrawArm(Vector3 shoulderEnd, Vector3 elbow, OpenTK.Vector4 color)
        {
            Vector3 centerup = new Vector3(shoulderEnd.X + (elbow.X - shoulderEnd.X) * 0.3f,
                                        shoulderEnd.Y + (elbow.Y - shoulderEnd.Y) * 0.3f,
                                        shoulderEnd.Z + (elbow.Z - shoulderEnd.Z) * 0.3f);
            Vector3 centerdown = new Vector3(shoulderEnd.X + (elbow.X - shoulderEnd.X) * 0.7f,
                                        shoulderEnd.Y + (elbow.Y - shoulderEnd.Y) * 0.7f,
                                        shoulderEnd.Z + (elbow.Z - shoulderEnd.Z) * 0.7f);
            DrawCylinderWithTwoRadius(centerup.X, centerup.Y, centerup.Z, shoulderEnd.X, shoulderEnd.Y,
                       shoulderEnd.Z, armRadius + 0.01f, armRadius, color);
            DrawCylinderWithTwoRadius(centerup.X, centerup.Y, centerup.Z, centerdown.X, centerdown.Y,
                       centerdown.Z, armRadius + 0.01f, armRadius + 0.01f, color);
            DrawCylinderWithTwoRadius(centerdown.X, centerdown.Y, centerdown.Z, elbow.X, elbow.Y,
                       elbow.Z, armRadius + 0.01f, armRadius - 0.005f, color);
        }

        /// <summary>
        /// The forearm is represented by two cylinders
        /// </summary>
        /// <param name="elbow"></param>
        /// <param name="wrist"></param>
        /// <param name="color"></param>
        private void DrawForeArm(Vector3 elbow, Vector3 wrist, OpenTK.Vector4 color)
        {
            Vector3 center = new Vector3(elbow.X + (wrist.X - elbow.X) * 0.2f,
                                         elbow.Y + (wrist.Y - elbow.Y) * 0.2f,
                                         elbow.Z + (wrist.Z - elbow.Z) * 0.2f);
            DrawCylinderWithTwoRadius(center.X, center.Y, center.Z, elbow.X, elbow.Y,
                       elbow.Z, armRadius , armRadius - 0.005f, color);
            DrawCylinderWithTwoRadius(center.X, center.Y, center.Z, wrist.X, wrist.Y,
                       wrist.Z, armRadius , armRadius - 0.02f, color);
        }

       /// <summary>
       /// The function calls another function with coordinates of the wrist and the hand
       /// </summary>
       /// <param name="wrist"></param>
       /// <param name="handEnd"></param>
       /// <param name="color"></param>
       /// <param name="left"></param>
        private void DrawHand(Vector3 wrist, Vector3 handEnd, OpenTK.Vector4 color, bool left)
        {
            DrawHandSecondversion(wrist.X, wrist.Y, wrist.Z, handEnd.X, handEnd.Y, handEnd.Z, color, left);                       
        }

        /// <summary>
        /// The torso is designed like a kind of sandglass. With two cones opposed and a cylinder in the center 
        /// and a cone on the top. Here is the function drawing the upper part and the cylinder.
        /// </summary>
        /// <param name="ShoulderCenter"></param>
        /// <param name="spine"></param>
        /// <param name="color"></param>
        private void DrawUpperTorso(Vector3 shoulderCenter, Vector3 spine, OpenTK.Vector4 color)
        {
            // Extremity point of the top cone
            float rate = 0.15f;
            Vector3 coneBasePoint = new Vector3(shoulderCenter.X + (spine.X - shoulderCenter.X) * rate,
                                                shoulderCenter.Y + (spine.Y - shoulderCenter.Y) * rate,
                                                shoulderCenter.Z + (spine.Z - shoulderCenter.Z) * rate);
            Vector3 cylinderPoint = new Vector3(shoulderCenter.X + (spine.X - shoulderCenter.X) * 0.5f,
                                               shoulderCenter.Y + (spine.Y - shoulderCenter.Y) * 0.5f,
                                               shoulderCenter.Z + (spine.Z - shoulderCenter.Z) * 0.5f);
            // Drawing of the cone on the top
            DrawCylinderWithTwoRadius(coneBasePoint.X, coneBasePoint.Y , coneBasePoint.Z, shoulderCenter.X,
                       shoulderCenter.Y , shoulderCenter.Z, upperTorsoRadius, upperTorsoRadius - 0.1f, color);

            DrawCylinderWithTwoRadius(coneBasePoint.X, coneBasePoint.Y, coneBasePoint.Z, cylinderPoint.X,
                    cylinderPoint.Y, cylinderPoint.Z, upperTorsoRadius, upperTorsoRadius - 0.01f, color);
            DrawCylinderWithTwoRadius(cylinderPoint.X, cylinderPoint.Y, cylinderPoint.Z,
                spine.X, spine.Y, spine.Z, upperTorsoRadius - 0.01f, upperTorsoRadius - 0.03f, color);
            DrawCylinderWithTwoRadius(cylinderPoint.X, cylinderPoint.Y, cylinderPoint.Z,
                spine.X, spine.Y - 0.05f, spine.Z, upperTorsoRadius - 0.031f, upperTorsoRadius - 0.031f, color);

        }

        /// <summary>
        /// Function drawing the lower part of the torso.
        /// </summary>
        /// <param name="spine"></param>
        /// <param name="hipCenter"></param>
        /// <param name="color"></param>
        private void DrawLowerTorso(Vector3 spine, Vector3 hipCenter, OpenTK.Vector4 color)
        {
            float cylinderRadius = 0.8f * lowerTorsoRadius;

            // Extremity point of the main cone
            float rate = 1.2f;
            Vector3 coneExtr = new Vector3(hipCenter.X + (hipCenter.X - spine.X ) * rate,
                                           hipCenter.Y + (hipCenter.Y - spine.Y ) * rate,
                                           hipCenter.Z + (hipCenter.Z - spine.Z ) * rate);
            // Drawing of the cylinder
            DrawCylinderWithTwoRadius(coneExtr.X, coneExtr.Y, coneExtr.Z,
                           spine.X, spine.Y, spine.Z, lowerTorsoRadius - 0.005f, lowerTorsoRadius - 0.03f, color);
        }

        /// <summary>
        /// A composition of a sphere and a cylinder draws the hip
        /// </summary>
        /// <param name="hipCenter"></param>
        /// <param name="hipEnd"></param>
        /// <param name="color"></param>
        private void DrawHip(Vector3 hipCenter, Vector3 hipEnd, OpenTK.Vector4 color)
        {
            
            DrawCylinder2P(hipCenter.X, hipCenter.Y, hipCenter.Z,
                           hipEnd.X, hipEnd.Y, hipEnd.Z, hipRadius, color);
            DrawSphere1P(hipEnd.X, hipEnd.Y, hipEnd.Z, hipRadius - 0.005f, color);
            
        }

        
        /// <summary>
        /// To represent a thigh, we draw a cylinder between the hipEnd and the knee
        /// We then add two cones sharing the same base on the center
        /// </summary>
        /// <param name="hipEnd"></param>
        /// <param name="knee"></param>
        /// <param name="color"></param>
        private void DrawThigh(Vector3 hipEnd, Vector3 knee, OpenTK.Vector4 color)
        {

            Vector3 centerUp = new Vector3(hipEnd.X + (knee.X - hipEnd.X) * 0.15f,
                                         hipEnd.Y + (knee.Y - hipEnd.Y) * 0.15f,
                                         hipEnd.Z + (knee.Z - hipEnd.Z) * 0.15f);
            // Point situated on the center of the thigh
            Vector3 centerdown = new Vector3(hipEnd.X + (knee.X - hipEnd.X) * 0.6f,
                                         hipEnd.Y + (knee.Y - hipEnd.Y) * 0.6f,
                                         hipEnd.Z + (knee.Z - hipEnd.Z) * 0.6f);
            // Drawing of the two cones
            DrawCylinderWithTwoRadius(centerUp.X, centerUp.Y, centerUp.Z, hipEnd.X, hipEnd.Y,
                       hipEnd.Z, legRadius + 0.008f, legRadius , color);
            DrawCylinderWithTwoRadius(centerdown.X, centerdown.Y, centerdown.Z, centerUp.X, centerUp.Y,
                       centerUp.Z, legRadius + 0.008f, legRadius + 0.008f, color);
            DrawCylinderWithTwoRadius(centerdown.X, centerdown.Y, centerdown.Z, knee.X, knee.Y,
                       knee.Z, legRadius + 0.008f, legRadius - 0.02f, color);
                       
        }

        /// <summary>
        /// Draws the bottom part of a leg with two cones sharing the same base
        /// </summary>
        /// <param name="knee"></param>
        /// <param name="ankle"></param>
        /// <param name="color"></param>
        private void DrawLeg(Vector3 knee, Vector3 ankle, OpenTK.Vector4 color)
        {
            // Point situated on the center of the thigh
            Vector3 center = new Vector3(knee.X + (ankle.X - knee.X) * 0.2f,
                                         knee.Y + (ankle.Y - knee.Y) * 0.2f,
                                         knee.Z + (ankle.Z - knee.Z) * 0.2f);
            // Drawing of the two cones
            DrawCylinderWithTwoRadius(center.X, center.Y, center.Z, knee.X, knee.Y,
                       knee.Z, legRadius - 0.01f , legRadius - 0.02f , color);
            DrawCylinderWithTwoRadius(center.X, center.Y, center.Z, ankle.X, ankle.Y,
                       ankle.Z, legRadius - 0.01f , legRadius - 0.04f, color);
        }

        /// <summary>
        /// Draws a foot with a simple cylinder
        /// </summary>
        /// <param name="ankle"></param>
        /// <param name="footEnd"></param>
        /// <param name="color"></param>
        private void DrawFoot(Vector3 ankle, Vector3 footEnd, OpenTK.Vector4 color, bool left)
        {
            DrawFootSecondVersion(ankle.X, ankle.Y, ankle.Z, footEnd.X, footEnd.Y, footEnd.Z, color, left);
        }

        /// <author> Jeffrey Goncalves </author>
        /// <summary>
        /// Draws a mouth centered at the origin (translations and projections have to be done before) 
        /// and based on its 2 outer corners
        /// </summary>
        /// <param name="leftOC"> the left outer corner point of the mouth</param>
        /// <param name="rightOC"> the right outer corner of the mouth</param>
        private void DrawMouth(Vector3 leftOC,Vector3 rightOC)
        {
            Vector3[] ULPoints = new Vector3[8];
            Vector3[] LLPoints = new Vector3[8];

            //Upper Lip's points
            ULPoints[0] = leftOC;
            ULPoints[1] = new Vector3(leftOC.X + 0.025f, leftOC.Y + 0.025f, leftOC.Z);
            ULPoints[2] = new Vector3(leftOC.X + 0.05f, leftOC.Y + 0.01f, leftOC.Z);
            ULPoints[3] = new Vector3(leftOC.X + 0.075f, leftOC.Y + 0.025f, leftOC.Z);
            ULPoints[4] = rightOC;
            ULPoints[5] = new Vector3(rightOC.X - 0.04f, leftOC.Y + 0.0075f, rightOC.Z);
            ULPoints[6] = new Vector3(rightOC.X - 0.06f, leftOC.Y + 0.0075f, rightOC.Z);
            ULPoints[7] = new Vector3(rightOC.X - 0.08f, leftOC.Y + 0.0025f, rightOC.Z);

            //Lower Lip's points
            LLPoints[0] = leftOC;
            LLPoints[1] = new Vector3(leftOC.X + 0.025f, leftOC.Y - 0.0005f, leftOC.Z);
            LLPoints[2] = new Vector3(leftOC.X + 0.05f, leftOC.Y - 0.001f, leftOC.Z);
            LLPoints[3] = new Vector3(leftOC.X + 0.075f, leftOC.Y - 0.0005f, leftOC.Z);
            LLPoints[4] = rightOC;
            LLPoints[5] = new Vector3(rightOC.X - 0.025f, rightOC.Y - 0.02f, rightOC.Z);
            LLPoints[6] = new Vector3(rightOC.X - 0.05f, rightOC.Y - 0.03f, rightOC.Z);
            LLPoints[7] = new Vector3(rightOC.X - 0.075f, rightOC.Y - 0.02f, rightOC.Z);
            
            Gl.glBegin(Gl.GL_POLYGON);
            {
                for(int i=0; i < 8; i++)
                {
                    Gl.glVertex3f(ULPoints[i].X, ULPoints[i].Y, ULPoints[i].Z);
                }

                for (int i = 0; i < 8; i++)
                {
                    Gl.glVertex3f(LLPoints[i].X, LLPoints[i].Y, LLPoints[i].Z);
                }
            }
            Gl.glEnd();
        }


        #endregion

        #region Mentor's methods

        private void DrawMentor(Skeleton mentor, FaceDataWrapper fdw, bool draw)
        {
            if (mentor.TrackingState == SkeletonTrackingState.Tracked)
                this.DrawBonesAndJointsMentor(mentor);

            if(draw)
            {
                drawFaceMentor(fdw.depthPointsList, fdw.colorPointsList, fdw.faceTriangles);

                GL.PushMatrix();
                {
                    OpenTK.Vector4 faceColor = new OpenTK.Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                    GL.Color4(faceColor);
                    GL.Normal3(0.0f, 0.0f, 1.0f);
                    GL.LineWidth(3.0f);
                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(mentorMTUL);
                    GL.Vertex3(mentorORCM);
                    GL.Vertex3(mentorMBUL);
                    GL.Vertex3(mentorOLCM);
                    GL.Vertex3(mentorMTUL);
                    GL.End();

                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(mentorMTLL);
                    GL.Vertex3(mentorORCM);
                    GL.Vertex3(mentorMBLL);
                    GL.Vertex3(mentorOLCM);
                    GL.Vertex3(mentorMTLL);
                    GL.End();

                    //                    GL.LineWidth(2.0f);
                    // Drawing of the right eye
                    Gl.glBegin(Gl.GL_TRIANGLE_STRIP);
                    {
                        float alpha;
                        float beta;
                        float alphaStep = (float)(Math.PI) / (float)generalSlices;
                        float betaStep = (float)(Math.PI) / (float)generalStacks;

                        float RVert = (float)Math.Sqrt(Math.Pow((mentorface21.X - mentorface22.X), 2) + Math.Pow((mentorface21.Y - mentorface22.Y), 2)) / 2; //Horizontal semi-axis of the ellipse
                        float RHori = (float)Math.Sqrt(Math.Pow((mentorface23.X - mentorface20.X), 2) + Math.Pow((mentorface23.Y - mentorface20.Y), 2)) / 2; //Vertical semi-axis of the ellipse

                        Gl.glVertex3f(mentorface23.X, mentorface23.Y, mentorface23.Z);
                        for (alpha = -(float)Math.PI / 2; alpha < (float)Math.PI / 2; alpha += alphaStep)
                        {
                            for (beta = 0; beta < (float)2 * Math.PI; beta += alphaStep)
                            {
                                Gl.glVertex3f(mentorface23.X + (float)Math.Cos(alpha) * (float)Math.Cos(beta) * RHori - RHori,
                                    mentorface21.Y + (float)Math.Sin(beta) * (float)Math.Cos(alpha) * RVert - RVert, mentorface22.Z);

                                Gl.glVertex3f(mentorface23.X + RHori * (float)Math.Cos(alpha + alphaStep) * (float)Math.Cos(beta) - RHori,
                                    mentorface21.Y + RVert * (float)Math.Cos(alpha + alphaStep) * (float)Math.Sin(beta) - RVert, mentorface22.Z);
                            }
                        }
                        Gl.glVertex3f(mentorface23.X, mentorface23.Y, mentorface23.Z);

                        //Gl.glVertex3f(face22.X, face22.Y, face22.Z);
                        //Gl.glVertex3f(face20.X, face20.Y, face20.Z);
                        //Gl.glVertex3f(face21.X, face21.Y, face21.Z);

                    }
                    Gl.glEnd();

                    // Drawing of the right eyebrow
                    GL.Begin(PrimitiveType.Polygon);
                    GL.Vertex3(mentorface15);
                    GL.Vertex3(mentorface16);
                    GL.Vertex3(mentorface17);
                    GL.Vertex3(mentorface18);
                    GL.End();

                    // Drawing of the left eye
                    Gl.glBegin(Gl.GL_TRIANGLE_STRIP);

                    float LVert = (float)Math.Sqrt(Math.Pow((mentorface54.X - mentorface55.X), 2) + Math.Pow((mentorface54.Y - mentorface55.Y), 2)) / 2;
                    float LHori = (float)Math.Sqrt(Math.Pow((mentorface56.X - mentorface53.X), 2) + Math.Pow((mentorface56.Y - mentorface53.Y), 2)) / 2;

                    float theta;
                    float phi;
                    float thetaStep = (float)(Math.PI) / (float)generalSlices;
                    float phiStep = (float)(Math.PI) / (float)generalStacks;
                    for (theta = -(float)Math.PI / 2; theta < (float)Math.PI / 2; theta += thetaStep)
                    {
                        for (phi = -(float)Math.PI; phi < (float)Math.PI; phi += phiStep)
                        {

                            Gl.glVertex3f(mentorface56.X + (float)Math.Cos(theta) * (float)Math.Cos(phi) * LHori + LHori,
                                mentorface56.Y + (float)Math.Sin(phi) * (float)Math.Cos(theta) * LVert, mentorface56.Z);

                            Gl.glVertex3f(mentorface56.X + LHori + LHori * (float)Math.Cos(theta + thetaStep) * (float)Math.Cos(phi),
                                mentorface56.Y + LVert * (float)Math.Cos(theta + thetaStep) * (float)Math.Sin(phi), mentorface56.Z);
                        }

                    }

                    Gl.glEnd();

                    // Drawing of the left eyebrow
                    GL.Begin(PrimitiveType.Polygon);
                    GL.Vertex3(mentorface48);
                    GL.Vertex3(mentorface49);
                    GL.Vertex3(mentorface50);
                    GL.Vertex3(mentorface51);
                    GL.End();
                }
                GL.PopMatrix();
            }
        }

        private void DrawBonesAndJointsMentor(Skeleton sk)
        {
            if (sk != null)
            {
                // Head and Shoulders
                // Here, the generalAjustment is set
                this.DrawBoneMentor(sk, JointType.Head, JointType.ShoulderCenter, trackedBoneColor);
                specificAdjustment = new Vector3(0, 0, 0);

                // Left Arm
                this.DrawBoneMentor(sk, JointType.ShoulderCenter, JointType.ShoulderLeft, trackedBoneColor);
                this.DrawBoneMentor(sk, JointType.ShoulderLeft, JointType.ElbowLeft, trackedBoneColor);
                this.DrawBoneMentor(sk, JointType.ElbowLeft, JointType.WristLeft, trackedBoneColor);
                this.DrawBoneMentor(sk, JointType.WristLeft, JointType.HandLeft, trackedBoneColor);
                specificAdjustment = new Vector3(0, 0, 0);

                // Right Arm
                this.DrawBoneMentor(sk, JointType.ShoulderCenter, JointType.ShoulderRight, trackedBoneColor);
                this.DrawBoneMentor(sk, JointType.ShoulderRight, JointType.ElbowRight, trackedBoneColor);
                this.DrawBoneMentor(sk, JointType.ElbowRight, JointType.WristRight, trackedBoneColor);
                this.DrawBoneMentor(sk, JointType.WristRight, JointType.HandRight, trackedBoneColor);
                specificAdjustment = new Vector3(0, 0, 0);

                // Render Torso
                // Here, the generalAjustment is set
                this.DrawBoneMentor(sk, JointType.ShoulderCenter, JointType.Spine, trackedBoneColor);
                this.DrawBoneMentor(sk, JointType.Spine, JointType.HipCenter, trackedBoneColor);
                specificAdjustment = new Vector3(0, 0, 0);

                legTracked = false;
                //Modified by Baptiste Germond
                //If the legs are tracked, we draw the avatar as usual
                if (sk.Joints[JointType.KneeRight].TrackingState == JointTrackingState.Tracked &&
                    sk.Joints[JointType.KneeLeft].TrackingState == JointTrackingState.Tracked)
                {
                    // Left Leg
                    legTracked = true;
                    this.DrawBoneMentor(sk, JointType.HipCenter, JointType.HipLeft, trackedBoneColor);
                    this.DrawBoneMentor(sk, JointType.HipLeft, JointType.KneeLeft, trackedBoneColor);
                    this.DrawBoneMentor(sk, JointType.KneeLeft, JointType.AnkleLeft, trackedBoneColor);
                    this.DrawBoneMentor(sk, JointType.AnkleLeft, JointType.FootLeft, trackedBoneColor);
                    specificAdjustment = new Vector3(0, 0, 0);

                    // Right Leg
                    this.DrawBoneMentor(sk, JointType.HipCenter, JointType.HipRight, trackedBoneColor);
                    this.DrawBoneMentor(sk, JointType.HipRight, JointType.KneeRight, trackedBoneColor);
                    this.DrawBoneMentor(sk, JointType.KneeRight, JointType.AnkleRight, trackedBoneColor);
                    this.DrawBoneMentor(sk, JointType.AnkleRight, JointType.FootRight, trackedBoneColor);
                }
                //If not, the legs are drawn using simulated points
                //It drawing the legs straight, like with the initial avatar
                else if (sk.Joints[JointType.HipCenter].TrackingState == JointTrackingState.Tracked)
                {
                    Joint joint0 = sk.Joints[JointType.HipCenter];
                    Vector3 tempSpec = specificAdjustment;
                    Vector3 tempGen = generalAdjustment;

                    Vector3 point0 = new Vector3(joint0.Position.X, joint0.Position.Y, joint0.Position.Z);
                    Vector3 pointTemp = drawAvatarNotTracked(JointType.HipRight, point0);
                    pointTemp = drawAvatarNotTracked(JointType.KneeRight, pointTemp);
                    pointTemp = drawAvatarNotTracked(JointType.AnkleRight, pointTemp);
                    pointTemp = drawAvatarNotTracked(JointType.FootRight, pointTemp);

                    specificAdjustment = tempSpec;
                    generalAdjustment = tempGen;

                    point0 = new Vector3(joint0.Position.X, joint0.Position.Y, joint0.Position.Z);
                    pointTemp = drawAvatarNotTracked(JointType.HipLeft, point0);
                    pointTemp = drawAvatarNotTracked(JointType.KneeLeft, pointTemp);
                    pointTemp = drawAvatarNotTracked(JointType.AnkleLeft, pointTemp);
                    pointTemp = drawAvatarNotTracked(JointType.FootLeft, pointTemp);
                }
                //Reinitialize adjustment
                specificAdjustment = new Vector3(0, 0, 0);
                generalAdjustment = new Vector3(0, 0, 0);
            }
        }

        public void drawFaceMentor(
            List<Vector3DF> faceP3D,
            List<Microsoft.Kinect.Toolkit.FaceTracking.PointF> faceP,
            FaceTriangle[] faceT)
        {
            List<Vector2> faceModelPts = new List<Vector2>();
            List<Vector2> faceModelPtsDraw = new List<Vector2>();
            var faceModel = new List<FaceModelTriangle>();

            // This vector will allow to center the face on the head
            Vector3 faceAdjustment = new Vector3();

            // the center point is situated between the two eyes and the upperlip
            Vector3 faceCenterPoint = new Vector3(faceP3D.ElementAt(20).X, faceP3D.ElementAt(20).Y, faceP3D.ElementAt(20).Z);
            faceCenterPoint += new Vector3(faceP3D.ElementAt(53).X, faceP3D.ElementAt(53).Y, faceP3D.ElementAt(53).Z);
            faceCenterPoint += new Vector3(faceP3D.ElementAt(40).X, faceP3D.ElementAt(40).Y, faceP3D.ElementAt(40).Z);
            faceCenterPoint = faceCenterPoint / 3;
            //faceAdjustment = headCenterPoint + new Vector3(0, 0, -headRadius) - faceCenterPoint;

            Vector3 radial = faceCenterPoint + mentorheadTilt / 2 - mentorheadCenterPoint;
            double magnitude = Math.Sqrt(radial.X * radial.X + radial.Y * radial.Y + radial.Z * radial.Z);
            float fmagnitude = (float)magnitude;
            faceAdjustment = mentorheadCenterPoint + headRadius * radial / fmagnitude - faceCenterPoint;

            // Upper lip
            mentorMTUL = new Vector3(faceP3D.ElementAt(7).X, faceP3D.ElementAt(7).Y, faceP3D.ElementAt(7).Z) + faceAdjustment;
            mentorMBUL = new Vector3(faceP3D.ElementAt(87).X, faceP3D.ElementAt(87).Y, faceP3D.ElementAt(87).Z) + faceAdjustment;
            // Lower lip
            mentorMBLL = new Vector3(faceP3D.ElementAt(41).X, faceP3D.ElementAt(41).Y, faceP3D.ElementAt(41).Z) + faceAdjustment;
            mentorMTLL = new Vector3(faceP3D.ElementAt(40).X, faceP3D.ElementAt(40).Y, faceP3D.ElementAt(40).Z) + faceAdjustment;
            // Right mouth
            mentorORCM = new Vector3(faceP3D.ElementAt(31).X, faceP3D.ElementAt(31).Y, faceP3D.ElementAt(31).Z) + faceAdjustment;
            // Left mouth
            mentorOLCM = new Vector3(faceP3D.ElementAt(64).X, faceP3D.ElementAt(64).Y, faceP3D.ElementAt(64).Z) + faceAdjustment;
            // Right eye
            mentorface19 = new Vector3(faceP3D.ElementAt(19).X, faceP3D.ElementAt(19).Y, faceP3D.ElementAt(19).Z) + faceAdjustment;
            mentorface20 = new Vector3(faceP3D.ElementAt(20).X, faceP3D.ElementAt(20).Y, faceP3D.ElementAt(20).Z) + faceAdjustment;
            mentorface21 = new Vector3(faceP3D.ElementAt(21).X, faceP3D.ElementAt(21).Y, faceP3D.ElementAt(21).Z) + faceAdjustment;
            mentorface22 = new Vector3(faceP3D.ElementAt(22).X, faceP3D.ElementAt(22).Y, faceP3D.ElementAt(22).Z) + faceAdjustment;
            mentorface23 = new Vector3(faceP3D.ElementAt(23).X, faceP3D.ElementAt(23).Y, faceP3D.ElementAt(23).Z) + faceAdjustment;
            mentorface24 = new Vector3(faceP3D.ElementAt(24).X, faceP3D.ElementAt(24).Y, faceP3D.ElementAt(24).Z) + faceAdjustment;
            // Right eyebrow
            mentorface15 = new Vector3(faceP3D.ElementAt(15).X, faceP3D.ElementAt(15).Y * 1.25f, faceP3D.ElementAt(15).Z) + faceAdjustment;
            mentorface16 = new Vector3(faceP3D.ElementAt(16).X, faceP3D.ElementAt(16).Y * 1.25f, faceP3D.ElementAt(16).Z) + faceAdjustment;
            mentorface17 = new Vector3(faceP3D.ElementAt(17).X, faceP3D.ElementAt(17).Y * 1.25f, faceP3D.ElementAt(17).Z) + faceAdjustment;
            mentorface18 = new Vector3(faceP3D.ElementAt(18).X, faceP3D.ElementAt(18).Y * 1.25f, faceP3D.ElementAt(18).Z) + faceAdjustment;
            // Left eye
            mentorface52 = new Vector3(faceP3D.ElementAt(52).X, faceP3D.ElementAt(52).Y, faceP3D.ElementAt(52).Z) + faceAdjustment;
            mentorface53 = new Vector3(faceP3D.ElementAt(53).X, faceP3D.ElementAt(53).Y, faceP3D.ElementAt(53).Z) + faceAdjustment;
            mentorface54 = new Vector3(faceP3D.ElementAt(54).X, faceP3D.ElementAt(54).Y, faceP3D.ElementAt(54).Z) + faceAdjustment;
            mentorface55 = new Vector3(faceP3D.ElementAt(55).X, faceP3D.ElementAt(55).Y, faceP3D.ElementAt(55).Z) + faceAdjustment;
            mentorface56 = new Vector3(faceP3D.ElementAt(56).X, faceP3D.ElementAt(56).Y, faceP3D.ElementAt(56).Z) + faceAdjustment;
            mentorface57 = new Vector3(faceP3D.ElementAt(57).X, faceP3D.ElementAt(57).Y, faceP3D.ElementAt(57).Z) + faceAdjustment;
            // Left eyebrow
            mentorface48 = new Vector3(faceP3D.ElementAt(48).X, faceP3D.ElementAt(48).Y * 1.25f, faceP3D.ElementAt(48).Z) + faceAdjustment;
            mentorface49 = new Vector3(faceP3D.ElementAt(49).X, faceP3D.ElementAt(49).Y * 1.25f, faceP3D.ElementAt(49).Z) + faceAdjustment;
            mentorface50 = new Vector3(faceP3D.ElementAt(50).X, faceP3D.ElementAt(50).Y * 1.25f, faceP3D.ElementAt(50).Z) + faceAdjustment;
            mentorface51 = new Vector3(faceP3D.ElementAt(51).X, faceP3D.ElementAt(51).Y * 1.25f, faceP3D.ElementAt(51).Z) + faceAdjustment;

            //Finally, we want to lengthen face elements
            float verticalFaceGap = 0.02f * 2.0f;
            float horizontalFaceGap = 0.05f * 2.0f;
            // Eyes spreading
            lengthenSegment(ref mentorface19, ref mentorface52, horizontalFaceGap);
            lengthenSegment(ref mentorface24, ref mentorface57, horizontalFaceGap);
            lengthenSegment(ref mentorface23, ref mentorface56, horizontalFaceGap);
            lengthenSegment(ref mentorface20, ref mentorface53, horizontalFaceGap);
            lengthenSegment(ref mentorface21, ref mentorface54, horizontalFaceGap);
            lengthenSegment(ref mentorface22, ref mentorface55, horizontalFaceGap);
            // Eyes enlargement
            lengthenSegment(ref mentorface23, ref mentorface20, horizontalFaceGap);
            lengthenSegment(ref mentorface22, ref mentorface21, verticalFaceGap);
            lengthenSegment(ref mentorface19, ref mentorface24, verticalFaceGap);
            lengthenSegment(ref mentorface53, ref mentorface56, horizontalFaceGap);
            lengthenSegment(ref mentorface54, ref mentorface55, verticalFaceGap);
            lengthenSegment(ref mentorface52, ref mentorface57, verticalFaceGap);
            // Eyebrows spreading
            lengthenSegment(ref mentorface15, ref mentorface48, horizontalFaceGap);
            lengthenSegment(ref mentorface16, ref mentorface49, horizontalFaceGap);
            lengthenSegment(ref mentorface18, ref mentorface51, horizontalFaceGap);
            lengthenSegment(ref mentorface49, ref mentorface51, verticalFaceGap / 3);
            lengthenSegment(ref mentorface16, ref mentorface18, verticalFaceGap / 3);
            // Mouth enlargement
            lengthenSegment(ref mentorMTUL, ref mentorMBLL, verticalFaceGap);
            lengthenSegment(ref mentorORCM, ref mentorOLCM, horizontalFaceGap * 1.5f);

            for (int i = 0; i < faceP.Count; i++)
            {
                faceModelPts.Add(new Vector2(faceP[i].X, faceP[i].Y));
                faceModelPtsDraw.Add(new Vector2(faceP[i].X, faceP[i].Y));
            }

            foreach (var t in faceT)
            {
                var triangle = new FaceModelTriangle();
                triangle.P1 = faceModelPtsDraw[t.First];
                triangle.P2 = faceModelPtsDraw[t.Second];
                triangle.P3 = faceModelPtsDraw[t.Third];
                faceModel.Add(triangle);
            }
        }

        private void DrawBoneMentor(Skeleton skeleton, JointType jointType0, JointType jointType1, OpenTK.Vector4 color)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];
            Vector3 point0 = new Vector3(joint0.Position.X, joint0.Position.Y, joint0.Position.Z);
            Vector3 point1 = new Vector3(joint1.Position.X, joint1.Position.Y, joint1.Position.Z);

            Vector3 currentBone = new Vector3();
            Vector3.Subtract(ref point0, ref point1, out currentBone);

            // Points are moved according to the old adjustment
            addAdjustmentsToPoints(ref point0, ref point1);

            // adjustment applicated on the edge Point
            Vector3 currentAdjustment;

            // Drawing of the top joint
            drawJoint(joint0, point0);
            if (IsBoneDrawable(joint0, joint1))
            {
                // We assume all drawn bones are inferred unless BOTH joints are tracked
                OpenTK.Vector4 drawColor = this.inferredBoneColor;
                if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
                {
                    drawColor = color;
                }
                // Draw the bone according to the concerned body part
                GL.PushMatrix();
                {
                    if ((jointType0 == JointType.ShoulderLeft && jointType1 == JointType.ElbowLeft) ||
                        (jointType0 == JointType.ShoulderRight && jointType1 == JointType.ElbowRight))
                    {
                        // Then, the edge point is moved according to the new adjustment
                        currentAdjustment = calculateAdjustment(currentBone, properShoulderEndToElbow);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawArm(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.AnkleLeft && jointType1 == JointType.FootLeft) ||
                             (jointType0 == JointType.AnkleRight && jointType1 == JointType.FootRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properAnkleToFoot);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        if (jointType1 == JointType.FootLeft)
                            DrawFoot(point0, point1, drawColor, true);
                        else
                            DrawFoot(point0, point1, drawColor, false);

                        // Drawing of the second joint (foot is an extremity)
                        drawJoint(joint1, point1);
                    }

                    else if ((jointType0 == JointType.ElbowLeft && jointType1 == JointType.WristLeft) ||
                             (jointType0 == JointType.ElbowRight && jointType1 == JointType.WristRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properElbowToWrist);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawForeArm(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.WristLeft && jointType1 == JointType.HandLeft) ||
                             (jointType0 == JointType.WristRight && jointType1 == JointType.HandRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properWristToHand);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        if (jointType1 == JointType.HandLeft)
                            DrawHand(point0, point1, drawColor, true);
                        else
                            DrawHand(point0, point1, drawColor, false);

                        // Drawing of the second joint (hand is an extremity)
                        drawJoint(joint1, point1);
                    }

                    else if ((jointType0 == JointType.Head && jointType1 == JointType.ShoulderCenter))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properHeadToShoulderCenter);
                        generalAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        mentorheadCenterPoint = new Vector3(point0);
                        mentorheadTilt = point0 - point1;
                        DrawHead(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.ShoulderCenter && jointType1 == JointType.Spine))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properShoulderCenterToSpine);
                        generalAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawUpperTorso(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.Spine && jointType1 == JointType.HipCenter))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properSpineToHipCenter);
                        generalAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        // we must align the hipCenter with the spine, else it dispalys the avatar twisted
                        point1.X = point0.X; point1.Z = point0.Z;
                        DrawLowerTorso(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.HipCenter) &&
                             (jointType1 == JointType.HipLeft || jointType1 == JointType.HipRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properHipCenterToHipEnd);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawHip(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.KneeLeft && jointType1 == JointType.AnkleLeft) ||
                             (jointType0 == JointType.KneeRight && jointType1 == JointType.AnkleRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properKneeToAnkle);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawLeg(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.ShoulderCenter) &&
                             (jointType1 == JointType.ShoulderLeft || jointType1 == JointType.ShoulderRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properShoulderCenterToShoulderEnd);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawShoulder(point0, point1, drawColor);
                    }

                    else if ((jointType0 == JointType.HipLeft && jointType1 == JointType.KneeLeft) ||
                             (jointType0 == JointType.HipRight && jointType1 == JointType.KneeRight))
                    {
                        currentAdjustment = calculateAdjustment(currentBone, properHipEndToKnee);
                        specificAdjustment += currentAdjustment;
                        point1 += currentAdjustment;
                        DrawThigh(point0, point1, drawColor);
                    }
                }
                GL.PopMatrix();
            }
        }
        #endregion

        #region OpenGL and GLControl methods
        /// <summary>
        /// Initialization of the 3D sheet (Backgroung color, lighting, materials etc.)
        /// </summary>
        public void init3DScene()
        {
            // initialise Glut
            Glut.glutInit();

            // Paint the background of the sheet using the LecturerTrainer's background color.
            System.Windows.Media.Color applicationBGColor = (System.Windows.Media.Color)dsv.FindResource("UnselectedTabColor");
            GL.ClearColor(((float)applicationBGColor.R) / 255,
                            ((float)applicationBGColor.G) / 255,
                            ((float)applicationBGColor.B) / 255,
                            ((float)applicationBGColor.A) / 255);
            GL.ShadeModel(ShadingModel.Smooth);

            // Materials
            float[] mat_specular = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] mat_shininess = { 50.0f };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, mat_specular);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, mat_shininess);

            //Add positioned light
            float[] lightColor0 = { 0.5f, 0.5f, 0.5f, 1.0f };     //Color (0.5, 0.5, 0.5)

            float[] lightPos0 = { 0.0f, 0.0f, 8.0f, 1.0f };       //Positioned by Asu
            GL.Light(LightName.Light0, LightParameter.Specular, lightColor0);
            GL.Light(LightName.Light0, LightParameter.Position, lightPos0);

            //Add directed light
            float[] lightColor1 = { 0.5f, 0.2f, 0.2f, 1.0f };     //Color (0.5, 0.2, 0.2)
            float[] lightPos1 = { -1.0f, 0.5f, 0.5f, 0.0f };    //Coming from the direction (-1, 0.5, 0.5)
            GL.Light(LightName.Light1, LightParameter.Diffuse, lightColor1);
            GL.Light(LightName.Light1, LightParameter.Position, lightPos1);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Light1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ColorMaterial);

            // Used to show the texture
            // Add by F BECHU: June 2016
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        }

        /// <summary>
        /// Shows the 3 axes : 
        ///     X --> green
        ///     Y --> blue
        ///   - Z --> red
        /// </summary>
        void DrawAxes()
        {
            GL.PushMatrix();
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(0.0, 0.0, 1.0);
                GL.Vertex2(0, 0); GL.Vertex2(0, 1);
                GL.Color3(0.0, 1.0, 0.0);
                GL.Vertex2(0, 0); GL.Vertex2(1, 0);
                GL.Color3(1.0, 0.0, 0.0);
                GL.Vertex2(0, 0); GL.Vertex3(0, 0, -1);
                GL.End();
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a cylinder between two points positioned at (X1, Y1, Z1) and (X2, Y2, Z2)
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="Z1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <param name="Z2"></param>
        /// <param name="radius"></param>
        void DrawCylinder2P(float X1, float Y1, float Z1, float X2, float Y2, float Z2, float radius, OpenTK.Vector4 color)
        {
            // Reversed Z axis give a better visibility
            Z1 = -Z1;
            Z2 = -Z2;
            float vX = X2 - X1;
            float vY = Y2 - Y1;
            float vZ = Z2 - Z1;

            if (vZ == 0)
                vZ = 0.0001f;

            // Size of the vector separating the two points
            double v = Math.Sqrt(vX * vX + vY * vY + vZ * vZ);

            // Angle between the vector and Z axis
            double aX = 57.2957795 * Math.Acos(vZ / v);
            if (vZ < 0.0)
                aX = -aX;
            float rX = -vY * vZ;
            float rY = vX * vZ;
            GL.PushMatrix();
            {
                //draw the cylinder body
                GL.Translate(X1, Y1, Z1);
                GL.Rotate(aX, rX, rY, 0.0);
                GL.Color4(color);
                Glut.glutSolidCylinder(radius, v, generalSlices, generalStacks);
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a solid cone between two points positioned at (X1, Y1, Z1) and (X2, Y2, Z2)
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="Z1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <param name="Z2"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        void DrawCone2P(float X1, float Y1, float Z1, float X2, float Y2, float Z2, float radius, OpenTK.Vector4 color)
        {
            // Reversed Z axis give a better visibility
            Z1 = -Z1;
            Z2 = -Z2;
            float vX = X2 - X1;
            float vY = Y2 - Y1;
            float vZ = Z2 - Z1;

            if (vZ == 0)
                vZ = 0.0001f;

            // Size of the vector separating the two points
            double v = Math.Sqrt(vX * vX + vY * vY + vZ * vZ);

            // Angle between the vector and Z axis
            double aX = 57.2957795 * Math.Acos(vZ / v);
            if (vZ < 0.0)
                aX = -aX;
            float rX = -vY * vZ;
            float rY = vX * vZ;
            GL.PushMatrix();
            {
                //draw the cone body
                GL.Translate(X1, Y1, Z1);
                GL.Rotate(aX, rX, rY, 0.0);
                GL.Color4(color);
                Glut.glutSolidCone(radius, v, generalSlices, generalStacks);
            }
            GL.PopMatrix();
        }
        /// <summary>
        /// <author> Alban Descottes</author>
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="Z1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <param name="Z2"></param>
        /// <param name="radius1"></param>
        /// <param name="radius2"></param>
        /// <param name="color"></param>
        void DrawCylinderWithTwoRadius(float X1, float Y1, float Z1, float X2, float Y2, float Z2, float radius1, float radius2, OpenTK.Vector4 color)
        {
            // Reversed Z axis give a better visibility
            Z1 = -Z1;
            Z2 = -Z2;
            float vX = X2 - X1;
            float vY = Y2 - Y1;
            float vZ = Z2 - Z1;

            if (vZ == 0)
                vZ = 0.0001f;

            // Size of the vector separating the two points
            double v = Math.Sqrt(vX * vX + vY * vY + vZ * vZ);

            // Angle between the vector and Z axis
            double aX = 57.2957795 * Math.Acos(vZ / v);
            if (vZ < 0.0)
                aX = -aX;
            float rX = -vY * vZ;
            float rY = vX * vZ;
            GL.PushMatrix();
            {
                GL.Translate(X1, Y1, Z1);
                GL.Rotate(aX, rX, rY, 0.0);
                GL.Color4(color);
                Glu.GLUquadric test = Glu.gluNewQuadric();
                Glu.gluCylinder(test, radius1, radius2, v, generalSlices, generalStacks);
            }
            GL.PopMatrix();
        }


        /// <summary>
        /// Better shape of the hands
        /// <author>Alban Descottes</author>
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="Z1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <param name="Z2"></param>
        /// <param name="color"></param>
        /// <param name="left"></param>
        void DrawHandSecondversion(float X1, float Y1, float Z1, float X2, float Y2, float Z2, OpenTK.Vector4 color, bool left)
        {
            Z1 = -Z1;
            Z2 = -Z2;
            float vX = X2 - X1;
            float vY = Y2 - Y1;
            float vZ = Z2 - Z1;

            if (vZ == 0)
                vZ = 0.0001f;

            // Size of the vector separating the two points
            double v = Math.Sqrt(vX * vX + vY * vY + vZ * vZ);

            // Angle between the vector and Z axis
            double aX = 57.2957795 * Math.Acos(vZ / v);
            if (vZ < 0.0)
                aX = -aX;
            float rX = -vY * vZ;
            float rY = vX * vZ;
            GL.PushMatrix();
            {
                //draw the cylinder body
                GL.Translate(X1, Y1, Z1);
                GL.Rotate(aX, rX, rY, 0.0);
                if(left)
                    GL.Rotate(80, 0, 0, 1.0f);
                else
                    GL.Rotate(-80, 0, 0, 1.0f);
                GL.Color4(color);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                
                // thoses lines represent the base of the hand (link with forearm)
                Gl.glBegin(Gl.GL_QUAD_STRIP);
                {
                    Gl.glNormal3f(0.0f, -1.0f, 0.0f);
                    Gl.glVertex3f(-0.04f, 0.01f, 0.02f);
                    Gl.glVertex3f(-0.025f, 0.01f, 0);
                    Gl.glVertex3f(-0.02f, 0.02f, 0.02f);
                    Gl.glVertex3f(-0.02f, 0.02f, 0);
                    Gl.glNormal3f(0.0f, 1.0f, 0.0f);//GL.Color4(Color.Blue);
                    Gl.glVertex3f(0.02f, 0.02f, 0.02f);
                    Gl.glVertex3f(0.02f, 0.02f, 0);
                    Gl.glNormal3f(1.0f, 1.0f, 0.0f);// GL.Color4(Color.Green);
                    Gl.glVertex3f(0.04f, 0.01f, 0.02f);
                    Gl.glVertex3f(0.025f, 0.01f, 0);
                    Gl.glNormal3f(1.0f, 0.0f, 0.0f);// GL.Color4(Color.Yellow);
                    Gl.glVertex3f(0.04f, -0.01f, 0.02f);
                    Gl.glVertex3f(0.025f, -0.01f, 0);
                    Gl.glNormal3f(-1.0f, -1.0f, 0.0f);// GL.Color4(Color.Red);
                    Gl.glVertex3f(-0.04f, -0.01f, 0.02f);
                    Gl.glVertex3f(-0.025f, -0.01f, 0);
                    Gl.glVertex3f(-0.04f, 0.01f, 0.02f);
                    Gl.glVertex3f(-0.025f, 0.01f, 0);

                }
                Gl.glEnd();
                Gl.glBegin(Gl.GL_QUAD_STRIP);
                {
                    /*if (left)
                    {*/
                        Gl.glNormal3f(-1.0f, 0.0f, 0.0f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.07f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.02f);
                        Gl.glNormal3f(0.0f, 1.0f, 0.0f);
                        Gl.glVertex3f(-0.04f, -0.01f, 0.07f);
                        Gl.glVertex3f(-0.04f, -0.01f, 0.02f);
                        Gl.glNormal3f(1.0f, 0.0f, 0.0f);
                        Gl.glVertex3f(0.04f, -0.01f, 0.07f);
                        Gl.glVertex3f(0.04f, -0.01f, 0.02f);
                        Gl.glNormal3f(1.0f, 0.0f, 0.0f);
                        Gl.glVertex3f(0.04f, 0.01f, 0.07f);
                        Gl.glVertex3f(0.04f, 0.01f, 0.02f);
                        Gl.glVertex3f(0.02f, 0.02f, 0.07f);
                        Gl.glVertex3f(0.02f, 0.02f, 0.02f);
                        Gl.glVertex3f(-0.02f, 0.02f, 0.07f);
                        Gl.glVertex3f(-0.02f, 0.02f, 0.02f);
                        Gl.glNormal3f(-1.0f, 0.0f, 0.0f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.07f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.02f);
                   /* }
                    else
                    {
                        Gl.glNormal3f(0.0f, 1.0f, 0.0f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.07f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.02f);
                        Gl.glVertex3f(-0.02f, 0.02f, 0.07f);
                        Gl.glVertex3f(-0.02f, 0.02f, 0.02f);
                        Gl.glNormal3f(1.0f, 0.0f, 0.0f);
                        Gl.glVertex3f(0.02f, 0.02f, 0.07f);
                        Gl.glVertex3f(0.02f, 0.02f, 0.02f);
                        Gl.glNormal3f(0.0f, 0.0f, 1.0f);
                        Gl.glVertex3f(0.04f, 0.01f, 0.07f);
                        Gl.glVertex3f(0.04f, 0.01f, 0.02f);
                        Gl.glVertex3f(0.04f, -0.01f, 0.07f);
                        Gl.glVertex3f(0.04f, -0.01f, 0.02f);
                        Gl.glNormal3f(0.0f, -1.0f, 0.0f);
                        Gl.glVertex3f(-0.04f, -0.01f, 0.07f);
                        Gl.glVertex3f(-0.04f, -0.01f, 0.02f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.07f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.02f);
                    }*/
                }
                Gl.glEnd();
                //Gl.glShadeModel(Gl.GL_SMOOTH);
                // thumb
                if (left)
                {
                    Gl.glBegin(Gl.GL_QUAD_STRIP);
                    {
                        Gl.glNormal3f(1.0f, 0.0f, -1.0f); 
                        Gl.glVertex3f(0.04f, -0.01f, 0.02f);
                        Gl.glVertex3f(0.04f, -0.01f, 0.055f);
                        Gl.glVertex3f(0.04f, -0.01f, 0.02f);
                        Gl.glVertex3f(0.06f, -0.01f, 0.055f);
                        Gl.glVertex3f(0.04f, 0.01f, 0.02f);
                        Gl.glVertex3f(0.06f, 0.01f, 0.055f);
                        Gl.glVertex3f(0.04f, 0.01f, 0.02f);
                        Gl.glVertex3f(0.04f, 0.01f, 0.055f);
                    }
                    Gl.glEnd();
                    GL.Translate(0.05f, 0, 0.04f);
                    Glut.glutSolidCylinder(0.007f, 0.05f, generalSlices, generalStacks);
                    GL.Translate(-0.05f - 0.03f, 0, 0.03f);
                }
                else
                {
                    Gl.glBegin(Gl.GL_QUAD_STRIP);
                    {
                        Gl.glNormal3f(-1.0f, 0.0f, -1.0f); 
                        Gl.glVertex3f(-0.04f, -0.01f, 0.02f);
                        Gl.glVertex3f(-0.04f, -0.01f, 0.055f);
                        Gl.glVertex3f(-0.04f, -0.01f, 0.02f);
                        Gl.glVertex3f(-0.06f, -0.01f, 0.055f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.02f);
                        Gl.glVertex3f(-0.06f, 0.01f, 0.055f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.02f);
                        Gl.glVertex3f(-0.04f, 0.01f, 0.055f);
                    }
                    Gl.glEnd();
                    GL.Translate(-0.05f, 0, 0.04f);
                    Glut.glutSolidCylinder(0.007f, 0.05f, generalSlices, generalStacks);
                    GL.Translate(0.05f - 0.03f, 0, 0.03f);
                }
                // draw the first part of the four fingers
                Glut.glutSolidCylinder(0.008f, 0.03f, generalSlices, generalStacks);
                for(int i=0;i<3; i++)
                {
                    GL.Translate(0.02f, 0, 0);
                    Glut.glutSolidCylinder(0.008f, 0.03f, generalSlices, generalStacks);
                }
                // draw the second part of the four fingers
                GL.Translate(-0.06f, 0, 0.03f);
                GL.Rotate(30, 1.0f, 0, 0);
                Glut.glutSolidCylinder(0.006f, 0.02f, generalSlices, generalStacks);
                for (int i = 0; i < 3; i++)
                {
                    GL.Translate(0.02f, 0, 0);
                    Glut.glutSolidCylinder(0.006f, 0.02f, generalSlices, generalStacks);
                }
                

            }
            GL.PopMatrix();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="Z1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <param name="Z2"></param>
        /// <param name="color"></param>
        /// <param name="left"></param>
        private void DrawFootSecondVersion(float X1, float Y1, float Z1, float X2, float Y2, float Z2, OpenTK.Vector4 color, bool left)
        {
            Z1 = -Z1;
            Z2 = -Z2;
            float vX = X2 - X1;
            float vY = Y2 - Y1;
            float vZ = Z2 - Z1;

            if (vZ == 0)
                vZ = 0.0001f;

            // Size of the vector separating the two points
            double v = Math.Sqrt(vX * vX + vY * vY + vZ * vZ);

            // Angle between the vector and Z axis
            double aX = 57.2957795 * Math.Acos(vZ / v);
            if (vZ < 0.0)
                aX = -aX;
            float rX = -vY * vZ;
            float rY = vX * vZ;
            GL.PushMatrix();
            {
                //draw the cylinder body
                GL.Translate(X1, Y1, Z1);
                GL.Rotate(aX, rX, rY, 0.0);
                GL.Color4(color);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                Gl.glShadeModel(Gl.GL_FLAT);
                Gl.glBegin(Gl.GL_QUAD_STRIP);
                {
                    Gl.glVertex3f(-0.03f, 0.005f, 0.1f);
                    Gl.glVertex3f(-0.03f, 0.005f, 0);
                    Gl.glVertex3f(-0.02f, 0.01f, 0.1f);
                    Gl.glVertex3f(-0.02f, 0.02f, 0);
                    Gl.glVertex3f(0.02f, 0.01f, 0.1f);
                    Gl.glVertex3f(0.02f, 0.02f, 0);
                    Gl.glVertex3f(0.03f, 0.005f, 0.1f);
                    Gl.glVertex3f(0.03f, 0.005f, 0);
                    Gl.glVertex3f(0.03f, -0.01f, 0.1f);
                    Gl.glVertex3f(0.03f, -0.01f, 0);
                    Gl.glVertex3f(-0.03f, -0.01f, 0.1f);
                    Gl.glVertex3f(-0.03f, -0.01f, 0);
                    Gl.glVertex3f(-0.03f, 0.005f, 0.1f);
                    Gl.glVertex3f(-0.03f, 0.005f, 0);
                }
                Gl.glEnd();
                Gl.glBegin(Gl.GL_POLYGON);
                {
                    Gl.glVertex3f(-0.03f, 0.005f, 0.1f);
                    Gl.glVertex3f(-0.02f, 0.01f, 0.1f);
                    Gl.glVertex3f(0.02f, 0.01f, 0.1f);
                    Gl.glVertex3f(0.03f, 0.005f, 0.1f);
                    Gl.glVertex3f(0.03f, -0.01f, 0.1f);
                    Gl.glVertex3f(-0.03f, -0.01f, 0.1f);
                    Gl.glVertex3f(-0.03f, -0.01f, 0.1f);
                }
                Gl.glEnd();
                Gl.glShadeModel(Gl.GL_SMOOTH);
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws an ellipsoid centered at (X2, Y2, Z2), and having the "radius" around X, Y, and Z 
        /// given by radiusX, radiusY, and radiusZ respectively. The ellipsoid is oriented like the vector
        /// separating (X1, Y1, Z1) and (X2, Y2, Z2).
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="Z1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <param name="Z2"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        /// <param name="radiusZ"></param>
        /// <param name="color"></param>
        void DrawEllipsoid2P(float X1, float Y1, float Z1, float X2, float Y2, float Z2,
                             float radiusX, float radiusY, float radiusZ, OpenTK.Vector4 color)
        {
            // Reversed Z axis give a better visibility
            Z1 = -Z1;
            Z2 = -Z2;
            float vX = X2 - X1;
            float vY = Y2 - Y1;
            float vZ = Z2 - Z1;

            if (vZ == 0)
                vZ = 0.0001f;

            // Size of the vector separating the two points
            double v = Math.Sqrt(vX * vX + vY * vY + vZ * vZ);

            // Angle between the vector and Z axis
            double aX = 57.2957795 * Math.Acos(vZ / v);
            if (vZ < 0.0)
                aX = -aX;
            float rX = -vY * vZ;
            float rY = vX * vZ;
            GL.PushMatrix();
            {
                //draw the cylinder body
                GL.Translate(X2, Y2, Z2);
                GL.Rotate(aX, rX, rY, 0.0);
                GL.Color4(color);
                DrawEllipsoid(radiusX, radiusY, radiusZ, color);
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws an ellipsoid centered at the origin, and having the "radius" around X, Y, and Z 
        /// given by radiusX, radiusY, and radiusZ respectively.
        /// </summary>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        /// <param name="radiusZ"></param>
        void DrawEllipsoid(float radiusX, float radiusY, float radiusZ, OpenTK.Vector4 color)
        {
            double tStep = (Math.PI) / (float)generalSlices;
            double sStep = (Math.PI) / (float)generalStacks;
            GL.PushMatrix();
            {
                GL.Color4(color);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                for (double t = -Math.PI / 2; t <= (Math.PI / 2) + .0001; t += tStep)
                {
                    GL.Begin(PrimitiveType.TriangleStrip);
                    for (double s = -Math.PI; s <= Math.PI + .0001; s += sStep)
                    {
                        GL.Vertex3(radiusX * Math.Cos(t) * Math.Cos(s), radiusY * Math.Cos(t) * Math.Sin(s), radiusZ * Math.Sin(t));
                        GL.Vertex3(radiusX * Math.Cos(t + tStep) * Math.Cos(s), radiusY * Math.Cos(t + tStep) * Math.Sin(s), radiusZ * Math.Sin(t + tStep));
                    }
                    GL.End();
                }
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a solid sphere centered in (X, Y, Z)
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        void DrawSphere1P(float X, float Y, float Z, float radius, OpenTK.Vector4 color)
        {
            // Reversed Z axis give a better visibility
            Z = -Z;
            GL.PushMatrix();
            {
                //draw the cylinder body
                GL.Translate(X, Y, Z);
                GL.Color4(color);
                Glut.glutSolidSphere(radius, generalSlices, generalStacks);
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Resizes the viewport
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void reshape(int w, int h)
        {
            GL.Viewport(0, 0, w, h);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            if (w <= h && w != 0)
            {
                GL.Ortho(-1.0, 1.0, -1.0 * (double)h / (double)w, 1.0 * (double)h / (double)w, -10.0, 10.0);
            }
            else if((w > h) && h != 0)
            {
                GL.Ortho(-1.0 * (double)w / (double)h, 1.0 * (double)w / (double)h, -1.0, 1.0, -10.0, 10.0);
            }
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        /// <summary>
        /// GLControl resize event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_Resize(object sender, EventArgs e)
        {
            if (!dsv.loaded) return;
            reshape(glControl.Width, glControl.Height);
        }

        /// <summary>
        /// GLControl paint event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            if (!dsv.loaded) return;
            display();
            glControl.SwapBuffers(); 
			
		}
        #endregion

        #region themeGestion
        /// <summary>
        /// Modification of the color of all the openGL
        /// </summary>
        /// <param name="themeName"></param>
        public void modifColorOpenGL(string themeName)
        {
            foreach (ThemeOpenGL t in themesList)
            {
                if (t.Name == themeName)
                {
                    Main.session.themeName = t.Name;
                    actualTheme = t;
                    inferredJointColor = t.IJC;
                    trackedJointColor = t.TJC;
                    inferredBoneColor = t.IBC;
                    trackedBoneColor = t.TBC;

                    pixelFeedbackColor = t.PFC;
                    switchPixel();

                    backgroundColor = t.BC;
                    OpenTK.Graphics.ES20.GL.ClearColor(backgroundColor);
                    View.DrawingSheetView.Get().glControl.Invalidate();
                }
            }
        }

        /// <summary>
        /// Change the color of the pixel of every feedbacks
        /// </summary>
        public void switchPixel()
        {
            foreach (ImageFeedbacksPerso img in DrawingSheetStreamViewModel.Get().listImg)
            {
                img.changeColor(pixelFeedbackColor, true, img.bitmapOpenGL);

            }
        }

        /// <summary>
        /// Function where all the themes are created and add to the themeList
        /// </summary>
        private void createAllThemes()
        {
            ThemeOpenGL temp = new ThemeOpenGL("Pastel", new OpenTK.Vector4(238 / 255f, 64 / 255f, 53 / 255f, 1),
                new OpenTK.Vector4(3 / 255f, 146 / 255f, 207 / 255f, 1), new OpenTK.Vector4(243 / 255f, 119 / 255f, 54 / 255f, 1),
                new OpenTK.Vector4(123 / 255f, 192 / 255f, 67 / 255f, 1), System.Drawing.Color.FromArgb(255, 53, 167, 156),
                System.Drawing.Color.FromArgb(255, 253, 244, 152), "blueSound.png");
            themesList.Add(temp);

            temp = new ThemeOpenGL("Default", new OpenTK.Vector4(1, 1, 0, 1),
                new OpenTK.Vector4(68 / 255f, 192 / 255f, 68 / 255f, 1), new OpenTK.Vector4(0.5f, 0.5f, 0.5f, 1),
                new OpenTK.Vector4(1, 0, 0, 1), System.Drawing.Color.FromArgb(255, 128, 128, 128),
                System.Drawing.Color.FromArgb(255, 30, 31, 36), "lightGreySound.png");
            actualTheme = temp;
            themesList.Add(temp);

            temp = new ThemeOpenGL("Purple", new OpenTK.Vector4(158 / 255f, 55 / 255f, 159 / 255f, 1),
                new OpenTK.Vector4(123 / 255f, 179 / 255f, 255 / 255f, 1), new OpenTK.Vector4(73 / 255f, 50 / 255f, 103 / 255f, 1),
                 new OpenTK.Vector4(232 / 255f, 106 / 255f, 240 / 255f, 1), System.Drawing.Color.FromArgb(255, 162, 0, 255),
                System.Drawing.Color.FromArgb(255, 239, 187, 255), "purpleSound2.png");
            themesList.Add(temp);
        }
        #endregion

        #region Text Display Methods

        /// <summary>
        /// Display the text during the training
        /// </summary>
        public void displayTextTraining()
        {
            string fullTextToDisplay = "";
            int i;
            /*If the coach's avatr is moving, we display the text depending on the response he is givving to the user*/
            if (TrainingWithAvatarViewModel.Get().PlayMode)
            {
                string fileName = Path.GetFileNameWithoutExtension(TrainingWithAvatarViewModel.Get().PathFile);
                string[] temp = fileName.Split('_');

                for (i = 1; i < temp.Length; i++)
                {
                    fullTextToDisplay += temp[i] + " ";
                }
            }
            /*If the suer can move, we indicate him that it is his turn*/
            else if (TrainingWithAvatarViewModel.Get().SkeletonList != null && TrainingWithAvatarViewModel.canBeInterrupted)
            {
                fullTextToDisplay = "Your turn ! ";
            }
            char[] textToDisplay = fullTextToDisplay.ToCharArray();

            GL.Color4(pixelFeedbackColor.R, pixelFeedbackColor.G, pixelFeedbackColor.B, pixelFeedbackColor.A);
            GL.Disable(EnableCap.Lighting);
            GL.RasterPos3(-0.025f * ((float)textToDisplay.Length - 1), 0.8f, 0);
            // GL.Scale(2.0, 2.0, 1.0);
            for (i = 0; i < textToDisplay.Length - 1; i++)
            {
                Glut.glutBitmapCharacter(Glut.GLUT_BITMAP_TIMES_ROMAN_24, textToDisplay[i]);
            }
            // GL.Scale(0.5, 0.5, 1.0);
            GL.Enable(EnableCap.Lighting);
        }

        /// <summary>
        /// Display a message during the training
        /// </summary>
        /// <param name="str"></param>
        public void DisplayTextTraining(string str)
        {
            //both versions work

            /** Version 1 **/
            /*
            char[] textToDisplay = str.ToCharArray();

            GL.Color4(pixelFeedbackColor.R, pixelFeedbackColor.G, pixelFeedbackColor.B, pixelFeedbackColor.A);
            GL.Disable(EnableCap.Lighting);
            GL.RasterPos3(-0.025f * ((float)textToDisplay.Length - 1), 0.8f, 0);
            for (int i = 0; i < textToDisplay.Length; i++)
            {
                Glut.glutBitmapCharacter(Glut.GLUT_BITMAP_TIMES_ROMAN_24, textToDisplay[i]);
            }
            GL.Enable(EnableCap.Lighting);
            */

            /** Version 2 **/
            GL.Color4(pixelFeedbackColor.R, pixelFeedbackColor.G, pixelFeedbackColor.B, pixelFeedbackColor.A);
            GL.Disable(EnableCap.Lighting);
            GL.RasterPos2(-0.025f * ((float)str.Length - 1), 0.8f);
            Glut.glutBitmapString(Glut.GLUT_BITMAP_TIMES_ROMAN_24, str);
            GL.Enable(EnableCap.Lighting);
        }

        // Timothée
        /// <summary>
        /// Display the text said
        /// </summary>
        public void showText()
        {
            GL.PushMatrix();
            {

                GL.Color4(pixelFeedbackColor.R, pixelFeedbackColor.G, pixelFeedbackColor.B, pixelFeedbackColor.A);
                GL.Disable(EnableCap.Lighting);
                GL.RasterPos2(-0.70, -0.95);

                // Use a char[] of 54 elements.
                // Use blank when nothing to display 
                for (int i = 0; i < txtShowOnscreen.Length - 1; i++)
                {
                    Glut.glutBitmapCharacter(Glut.GLUT_BITMAP_TIMES_ROMAN_24, txtShowOnscreen[i]);
                    txtShowOnscreen[i] = txtShowOnscreen[i + 1];
                }
                Glut.glutBitmapCharacter(Glut.GLUT_BITMAP_TIMES_ROMAN_24, txtShowOnscreen[txtShowOnscreen.Length - 1]);

                if (txtToDisplay.Count > 0)
                {
                    txtShowOnscreen[txtShowOnscreen.Length - 1] = txtToDisplay.Dequeue();
                }
                else
                {
                    txtShowOnscreen[txtShowOnscreen.Length - 1] = ' ';
                }

                GL.Enable(EnableCap.Lighting);
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Used with the event throw in AudioProvider.
        /// Get the text said.
        /// </summary>
        /// <param name="sender"> Sender of the event </param>
        /// <param name="f"> The feedback which contain the text </param>
        public void getTxtToDisplay(object sender, Feedback f)
        {
            foreach (char c in f.feedback.ToCharArray())
            {
                txtToDisplay.Enqueue(c);
            }
        }

        /// <summary>
        /// Used to start to display the text said
        /// </summary>
        public void startDisplayTxtSaid()
        {
            // Timothée
            // Configure the system to display the text
            txtToDisplay = new Queue<char>();
            txtShowOnscreen = "                                                      ".ToCharArray();
            AudioAnalysis.AudioProvider.textToShowEvent += getTxtToDisplay;
            displayText = true;
        }

        /// <summary>
        /// Used to stop to display the text said
        /// </summary>
        public void stopDisplayTxtSaid()
        {
            AudioAnalysis.AudioProvider.textToShowEvent -= getTxtToDisplay;
            displayText = false;
        }
        #endregion

        #region Teleprompter Methods

        // Timothée
        /// <summary>
        /// Activates and initializes the teleprompter
        /// </summary>
        public void startTelePrompter()
        {
            AudioAnalysis.AudioProvider.linePrompterEvent += nextLinePrompter;
            prompterText = MainWindow.main.audioProvider.startPrompter();
            teleprompterActivated = true;
            actualLine = 0;
        }

        // Timothée
        /// <summary>
        /// Deactivates the teleprompter
        /// </summary>
        public void stopTeleprompter()
        {
            teleprompterActivated = false;
            MainWindow.main.audioProvider.stopPrompteur();
            AudioAnalysis.AudioProvider.linePrompterEvent -= nextLinePrompter;
        }

        // Timothée
        /// <summary>
        /// Used to know when we have to go to the next line in the prompter
        /// </summary>
        /// <param name="sender"> Who send the event </param>
        /// <param name="f"> the specific line to use in the teleprompter </param>
        private void nextLinePrompter(object sender, IntFeedback f)
        {
            actualLine = f.feedback;

            // If it is the last one, we stop the prompter
            if (actualLine >= prompterText.Count)
            {
                stopTeleprompter();
            }
        }

        // Timothée
        /// <summary>
        /// Used to display on the screen the teleprompter
        /// </summary>
        private void displayPrompterText()
        {
            GL.PushMatrix();
            {
                GL.Color4(pixelFeedbackColor.R, pixelFeedbackColor.G, pixelFeedbackColor.B, pixelFeedbackColor.A);
                GL.Disable(EnableCap.Lighting);

                // To not go outside of the list
                if (actualLine + 5 >= prompterText.Count)
                {
                    for (int nbLine = actualLine; nbLine < prompterText.Count; nbLine++)
                    {
                        GL.RasterPos2(1.1, 0.90 - (nbLine - actualLine) * 0.1);
                        Glut.glutBitmapString(Glut.GLUT_BITMAP_TIMES_ROMAN_24, prompterText[nbLine]);
                    }
                }
                else
                {
                    // Display five line of the text
                    for (int nbLine = actualLine; nbLine < actualLine + 5; nbLine++)
                    {
                        GL.RasterPos2(1.1, 0.90 - (nbLine - actualLine) * 0.1);
                        Glut.glutBitmapString(Glut.GLUT_BITMAP_TIMES_ROMAN_24, prompterText[nbLine]);
                    }
                }

                GL.Enable(EnableCap.Lighting);
            }
            GL.PopMatrix();
        }

        #endregion
    }

}
