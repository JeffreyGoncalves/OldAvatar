using LiveCharts;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LecturerTrainer.Model
{
    [Serializable]
    class NumberGraph : IGraph
    {
        private int _number;
        public int number
        {
            get { return _number; }
            set { _number = value; }
        }

        private string _legend;
        public string legend
        {
            get { return _legend; }
            set { _legend = value; }
        }

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

        public NumberGraph()
        {
            Labels = new List<string>();
            listTotalValue = new List<string>();
            listSeries = new SeriesCollection();
            lseries = new List<GraphSaves>();
        }

        public void copySeriesChartTolSeries()
        {
        }

        public void copylSeriesTolistSeries()
        {
        }

    }
}
