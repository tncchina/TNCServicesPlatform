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
using LiveCharts.Wpf;

namespace TNCApp_New
{
    /// <summary>
    /// Interaction logic for DataVisualization.xaml
    /// </summary>
    public partial class DataVisualization : Window
    {
        //string species,string amount
        public DataVisualization(IDictionary<string, int> dict)
        {
            InitializeComponent();
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


            Labels = ValueY.ToArray();
            Formatter = value => value.ToString("N");

            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
    }
    }
