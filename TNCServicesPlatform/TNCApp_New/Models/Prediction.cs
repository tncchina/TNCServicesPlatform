using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TNCAnimalLabelWebAPI.Models
{
    [Serializable]
    public class Prediction
    {
        public string TagId { get; set; }
        public string Tag { get; set; }
        public float Probability { get; set; }
    }
}