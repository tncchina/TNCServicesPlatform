using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TNCServicesPlatform.StorageAPI.Models
{
    public class AnimalLocation
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public Double Longtitude { get; set; }
        public Double Latitude { get; set; }
    }
}
