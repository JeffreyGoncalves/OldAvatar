using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LecturerTrainer.Model
{
    /// <summary>
     /// Class to modelize a Pie Chart
     /// It's possible to add on its chart: PieSeries
     /// More comments are available in the CartesianGraph class
     /// </summary>
     /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    [Serializable]
    public class PieGraph : IGraph
    {
        #region members
        private string _title;
        public string title
        {
            get { return _title; }
            set { _title = value; }
        }

        private string _subTitle;
        public string subTitle
        {
            get { return _subTitle; }
            set { _subTitle = value; }
        }

        private List<string> _Labels;
        public List<string> Labels
        {
            get { return _Labels; }
            set { _Labels = value; }
        }

        private string _XTitle;
        public string XTitle
        {
            get { return _XTitle; }
            set { _XTitle = value; }
        }

        private string _YTitle;
        public string YTitle
        {
            get { return _YTitle; }
            set { _YTitle = value; }
        }

        private List<string> _listTotalValue;
        public List<string> listTotalValue
        {
            get { return _listTotalValue; }
            set { _listTotalValue = value; }
        }

        [XmlIgnore]
        private SeriesCollection _listSeries;
        [XmlIgnore]
        public SeriesCollection listSeries
        {
            get { return _listSeries; }
            set { _listSeries = value; }
        }

        private List<GraphSaves> _lseries;
        public List<GraphSaves> lseries
        {
            get { return _lseries; }
            set { _lseries = value; }
        }
        #endregion

        #region constructor
        public PieGraph()
        {
            Labels = new List<string>();
            listTotalValue = new List<string>();
            listSeries = new SeriesCollection();
            lseries = new List<GraphSaves>();
        }
        #endregion

        #region methods
        /// <summary>
        /// Method to call before saving a PieGraph
        /// </summary>
        public void copySeriesChartTolSeries()
        {
            foreach (Series serie in listSeries)
            {
                GraphSaves gr = new GraphSaves();
                gr.Series.AddRange((ChartValues<double>)serie.Values);
                if (serie.GetType() == typeof(PieSeries))
                    gr.seriesType = "Pie";
                gr.seriesTitle = serie.Title;
                lseries.Add(gr);
            }
        }

        /// <summary>
        /// Method to call after loading a PieGraph
        /// </summary>
        public void copylSeriesTolistSeries()
        {
            listSeries.Clear();
            foreach (GraphSaves grsaves in lseries)
            {
                Series serie = null;
                switch (grsaves.seriesType)
                {
                    case "Pie":
                        serie = new ColumnSeries();
                        break;
                }
                if (serie != null)
                {
                    serie.Values = grsaves.Series;
                    serie.Title = grsaves.seriesTitle;
                    listSeries.Add(serie);
                }
            }
        }
        #endregion
    }
}
