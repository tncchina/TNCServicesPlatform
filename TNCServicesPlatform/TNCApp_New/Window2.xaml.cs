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
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            Continuation.IsChecked = false;
            Online_status_checkbox.IsChecked = true;
        }

        private void W2Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Continuation_Checked(object sender, RoutedEventArgs e)
        {
            Continuation.IsChecked = true;

        }

        private void Continuation_Unchecked(object sender, RoutedEventArgs e)
        {
            Continuation.IsChecked = false;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Online_status_checkbox.IsChecked = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Online_status_checkbox.IsChecked = true;
        }

    }
}
