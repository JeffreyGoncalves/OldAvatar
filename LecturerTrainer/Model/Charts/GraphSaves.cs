using LiveCharts;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Class allowing to save a SeriesCollection (LiveChart) because it isn't serializable
    /// </summary>
    /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    public class GraphSaves
    {
        #region members
        private ChartValues<double> _Series;
        public ChartValues<double> Series
        {
            get { return _Series; }
            set { _Series = value; }
        }

        private string _seriesTitle;
        public string seriesTitle
        {
            get { return _seriesTitle; }
            set { _seriesTitle = value; }
        }

        private string _seriesType;
        /// <summary>
        /// a string which represents the type of serie: LineSeries, PieSeries, ...
        /// </summary>
        public string seriesType
        {
            get { return _seriesType; }
            set { _seriesType = value; }
        }
        #endregion

        public GraphSaves()
        {
            Series = new ChartValues<double>();
        }
    }
}
