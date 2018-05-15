using LiveCharts;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Interface for each new type of charts
    /// </summary>
    /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    public interface IGraph
    {
        string title { get; set; }
        string subTitle {get;set;}

        List<string> Labels { get; set; }
        string XTitle { get; set; }
        string YTitle { get; set; }
        List<string> listTotalValue { get; set; }
        List<GraphSaves> lseries { get; set; }
        [XmlIgnore]
        SeriesCollection listSeries { get; set; }

        void copySeriesChartTolSeries();
        void copylSeriesTolistSeries();
    }
}
