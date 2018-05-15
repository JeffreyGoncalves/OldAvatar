using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Class to modelize a Cartesian Chart
    /// It's possible to add on its chart: ColumnSeries, RowSeries, LineSeries and other
    /// </summary>
    /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    [Serializable()]
    public class CartesianGraph : IGraph
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
        /// <summary>
        /// Corresponding of the X values
        /// </summary>
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
        /// <summary>
        /// Total of each series
        /// </summary>
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
        /// <summary>
        /// Only use to save char and series because a SeriesCollection isn't serializable
        /// </summary>
        public List<GraphSaves> lseries
        {
            get { return _lseries; }
            set { _lseries = value; }
        }
        #endregion

        #region contructor
        public CartesianGraph()
        {
            Labels = new List<string>();
            listTotalValue = new List<string>();
            listSeries = new SeriesCollection();
            lseries = new List<GraphSaves>();
        }
        #endregion

        #region methods
        /// <summary>
        /// Method to call before saving a CartesianGraph
        /// </summary>
        public void copySeriesChartTolSeries()
        {
            lseries.Clear();
            foreach (Series serie in listSeries)
            {
                GraphSaves gr = new GraphSaves();
                gr.Series.AddRange((ChartValues<double>)serie.Values);
                if (serie.GetType() == typeof(ColumnSeries))
                    gr.seriesType = "Column";
                else if (serie.GetType() == typeof(LineSeries))
                    gr.seriesType = "Line";
                else if (serie.GetType() == typeof(RowSeries))
                    gr.seriesType = "Row";
                gr.seriesTitle = serie.Title;
                lseries.Add(gr);
            }
        }

        /// <summary>
        /// Method to call after loading a CartesianGraph
        /// </summary>
        public void copylSeriesTolistSeries()
        {
            listSeries.Clear();
            foreach (GraphSaves grsaves in lseries)
            {
                Series serie = null;
                switch (grsaves.seriesType)
                {
                    case "Column":
                        serie = new ColumnSeries();
                        break;
                    case "Line":
                        serie = new LineSeries();
                        break;
                    case "Row":
                        serie = new RowSeries();
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
