using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TNCServicesPlatform.DataModel.Interfaces
{
    public interface IKeyVaultAccessModel
    {
        Task<SecretBundle> GetKeyByName(string name);
    }
}
