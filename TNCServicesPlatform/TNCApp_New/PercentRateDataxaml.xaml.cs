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
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace TNCApp_New
{
    /// <summary>
    /// Interaction logic for PercentRateDataxaml.xaml
    /// </summary>
    public partial class PercentRateDataxaml : Window
    {
        public PercentRateDataxaml(IDictionary<string, int> dictRatio)
        {
            InitializeComponent();
            PointLabel = chartPoint =>
                string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
            SeriesCollection = new SeriesCollection
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

            //adding values or series will update and animate the chart automatically
            //SeriesCollection.Add(new PieSeries());
            //SeriesCollection[0].Values.Add(5);

            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public Func<ChartPoint, string> PointLabel { get; set; }
    }


}
