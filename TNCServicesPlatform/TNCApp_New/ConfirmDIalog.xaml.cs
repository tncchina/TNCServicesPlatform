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

namespace TNCApp_New
{
    /// <summary>
    /// Interaction logic for ConfirmDIalog.xaml
    /// </summary>
    public partial class ConfirmDIalog : Window
    {
        public int Path { get; set; }
        public ConfirmDIalog(List<string> confirmList)
        {
            InitializeComponent();
            this.ConfrimList.ItemsSource = confirmList;
        }

        private void ConfirmB_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConfrimList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Path = (sender as ListBox).SelectedIndex;
        }



        /*        private void ConfrimList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
                {
                    Path = this.ConfrimList.SelectedItems.ToString();
                    this.Close();
                }

                private void ConfrimList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
                {
                    Path = this.ConfrimList.SelectedItems.ToString();
                    this.Close();
                }

                private void ConfrimList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
                {
                    Path = this.ConfrimList.SelectedItems.ToString();
                    this.Close();
                }*/
    }
}
