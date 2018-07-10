using LecturerTrainer.Model;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace LecturerTrainer.ViewModel
{
    /// <summary>
    /// Class managing the charts show
    /// </summary>
    /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    public class ResultsViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Enumeration which associate ValueCbxChoice to a recording
        /// </summary>
        private enum ValueCbx
        {
            Hips = 0,
            LeftHand = 1,
            LeftKnee = 2,
            LeftShoulder = 3,
            RightHand = 4,
            RightKnee = 5,
            RightShoulder = 6,
            HandsJoined = 7,
            ArmsCrossed = 8,
            Emotions = 9,
            LookDir = 10,
            WordPMin = 11
        }

        #region attributs
        private bool[] ValueCheckBoxChoice; // correspond to the user choice with checkbox in the window : ChoiceResultView

        private ResultParts _rpVoice;
        public ResultParts RpVoice
        {
            get { return _rpVoice; }
            set { _rpVoice = value; }
        }

        private ResultParts _rpAgitation;
        public ResultParts RpAgitation
        {
            get { return _rpAgitation; }
            set { _rpAgitation = value; }
        }

        private ResultParts _rpArmsMotion;
        public ResultParts RpArmsMotion
        {
            get { return _rpArmsMotion; }
            set { _rpArmsMotion = value; }
        }

        private ResultParts _rpFace;
        public ResultParts RpFace
        {
            get { return _rpFace; }
            set { _rpFace = value; }
        }

        private ObservableCollection<ResultParts> _results;
        public ObservableCollection<ResultParts> Results
        {
            get { return _results; }
            set { _results = value; }
        }
        #endregion

        #region static attributs
        /// <summary>
        /// Singleton patern : the single instance of the class 
        /// </summary>
        private static ResultsViewModel instance = null;
        #endregion

        #region constructor and accessor
        /// <summary>
        /// Singleton patern : Constructor used to save all the data in a file. 
        /// </summary>
        private ResultsViewModel()
        {
            Results = new ObservableCollection<ResultParts>();
            RpVoice = new ResultParts("Results of the part \"Voice\"");
            RpAgitation = new ResultParts("Results of the Agitation part");
            RpArmsMotion = new ResultParts("Results of the Arms Motion part");
            RpFace = new ResultParts("Results of the part \"Face\"");

            ValueCheckBoxChoice = new bool[20];
            for (int i = 0; i < 20; i++)
            {
                ValueCheckBoxChoice[i] = true;
            }

            Chart.Colors = new List<Color>
            {
                (Color)Application.Current.Resources["ColorGraph1"],(Color)Application.Current.Resources["ColorGraph2"]
            };
        }

        /// <summary>
        /// Singleton Pattern
        /// </summary>
        public static ResultsViewModel Get()
        {
            if (instance == null)
            {
                instance = new ResultsViewModel();
            }
            return instance;
        }

        #endregion

        #region Statistics methods

        public void checkBoxUpdates(List<bool> lchoice)
        {
            for (int i = 0; i < lchoice.Count; i++)
            {
                ValueCheckBoxChoice[i] = lchoice[i];
            }
        }

        public void checkBoxSingleUpdate(int value, bool enable)
        {
            ValueCheckBoxChoice[value] = enable;
        }

        /// <summary>
        /// Add charts of the list passed as parameters to the Agitation part
        /// </summary>
        /// <param name="listAgit"></param>
        public void getAgitationStatistics(List<IGraph> listAgit)
        {
            bool hands = false, shoulder = false, knee = false;
            ObservableCollection<IGraph> ItemsEmpty = new ObservableCollection<IGraph>();
            if (listAgit != null)
            {
                foreach (IGraph p in listAgit)
                {
                    if (p.title.ToLower().Contains("hips") && getValueCheckBoxChoice(ValueCbx.Hips))
                    {
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    }
                    else if (p.title.ToLower().Contains("hands") && getValueCheckBoxChoice(ValueCbx.LeftHand) && getValueCheckBoxChoice(ValueCbx.RightHand))
                    {
                        hands = true;
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    }
                    else if (p.title.ToLower().Contains("left hand") && getValueCheckBoxChoice(ValueCbx.LeftHand) && !hands)
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    else if (p.title.ToLower().Contains("right hand") && getValueCheckBoxChoice(ValueCbx.RightHand) && !hands)
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    else if (p.title.ToLower().Contains("shoulders") && getValueCheckBoxChoice(ValueCbx.LeftShoulder) && getValueCheckBoxChoice(ValueCbx.RightShoulder))
                    {
                        shoulder = true;
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    }
                    else if (p.title.ToLower().Contains("left shoulder") && getValueCheckBoxChoice(ValueCbx.LeftShoulder) && !shoulder)
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    else if (p.title.ToLower().Contains("right shoulder") && getValueCheckBoxChoice(ValueCbx.RightShoulder) && !shoulder)
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    else if (p.title.ToLower().Contains("knees") && getValueCheckBoxChoice(ValueCbx.LeftKnee) && getValueCheckBoxChoice(ValueCbx.RightKnee))
                    {
                        knee = true;
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    }
                    else if (p.title.ToLower().Contains("left knee") && getValueCheckBoxChoice(ValueCbx.LeftKnee) && !knee)
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                    else if (p.title.ToLower().Contains("right knee") && getValueCheckBoxChoice(ValueCbx.RightKnee) && !knee)
                        if (p.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(p);
                        else
                            RpAgitation.Items.Add(p);
                }
                foreach (IGraph g in ItemsEmpty)
                    RpAgitation.Items.Add(g);
            }
        }

        /// <summary>
        /// Add charts of the list passed as parameters to the Arms Motion part
        /// </summary>
        /// <param name="grArmsMot"></param>
        public void getArmsMotion(List<IGraph> grArmsMot)
        {
            ObservableCollection<IGraph> ItemsEmpty = new ObservableCollection<IGraph>();
            foreach (IGraph chart in grArmsMot)
            {
                if ((chart.title.ToLower().Contains("hand") && getValueCheckBoxChoice(ValueCbx.HandsJoined)) ||
                    (chart.title.ToLower().Contains("arms") && getValueCheckBoxChoice(ValueCbx.ArmsCrossed)))
                {
                    if (chart.GetType() == typeof(GraphEmpty))
                        ItemsEmpty.Add(chart);
                    else
                        RpArmsMotion.Items.Add(chart);
                }
            }
            foreach (IGraph graph in ItemsEmpty)
            {
                RpArmsMotion.Items.Add(graph);
            }
        }

        /// <summary>
        /// Returns the boolean associate to your ValueCheckBoxChoice and your enumerate (recording)
        /// </summary>
        /// <param name="val"></param>
        private bool getValueCheckBoxChoice(ValueCbx val)
        {
            return ValueCheckBoxChoice[(int)val];
        }

        /// <summary>
        /// Add charts of the list passed as parametes to the Face Statistics
        /// </summary>
        public void getFaceStatistics(IGraph grEmotion, IGraph grLookDirec)
        {

            ObservableCollection<IGraph> ItemsEmpty = new ObservableCollection<IGraph>();
            // If face emotions is checked
            if (getValueCheckBoxChoice(ValueCbx.Emotions))
            {
                if (grEmotion.GetType() == typeof(GraphEmpty))
                {
                    GraphEmpty grEmpty = new GraphEmpty();
                    grEmpty.title = "No emotions was detected";
                    ItemsEmpty.Add(grEmpty);
                }
                else
                {
                    RpFace.Items.Add(grEmotion);
                }
            }
            // If looking direction is check
            if (getValueCheckBoxChoice(ValueCbx.LookDir))
            {
                //if (grLookDirec.GetType() == typeof(GraphEmpty))
                //{
                GraphEmpty grEmpty = new GraphEmpty();
                grEmpty.title = "No looking direction was detected";
                ItemsEmpty.Add(grEmpty);
                //}
                //else
                //{
                //    RpFace.Items.Add(grEmotion);
                //}
            }

            foreach (IGraph graph in ItemsEmpty)
                RpFace.Items.Add(graph);
        }

        /// <summary>
        /// Add charts of the list passed as parameters to the Voice part
        /// </summary>
        public void getVoiceStatistics(List<IGraph> listVoice)
        {
            ObservableCollection<IGraph> ItemsEmpty = new ObservableCollection<IGraph>();
            if (listVoice != null)
            {
                foreach (IGraph chart in listVoice)
                {
                    if (chart.title.ToLower().Contains("words per minute") && ValueCheckBoxChoice[11])
                    {
                        if (chart.GetType() == typeof(GraphEmpty))
                            ItemsEmpty.Add(chart);
                        else
                            RpVoice.Items.Add(chart);
                    }
                }
            }
            foreach (IGraph g in ItemsEmpty)
                RpVoice.Items.Add(g);
        }
        #endregion

        /// <summary>
        /// Allow to add ResultsParts to the window
        /// </summary>
        public void addResultsPartToView()
        {
            if (_rpAgitation.Items.Count > 0)
                this.Results.Add(_rpAgitation);
            if (_rpArmsMotion.Items.Count > 0)
                this.Results.Add(_rpArmsMotion);
            if (_rpFace.Items.Count > 0)
                this.Results.Add(_rpFace);
            if (_rpVoice.Items.Count > 0)
                this.Results.Add(_rpVoice);
        }

        #region Save and Load Methods with file(s)
        /// <summary>
        /// Save all charts in an unique file (named charts.xml) in the path in parameters
        /// </summary>
        /// <param name="path">the path to save the file</param>
        public void SaveGraph(string path)
        {
            /**Each chart is recording is a separate file with the XmlSerializer**/
            int i = 0;
            foreach (IGraph graph in RpAgitation.Items)
            {
                graph.copySeriesChartTolSeries();
                XmlSerializer SerializerObj = new XmlSerializer(graph.GetType());

                TextWriter WriteFileStream = new StreamWriter(path + "chart" + i + ".xml", true);
                WriteFileStream.WriteLine("Agitation");
                SerializerObj.Serialize(WriteFileStream, graph);
                WriteFileStream.Close();
                i++;
            }
            RpAgitation.Items.Clear();

            foreach (IGraph graph in RpArmsMotion.Items)
            {
                graph.copySeriesChartTolSeries();
                XmlSerializer SerializerObj = new XmlSerializer(graph.GetType());
                TextWriter WriteFileStream = new StreamWriter(path + "chart" + i + ".xml", true);
                WriteFileStream.WriteLine("ArmsMotion");
                SerializerObj.Serialize(WriteFileStream, graph);
                WriteFileStream.Close();
                i++;
            }
            RpArmsMotion.Items.Clear();

            foreach (IGraph graph in RpFace.Items)
            {
                if (graph != null)
                {
                    graph.copySeriesChartTolSeries();
                    XmlSerializer SerializerObj = new XmlSerializer(graph.GetType());

                    TextWriter WriteFileStream = new StreamWriter(path + "chart" + i + ".xml", true);
                    WriteFileStream.WriteLine("Face");
                    SerializerObj.Serialize(WriteFileStream, graph);
                    WriteFileStream.Close();
                    i++;
                }
            }
            RpFace.Items.Clear();

            foreach (IGraph graph in RpVoice.Items)
            {
                graph.copySeriesChartTolSeries();
                XmlSerializer SerializerObj = new XmlSerializer(graph.GetType());

                TextWriter WriteFileStream = new StreamWriter(path + "chart" + i + ".xml", true);
                WriteFileStream.WriteLine("Voice");
                SerializerObj.Serialize(WriteFileStream, graph);
                WriteFileStream.Close();
                i++;
            }
            RpVoice.Items.Clear();

            /**All files will be put together into a global file**/
            TextWriter WriteFileStream2 = new StreamWriter(path + "charts.xml", true);

            for (int j = 0; j < i; j++)
            {
                TextReader ReadFileStream = new StreamReader(path + "chart" + j + ".xml");

                String dd = ReadFileStream.ReadLine();
                while (dd != null)
                {
                    WriteFileStream2.WriteLine(dd);
                    dd = ReadFileStream.ReadLine();
                }
                ReadFileStream.Close();
            }
            WriteFileStream2.Close();
            
            string patern = "[0-9]+\\.xml$";

            foreach (string pathtemp in Directory.EnumerateFiles(path))
            {
                if (Regex.IsMatch(pathtemp, patern))
                {
                    File.Delete(pathtemp); //we delete the temporary files
                }
            }
        }

        /// <summary>
        /// Allow to load charts from the file in the path
        /// </summary>
        /// <param name="path">the path of the file</param>
        /// <param name="add">if we add directly the char in the ResultsPart</param>
        /// <param name="listChart">we add the chart in the list</param>
        /// <param name="listDate">we add the date in the list</param>
        /// <param name="date">the date of the chart</param>
        public void LoadGraph(string path, bool add, List<List<IGraph>> listChart, List<List<string>> listDate, string date)
        {
            TextReader ReadFileStream = new StreamReader(path + "charts.xml");
            string type = null;
            string line = "init";
            int num = 0;
            while (!string.IsNullOrEmpty(line)) // we recreate the file chart.xml with one chart per file
            {
                FileStream fs = File.Create(path + "chart" + num + ".xml");
                TextWriter WriteFileStream = new StreamWriter(fs);
                type = ReadFileStream.ReadLine();
                string patern = ReadFileStream.ReadLine();
                WriteFileStream.WriteLine(patern);
                line = ReadFileStream.ReadLine();
                if (string.IsNullOrEmpty(line)) // if it's the end of the file
                {
                    WriteFileStream.Close();
                    File.Delete(path + "chart" + num + ".xml");
                    break;
                }

                string[] tab = line.Split(' ');
                string name = tab[0].Substring(1);
                string final = tab[0].Insert(1, "/");
                final = final.Insert(final.Length, ">");


                while (line != final && !string.IsNullOrEmpty(line))
                {
                    WriteFileStream.WriteLine(line);
                    line = ReadFileStream.ReadLine();

                }
                WriteFileStream.WriteLine(line);
                WriteFileStream.Close();

                FileStream ReadFileStream2 = new FileStream(path + "chart" + num + ".xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                XmlSerializer SerializerObj = null;
                IGraph graph = null;

                switch (name) // we deserialize the chart and we create the object corresponding
                {
                    case "CartesianGraph":
                        SerializerObj = new XmlSerializer(typeof(CartesianGraph));
                        graph = (CartesianGraph)SerializerObj.Deserialize(ReadFileStream2);
                        break;
                    case "PieGraph":
                        SerializerObj = new XmlSerializer(typeof(PieGraph));
                        graph = (PieGraph)SerializerObj.Deserialize(ReadFileStream2);
                        break;
                    case "GraphEmpty":
                        SerializerObj = new XmlSerializer(typeof(GraphEmpty));
                        graph = (GraphEmpty)SerializerObj.Deserialize(ReadFileStream2);
                        break;
                    default:
                        break;
                }
                fs.Close();
                ReadFileStream2.Close();
                num++;

                if (graph != null)
                {
                    graph.copylSeriesTolistSeries();
                    switch (type) // in function of the type of the chart
                    {
                        case "Agitation":
                            listChart[0].Add(graph);
                            listDate[0].Add(date);
                            if (add)
                                RpAgitation.Items.Add(graph);
                            break;
                        case "ArmsMotion":
                            listChart[1].Add(graph);
                            listDate[1].Add(date);
                            if (add)
                                RpArmsMotion.Items.Add(graph);
                            break;
                        case "Face":
                            listChart[2].Add(graph);
                            listDate[2].Add(date);
                            if (add)
                                RpFace.Items.Add(graph);
                            break;
                        case "Voice":
                            listChart[3].Add(graph);
                            listDate[3].Add(date);
                            if (add)
                                RpVoice.Items.Add(graph);
                            break;
                    }
                }
            }
            for (int i = 0; i < num; i++)
            {
                if (File.Exists(path + "chart" + i + ".xml"))
                {
                    File.Delete(path + "chart" + i + ".xml"); // we delete temporary files
                }
            }

            ReadFileStream.Close();
        }

        /// <summary>
        /// Allow to obtain the title of each chart in the file charts.xml in the folder of the path
        /// </summary>
        /// <returns>every name of chart in the file</returns>
        public HashSet<string> getNamesGraphs(string path)
        {
            HashSet<string> list = new HashSet<string>();
            TextReader ReadFileStream = null;
            try
            {
                ReadFileStream = new StreamReader(path + "charts.xml");
                string line = ReadFileStream.ReadLine();

                string paternBegin = "\\s*<title>"; // the marker "title" allows to retrieve the title of the chart
                string paternEnd = "</title>";

                while (!string.IsNullOrEmpty(line))
                {
                    if (Regex.IsMatch(line, paternBegin))
                    {
                        string name = "";
                        name = Regex.Replace(line, paternBegin, "");
                        name = Regex.Replace(name, paternEnd, "");
                        list.Add(name);
                    }

                    line = ReadFileStream.ReadLine();
                }
            }
            catch (Exception)
            {
                throw new IOException();
            }

            finally
            {
                if (ReadFileStream != null)
                {
                    ReadFileStream.Close();
                }
            }

            return list;
        }

        /// <summary>
        /// Allow to load only one file and to display chart like a display just after a record
        /// </summary>
        /// <param name="listPath">the path of the file to analysis</param>
        /// <param name="listChart">the list of the list of chart in function of the data category</param>
        /// <param name="listDate">the list of the list of date for each chart in the listChart</param>
        public void loadOneSessionAnalysis(List<string> listPath, List<List<IGraph>> listChart, List<List<string>> listDate)
        {
            LoadGraph(listPath[0] + "/", false, listChart, listDate, "");

            if (listChart[0] != null && listChart[0].Count > 0)
            {
                getAgitationStatistics(listChart[0]);
            }
            if (listChart[1] != null && listChart[1].Count > 0)
            {
                getArmsMotion(listChart[1]);
            }

            if (listChart[2] != null && listChart[2].Count > 0)
            {
                //getFaceStatistics(listChart[2][0], listChart[2][1]);
            }

            if (listChart[3] != null && listChart[3].Count > 0)
            {
                getVoiceStatistics(listChart[3]);
            }

        }

        /// <summary>
        /// Allow to load every file "charts.xml" included in the listpath
        /// </summary>
        /// <param name="listPath">the list of each path to load file</param>
        public void loadManyCharts(List<string> listPath)
        {
            List<List<IGraph>> listChart = new List<List<IGraph>>();

            listChart.Add(new List<IGraph>()); //0: for the agitation
            listChart.Add(new List<IGraph>()); //1: for the arms motion
            listChart.Add(new List<IGraph>()); //2: for the face
            listChart.Add(new List<IGraph>()); //3: for the audio

            List<List<string>> listDate = new List<List<string>>();
            listDate.Add(new List<string>()); //0: for the agitation
            listDate.Add(new List<string>()); //1: for the arms motion
            listDate.Add(new List<string>()); //2: for the face
            listDate.Add(new List<string>()); //3: for the audio

            if (listPath.Count == 1) // if there is only one file
            {
                loadOneSessionAnalysis(listPath, listChart, listDate);
                return;
            }
            foreach (string path in listPath)
            {
                string[] tab = path.Split('\\');
                string[] name = tab[tab.Length - 1].Split('_');
                string pattern = "[0-9]{4}_[0-9]{1,2}_[0-9]{1,2}(_[0-9]+)*";
                Regex searchRegEx = new Regex(pattern);
                Match correspond = searchRegEx.Match(tab[tab.Length - 1]);
                string date = correspond.Groups[0].Value;
                name = date.Split('_');
                date = name[02] + "-" + name[1] + "-" + name[0]; // we retrieve the date from the name of the folder
                if (name.Length > 3)
                {
                    date += "_" + name[3]; // if there is more than one folder for the same day
                }
                LoadGraph(path + "/", false, listChart, listDate, date);
            }

            #region Agitation parts

            CartesianGraph graphlineHips = new CartesianGraph(); // we create the chart for the Hips agitation
            graphlineHips.title = "Hips Agitation between " + listDate[0][0] + " and " + listDate[0][listDate[0].Count - 1];
            graphlineHips.subTitle = "The value represents an average of the agitation per minute";

            CartesianGraph graphlineLKnee = new CartesianGraph(); // we create the chart for the Knees agitation
            graphlineLKnee.title = "Knees Agitation between " + listDate[0][0] + " and " + listDate[0][listDate[0].Count - 1];
            graphlineLKnee.subTitle = "The value represents an average of the agitation per minute";

            CartesianGraph graphlineLHand = new CartesianGraph(); // we create the chart for the Hands agitation
            graphlineLHand.title = "Hands Agitation between " + listDate[0][0] + " and " + listDate[0][listDate[0].Count - 1];
            graphlineLHand.subTitle = "The value represents an average of the agitation per minute";

            CartesianGraph graphlineLShoulder = new CartesianGraph(); // we create the chart for the Shoulders agitation
            graphlineLShoulder.title = "Shoulders Agitation between " + listDate[0][0] + " and " + listDate[0][listDate[0].Count - 1];
            graphlineLShoulder.subTitle = "The value represents an average of the agitation per minute";

            int i = 0;
            // we create each series to add values and to add in the corresponding chart
            LineSeries lship = new LineSeries();
            lship.Values = new ChartValues<double>();
            lship.Title = "Hips";

            LineSeries lsl = new LineSeries();
            lsl.Title = "Left Hand";
            lsl.Values = new ChartValues<double>();
            LineSeries lsr = new LineSeries();
            lsr.Title = "Right Hand";
            lsr.Values = new ChartValues<double>();

            LineSeries lskl = new LineSeries();
            lskl.Title = "Left Knee";
            lskl.Values = new ChartValues<double>();
            LineSeries lskr = new LineSeries();
            lskr.Values = new ChartValues<double>();
            lskr.Title = "Right Knee";

            LineSeries lssl = new LineSeries();
            lssl.Title = "Left Shoulder";
            lssl.Values = new ChartValues<double>();
            LineSeries lssr = new LineSeries();
            lssr.Values = new ChartValues<double>();
            lssr.Title = "Right Shoulder";

            foreach (IGraph graph in listChart[0]) // we test the title of the chart
            {
                if (graph.title.ToLower().Contains("hips"))
                {
                    graphlineHips.Labels.Add(listDate[0][i]);

                    if (graph.GetType() == typeof(GraphEmpty))
                        lship.Values.Add((double)0);
                    else
                        lship.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                else if (graph.title.ToLower().Contains("hands"))
                {
                    graphlineLHand.Labels.Add(listDate[0][i]);

                    if (graph.GetType() == typeof(GraphEmpty))
                    {
                        lsl.Values.Add((double)0);
                        lsr.Values.Add((double)0);
                    }
                    else
                    {
                        lsl.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                        lsr.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[1].Values, graph.Labels.Last().ToString()));
                    }
                }
                else if (graph.title.ToLower().Contains("right hand"))
                {
                    graphlineLHand.Labels.Add(listDate[0][i]);
                    lsl.Values.Add((double)0);

                    if (graph.GetType() == typeof(GraphEmpty))
                        lsr.Values.Add((double)0);
                    else
                        lsr.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                else if (graph.title.ToLower().Contains("left hand"))
                {
                    graphlineLHand.Labels.Add(listDate[0][i]);
                    lsr.Values.Add((double)0);

                    if (graph.GetType() == typeof(GraphEmpty))
                        lsl.Values.Add((double)0);
                    else
                        lsl.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                else if (graph.title.ToLower().Contains("knees"))
                {
                    graphlineLKnee.Labels.Add(listDate[0][i]);

                    if (graph.GetType() == typeof(GraphEmpty))
                    {
                        lskl.Values.Add((double)0);
                        lskr.Values.Add((double)0);
                    }
                    else
                    {
                        lskl.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                        lskr.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[1].Values, graph.Labels.Last().ToString()));
                    }
                }
                else if (graph.title.ToLower().Contains("right knee"))
                {
                    graphlineLKnee.Labels.Add(listDate[0][i]);
                    lskl.Values.Add((double)0);
                    if (graph.GetType() == typeof(GraphEmpty))
                        lskr.Values.Add((double)0);
                    else
                        lskr.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                else if (graph.title.ToLower().Contains("left knee"))
                {
                    graphlineLKnee.Labels.Add(listDate[0][i]);
                    lskr.Values.Add((double)0);
                    if (graph.GetType() == typeof(GraphEmpty))
                        lskl.Values.Add((double)0);
                    else
                        lskl.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                else if (graph.title.ToLower().Contains("shoulders"))
                {
                    graphlineLShoulder.Labels.Add(listDate[0][i]);

                    if (graph.GetType() == typeof(GraphEmpty))
                    {
                        lssl.Values.Add((double)0);
                        lssr.Values.Add((double)0);
                    }
                    else
                    {
                        lssl.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                        lssr.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[1].Values, graph.Labels.Last().ToString()));
                    }
                }
                else if (graph.title.ToLower().Contains("right shoulder"))
                {
                    graphlineLShoulder.Labels.Add(listDate[0][i]);
                    lssl.Values.Add((double)0);
                    if (graph.GetType() == typeof(GraphEmpty))
                        lssr.Values.Add((double)0);
                    else
                        lssr.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                else if (graph.title.ToLower().Contains("left shoulder"))
                {
                    graphlineLShoulder.Labels.Add(listDate[0][i]);
                    lssr.Values.Add((double)0);
                    if (graph.GetType() == typeof(GraphEmpty))
                        lssl.Values.Add((double)0);
                    else
                        lssl.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                i++;
            }



            bool hand = false, knee = false, shoulder = false;

            //Here, we verify if the user check the checkbox in the window ChoiceResultView
            if (ValueCheckBoxChoice[0])
            {
                graphlineHips.listSeries.Add(lship);
                RpAgitation.Items.Add(AllDataChartNull(graphlineHips));
            }

            if (ValueCheckBoxChoice[1])
            {
                graphlineLHand.listSeries.Add(lsl);
                hand = true;
            }
            if (ValueCheckBoxChoice[4])
            {
                graphlineLHand.listSeries.Add(lsr);
                hand = true;
            }
            if (hand)
                RpAgitation.Items.Add(AllDataChartNull(graphlineLHand));

            if (ValueCheckBoxChoice[2])
            {
                graphlineLKnee.listSeries.Add(lskl);
                knee = true;
            }
            if (ValueCheckBoxChoice[5])
            {
                graphlineLKnee.listSeries.Add(lskr);
                knee = true;
            }
            if (knee)
                RpAgitation.Items.Add(AllDataChartNull(graphlineLKnee));

            if (ValueCheckBoxChoice[3])
            {
                graphlineLShoulder.listSeries.Add(lssl);
                shoulder = true;
            }
            if (ValueCheckBoxChoice[6])
            {
                graphlineLShoulder.listSeries.Add(lssr);
                shoulder = true;
            }
            if (shoulder)
                RpAgitation.Items.Add(AllDataChartNull(graphlineLShoulder));

            #endregion

            #region ArmsMotion part

            CartesianGraph graphlineHandJoin = new CartesianGraph();
            graphlineHandJoin.title = "Joined Hands between " + listDate[1][0] + " and " + listDate[1][listDate[1].Count - 1];
            graphlineHandJoin.subTitle = "The value represents an average of the agitation per minute";
            CartesianGraph graphlineArmsCrossed = new CartesianGraph();
            graphlineArmsCrossed.title = "Crossed Arms between " + listDate[1][0] + " and " + listDate[1][listDate[1].Count - 1];
            graphlineArmsCrossed.subTitle = "The value represents an average of the agitation per minute";

            LineSeries lshj = new LineSeries();
            lshj.Title = "Hands Joined";
            lshj.Values = new ChartValues<double>();
            LineSeries lsac = new LineSeries();
            lsac.Title = "Arms Crossed";
            lsac.Values = new ChartValues<double>();

            i = 0;
            foreach (IGraph graph in listChart[1])
            {
                if (graph.title.ToLower().Contains("hand"))
                {
                    graphlineHandJoin.Labels.Add(listDate[1][i]);
                    if (graph.GetType() == typeof(GraphEmpty))
                        lshj.Values.Add((double)0);
                    else
                        lshj.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                else if (graph.title.ToLower().Contains("arms"))
                {
                    graphlineArmsCrossed.Labels.Add(listDate[1][i]);
                    if (graph.GetType() == typeof(GraphEmpty))
                        lsac.Values.Add((double)0);
                    else
                        lsac.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                i++;
            }
            graphlineHandJoin.listSeries.Add(lshj);
            graphlineArmsCrossed.listSeries.Add(lsac);
            if (ValueCheckBoxChoice[7])
                RpArmsMotion.Items.Add(AllDataChartNull(graphlineHandJoin));
            if (ValueCheckBoxChoice[8])
                RpArmsMotion.Items.Add(AllDataChartNull(graphlineArmsCrossed));

            #endregion

            #region Face part
            CartesianGraph graphLineHappy = new CartesianGraph();
            graphLineHappy.title = "HappyEmotion between " + listDate[1][0] + " and " + listDate[1][listDate[1].Count - 1];
            graphLineHappy.subTitle = "The value represents an average of the percent of happy face during a record";

            LineSeries lineSeriesHappy = new LineSeries();
            lineSeriesHappy.Title = "Happy Emotion";
            lineSeriesHappy.Values = new ChartValues<double>();

            i = 0;
            foreach (IGraph graph in listChart[2])
            {
                if (graph.title.ToLower().Contains("emotion"))
                {
                    graphLineHappy.Labels.Add(listDate[2][i]);
                    if (graph.GetType() == typeof(GraphEmpty))
                        lineSeriesHappy.Values.Add((double)0);
                    else
                        lineSeriesHappy.Values.Add(averageInCharts((ChartValues<double>)graph.listSeries[0].Values, graph.Labels.Last().ToString()));
                }
                i++;
            }

            graphLineHappy.listSeries.Add(lineSeriesHappy);
            if (ValueCheckBoxChoice[9])
                RpFace.Items.Add(AllDataChartNull(graphLineHappy));

            #endregion

            #region audio part
            CartesianGraph graphlineAudion = new CartesianGraph();
            graphlineAudion.title = "Speed rate average between " + listDate[1][0] + " and " + listDate[1][listDate[1].Count - 1];
            graphlineAudion.subTitle = "The value represents an average of the speed rate per session";

            LineSeries lsaud = new LineSeries();
            lsaud.Title = "Speech rate";
            lsaud.Values = new ChartValues<double>();
            i = 0;
            foreach (IGraph graph in listChart[3])
            {
                if (graph.title.ToLower().Contains("speech") && graph.title.ToLower().Contains("rate"))
                {
                    graphlineAudion.Labels.Add(listDate[3][i]);
                    if (graph.GetType() == typeof(GraphEmpty))
                        lsaud.Values.Add((double)0);
                    else
                        lsaud.Values.Add(((ChartValues<double>)graph.listSeries[0].Values).Average());
                }
                i++;
            }
            graphlineAudion.listSeries.Add(lsaud);
            if (ValueCheckBoxChoice[11])
                RpVoice.Items.Add(AllDataChartNull(graphlineAudion));
            #endregion
        }
        #endregion

        #region methods on a Chart
        /// <summary>
        /// Allo to verify if all the series of the chart are empty or not
        /// </summary>
        /// <param name="graph">the chart</param>
        /// <returns>the chart if ther is at least 1 series isn't null, an Empty chart if not</returns>
        public IGraph AllDataChartNull(IGraph graph)
        {
            bool isnull = true;
            if (graph.listSeries.Count > 0)
            {
                foreach (Series serie in graph.listSeries)
                {
                    if (serie.Values != null && serie.Values.Count > 0)
                    {
                        isnull = false;
                    }
                }
            }

            if (isnull)
            {
                GraphEmpty gr = new GraphEmpty();
                gr.title = "No " + graph.title;
                return gr;
            }
            else
            {
                return graph;
            }

        }

        /// <summary>
        /// Allow to return a average for 1 minute of a series ( used to display chart with several days)
        /// </summary>
        /// <param name="data">the data of the series</param>
        /// <param name="lastv">the time of the record</param>
        /// <returns>the average</returns>
        public double averageInCharts(ChartValues<double> data, string lastv)
        {
            double somme = 0;
            foreach (double obj in data)
            {
                int cast = (int)obj;
                if (cast > 0)
                {
                    somme += cast;
                }
            }
            string lastvalue = lastv;

            string pattern = "[0-9]+(:[0-9]{1,2})*"; // we retrieve the time of the record from the string
            Regex searchRegEx = new Regex(pattern);
            Match correspond = searchRegEx.Match(lastvalue);
            string date = correspond.Groups[0].Value;
            string[] value = date.Split(':');
            int[] v = new int[4];
            if (value != null)
            {
                if (value.Length == 2)
                {
                    v[0] = int.Parse(value[0]);
                    v[1] = int.Parse(value[1]);
                }
                else
                {
                    v[1] = int.Parse(value[0]);
                }
            }

            correspond = correspond.NextMatch();
            date = correspond.Groups[0].Value;
            if (date.Length > 0)
            {
                string[] value2 = date.Split(':');
                if (value2 != null)
                {
                    if (value2.Length == 2)
                    {
                        v[2] = int.Parse(value2[0]);
                        v[3] = int.Parse(value2[1]);
                    }
                    else
                    {
                        v[3] = int.Parse(value2[0]);
                    }
                }
            }

            int maxtime = v[2] * 60 + v[3]; //here, we have the time of the record
            if (maxtime <= 0)
            {
                return somme;
            }

            double val = somme / maxtime * 60f; //this is the average

            return Math.Round(val, 2);
        }
        #endregion

        #region customization method
        /// <summary>
        /// Allow to change the color of each series in every chart
        /// </summary>
        /// <param name="value">if it is the first series or the second series</param>
        public void changeColor(int value)
        {
            List<IGraph> list = new List<IGraph>();
            list.AddRange(RpAgitation.Items);
            list.AddRange(RpArmsMotion.Items);
            list.AddRange(RpFace.Items);
            list.AddRange(RpVoice.Items);

            foreach (IGraph chart in list)
            {
                if (value == 0 && chart.listSeries.Count > 0)
                {
                    Color color1 = Color.FromArgb(colorOne.Color.A, colorOne.Color.R, colorOne.Color.G, colorOne.Color.B);
                    if (chart.listSeries[value].GetType() == typeof(ColumnSeries))
                    {
                        ((Series)chart.listSeries[value]).Fill = colorOne;
                    }
                    else
                    {
                        ((Series)chart.listSeries[value]).Stroke = colorOne;
                        ((Series)chart.listSeries[value]).Fill = new SolidColorBrush(Color.FromArgb(50, color1.R, color1.G, color1.B));
                        CustomersLegend.Get().Series[0].Stroke = (SolidColorBrush)colorOne; //not used for now because the library doesn't implemented that
                    }
                    Application.Current.Resources["ColorGraph1"] = color1;
                }
                else if (value == 1 && chart.listSeries.Count > 1)
                {
                    Color color2 = Color.FromArgb(colorTwo.Color.A, colorTwo.Color.R, colorTwo.Color.G, colorTwo.Color.B);
                    if (chart.listSeries[value].GetType() == typeof(ColumnSeries))
                    {
                        ((Series)chart.listSeries[value]).Fill = colorTwo;
                    }
                    else
                    {
                        ((Series)chart.listSeries[value]).Stroke = colorTwo;
                        ((Series)chart.listSeries[value]).Fill = new SolidColorBrush(Color.FromArgb(50, color2.R, color2.G, color2.B));
                    }
                    Application.Current.Resources["ColorGraph2"] = color2;
                }

            }
            Main.session.fillPersoWithSpecial("ColorGraph1");
            Main.session.fillPersoWithSpecial("ColorGraph2");
        }
        #endregion

        #region property between code and XAML
        private SolidColorBrush colorOne = null;
        public SolidColorBrush ColorOne
        {
            get
            {
                return colorOne;
            }
            set
            {
                colorOne = value;
                NotifyPropertyChanged("ColorOne");
            }
        }

        private SolidColorBrush colorTwo = null;
        public SolidColorBrush ColorTwo
        {
            get
            {
                return colorTwo;
            }
            set
            {
                colorTwo = value;
                NotifyPropertyChanged("ColorTwo");
            }
        }



        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _seriesSwitched;
        public bool seriesSwitched
        {
            get
            {
                return _seriesSwitched;
            }
            set
            {
                if (_seriesSwitched != value)
                {
                    _seriesSwitched = value;
                    NotifyPropertyChanged("seriesSwitched");
                }
            }
        }
        #endregion
    }
}
