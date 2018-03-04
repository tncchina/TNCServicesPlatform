using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TNCServicesPlatform.StorageAPI.Controllers
{
    [Route("api/storage/")]
    public partial class StorageController_AzureStorageAPIController : Controller
    {
        [HttpPost]
        [Route("{type}/_upload")]
        public async Task<IActionResult> UploadFileToAzureBlob([FromBody] JObject item, [FromRoute] string type, [FromQuery] string source = "")
        {
            return this.Ok($"Uploaded {type} from {source}, content {item}");
        }
    }
}
