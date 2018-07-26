using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TNCServicesPlatform.StorageAPI.Models;
using System.Globalization;

namespace TNCApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        async Task<AnimalImage> UploadImage(string imagePath)
        {
            try
            {
                var client = new HttpClient();
                AnimalImage image = new AnimalImage();
                image.ImageName = Path.GetFileNameWithoutExtension(imagePath);
                image.FileFormat = Path.GetExtension(imagePath);

                // 1. Upload meta data to Cosmos DB
                string uploadUrl = "http://tncapi.azurewebsites.net/api/storage/Upload2";
                string imageJson = JsonConvert.SerializeObject(image);
                byte[] byteData = Encoding.UTF8.GetBytes(imageJson);
                HttpResponseMessage response;

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    this.richTextBox1.Text = ("Sending image info to Cosmos DataBase");
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

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
                if (result == DialogResult.OK) // Test result.
                {
                }

                // 1. upload image
                string imagePath = openFileDialog1.FileName;
                this.pictureBox1.ImageLocation = openFileDialog1.FileName;

                AnimalImage image = await UploadImage(imagePath);
                //this.richTextBox1.Text = image.UploadBlobSASUrl;

                // 2. image classification
                string imageUrl = $"https://tncstorage4test.blob.core.windows.net/animalimages/{image.ImageBlob}";
                this.richTextBox1.Text = ("Waiting for prediction result...");
                var preResult = await MakePredictionRequestCNTK(imageUrl);
                this.richTextBox1.Clear();
                var sorted = preResult.Predictions.OrderByDescending(f => f.Probability);
                foreach (var pre in sorted)
                {
                    // this.richTextBox1.Text += pre.Tag + ":  " + pre.Probability.ToString("P", CultureInfo.InvariantCulture) + "\n";

                    this.richTextBox1.Text += pre.Tag + ":  " + pre.Probability + "\n";
                }
            }
            catch (Exception ex)
            {
                this.richTextBox1.Text = ex.ToString();
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
