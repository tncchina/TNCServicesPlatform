using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Web;
using TNCServicesPlatform.DataModel.Interfaces;
using System.Diagnostics;

namespace TNCServicesPlatform.StorageAPI.Controllers
{
    [Route("api/prediction/")]
    public class PredictionController : Controller
    {
        private const string projectId = "e6928114-b961-44f0-84d1-8bf1589c8a38";
        private const string iterationId = "7fe03d9f-6852-47b3-9f06-f7592f13de53";
        private const string application = "TNC";
        private string baseUrl = $"https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/{projectId}/url?";
        //private string cntkUrl = "http://tncanimallabelwebapi.azurewebsites.net/api/Prediction";
        private string cntkUrlNet = "http://tncimg.westus2.azurecontainer.io:8080/tncapi/v1.0/Prediction/22222222/url";
        private string cntkUrl = "https://tnccnktapi.azurewebsites.net/api/Prediction";

        private readonly IKeyVaultAccessModel _kv;
        private string predictionKey;
        private static object predictionLock = new object();

        // Initialize controller with depenedncy injection -  kvInstance singleton
        public PredictionController(IKeyVaultAccessModel kvInstance)
        {
            _kv = kvInstance;

            predictionKey = _kv.GetKeyByName("CustomVisionPredictionKey").Result.Value;
        }

        /// <summary>
        /// Sample Request Body: 
        /// {"url":"https://images.pexels.com/photos/145947/pexels-photo-145947.jpeg?cs=srgb&dl=animal-cute-monkey-145947.jpg&fm=jpg"}
        /// </summary>
        /// <returns>The URL.</returns>
        /// <param name="imageUrl">Image URL.</param>
        [HttpPost]
        [Route("url")]
        public async Task<string> URL([FromBody] JObject imageUrl)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Prediction-key", predictionKey);

                // Request parameters
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["iterationId"] = iterationId;
                queryString["application"] = application;
                var uri = baseUrl + queryString;

                HttpResponseMessage response;
                byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + imageUrl["url"] + "\"}");

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uri, content);
                }

                return (await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        [HttpPost]
        [Route("cntk")]
        public async Task<string> CNTK([FromBody] string imageUrl)
        {
            try
            {
                var client = new HttpClient();
                // client.DefaultRequestHeaders.Add("Prediction-key", predictionKey)

                lock (predictionLock)
                {
                    // Request parameters
                    var queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString["iterationId"] = iterationId;
                    queryString["application"] = application;
                    //var uri = cntkUrl + queryString;
                    var uri = cntkUrl;

                    HttpResponseMessage response;
                    byte[] byteData = Encoding.UTF8.GetBytes("\"" + imageUrl + "\"");

                    using (var content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = client.PostAsync(uri, content).Result;
                    }

                    return (response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        [HttpPost]
        [Route("cntknet")]
        public async Task<string> CNTKNet([FromBody] string imageUrl)
        {
            try
            {
                var client = new HttpClient();
                // client.DefaultRequestHeaders.Add("Prediction-key", predictionKey)

                lock (predictionLock)
                {
                    // Request parameters
                    var queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString["iterationId"] = iterationId;
                    queryString["application"] = application;
                    //var uri = cntkUrl + queryString;
                    var uri = cntkUrlNet;

                    HttpResponseMessage response;
                    byte[] byteData = Encoding.UTF8.GetBytes("\"" + imageUrl + "\"");

                    using (var content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = client.PostAsync(uri, content).Result;
                    }

                    return (response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }
    }
}
