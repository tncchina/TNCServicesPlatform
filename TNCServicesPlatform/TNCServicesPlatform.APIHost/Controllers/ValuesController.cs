using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault.Models;
using TNCServicesPlatform.DataModel.Interfaces;

namespace TNCServicesPlatform.APIHost.Controllers
{
    /// <summary>
    /// This is the controller has many examples
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IKeyVaultAccessModel _kv;
        // Initialize controller with depenedncy injection -  kvInstance singleton
        public ValuesController(IKeyVaultAccessModel kvInstance)
        {
            _kv = kvInstance;
        }
        
        /// <summary>
        /// Gets the name of the key by.
        /// </summary>
        /// <param name="keyname">The keyname in the secretmap.json</param>
        [HttpGet]
        [Route("_echo/{keyname}")]
        public async Task<SecretBundle> GetKeyByName([FromRoute] string keyname)
        {
            var keyResponse = await _kv.GetKeyByName(keyname);
            return keyResponse;
        }

    }
}
