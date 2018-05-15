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

        public static Dictionary<String, double> dicEmotion = null;

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
                    dicEmotion = new Dictionary<string, double>();
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
                    //if (lipSide2 >= bound2up && browLow3 > bound3down && lipDown4 < bound4down) //Xavier's values
                    if (lipSide2 >= 0.12 && lipDown4 < -0.12) //Fiona's values
                    {
                        emoEvent(null, new LongFeedback("Happy", true));
                        if(rec)
                            recordEmotion();
                        happy = true;
                        surprised = false;
                        neutral = false;
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
                            recordEmotion();
                        surprised = true;
                        happy = false;
                        neutral = false;
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
                        if (rec)
                            recordEmotion();
                        happy = false;
                        surprised = false;
                        neutral = true;
                    }
                }
                catch (NullReferenceException) { faceDetected = false; }
            }
            else{count++;}
        }

        /// <summary>
        /// Method that allows to record the emotion in the dictionnary
        /// </summary>
        public static void recordEmotion()
        {
            if(neutral)
            {
                if(dicEmotion.ContainsKey("Neutral"))
                {
                    dicEmotion["Neutral"] +=1;
                }
                else
                {
                    dicEmotion.Add("Neutral",1);
                }
                
                
            }
            else if(happy)
            {

                if (dicEmotion.ContainsKey("Happy"))
                {
                    dicEmotion["Happy"] += 1;
                }
                else
                {
                    dicEmotion.Add("Happy", 1);
                }
            }
            else if(surprised)
            {

                if (dicEmotion.ContainsKey("Suprised"))
                {
                    dicEmotion["Suprised"] += 1;

                }
                else
                {
                    dicEmotion.Add("Suprised", 1);
                }
            }
        }

        /// <summary>
        /// function to obtain the proportion of each emotion
        /// </summary>
        /// <returns>the graph</returns>
        /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
        public static T getStatistics<T>(T graph) where T : IGraph
        {
            graph.title = "Recognition of emotions";

            foreach (KeyValuePair<string, double> entry in dicEmotion)
            {
                PieSeries s1 = new PieSeries
                {
                    Title = entry.Key,
                    Values = new ChartValues<double> { entry.Value },
                    LabelPoint = chartPoint => string.Format("({0:P})", chartPoint.Participation)
                };
                graph.listSeries.Add(s1);
            }

            return graph;
        }

    }
}