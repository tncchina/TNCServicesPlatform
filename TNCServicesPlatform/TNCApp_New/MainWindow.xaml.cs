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

        async Task<ImagePredictionResult> MakePredictionRequestCNTK(string imageUrl)
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
                var resObj = JsonConvert.DeserializeObject<ImagePredictionResult>(res);
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

     
    
        void LocalProcess()
        {


            this.ListV.ItemsSource = new List<String>();
            this.UploadingBar.Value = 0;
            this.richTextBox1.Clear();
            this.pictureBox1.Source = new BitmapImage();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.ShowDialog();
            switch (Path.GetExtension(openFileDialog.FileName))
            {
                case ".jpg":
                    var imagePaths = openFileDialog.FileNames;
                    this.richTextBox1.Text = ("Processing...");
                    AllowUIToUpdate();
                    var totalFilesNum = imagePaths.Length;
                    var csv = new StringBuilder();
                    var newLine = string.Format("编号,原始文件编号,文件格式,文件夹编号,相机编号,布设点位编号,拍摄日期,拍摄时间,工作天数,对象类别,物种名称,动物数量,性别,独立探测首张,备注");
                    csv.AppendLine(newLine);
                    int i = 0;
                    List<String> items = new List<String>();
                    foreach (String imagePath in imagePaths)
                    {

                        var result = LocalPrediction.EvaluateCustomDNN(imagePath);

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
                        var first = "test";
                        var second = sorted.ToList()[0].Probability;
                        newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},",
                            $"{first}-{i.ToString("D4")}",
                            Path.GetFileNameWithoutExtension(imagePath),
                            Path.GetExtension(imagePath),
                            first,
                            second,
                            first,
                            File.GetCreationTime(imagePath),
                            File.GetCreationTime(imagePath),
                            1,
                            2,
                            3,
                            4,
                            5,
                            6,
                            7
                            );

                        csv.AppendLine(newLine);
                    }
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.ShowDialog();
                    var filePath = saveFileDialog.FileName;
                    File.WriteAllText(filePath, csv.ToString(), Encoding.Default);
                    return;
                case ".csv":
                    double ss;
                    var imagePath1 = openFileDialog.FileName;
                    csv = new StringBuilder();
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
                            var result = LocalPrediction.EvaluateCustomDNN(NewPath);
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
                    saveFileDialog = new SaveFileDialog();
                    saveFileDialog.ShowDialog();
                    filePath = saveFileDialog.FileName;
                    File.WriteAllText(filePath, csv.ToString(), Encoding.Default);
                    this.UploadingBar.Value = 100;
                    return;
                default:this.richTextBox1.Text=("input not valid");
                    return;
            }

            this.UploadingBar.Value =100;
            this.richTextBox1.Text = ("Done");
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
            if (StateLocal)
            {
                LocalProcess();                
            }
            else
            {
                OnlineProcess();
            }
           
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
    }
}
