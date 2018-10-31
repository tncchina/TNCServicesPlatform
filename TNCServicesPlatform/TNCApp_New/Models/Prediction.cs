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
        public PredictionRegion  Region { get; set; }
    }

    public class PredictionRegion
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }
}