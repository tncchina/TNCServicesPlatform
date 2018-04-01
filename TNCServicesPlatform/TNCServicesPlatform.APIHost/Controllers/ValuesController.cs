using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault.Models;

namespace TNCServicesPlatform.APIHost.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        [Route("_echo/{keyname}")]
        public async Task<SecretBundle> GetKeyByName([FromRoute] string keyname)
        {
            var keyResponse = await Startup.GetKeyByName(keyname);
            return keyResponse;
        }
        
    }
}
