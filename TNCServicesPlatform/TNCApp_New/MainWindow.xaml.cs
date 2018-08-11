using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using CNTK;
using CNTKImageProcessing;
using Microsoft.Cognitive.CustomVision.Training.Models;
using TNCServicesPlatform.StorageAPI.Models;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Microsoft.WindowsAzure.Storage.Blob;
using TNCAnimalLabelWebAPI;
using TNCAnimalLabelWebAPI.Models;
using TNCApp_New.CNTK;

namespace TNCApp_New
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool StateLocal { get; set; }
        public MainWindow()
        {
            
            InitializeComponent();
            this.UploadingBar.Value = 0;
            this.StateLocal = true;


        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        async Task<AnimalImage> UploadImage(string imagePath)
        {
            try
            {
                this.UploadingBar.Value = 0;
                var client = new HttpClient();
                AnimalImage image = new AnimalImage();
                image.ImageName = Path.GetFileName(imagePath);

                // 1. Upload meta data to Cosmos DB
                string uploadUrl = "http://tncapi.azurewebsites.net/api/storage/Upload2";
                string imageJson = JsonConvert.SerializeObject(image);
                byte[] byteData = Encoding.UTF8.GetBytes(imageJson);
                HttpResponseMessage response;

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    this.richTextBox1.Text = ("Sending image info to Cosmos DataBase");
                    this.UploadingBar.Value += 33;
                   response = await client.PostAsync(uploadUrl, content);
                    //response = client.PostAsync(uploadUrl, content).Result;
                }

                string responseStr = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseStr);
                AnimalImage imageResponse = JsonConvert.DeserializeObject<AnimalImage>(responseStr);

                // 2. uppload image self to blob storage
                byte[] blobContent = File.ReadAllBytes(imagePath);
                CloudBlockBlob blob = new CloudBlockBlob(new Uri(imageResponse.UploadBlobSASUrl));
                MemoryStream msWrite = new MemoryStream(blobContent);
                msWrite.Position = 0;

                using (msWrite)
                {
                    this.richTextBox1.Text = ("Uploading image data to Storage Colletion");
                    await blob.UploadFromStreamAsync(msWrite);
                    this.UploadingBar.Value += 33;
                }

                return imageResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        async Task<ImagePredictionResultModel> MakePredictionRequestCNTK(string imageUrl)
        {
            try
            {
                var client = new HttpClient();
                var uri = "http://tncapi.azurewebsites.net/api/prediction/cntk";

                byte[] byteData = Encoding.UTF8.GetBytes("\"" + imageUrl + "\"");
                HttpResponseMessage response;

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uri, content);
                    this.UploadingBar.Value += 33;
                }

                string res = response.Content.ReadAsStringAsync().Result;
                var resObj = JsonConvert.DeserializeObject<ImagePredictionResultModel>(res);
                return resObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.ListV.ItemsSource = new List<String>();
            this.UploadingBar.Value = 0;
            this.richTextBox1.Clear();
            this.pictureBox1.Source = new BitmapImage();

        }



        async void LocalProcess()
        {


            this.ListV.ItemsSource = new List<String>();
            this.UploadingBar.Value = 0;
            this.richTextBox1.Clear();
            this.pictureBox1.Source = new BitmapImage();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.ShowDialog();
            var csv = new StringBuilder();
           
            switch (Path.GetExtension(openFileDialog.FileName))
            {
                case ".jpg":
                case ".JPG":
                    var imagePaths = openFileDialog.FileNames;
                    this.richTextBox1.Text = ("Processing...");
                    AllowUIToUpdate();
                    var totalFilesNum = imagePaths.Length;
                    var newLine = string.Format("编号,原始文件编号,文件格式,文件夹编号,相机编号,布设点位编号,拍摄日期,拍摄时间,工作天数,对象类别,物种名称,动物数量,性别,独立探测首张,备注");
                    Window1 dlg = new Window1();

                    // Configure the dialog box
                    dlg.Owner = this;
                    // Open the dialog box modally 
                    dlg.ShowDialog();
                    dlg.Top = this.Top + 20;
                    //this.PopUpBar.Visibility = Visibility.Visible;
                    string cameraNumber= dlg.CameraNumber.Text;
                    string folderName= dlg.CameraLocation.Text;
                    string positionNumber= dlg.CameraLocation.Text;
                    int workDays =1 ;
                    var Last_WorkDay = File.GetCreationTime(imagePaths[0]).Date;
                    csv.AppendLine(newLine);
                    int i = 0;
                    ImagePredictionResultModel result;
                    List<String> items = new List<String>();
                    string folderPath = Path.GetDirectoryName(imagePaths[0]);
                    string pathString = System.IO.Path.Combine(folderPath, folderName);
                    System.IO.Directory.CreateDirectory(pathString);
                    foreach (String imagePath in imagePaths)
                    {
                        Bitmap bmp = new Bitmap(imagePath);
                        var shootingDate = File.GetCreationTime(imagePath).Date;
                        var shootingTime = File.GetCreationTime(imagePath).ToShortTimeString();
                        var fileNameNoext = Path.GetFileNameWithoutExtension(imagePath);
                        var fileExt = Path.GetExtension(imagePath);
                         workDays += (shootingDate - Last_WorkDay).Days;
                        if (this.StateLocal != true)
                        {
                            AnimalImage image = await UploadImage(imagePath);
                            //this.richTextBox1.Text = image.UploadBlobSASUrl;
                            // 2. image classification
                            string imageUrl = $"https://tncstorage4test.blob.core.windows.net/animalimages/{image.ImageBlob}";
                            this.richTextBox1.Text = ("Waiting for prediction result...");
                             result = await MakePredictionRequestCNTK(imageUrl);
                        }
                        else
                        {
                             result = LocalPrediction.EvaluateCustomDNN(imagePath);
                        }
                        var sorted = result.Predictions.OrderByDescending(f => f.Probability);
                        this.ListV.ItemsSource = new List<String>();
                        items = new List<string>();
                        foreach (var pre in sorted)
                        {
                            // this.richTextBox1.Text += pre.Tag + ":  " + pre.Probability.ToString("P", CultureInfo.InvariantCulture) + "\n";
                            items.Add(pre.Tag + ":  " + Math.Round(pre.Probability * 100, 6) + "%\n");

                        }
                        this.ListV.ItemsSource = items;
                        this.pictureBox1.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                        this.UploadingBar.Value += Math.Round(1.0 / totalFilesNum * 100);
                        AllowUIToUpdate();

                        //lots of variable not doing
                        newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                            $"{positionNumber}-{i.ToString("D4")}",
                            fileNameNoext,
                            fileExt,
                            folderName,
                            cameraNumber,
                            positionNumber,
                            shootingDate,
                            shootingTime,
                            workDays,
                            "Unkown Type",
                            sorted.ToList<Prediction>()[0].Tag,
                            sorted.ToList<Prediction>()[0].Probability,
                            4,
                            5,
                            6,
                            7
                            );
                        csv.AppendLine(newLine);
                        bmp.Save($"{Path.Combine(pathString, $"{positionNumber}-{i.ToString("D4")}{fileExt}")}", System.Drawing.Imaging.ImageFormat.Jpeg);
                        i++;
                    }
                    File.WriteAllText(Path.Combine(pathString, $"{positionNumber}.csv"), csv.ToString(), Encoding.Default);
                    this.UploadingBar.Value = 100;
                    this.richTextBox1.Text = ("Done");
                    return; 
                case ".csv":
                case ".CSV":
                    double ss;
                    var imagePath1 = openFileDialog.FileName;
                    this.richTextBox1.Text = ("Processing...");
                    
                    using (var reader = new StreamReader(imagePath1, Encoding.Default))
                    {

                        var file = new StreamReader(imagePath1, Encoding.Default).ReadToEnd(); 
                        var lines = file.Split('\n');
                        var count = lines.Count();
                        var N_count = count;
                        for (int j = 0; j<count;j++)
                        {
                            var line = lines[j];

                            if (j == 0)
                            {
                                line = line.Replace("\r", ",预测种类1,预测概率1,预测种类2,预测概率2,预测种类3,预测概率3" + "\r");
                                csv.AppendLine(line);
                                continue;
                            }
                            var values = line.Split(',');
                            if (values.Count() < 2){
                                continue;
                            }
                            var FileName = values[0];
                            var FolderName = values[3];
                            var extension = values[2];
                            if (extension != "JPG")
                            {
                                 csv.AppendLine(line);
                                continue;
                            }
                            var NewPath = Path.GetDirectoryName(imagePath1) + $"\\{FolderName}\\{FileName}.{extension}";
                            if (!File.Exists(NewPath)) continue;
                            if (this.StateLocal != true)
                            {
                                AnimalImage image = await UploadImage(NewPath);
                                //this.richTextBox1.Text = image.UploadBlobSASUrl;
                                // 2. image classification
                                string imageUrl = $"https://tncstorage4test.blob.core.windows.net/animalimages/{image.ImageBlob}";
                                this.richTextBox1.Text = ("Waiting for prediction result...");
                                result = await MakePredictionRequestCNTK(imageUrl);
                            }
                            else
                            {
                                result = LocalPrediction.EvaluateCustomDNN(NewPath);
                            }
                            var sorted = result.Predictions.OrderByDescending(f => f.Probability);
                            this.ListV.ItemsSource = new List<String>();
                            items = new List<string>();
                            foreach (var pre in sorted)
                            {
                                // this.richTextBox1.Text += pre.Tag + ":  " + pre.Probability.ToString("P", CultureInfo.InvariantCulture) + "\n";
                                items.Add(pre.Tag + ":  " + Math.Round(pre.Probability * 100, 6) + "%\n");

                            }
                            this.ListV.ItemsSource = items;
                            this.pictureBox1.Source = new BitmapImage(new Uri(NewPath, UriKind.RelativeOrAbsolute));
                            ss = j;
                            this.UploadingBar.Value = Math.Round(ss / N_count * 100);
                            AllowUIToUpdate();
                            var predictions = sorted.ToList<Prediction>();
                            line = line.Replace("\r", $",{predictions[0].Tag},{predictions[0].Probability},{predictions[1].Tag},{predictions[1].Probability},{predictions[2].Tag},{predictions[2].Probability}," + "\r");
                            csv.AppendLine(line);
                        }
                    }
                    break;
                default:this.richTextBox1.Text=("input not valid");
                    return;
            }



            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.ShowDialog();
            var filePath = saveFileDialog.FileName;
            File.WriteAllText(filePath, csv.ToString(), Encoding.Default);
            this.UploadingBar.Value = 100;
            this.richTextBox1.Text = ("Done");
            return;

        }

        async void OnlineProcess()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.ShowDialog(); 

                // 1. upload image
                string imagePath = openFileDialog.FileName;

                this.pictureBox1.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));

                AnimalImage image = await UploadImage(imagePath);
                //this.richTextBox1.Text = image.UploadBlobSASUrl;

                // 2. image classification
                string imageUrl = $"https://tncstorage4test.blob.core.windows.net/animalimages/{image.ImageBlob}";
                this.richTextBox1.Text = ("Waiting for prediction result...");
                var preResult = await MakePredictionRequestCNTK(imageUrl);
                this.UploadingBar.Value += 33;
                this.richTextBox1.Clear();
                List<String> items = new List<String>();
                var sorted = preResult.Predictions.OrderByDescending(f => f.Probability);
                foreach (var pre in sorted)
                {
                    // this.richTextBox1.Text += pre.Tag + ":  " + pre.Probability.ToString("P", CultureInfo.InvariantCulture) + "\n";
                    items.Add(pre.Tag + ":  " + Math.Round(pre.Probability * 100, 6) + "%\n");


                }
                this.ListV.ItemsSource = items;
                this.richTextBox1.Text = "Done Uploading";

            }
            catch (Exception ex)
            {
                //this.ListV.Items.Clear();
                //this.ListV.Items.Add(ex.ToString());
                throw ex;
            }
        }
        private  void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            // if (StateLocal)
            //  {
            DataVisualization div = new DataVisualization();

            // Configure the dialog box
            div.Owner = this;
            // Open the dialog box modally 
            div.ShowDialog();
            LocalProcess();                
//}
          //  else
         //   {
          //      OnlineProcess();
         //   }
           
        }

        void AllowUIToUpdate()
        {

            DispatcherFrame frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)

            {

                frame.Continue = false;

                return null;

            }), null);

            Dispatcher.PushFrame(frame);

        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.StateLocal = false;
            this.TextLineBox.Text = "Online";
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.StateLocal = true;
            this.TextLineBox.Text = "Offline";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
