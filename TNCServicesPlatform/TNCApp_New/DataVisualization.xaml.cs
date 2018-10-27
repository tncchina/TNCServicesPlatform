using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CNTK;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;

namespace TNCApp_New
{
    /// <summary>
    /// Interaction logic for DataVisualization.xaml
    /// </summary>
    public partial class DataVisualization : Window
    {
        //string species,string amount
        public DataVisualization(IDictionary<string, int> dict, IDictionary<string,int>dictRatio )
        {

            InitializeComponent();

            PointLabel = chartPoint =>
                string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            DataContext = this;
            var ValueX = new ChartValues<int>();
            var ValueY = new List<String>();

            foreach (KeyValuePair<string, int> entry in dict)
            {
                ValueX.Add(entry.Value);
                ValueY.Add(entry.Key);
                // do something with entry.Value or entry.Key
            }

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Values = ValueX
                }
            };
            SeriesCollection1 = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "correct",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(dictRatio["correct"]) },
                    DataLabels = true,
                    LabelPoint = PointLabel
                },
                new PieSeries
                {
                    Title = "wrong",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(dictRatio["wrong"]) },
                    DataLabels = true,
                    LabelPoint = PointLabel
                },

            };

            Labels = ValueY.ToArray();
            Formatter = value => value.ToString("N");

            DataContext = this;
        }
        public Func<ChartPoint, string> PointLabel { get; set; }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }
        public SeriesCollection SeriesCollection { get; set; }
        public SeriesCollection SeriesCollection1 { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
    }
    }
