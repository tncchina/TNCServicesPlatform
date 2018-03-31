using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json.Linq;

namespace TNCServicesPlatform.StorageAPI.Controllers
{
    [Route("api/storage/")]
    public class StorageController : Controller
    {
        [HttpPost]
        [Route("_echo")]
        public async Task<IActionResult> Echo([FromBody] JObject item)
        {
            return this.Ok(new string[] { "Hello", "World", item.ToString() });
        }
    }
}
