using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using TNCAnimalLabelWebAPI.Models;

namespace TNCApp_New.Models
{
    [Serializable]
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
        public bool IsPhoto { get; set; }
        public List<Prediction> Predictions { get; set; }
        public int CSVindex { get; set; }



    }

}
