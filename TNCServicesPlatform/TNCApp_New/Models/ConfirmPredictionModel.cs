using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using TNCAnimalLabelWebAPI.Models;

namespace TNCApp_New.Models
{
    class ConfirmPredictionModel
    {
/*        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileFormat { get; set; }
        public string FolderName { get; set; }*/


/*        public string CameraName { get; set; }
        public string PostionName { get; set; }*/

/*        public string ShootingDate { get; set; }
        public string ShootingTime { get; set; }*/


        public string FilePath { get; set; }
        //public string NewFolderPath { get; set; }

        public List<Prediction> Predictions { get; set; }

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
                if (Math.Round(sorted.ToList()[0].Probability * 100, 6) < 90)
                {
                    //do something to the window
                    this.ListV.ItemsSource = new List<String>();
                    items = new List<string>();
                    foreach (var pre in sorted)
                    {
                        // this.richTextBox1.Text += pre.Tag + ":  " + pre.Probability.ToString("P", CultureInfo.InvariantCulture) + "\n";
                        items.Add(pre.Tag + ":  " + Math.Round(pre.Probability * 100, 6) + "%\n");

                    }

                }
                speciesName = sorted.ToList<Prediction>()[0].Tag;
                var firstDetected = (speciesName == lastPhotoSpecie) ? "YES" : "NO";
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
            this.UploadingBar.Value = 100;
            this.richTextBox1.Text = ("Done");
            DataVisualization div = new DataVisualization(dict);
            // Configure the dialog box
            div.Owner = this;
            // Open the dialog box modally 
            div.Show();
            return;
        }
    }

}
