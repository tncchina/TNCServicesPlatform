using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault.Models;
using TNCServicesPlatform.APIHost.DataModels;

namespace TNCServicesPlatform.APIHost.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly KeyVaultAccessModel _kv;
        // Initialize controller with depenedncy injection -  kvInstance singleton
        public ValuesController(KeyVaultAccessModel kvInstance)
        {
            _kv = kvInstance;
        }

        // GET api/values
        [HttpGet]
        [Route("_echo/{keyname}")]
        public async Task<SecretBundle> GetKeyByName([FromRoute] string keyname)
        {
            var keyResponse = await _kv.GetKeyByName(keyname);
            return keyResponse;
        }
        
    }
}
