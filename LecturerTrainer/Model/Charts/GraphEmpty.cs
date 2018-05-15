using LiveCharts;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Class to modelize a Graph empty
    /// On the screen, there will be only on sentence : the title
    /// </summary>
    /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    [Serializable]
    public class GraphEmpty : IGraph
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
        public GraphEmpty()
        {
            Labels = new List<string>();
            listTotalValue = new List<string>();
            listSeries = new SeriesCollection();
            lseries = new List<GraphSaves>();
        }
        #endregion

        #region methods
        public void copySeriesChartTolSeries()
        {
        }

        public void copylSeriesTolistSeries()
        {
        }
        #endregion
    }
}
