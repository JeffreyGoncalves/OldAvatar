using LiveCharts.Wpf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Logique d'interaction pour CustomersLegend.xaml
    /// </summary>
    public partial class CustomersLegend : UserControl, IChartLegend
    {
        private List<SeriesViewModel> _series;

        /// <summary>
        /// the instance of the class 
        /// </summary>
        private static CustomersLegend instance = null;

        public CustomersLegend()
        {
            InitializeComponent();

            DataContext = this;
            instance = this;
        }

        public List<SeriesViewModel> Series
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged("Series");
            }
        }

        public static CustomersLegend Get()
        {
            if (instance == null)
            {
                instance = new CustomersLegend();
            }
            return instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
