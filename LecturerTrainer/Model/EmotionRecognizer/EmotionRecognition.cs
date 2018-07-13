using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LecturerTrainer.View;
using Microsoft.Kinect.Toolkit.FaceTracking;
using LiveCharts.Wpf;
using LiveCharts;

namespace LecturerTrainer.Model.EmotionRecognizer
{
    class EmotionRecognition
    {
        public static bool detect = false; //this variable is used in SkeletonFaceTracker to detect that the checkbox has been checked
        public static event EventHandler<LongFeedback> emoEvent;
        public static double bound1 = 0.2;
        public static double bound2down = -0.3;
        public static double bound2up = 0.15;
        public static double bound3down = 0;
        public static double bound3up = 0.015;
        public static double bound4up = 0.2;
        public static double bound4down = -0.15;
        public static double bound5up = 0;
        public static bool happy = false, surprised = false, neutral = false;

        public static int count = 30;
        public static bool faceDetected = false;


        /// <summary>
        /// List of time when the user does a surprised face if facetracking is activated
        /// </summary>
        private static List<int> surprisedFace;

        /// <summary>
        /// List of time when the user does a happy face if facetracking is activated
        /// </summary>
        private static List<int> happyFace;
      
        
        /// <summary>
        /// True if we have to record the emotions
        /// </summary>
        private static bool rec = false;

        /// <summary>
        /// Public attribute for rec. Allows to reset the structures.
        /// </summary>
        public static bool record
        {
            get
            {
                return rec;
            }
            set
            {
                rec = value;
                if (rec)
                {
                    surprisedFace = new List<int>();
                    happyFace = new List<int>();
                }
            }
        }

        public static void raiseEmotionEvent(ServerFeedback feedback)
        {
            emoEvent(null, new LongFeedback(feedback.feedbackMessage, feedback.display));
        }

        public static void EmotionRecognizer()
        {
            /*Using a counter so it won't go to much in the try/catch and make the software lag*/
            if (count == 30 || faceDetected==true)
            {
                count = 0;
                try
                {
                    faceDetected = true;
                    var temp = KinectDevice.skeletonFaceTracker.AU;
                    var jawLow1 = temp[AnimationUnit.JawLower];
                    var lipSide2 = temp[AnimationUnit.LipStretcher];
                    var browLow3 = temp[AnimationUnit.BrowLower];
                    var lipDown4 = temp[AnimationUnit.LipCornerDepressor];
                    var browHigh5 = temp[AnimationUnit.BrowRaiser];


                    //poker face
                    /*if (jawLow1 < bound1 && lipSide2 > bound2down && lipSide2 < bound2up && browLow3 > bound3down && browLow3 < bound3up && lipDown4 < bound4up && lipDown4 > bound4down && browHigh5 < bound5up)
                    {
                        emoEvent(null, new LongFeedback("Poker Face", true));
                    }
                    */
                    //happy
                    if (lipSide2 >= 0.12 && lipDown4 < -0.12) //Fiona's values
                    {
                        emoEvent(null, new LongFeedback("Happy", true));
                        if (rec)
                        {
                            if (!happyFace.Contains((int)(Tools.getStopWatch() / 100)))
                            {
                                happyFace.Add((int)(Tools.getStopWatch() / 100));
                            }
                        }
                        happy = true;
                        surprised = false;
                    }
                    //sad
                    /* else if (lipSide2 > bound2down && browLow3 >= bound3up && lipDown4 >= bound4up)
                     {
                         emoEvent(null, new LongFeedback("Sad", true));
                     }
                     */
                    //surprised
                    else if (browLow3 > -0.15 && browLow3 < 0.15 && browHigh5 > 0.15)
                    {
                        emoEvent(null, new LongFeedback("Surprised", true));
                        if (rec)
                        {
                            if (!surprisedFace.Contains((int)(Tools.getStopWatch() / 100)))
                            {
                                surprisedFace.Add((int)(Tools.getStopWatch() / 100));
                            }
                        }
                        surprised = true;
                        happy = false;
                    }
                    //frightened
                    /* else if (jawLow1 >= bound1 && browLow3 <= bound3down && browHigh5 >= bound5up)
                     {
                         emoEvent(null, new LongFeedback("Frightened", true));
                     }
                     //angry
                     else if (lipSide2 <= bound2down && browLow3 >= bound3up && lipDown4 < bound4up)
                     {
                         emoEvent(null, new LongFeedback("Angry", true));
                     }
                     //disgusted
                     else if (jawLow1 < bound1 && browLow3 < bound3up && lipDown4 >= bound4up)
                     {
                         emoEvent(null, new LongFeedback("Disgusted", true));
                     }
                     */
                    //neutral
                    else
                    {
                        emoEvent(null, new LongFeedback("Neutral", true));
                        happy = false;
                        surprised = false;
                    }
                }
                catch (NullReferenceException) { faceDetected = false; }
            }
            else{count++;}
        }

        /// <summary>
        /// method that returns an empty graph if ther is just neutral faces or the charts of the surprised/happy faces
        /// </summary>
        /// <returns>the graph</returns>
        /// <remarks>Author: Alban Descottes 2018</remarks>
        public static List<IGraph> getEmotionsStatistics()
        {
            List<IGraph> list = new List<IGraph>();
            CartesianGraph graph = new CartesianGraph();
            graph.title = "Recognition of emotions";
            graph.subTitle = "Time unit: " + Tools.ChooseTheCorrectUnitTime();
            graph.XTitle = "Time";
            graph.YTitle = "Value";
            // if there is just one of the both emotions (surprised or happy) during the record, it adds the charts
            bool hf = Tools.addSeriesToCharts(graph, new ColumnSeries(), "Happy Face", happyFace, "Total happy faces: ", false);
            bool sf = Tools.addSeriesToCharts(graph, new ColumnSeries(), "Surprised Face", surprisedFace, "Total surprised faces: ", false);
            if ( hf || sf)
                list.Add(graph);
            else
                list.Add(Tools.createEmptyGraph("Just neutral faces"));
            return list;
        }

    }
}