using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CNTK;
using CNTKImageProcessing;
using TNCAnimalLabelWebAPI;
using TNCAnimalLabelWebAPI.Models;

namespace TNCApp_New.CNTK
{
    class LocalPrediction
    {
        public static ImagePredictionResultModel EvaluateCustomDNN(string imagePath)
        {
            string domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string workingDirectory = Environment.CurrentDirectory;
            DeviceDescriptor device = DeviceDescriptor.CPUDevice;
            string[] class_labels = new string[] { "空","牛","松鼠","人","雉鸡","猕猴","鼠","麂","滇金丝猴","鸟","狗",
                                                   "山羊","黄喉貂","豹猫","绵羊","黄鼬","黑熊","野兔","鬣羚","马","豪猪","其他"};

            // Load the model.
            // This example requires the ResNet20_CIFAR10_CNTK.model.
            // The model can be downloaded from <see cref="https://www.cntk.ai/Models/CNTK_Pretrained/ResNet20_CIFAR10_CNTK.model"/>
            // Please see README.md in <CNTK>/Examples/Image/Classification/ResNet about how to train the model.
            // string modelFilePath = Path.Combine(domainBaseDirectory, @"CNTK\Models\ResNet20_CIFAR10_CNTK.model");
            string modelFilePath = Path.Combine(domainBaseDirectory, @"CNTK\Models\TNC_ResNet18_ImageNet_CNTK.model");
            if (!File.Exists(modelFilePath))
            {
                throw new FileNotFoundException(modelFilePath, string.Format("Error: The model '{0}' does not exist. Please follow instructions in README.md in <CNTK>/Examples/Image/Classification/ResNet to create the model.", modelFilePath));
            }

            Function modelFunc = Function.Load(modelFilePath, device);

            // Get input variable. The model has only one single input.
            Variable inputVar = modelFunc.Arguments.Single();

            // Get shape data for the input variable
            NDShape inputShape = inputVar.Shape;
            int imageWidth = inputShape[0];
            int imageHeight = inputShape[1];
            int imageChannels = inputShape[2];
            int imageSize = inputShape.TotalSize;

            // Get output variable
            Variable outputVar = modelFunc.Output;

            var inputDataMap = new Dictionary<Variable, Value>();
            var outputDataMap = new Dictionary<Variable, Value>();

            // Retrieve the image file.
            //Bitmap bmp = new Bitmap(Bitmap.FromFile(imageUrl));
            //System.Net.Http.HttpClient httpClient = new HttpClient();
            //Stream imageStream = await httpClient.GetStreamAsync(imageUrl);
            Bitmap bmp = new Bitmap(imagePath);

            var resized = bmp.Resize(imageWidth, imageHeight, true);
            List<float> resizedCHW = resized.ParallelExtractCHW();

            // Create input data map
            var inputVal = Value.CreateBatch(inputVar.Shape, resizedCHW, device);
            inputDataMap.Add(inputVar, inputVal);

            // Create output data map
            outputDataMap.Add(outputVar, null);

            // Start evaluation on the device
            modelFunc.Evaluate(inputDataMap, outputDataMap, device);

            // Get evaluate result as dense output
            var outputVal = outputDataMap[outputVar];
            var outputData = outputVal.GetDenseData<float>(outputVar);
            float[] softmax_vals = ActivationFunctions.Softmax(outputData[0]);

            // construct a ImagePredictionResultModel.    "class name": prediction of the class.
            ImagePredictionResultModel predictionResult = new ImagePredictionResultModel();
            predictionResult.Id = "TNC100";
            predictionResult.Project = "TNCAnimalLabel";
            predictionResult.Iteration = "1.00";
            predictionResult.Created = DateTime.Now;
            predictionResult.Predictions = new List<TNCAnimalLabelWebAPI.Models.Prediction>();

            int class_id = 0;
            for (; class_id < (softmax_vals.Length); class_id++)
            {
                TNCAnimalLabelWebAPI.Models.Prediction prediction = new TNCAnimalLabelWebAPI.Models.Prediction();
                prediction.TagId = class_id.ToString();
                prediction.Tag = class_labels[class_id];
                prediction.Probability = softmax_vals[class_id];
                predictionResult.Predictions.Add(prediction);
            }

            return predictionResult;
        }
    }
}
