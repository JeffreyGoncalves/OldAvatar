using LecturerTrainer.ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.EmotionRecognizer
{
    /// <summary>
    /// Analyse of the eye direction of the speaker
    /// Modified and added by Baptiste Germond
    /// </summary>
    class lookingDirection
    {
        /// <summary>
        /// Constant values to identifie view area
        /// </summary>
        public const int RIGHT = -1;
        public const int EXTRIGHT = -2;
        public const int LEFT = 1;
        public const int EXTLEFT = 2;
        public const int CENTER = 0;

        /// <summary>
        /// this variable is used in SkeletonFaceTracker to detect that the checkbox has been checked
        /// </summary>
        public static bool detect = false; 

        public static event EventHandler<InstantFeedback> lookEvent;

        /// <summary>
        /// Boolean allowing to detect if the eye enter a different view area
        /// </summary>
        private static bool right = false;
        private static bool left = false;
        private static bool centre = false;
        private static bool extremRight = false;
        private static bool extremLeft = false;

        /// <summary>
        /// Boolean use to select which feedback to display on the screen
        /// </summary>
        public static bool feedL = false;
        public static bool feedR = false;
        public static bool feedC = false;

        /// <summary>
        /// Counter use to detect how long the user left his eyes in the view area
        /// </summary>
        private static int countC = 0;
        private static int countR = 0;
        private static int countER = 0;
        private static int countL = 0;
        private static int countEL = 0;

        /// Value to indicate if the eye of the user stayed for too long in a particular area
        /// <summary>
        /// Limit for extremLeft and extremRight
        /// </summary>
        private static int limitLow = 75;
        /// <summary>
        /// //Limit for righ, left and center
        /// </summary>
        private static int limitHigh = 150;

        /// <summary>
        /// Keep the historical of the view areas the eye of the user has been to show him the best area he should look at
        /// </summary>
        private static int[] lookHistorical = new int[15];

        /// <summary>
        /// Counter to retry using the view direction if the face is not detected
        /// </summary>
        public static int count = 30;

        /// <summary>
        /// Boolean to know if the face is detected
        /// </summary>
        public static bool faceDetected = false;

        /// <summary>
        /// Allow to record all the movment of the eyes, it is used for the statistics
        /// </summary>
        public static Dictionary<String,int > dicDirect = null;


        /// <summary>
        /// True if we have to record the movment of the eyes
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
                    dicDirect = new Dictionary<String, int>();
                    dicDirect.Add("Extrem Left", 0);
                    dicDirect.Add("Left", 0);
                    dicDirect.Add("Center", 0);
                    dicDirect.Add("Right", 0);
                    dicDirect.Add("Extrem Right", 0);
                }
            }
        }

        /// <summary>
        /// Main method to recognize which view area the user is looking at
        /// Modified by Baptiste Germond
        /// </summary>
        public static void lookRecognizer()
        {
            //Using a counter so it won't go to much in the try/catch and make the software lag
            //It prevents the method to be executed if the face is not recognize
            if (count == 30 || faceDetected ==true)
            {
                count = 0;
                try
                {
                    var temp = KinectDevice.skeletonFaceTracker.facePoints3D;
                    try
                    {
                        faceDetected = true;
                        var rightEye = temp.ElementAt(20);
                        var leftEye = temp.ElementAt(53);

                        //Test if the user is looking at the center area
                        if (rightEye.Z - leftEye.Z >= -0.015 && rightEye.Z - leftEye.Z <= 0.015)
                        {
                            /*If he just entered the zone (!centre) 
                             * and he is not just passing by to go to another area (countC > 4)*/
                            if (!centre && countC > 4)
                            {
                                addHistorical(CENTER); //Adding the center area to the historical
                                boolManagement(CENTER); // Indicate that the user is looking at the center and reseting all counter
                                if (rec)
                                {
                                    dicDirect["Center"] += 1; //If we are recording, we had +1 in the dictionary use for the statistics
                                }
                            }
                            countC++; //Counter to know howlong the user is looking at the same view area
                        }

                        //Test if the user is looking at the right area
                        else if (rightEye.Z - leftEye.Z >= -0.057 && rightEye.Z - leftEye.Z < -0.015)
                        {
                            if (!right)
                            {
                                addHistorical(RIGHT);
                                boolManagement(RIGHT);
                                if (rec)
                                {
                                    dicDirect["Right"] += 1;
                                }
                            }
                            countR++;
                        }
                        //Test if the user is looking at the extrem right area
                        else if (rightEye.Z - leftEye.Z < -0.057)
                        {
                            if (!extremRight)
                            {
                                addHistorical(RIGHT);
                                boolManagement(EXTRIGHT);
                                if (rec)
                                {
                                    dicDirect["Extrem Right"] += 1;
                                }
                            }
                            countER++;
                        }
                        //Test if the user is looking at the left area
                        else if (rightEye.Z - leftEye.Z > 0.015 && rightEye.Z - leftEye.Z <= 0.057)
                        {
                            if (!left)
                            {
                                addHistorical(LEFT);
                                boolManagement(LEFT);
                                if (rec)
                                {
                                    dicDirect["Left"] += 1;
                                }
                            }
                            countL++;
                        }
                        //Test if the user is looking at the extrem left area
                        else if (rightEye.Z - leftEye.Z > 0.057)
                        {
                            if (!extremLeft)
                            {
                                addHistorical(LEFT);
                                boolManagement(EXTLEFT);
                                if (rec)
                                {
                                    dicDirect["Extrem Left"] += 1;
                                }
                            }
                            countEL++;
                        }

                        //If the user is still looking at the same view area and if the counter reached the limit
                        if (centre && countC > limitHigh)
                        {
                            int lessUsed = countInHistorical(RIGHT, LEFT); //We decide which was less used between the two other areas
                            updateFeedback(lessUsed); //We display the feedback to notifie the user that he should look at another area
                        }
                        else if (right && countR > limitHigh)
                        {
                            int lessUsed = countInHistorical(CENTER, LEFT);
                            updateFeedback(lessUsed);
                        }
                        else if (extremRight && countER > limitLow)
                        {
                            int lessUsed = countInHistorical(CENTER, LEFT);
                            updateFeedback(lessUsed);
                        }
                        else if (left && countL > limitHigh)
                        {
                            int lessUsed = countInHistorical(CENTER, RIGHT);
                            updateFeedback(lessUsed);
                        }
                        else if (extremLeft && countEL > limitLow)
                        {
                            int lessUsed = countInHistorical(CENTER, RIGHT);
                            updateFeedback(lessUsed);
                        }
                        else
                        {
                            //Value very different from the others which means that all feedback should be deactivate
                            updateFeedback(-100); 
                        }
                    }
                    catch (System.ArgumentException) { faceDetected = false; }
                }
                catch (NullReferenceException) { faceDetected = false; }
            }
            else { count++; }
        }

        /// <summary>
        /// Raise the event to display a new looking direction feedback
        /// </summary>
        public static void raiseLookEvent(ServerFeedback feedback)
        {
            lookEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }

        /// <summary>
        /// Add a view area in the local historical of the view areas
        /// </summary>
        public static void addHistorical(int k)
        {
            int i;
            //First we shift the historical
            for (i=14; i>0; i--)
            {
                lookHistorical[i] = lookHistorical[i - 1];
            }
            //Then we place the new value at the begining
            lookHistorical[0] = k;
        }

        /// <summary>
        /// Indicate the area the user is looking at, and put every other boolean to false
        /// Reset the counters
        /// Added by Baptiste Germond
        /// </summary>
        public static void boolManagement(int look)
        {
            if(look == CENTER)
            {
                centre = true;
                right = false;
                extremRight = false;
                left = false;
                extremLeft = false;
                countEL = 0;
                countER = 0;
                countL = 0;
                countR = 0;
            }
            else if (look == RIGHT)
            {
                centre = false;
                right = true;
                extremRight = false;
                left = false;
                extremLeft = false;
                countEL = 0;
                countER = 0;
                countL = 0;
                countC = 0;
            }
            else if (look == EXTRIGHT)
            {
                centre = false;
                right = false;
                extremRight = true;
                left = false;
                extremLeft = false;
                countEL = 0;
                countR = 0;
                countL = 0;
                countC = 0;
            }
            else if (look == LEFT)
            {
                centre = false;
                right = false;
                extremRight = false;
                left = true;
                extremLeft = false;
                countEL = 0;
                countER = 0;
                countR = 0;
                countC = 0;
            }
            else if (look == EXTLEFT)
            {
                centre = false;
                right = false;
                extremRight = false;
                left = false;
                extremLeft = true;
                countR = 0;
                countER = 0;
                countL = 0;
                countC = 0;
            }

        }

        /// <summary>
        /// Determine which of two different areas was less used in the historical
        /// Added by Baptiste Germond
        /// </summary>
        public static int countInHistorical(int first, int second)
        {
            int i;
            int countFirst =0, countSecond = 0;
            //First we count each occurrence of both area
            for (i = 0; i < 15; i++)
            {
                if (lookHistorical[i] == first)
                {
                    countFirst++;
                }
                if (lookHistorical[i] == second)
                {
                    countSecond++;
                }
            }
            //Then we return the lower of both
            if (countFirst>countSecond)
            {
                return second;
            }
            else
            {
                return first;
            }
        }

        /// <summary>
        /// Update the feedback the software should display
        /// Ensure that only one feedback for the eaye direction is display
        /// Added by Baptiste Germond
        /// </summary>
        public static void updateFeedback(int lookFeed)
        {
            if (lookFeed == LEFT)
            {
                feedL = true;
                feedR = false;
                feedC = false;
                lookEvent(null, new InstantFeedback("Look to the left"));
            }
            else if (lookFeed == RIGHT)
            {
                feedR = true;
                feedC = false;
                feedL = false;
                lookEvent(null, new InstantFeedback("Look to the right"));
            }
            else if (lookFeed == CENTER)
            {
                feedR = false;
                feedC = true;
                feedL = false;
                lookEvent(null, new InstantFeedback("Look to the center"));
            }
            else
            {
                feedR = false;
                feedC = false;
                feedL = false;
            }
        }

        /// <summary>
        /// Function to obtain the number of each looking direction
        /// </summary>
        /// <returns>The graph</returns>
        public static T getStatistics<T>(T chart) where T : IGraph
        {
            chart.title = "Looking Direction";
            PieSeries s1 = new PieSeries
            {
                Title = "Extrem Right",
                Values = new ChartValues<double> { dicDirect["Extrem Right"] },
                LabelPoint = chartPoint => string.Format("({1:P})", chartPoint.Participation)
            };
            PieSeries s2 = new PieSeries
            {
                Title = "Right",
                Values = new ChartValues<double> { dicDirect["Right"] },
                LabelPoint = chartPoint => string.Format("({1:P})", chartPoint.Participation)
            };
            PieSeries s3 = new PieSeries
            {
                Title = "Center",
                Values = new ChartValues<double> { dicDirect["Center"] },
                LabelPoint = chartPoint => string.Format("({1:P})", chartPoint.Participation)
            };
            PieSeries s4 = new PieSeries
            {
                Title = "Left",
                Values = new ChartValues<double> { dicDirect["Left"] },
                LabelPoint = chartPoint => string.Format("({1:P})", chartPoint.Participation)
            };
            PieSeries s5 = new PieSeries
            {
                Title = "Extrem Left",
                Values = new ChartValues<double> { dicDirect["Extrem Left"] },
                LabelPoint = chartPoint => string.Format("({1:P})", chartPoint.Participation)
            };
            chart.listSeries.Add(s1);
            chart.listSeries.Add(s2);
            chart.listSeries.Add(s3);
            chart.listSeries.Add(s4);
            chart.listSeries.Add(s5);

            return chart;
        }
    }
}