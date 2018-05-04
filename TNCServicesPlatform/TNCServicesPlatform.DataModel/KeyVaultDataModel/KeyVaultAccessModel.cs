using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.IO;
using System.Threading.Tasks;
using TNCServicesPlatform.DataModel.Interfaces;

namespace TNCServicesPlatform.DataModel
{
    public class KeyVaultAccessModel : IKeyVaultAccessModel
    {
        private string _kvAuthenticationFilePath;
        private string _kvKeyMappingFilePath;
        private static IConfiguration _secretConfiguration;
        private static IConfiguration _secretMapConfiguration;
        private KeyVaultClient _kvClient;

        public KeyVaultAccessModel()
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            _kvAuthenticationFilePath = Path.Combine(basePath, "secret.json");
            _kvKeyMappingFilePath = Path.Combine(basePath, "secretmap.json");
            Initialize();
        }

        public KeyVaultAccessModel(string kvAuthJsonPath, string kvMappingJsonPath)
        {
            _kvAuthenticationFilePath = kvAuthJsonPath;
            _kvKeyMappingFilePath = kvMappingJsonPath;
            Initialize();
        }

        private void Initialize()
        {
            if (_secretConfiguration == null && _secretMapConfiguration == null)
            {
                _secretConfiguration = new ConfigurationBuilder().AddJsonFile(_kvAuthenticationFilePath).Build();
                _secretMapConfiguration = new ConfigurationBuilder().AddJsonFile(_kvKeyMappingFilePath).Build();
                _kvClient = new KeyVaultClient(GetToken);
            }
            else
            {
                throw new InvalidOperationException("Error : KeyVault Singleton has already been initlaized.");
            }
        }

        //the method that will be provided to the KeyVaultClient
        private static async Task<string> GetToken(string authority, string resource, string scope)
        {
            var kvClientId = _secretConfiguration.GetSection("KeyVaultAuth:ClientId").Value;
            var kvClientSecret = _secretConfiguration.GetSection("KeyVaultAuth:ClientSecret").Value;
            var authContext = new AuthenticationContext(authority);
            var clientCred = new ClientCredential(kvClientId, kvClientSecret);
            var result = await authContext.AcquireTokenAsync(resource, clientCred);
            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");
            return result.AccessToken;
        }

        public async Task<SecretBundle> GetKeyByName(string name)
        {
            var testx = _secretMapConfiguration;
            var kv = new KeyVaultClient(GetToken);
            var secT = await _kvClient.GetSecretAsync(_secretMapConfiguration.GetSection("KeyVaultAuth:KeyNameUrlMap:" + name).Value);
            return secT;
        }
    }
}
