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

namespace TNCServicesPlatform.StorageAPI.Controllers
{
    [Route("api/prediction/")]
    public class PredictionController : Controller
    {
        [HttpPost]
        [Route("url")]
        public async Task<string> URL([FromBody] JObject imageUrl)
        {
            string projectId = "";
            string predictionKey = "";

            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Prediction-key", predictionKey);

            // Request parameters
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["iterationId"] = "7fe03d9f-6852-47b3-9f06-f7592f13de53";
            queryString["application"] = "TNC";
            var uri = $"https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/{projectId}/url?" + queryString;

            HttpResponseMessage response;
            var key = imageUrl.Properties().Select(p => p.Name).FirstOrDefault();
            
            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + imageUrl["url"] + "\"}");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }

            return (await response.Content.ReadAsStringAsync());
        }
    }
}
