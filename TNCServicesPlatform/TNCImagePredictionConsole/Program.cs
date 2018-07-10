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

namespace TNCImagePredictionConsole
{
    class Program
    {       
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("TNCImagePredictionConsole.exe <Path_To_Local_Image>");
                Console.WriteLine(@"Example: TNCImagePredictionConsole.exe E:\Monkey.jpg");
            }

            // 1. upload image
            string imagePath = args[0];
            AnimalImage image = UploadImage(imagePath).Result;

            // 2. image classification
            string imageUrl = $"https://tncstorage4test.blob.core.windows.net/animalimages/{image.ImageBlob}";
            MakePredictionRequest(imageUrl);

            Console.ReadLine();
        }

        static async Task<AnimalImage> UploadImage(string imagePath)
        {
            try
            {
                var client = new HttpClient();
                AnimalImage image = new AnimalImage();
                image.ImageName = Path.GetFileNameWithoutExtension(imagePath);
                image.FileFormat = Path.GetExtension(imagePath);

                // 1. Upload meta data to Cosmos DB
                string uploadUrl = "http://tncapi.azurewebsites.net/api/storage/Upload";
                string imageJson = JsonConvert.SerializeObject(image);
                byte[] byteData = Encoding.UTF8.GetBytes(imageJson);
                HttpResponseMessage response;

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uploadUrl, content);
                }

                string responseStr = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseStr);
                AnimalImage imageResponse = JsonConvert.DeserializeObject<AnimalImage>(responseStr);

                // 2. uppload image self to blob storage
                byte[] blobContent = File.ReadAllBytes(imagePath);
                CloudBlockBlob blob = new CloudBlockBlob(new Uri(imageResponse.UploadBlobSASUrl));
                MemoryStream msWrite = new MemoryStream(blobContent);
                msWrite.Position = 0;
                using (msWrite)
                {
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

        static async void MakePredictionRequest(string imageUrl)
        {
            try
            {
                var client = new HttpClient();
                var uri = "http://tncapi.azurewebsites.net/api/prediction/url";

                byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + imageUrl + "\"}");
                HttpResponseMessage response;

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uri, content);
                }

                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
    }
}
