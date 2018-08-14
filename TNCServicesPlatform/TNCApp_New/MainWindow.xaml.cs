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
using TNCApp_New.Models;
using ListBox = System.Windows.Controls.ListBox;

namespace TNCApp_New
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static ManualResetEvent mre = new ManualResetEvent(false);
        public bool StateLocal { get; set; }
        public int ConfidenceRate { get; set; }
        public MainWindow()
        {
            
            InitializeComponent();
            this.UploadingBar.Value = 0;
            this.StateLocal = true;
            this.ConfidenceRate = 90;

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        //using the image path, upload the image to cosmos and storage account
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
        //start the entier prediction process
        //turn to different operation based on different file type loaded

        async void StartPrediction()
        {
            this.ListV.ItemsSource = new List<String>();
            this.UploadingBar.Value = 0;
            this.richTextBox1.Clear();
            this.pictureBox1.Source = new BitmapImage();
            this.ConfirmButton.Visibility = Visibility.Hidden;
            this.ConfirmTextBox.Visibility = Visibility.Hidden;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.ShowDialog();
            var csv = new StringBuilder();
            var ConfirmPredictions = new List<ConfirmPredictionModel>();

            IDictionary<string, int> dict = new Dictionary<string, int>();

            switch (Path.GetExtension(openFileDialog.FileName))
            {
                case ".jpg":
                case ".JPG":
                    var imagePaths = openFileDialog.FileNames;
                    this.richTextBox1.Text = ("Processing...");
                    AllowUIToUpdate();
                    var totalFilesNum = imagePaths.Length;
                   
                    Window1 dlg = new Window1();

                    //Ask for camera information
                    // Configure the dialog box
                    dlg.Owner = this;
                    // Open the dialog box modally 
                    dlg.ShowDialog();
                    dlg.Top = this.Top + 20;
                    string cameraNumber= dlg.CameraNumber.Text;
                    string folderName= dlg.CameraLocation.Text;
                    string positionNumber= dlg.CameraLocation.Text;
                    //csv.AppendLine(newLine);
                    int i = 0;
                    ImagePredictionResultModel result;
                    List<String> items = new List<String>();

                    
                    foreach (String imagePath in imagePaths)
                    {

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
                        ConfirmPredictions.Add(new ConfirmPredictionModel()
                        {
                            FilePath = imagePath,
                            Predictions = result.Predictions
                        });
                        this.pictureBox1.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                        this.UploadingBar.Value += Math.Round(1.0 / totalFilesNum * 100);
                        AllowUIToUpdate();
                        
                    }
                    Task mytask = Task.Run(() =>
                    {
                        GenerateCSV(cameraNumber, folderName, ConfirmPredictions);
                    });
                    
                    this.UploadingBar.Value = 100;
                    this.richTextBox1.Text = ("Done");
                    return; 
                case ".csv":
                case ".CSV":
                    double ss;
                    var imagePath1 = openFileDialog.FileName;
                    this.richTextBox1.Text = ("Processing...");
                        var file = new StreamReader(imagePath1, Encoding.Default).ReadToEnd(); 
                        var lines = file.Split('\n');
                        var count = lines.Count();
                        var N_count = count;
                        for (int j = 0; j<count;j++)
                        {
                            var line = lines[j];

                            if (j == 0)
                            {
                                continue;
                            }
                            var values = line.Split(',');
                            //check if the row is a valid row, if not continue
                            if (values.Count() < 2){
                                continue;
                            }

                            var fileName = values[0];
                            var folderNames = values[3];
                            var extension = values[2];
                            if (extension != "JPG")
                            {
                                continue;
                            }
                            var NewPath = Path.GetDirectoryName(imagePath1) + $"\\{folderNames}\\{fileName}.{extension}";
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
                            this.pictureBox1.Source = new BitmapImage(new Uri(NewPath, UriKind.RelativeOrAbsolute));
                            ss = j;
                            this.UploadingBar.Value = Math.Round(ss / N_count * 100);
                            AllowUIToUpdate();
                        ConfirmPredictions.Add(new ConfirmPredictionModel()
                        {
                            FilePath = NewPath,
                            Predictions = result.Predictions,
                            CSVindex = j
                        });  
                        }

                    Task mytask1 = Task.Run(() => { GenerateCSV_CSV(lines, ConfirmPredictions); });
                        return;

                default:this.richTextBox1.Text=("input not valid");
                    return;
            }




        }

        private void GenerateCSV(string cameraNumber, string cameraLocation, List<ConfirmPredictionModel> models)
        {
            int i = 0;
            int workDays = 1;

            var lastWorkDay = File.GetCreationTime(models[0].FilePath).Date;
            IDictionary<string, int> dict = new Dictionary<string, int>();
            var csv = new StringBuilder();
            List<String> items = new List<String>();
            var newLine = string.Format("编号,原始文件编号,文件格式,文件夹编号,相机编号,布设点位编号,拍摄日期,拍摄时间,工作天数,对象类别,物种名称,动物数量,性别,独立探测首张,备注");
            csv.AppendLine(newLine);
            string folderPath = Path.GetDirectoryName(models[0].FilePath);
            string pathString = System.IO.Path.Combine(folderPath, cameraLocation);
            System.IO.Directory.CreateDirectory(pathString);
            string speciesName = "";
            var lastPhotoSpecie = speciesName;
            foreach (var model in models)
            {

                var imagePath = model.FilePath;
                Bitmap bmp = new Bitmap(imagePath);
                var shootingDate = File.GetCreationTime(imagePath).Date;
                var shootingTime = File.GetCreationTime(imagePath).ToShortTimeString();
                var fileNameNoext = Path.GetFileNameWithoutExtension(imagePath);
                var fileExt = Path.GetExtension(imagePath);
                workDays += (shootingDate - lastWorkDay).Days;
                var sorted = model.Predictions.OrderByDescending(f => f.Probability);
                //this is the part for the threshhold
                int CorrectedIndex = 0;
                speciesName = sorted.ToList()[0].Tag;
                if (Math.Round(sorted.ToList()[0].Probability * 100, 6) < this.ConfidenceRate)
                {

                    items = new List<string>();
                    foreach (var pre in sorted)
                    {
                        // this.richTextBox1.Text += pre.Tag + ":  " + pre.Probability.ToString("P", CultureInfo.InvariantCulture) + "\n";
                        items.Add(pre.Tag + ":  " + Math.Round(pre.Probability * 100, 6) + "%\n");

                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        this.pictureBox1.Source = new BitmapImage(new Uri(imagePath));
                        this.richTextBox1.Text =
                            "please reconfirm the Animal that below confidence rate " + $"{ConfidenceRate}%";
                        //do something to the window
                        this.ListV.ItemsSource = new List<String>();
                        this.ListV.ItemsSource = items;
                        this.ConfirmButton.Visibility = Visibility.Visible;
                        this.ConfirmTextBox.Visibility = Visibility.Visible;
                    });

                    //AllowUIToUpdate();
                    mre.WaitOne();
                    mre.Reset();
                    
                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.ListV.SelectedIndex != -1)
                        {
                            CorrectedIndex = this.ListV.SelectedIndex;
                            speciesName = sorted.ToList<Prediction>()[CorrectedIndex].Tag;
                        }
                        else
                        {
                            speciesName = ConfirmTextBox.Text;
                        }
                        
                    });

                }
                
                var firstDetected = (speciesName == lastPhotoSpecie) ? "NO" : "YES";
                lastPhotoSpecie = speciesName;
                newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                    $"{cameraLocation}-{i.ToString("D4")}",
                    fileNameNoext,
                    fileExt,
                    cameraLocation,
                    cameraNumber,
                    cameraLocation,
                    shootingDate,
                    shootingTime,
                    workDays,
                    "Unkown Type",
                    speciesName,
                    "Unknown number of animal",
                    "Unknown gender",
                    firstDetected,
                    "NAN"
                );
                if (dict.ContainsKey(speciesName))
                {
                    dict[speciesName]++;
                }
                else
                {
                    dict.Add(speciesName, 1);
                }
                csv.AppendLine(newLine);
                bmp.Save($"{Path.Combine(pathString, $"{cameraLocation}-{i.ToString("D4")}{fileExt}")}", System.Drawing.Imaging.ImageFormat.Jpeg);
                i++;
            }
            File.WriteAllText(Path.Combine(pathString, $"{cameraLocation}.csv"), csv.ToString(), Encoding.Default);
            this.Dispatcher.Invoke(() => {
                this.UploadingBar.Value = 100;
                this.richTextBox1.Text = ("Done");
                this.ConfirmButton.Visibility = Visibility.Hidden;
                this.ConfirmTextBox.Visibility = Visibility.Hidden;
                DataVisualization div = new DataVisualization(dict);
                // Configure the dialog box
                div.Owner = this;
                // Open the dialog box modally 
                div.Show();
            });


            return;
        }
        private void GenerateCSV_CSV(string[] csvstrings, List<ConfirmPredictionModel> models)
        {

            var lastWorkDay = File.GetCreationTime(models[0].FilePath).Date;
            IDictionary<string, int> dict = new Dictionary<string, int>();
            var csv = new StringBuilder();
            List<String> items = new List<String>();
            var line = csvstrings[0];
            csvstrings[0] = csvstrings[0].Replace("\r", "对象类别,物种名称,动物数量,性别,独立探测首张,备注");
            string speciesName = "";
            var lastPhotoSpecie = speciesName;
            foreach (var model in models)
            {

                var imagePath = model.FilePath;
                Bitmap bmp = new Bitmap(imagePath);
                var sorted = model.Predictions.OrderByDescending(f => f.Probability);

                //this is the part for the threshhold
                int CorrectedIndex = 0;
                speciesName = sorted.ToList()[0].Tag;
                if (Math.Round(sorted.ToList()[0].Probability * 100, 6) < this.ConfidenceRate)
                {

                    items = new List<string>();
                    foreach (var pre in sorted)
                    {
                        // this.richTextBox1.Text += pre.Tag + ":  " + pre.Probability.ToString("P", CultureInfo.InvariantCulture) + "\n";
                        items.Add(pre.Tag + ":  " + Math.Round(pre.Probability * 100, 6) + "%\n");

                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        this.pictureBox1.Source = new BitmapImage(new Uri(imagePath));
                        this.richTextBox1.Text =
                            "please reconfirm the Animal that below confidence rate " + $"{ConfidenceRate}%";
                        //do something to the window
                        this.ListV.ItemsSource = new List<String>();
                        this.ListV.ItemsSource = items;
                        this.ConfirmButton.Visibility = Visibility.Visible;
                        this.ConfirmTextBox.Visibility = Visibility.Visible;
                    });

                    //AllowUIToUpdate();
                    mre.WaitOne();
                    mre.Reset();

                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.ListV.SelectedIndex != -1)
                        {
                            CorrectedIndex = this.ListV.SelectedIndex;
                            speciesName = sorted.ToList<Prediction>()[CorrectedIndex].Tag;
                        }
                        else
                        {
                            speciesName = ConfirmTextBox.Text;
                        }

                    });

                }

                var firstDetected = (speciesName == lastPhotoSpecie) ? "NO" : "YES";
                lastPhotoSpecie = speciesName;
                csvstrings[model.CSVindex] = csvstrings[model.CSVindex].Replace("\r", string.Format("{0},{1},{2},{3},{4},{5}", "Unkown Type",
                                                                                          speciesName,
                                                                                          "Unknown number of animal",
                                                                                          "Unknown gender",
                                                                                          firstDetected, "NAN") + "\r");
                if (dict.ContainsKey(speciesName))
                {
                    dict[speciesName]++;
                }
                else
                {
                    dict.Add(speciesName, 1);
                }

            }

            this.Dispatcher.Invoke(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.ShowDialog();
                var filePath = saveFileDialog.FileName;
                if (!File.Exists(filePath))
                {
                    this.richTextBox1.Text = ("folder path not valid");
                    return;
                }
                File.WriteAllText(filePath, csvstrings.ToString(), Encoding.Default);
                this.UploadingBar.Value = 100;
                this.richTextBox1.Text = ("Done");
                DataVisualization div = new DataVisualization(dict);

                // Configure the dialog box
                div.Owner = this;
                // Open the dialog box modally 
                div.Show();
            });


            return;
        }
        private  void BtnUpload_Click(object sender, RoutedEventArgs e)
        {

            
            StartPrediction();

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

        private void ListV_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void ListV_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            mre.Set();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            mre.Set();
        }
    }
}
