using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TNCAnimalLabelWebAPI.Models
{
    public class ImagePredictionResultModel
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Iteration { get; set; }
        public DateTime Created { get; set; }
        //public Prediction[] Predictions { get; set; }
        public List<Prediction> Predictions { get; set; }

    }
}