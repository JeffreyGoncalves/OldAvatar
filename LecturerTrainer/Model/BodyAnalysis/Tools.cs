using LecturerTrainer.ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace LecturerTrainer.Model
{

    /// <summary>
    /// Tools used in the program.
    /// </summary>
    /// <remarks>Author: Clement Michard</remarks>
    public class Tools
    {
        #region fields
        //Add by Florian BECHU: Summer 2016
        private static DispatcherTimer dispatcherTimer;
        private static Int64 time;
        /// <summary>
        /// used for the charts. It is 1/10 of the total time of the record.
        /// </summary>
        private static int correctTime = 0;
        /// <summary>
        /// time interval for the timer
        /// </summary>
        private static int clock;
        #endregion

        #region conversion methods
        /// <summary>
        /// Convert a double in formatted string.
        /// </summary>
        /// <param name="n">Double to convert.</param>
        /// <returns>String representing the double.</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static string format(double n)
        {
            return n.ToString(new CultureInfo("en-US"));
        }

        /// <summary>
        /// Convert string to double.
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <returns>double</returns>
        /// <remarks>Author: Clement Michard</remarks>
        public static double str2Double(string s)
        {
            return Convert.ToDouble(s, new CultureInfo("en-US"));
        }

        /// <summary>
        /// function used to convert a number of frames to a number of milliseconds.
        /// </summary>
        public static int convertFrameToMillisecond(int nbFrame)
        {
            return nbFrame * (1000 / 30);
        }

        /// <summary>
        /// Use to show the time value in charts
        /// </summary>
        /// <param name="value">the time in second</param>
        /// <returns>a string with a form: 1:20 (example)</returns>
        public static string convertSecondtoMinute(int value)
        {
            if (value > 60)
            {
                int cpt = 0;
                while (value >= 60)
                {
                    cpt++;
                    value -= 60;
                }
                return cpt.ToString() + ":" + value;
            }
            else
            {
                return value.ToString();
            }
        }
        #endregion

        #region methods binded to the skeleton
        /// <summary>
        /// verify if all the joins are tracked
        /// </summary>
        /// <param name="sk">the skeleton</param>
        /// <returns>true if all the joins are tracked</returns>
        public static bool allJointsTracked(Skeleton sk)
        {
            foreach (Joint j in sk.Joints)
            {
                if (j.TrackingState != JointTrackingState.Tracked)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// verify if a majority of the joins are tracked
        /// </summary>
        /// <param name="sk">the skeleton</param>
        /// <returns>true if 55% of the joins are tracked</returns>
        public static bool MajorityJointsTracked(Skeleton sk)
        {
            int i = 0, max = 0;
            foreach (Joint j in sk.Joints)
            {
                max++;
                if (j.TrackingState != JointTrackingState.Tracked)
                {
                    i++;
                }
            }
            float nbTraked = (float)i/ ((float)max);
            
            if(nbTraked < 0.45) //Florian BECHU 2016 's values
            {
                return true;
            }
            else
            {
                return false;
            } 
        }
        #endregion

       

        #region stopwatch
        /// <summary>
        /// StopWatch is more accurate than other Timer, so we use that when we're recording the avatar
        /// It's also used for test in the replay mode
        /// </summary>
        /// <author>Alban Descottes 2018</author>
        private static Stopwatch stopWatch;

        public static void startStopWatch()
        {
            stopWatch.Start();
        }

        public static void resetStopWatch()
        {
            stopWatch.Reset();
        }

        public static void stopStopWatch()
        {
            if (stopWatch.IsRunning)
                stopWatch.Stop();
        }

        public static void initStopWatch()
        {
            stopWatch = new Stopwatch();
        }

        public static long getStopWatch()
        {
            return stopWatch.ElapsedMilliseconds;
        }

        public static bool getStateStopWatch()
        {
            return stopWatch.IsRunning;
        }

        public static string FormatTime(int time)
        {
            int h, m, s/*, ms*/;
            string stringTime = "";

            h = time / 3600000;
            if (h < 10)
                stringTime += "0";
            stringTime += h + ":";

            m = (time % 3600000) / 60000;
            if (m < 10)
                stringTime += "0";
            stringTime += m + ":";

            s = ((time % 3600000) % 60000) / 1000;
            if (s < 10)
                stringTime += "0";
            stringTime += s;
            // not very usefull to display on the UI
            /*stringTime += ".";
            ms = ((time % 3600000) % 60000) % 1000;
            if (ms < 100)
                stringTime += "0";
            if (ms < 10)
                stringTime += "0";
            stringTime += ms;*/
            return stringTime;
        }

        public static int secondEllapsed()
        {
            return (int)stopWatch.ElapsedMilliseconds/1000;
        }

        #endregion

        #region methods for the charts
        /// <summary>
        /// function used by the creation of the charts. Allow to choose the right unit of the charts.
        /// </summary>
        /// <returns>a String showed in the charts</returns>
        /// <remarks>Add by Florian BECHU: Summer 2016</remarks>
        public static String ChooseTheCorrectUnitTime()
        {
            long temp = TrainingSideToolViewModel.Get().timeRecorded;

            if(temp <10000)
            {
                correctTime = 1000; //number of frame
                return "Unit per sec";
            }
            else if(temp < 1000*60*10) // 10 min
            {
                correctTime = (int)temp / 10;
                correctTime = (int)(Math.Ceiling((double)correctTime / 1000) * 1000);
                int unit = (int)correctTime / 1000;
                return "Unit per " + unit + " sec";
            }
            else
            {
                correctTime = (int)temp / 10;
                correctTime = (int)(Math.Ceiling((double)correctTime / 1000) * 1000);
                int unit = (int)correctTime / 1000;

                return "Unit per " + unit/60 + " min" + (unit%60 >0 ? unit % 60+" sec":"");
            }
        }

        /// <summary>
        /// return the right unit time to show the charts
        /// </summary>
        public static int getCorrectTime()
        {
            return correctTime;
        }


        /// <summary>
        /// obtain the date included in a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>a DateTime</returns>
        /// <remarks>Add by Florian BECHU: Summer 2016</remarks>
        public static DateTime getDateFromPath(string path) // sDate de type AAAA_MM_DD
        {
            string[] tab = path.Split('\\');
            string pattern = "[0-9]{4}_[0-9]{1,2}_[0-9]{1,2}";
            Regex searchRegEx = new Regex(pattern);
            Match correspond = searchRegEx.Match(tab[tab.Length - 1]);
            string sDdate = correspond.Groups[0].Value;
            string[] tab2 = sDdate.Split('_');
            DateTime date = new DateTime(int.Parse(tab2[0]), int.Parse(tab2[1]), int.Parse(tab2[2]));
            return date;
        }

        /// <summary>
        /// return the sum of a serie for a chart
        /// </summary>
        /// <param name="serie">the serie</param>
        /// <returns>the sum</returns>
        public static double getSumSeries(Series serie)
        {
            double sum = 0;
            foreach (double d in serie.Values)
            {
                sum += d;
            }
            return sum;
        }

        /// <summary>
        /// Method to create a serie with the type of "series", with data included in "list" and add the serie in "chart"
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="chart">the chart in which we must add the serie</param>
        /// <param name="series">the series ( example: LineSeries, PieSeries, ...) to add in chart</param>
        /// <param name="seriesName">correspond to the name of the serie ( appears in the chart legend)</param>
        /// <param name="list">the list with the data</param>
        /// <param name="totalValue">the string for the Total Value</param>
        /// <param name="addEmpty">if is true and the data is empty, an empty serie will be add in the chart</param>
        /// <returns>true if a serie had been added in the chart, false otherwise</returns>
        /// <remarks>Add by Florian BECHU: Summer 2016</remarks>
        public static bool addSeriesToCharts<U>(IGraph chart, Series series, string seriesName,U list,string totalValue,bool addEmpty) where U : ICollection<int>
        {

            /*foreach (int nteger in list)
                Console.Out.WriteLine("-- val " + nteger);*/
            List<string> listLabel = new List<string>();
            series.Title = seriesName;

            if(!addEmpty && list.Count<=0)
                return false;

            series.Values = new ChartValues<double>();
            for (int i = 0; i < (TrainingSideToolViewModel.Get().timeRecorded / getCorrectTime()) + 1; i++) // Chart Initializing. We put all values at 0 and we create labels for the X-axis
            {
                int val1 = i * (getCorrectTime() / 1000);
                int val2 = val1 + (getCorrectTime() / 1000) - 1;
                if (val2 - val1 == 0) // if the sessions is lower than 10 secs
                {
                    listLabel.Add((val1 + 1).ToString() + "sec");
                    series.Values.Add((double)0);
                }
                else
                {
                    listLabel.Add(convertSecondtoMinute(val1) + "--" + convertSecondtoMinute(val2));
                    series.Values.Add((double)0);
                }
            }

            int c = 0;
            int indice = 0;
            int gap = (int)TrainingSideToolViewModel.Get().timeRecorded / 100 / series.Values.Count;
            for (int i = 0; i < list.Count; i++) 
            {
                if (list.ElementAt(i) < c + gap && list.ElementAt(i) >= c)
                {
                    series.Values[indice] = (double)series.Values[indice] + 1;
                }
                else
                {
                    if (i == list.Count - 1)
                        break;
                    c += gap;
                    indice++;
                    i--;
                }

            }
            
            chart.listSeries.Add(series);
            if (totalValue.Count() > 0)
            {
                chart.listTotalValue.Add(totalValue + getSumSeries(series));
            }
            chart.Labels = listLabel;
            chart.XTitle = "Time";
            if (list.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Method to create a serie with the type of "series", with data included in "list" and add the serie in "chart"
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="chart">the chart in which we must add the serie</param>
        /// <param name="series">the series ( example: LineSeries, PieSeries, ...) to add in chart</param>
        /// <param name="seriesName">correspond to the name of the serie ( appears in the chart legend)</param>
        /// <param name="list">the list with the data</param>
        /// <param name="totalValue">the string for the Total Value</param>
        /// <param name="addEmpty">if is true and the data is empty, an empty serie will be add in the chart</param>
        /// <returns>true if a serie had been added in the chart, false otherwise</returns>
        /// <remarks>Add by Florian BECHU: Summer 2016</remarks>
        public static bool addKeyValuePairSeriesToCharts<U>(IGraph chart, Series series, string seriesName, U list, string totalValue, bool addEmpty) where U : Dictionary<int,double>
        {
            List<string> listLabel = new List<string>();
            int firstValue = 0;
            series.Title = seriesName;

            if (!addEmpty && list.Count <= 0)
                return false;

            series.Values = new ChartValues<double>();
            for (int i = 0; i < (TrainingSideToolViewModel.Get().timeRecorded / getCorrectTime()) + 1; i++) // Chart Initializing. We put all values at 0 and we create labels for X-axis
            {
                int val1 = i * (getCorrectTime() / 1000);
                int val2 = val1 + (getCorrectTime() / 1000) - 1;
                if (val2 - val1 == 0) // if the sessions is lower than 10 secs
                {
                    listLabel.Add((val1 + 1).ToString() + "sec");
                    series.Values.Add((double)0);
                }
                else
                {
                    listLabel.Add(convertSecondtoMinute(val1) + "--" + convertSecondtoMinute(val2));
                    series.Values.Add((double)0);
                }
            }

            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++) //we add data in serie
                {
                    int currentValue = list.Keys.ElementAt(i);
                    
                    int indice = currentValue / getCorrectTime();
                    if (indice < series.Values.Count)
                    {
                        series.Values[indice] = list.Values.ElementAt(i);
                        firstValue = currentValue;
                    }
                }
            }
            chart.listSeries.Add(series);
            if(totalValue.Count()>0)
            {
                chart.listTotalValue.Add(totalValue + getSumSeries(series));
            }
            chart.Labels = listLabel;
            chart.XTitle = "Time";
            if (list.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Create an empty chart
        /// </summary>
        /// <param name="name">the string will be appear </param>
        /// <returns>the GraphEmpty</returns>
        public static GraphEmpty createEmptyGraph(string name)
        {
            GraphEmpty gr = new GraphEmpty();
            gr.title = name;
            return gr;
        }
        #endregion
    }
}
