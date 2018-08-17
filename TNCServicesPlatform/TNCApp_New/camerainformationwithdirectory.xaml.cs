using System;
using System.Collections.Generic;
using System.IO;
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
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace TNCApp_New
{
    /// <summary>
    /// Interaction logic for camerainformationwithdirectory.xaml
    /// </summary>
    public partial class camerainformationwithdirectory : Window
    {
        public List<string> CameraNumbers { get; set; }
        public List<string> CameraLocations { get; set; }
        public List<string> DateTimes { get; set; }
        public camerainformationwithdirectory(List<string> directorys)
        {
            InitializeComponent();
            CameraNumbers =new List<string>();
            CameraLocations = new List<string>();
            DateTimes = new List<string>();
            GenerateControls(directorys);
            
        }  
        
        public void GenerateControls(List<string> directorys)
        {
            StackName.Children.Add(new TextBlock()
            {
                Name = "folderNameTB",
                Text = "folderName" + ":",
                FontSize = 15

            });

            StackLocation.Children.Add(new TextBlock()
            {
                Name = "CameraLocationTB",
                Text = "CameraLocation" + ":",
                FontSize = 15

            });
            StackNumber.Children.Add(new TextBlock()
            {
                Name = "CameraNumberTB",
                Text = "CameraNumber" + ":",
                FontSize = 15

            });
            StackTime.Children.Add(new TextBlock()
            {
                Name = "Timeajustment",
                Text = "CorrectTime M/D/Y xx/xx/xxxx xx:xx(leave blank if no change)" + ":",
                FontSize = 15

            });
            int i =0;
            foreach (var dir in directorys)
            {
                StackName.Children.Add(new TextBlock()
                {
                    Name = "TextBlock"  + i,
                    Text = dir,
                    FontSize = 15
                });
                StackLocation.Children.Add(new TextBox()
                {
                    Name = "TextBoxLocation" + i,
                    FontSize = 15
                });
                StackNumber.Children.Add(new TextBox()
                 {
                     Name = "TextBoxNumber"  + i,
                     FontSize = 15
                });
                StackTime.Children.Add(new TextBox()
                {
                    Name = "TextBoxDateTime" + i,
                    FontSize = 15
                });
                i++;
            }
        }
        protected void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (var textbox in StackLocation.Children)
            {
                if (i==0)
                {
                    i++;
                    continue;
                }
                CameraLocations.Add((textbox as TextBox).Text);
                
            }

            i = 0 ;
            foreach (var textbox in StackNumber.Children)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }
                CameraNumbers.Add((textbox as TextBox).Text);
            }
            i = 0;
            foreach (var textbox in StackTime.Children)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }
                DateTimes.Add((textbox as TextBox).Text);
            }
            this.Close();
        }

    }
}
