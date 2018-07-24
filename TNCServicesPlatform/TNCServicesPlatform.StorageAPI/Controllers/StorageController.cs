using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json.Linq;
using TNCServicesPlatform.StorageAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure.Documents.Client;
using TNCServicesPlatform.DataModel.Interfaces;
using Microsoft.WindowsAzure.Storage.Blob;
using TNCServicesPlatform.StorageAPI.Common;
using System.Diagnostics;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace TNCServicesPlatform.StorageAPI.Controllers
{
    [Route("api/storage/")]
    public class StorageController : Controller
    {
        private const string CosmosDBName = "goldenmonkey";
        private const string CosmosDBCollectionName = "animalimages";
        private const string BlobStorageContainerName = "animalimages";

        // TODO: use config
        private string BlobStorageCS;
        private string CosmosDBKey;
        private string CosmosDBUrl = "https://tncdb4test.documents.azure.com:443";

        private readonly CloudStorageAccount BlobStorageAccount;
        private readonly DocumentClient CosmosDBClient;
        private readonly Uri CosmosDBCollectionUri;

        private readonly IKeyVaultAccessModel _kv;

        // Initialize controller with depenedncy injection -  kvInstance singleton
        public StorageController(IKeyVaultAccessModel kvInstance)
        {
            _kv = kvInstance;

            BlobStorageCS = _kv.GetKeyByName("BlobStorageCS").Result.Value;
            CosmosDBKey = _kv.GetKeyByName("CosmosDBKey").Result.Value;

            BlobStorageAccount = CloudStorageAccount.Parse(BlobStorageCS);
            CosmosDBClient = new DocumentClient(new Uri(CosmosDBUrl), CosmosDBKey);
            CosmosDBCollectionUri = UriFactory.CreateDocumentCollectionUri(CosmosDBName, CosmosDBCollectionName);
        }

        // POST api/values
        [HttpPost]
        [Route("Upload2")]
        public async Task<AnimalImage> UploadImage2([FromBody]AnimalImage animalImage)
        {
            try
            {
                var key = await _kv.GetKeyByName("WebSiteKey");
                CloudBlobClient blobClient = BlobStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(BlobStorageContainerName);
                await container.CreateIfNotExistsAsync();

                animalImage.Id = Guid.NewGuid().ToString().ToLowerInvariant();
                animalImage.ImageName = animalImage.ImageName.ToLowerInvariant();
                animalImage.FileFormat = animalImage.FileFormat.ToLowerInvariant();
                animalImage.ImageBlob = $"{animalImage.Id}/{animalImage.ImageName}{animalImage.FileFormat}";

                // This call creates a local CloudBlobContainer object, but does not make a network call
                // to the Azure Storage Service. The container on the service that this object represents may
                // or may not exist at this point. If it does exist, the properties will not yet have been
                // popluated on this object.
                CloudBlockBlob blockblob = container.GetBlockBlobReference(animalImage.ImageBlob);
                animalImage.UploadBlobSASUrl = Utils.GenerateWriteSasUrl(blockblob);

                // upload data to Cosmos DB
                await CosmosDBClient.UpsertDocumentAsync(CosmosDBCollectionUri, animalImage);

                //return blob url for uploading image
                return animalImage;

            }
            catch (Exception ex)
            {
                // TODO: need better exception handling
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        // POST api/values
        [HttpPost]
        [Route("Upload")]
        public async Task<string> UploadImage([FromBody]string animalImageName)
        {
            try
            {
                var animalImage = new AnimalImage();

                var key = await _kv.GetKeyByName("WebSiteKey");
                CloudBlobClient blobClient = BlobStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(BlobStorageContainerName);
                await container.CreateIfNotExistsAsync();

                animalImage.Id = Guid.NewGuid().ToString().ToLowerInvariant();
                animalImage.ImageName = animalImageName.ToLowerInvariant();
                animalImage.ImageBlob = $"{animalImage.Id}/{animalImage.ImageName}";

                // This call creates a local CloudBlobContainer object, but does not make a network call
                // to the Azure Storage Service. The container on the service that this object represents may
                // or may not exist at this point. If it does exist, the properties will not yet have been
                // popluated on this object.
                CloudBlockBlob blockblob = container.GetBlockBlobReference(animalImage.ImageBlob);
                animalImage.UploadBlobSASUrl = Utils.GenerateWriteSasUrl(blockblob);

                // upload data to Cosmos DB
                await CosmosDBClient.UpsertDocumentAsync(CosmosDBCollectionUri, animalImage);

                //return blob url for uploading image
                return animalImage.UploadBlobSASUrl;

            }
            catch (Exception ex)
            {
                // TODO: need better exception handling
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<AnimalImage> Get(string id)
        {
            try
            {
                AnimalImage animalImage;
                var response = await CosmosDBClient.ReadDocumentAsync(
                    UriFactory.CreateDocumentUri(CosmosDBName, CosmosDBCollectionName, id),
                    new RequestOptions() { PartitionKey = new PartitionKey(id) });

                animalImage = (AnimalImage)(dynamic)response.Resource;

                CloudBlobClient blobClient = BlobStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(BlobStorageContainerName);
                CloudBlockBlob blockblob = container.GetBlockBlobReference(animalImage.ImageBlob);

                animalImage.DownloadBlobSASUrl = Utils.GenerateReadSasUrl(blockblob);

                return animalImage;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        [HttpPost]
        [Route("_echo")]
        public async Task<IActionResult> Echo([FromBody] JObject item)
        {
            return this.Ok(new string[] { "Hello", "World", item.ToString() });
        }
    }
}
