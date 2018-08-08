using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace TNCServicesPlatform.StorageAPI.Models
{
    public class AnimalImage
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Tag { get; set; }

        public string ImageName { get; set; }

        public string ImageBlob { get; set; }

        public string UploadBlobSASUrl { get; set; }

        public string DownloadBlobSASUrl { get; set; }

        public string Notes { get; set; }
    }
}