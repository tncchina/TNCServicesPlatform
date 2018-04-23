using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TNCServicesPlatform.StorageAPI.Common;
using TNCServicesPlatform.StorageAPI.Models;

namespace TNCServicesPlatform.StorageAPI.Controllers
{
    [Route("api/storage/")]
    public class StorageController_AzureStorageAPIController : Controller
    {
        private const string CosmosDBName = "goldenmonkey";
        private const string CosmosDBCollectionName = "animalimages";
        private const string BlobStorageContainerName = "animalimages";

        // TODO: use KeyVault
        private const string BlobStorageCS = "";
        private const string CosmosDBUrl = "";
        private const string CosmosDBKey = "";

        private static readonly CloudStorageAccount BlobStorageAccount = CloudStorageAccount.Parse(BlobStorageCS);
        private static readonly DocumentClient CosmosDBClient = new DocumentClient(new Uri(CosmosDBUrl), CosmosDBKey);
        private static readonly Uri CosmosDBCollectionUri = UriFactory.CreateDocumentCollectionUri(CosmosDBName, CosmosDBCollectionName);

        // POST api/values
        [HttpPost]
        [Route("Upload")]
        public async Task<AnimalImage> UploadImage([FromBody]AnimalImage animalImage)
        {
            CloudBlobClient blobClient = BlobStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(BlobStorageContainerName);
            await container.CreateIfNotExistsAsync();

            animalImage.Id = Guid.NewGuid().ToString().ToLowerInvariant();
            animalImage.ImageName = animalImage.ImageName.ToLowerInvariant();
            animalImage.FileFormat = animalImage.FileFormat.ToLowerInvariant();
            animalImage.ImageBlob = $"{animalImage.Id}/{animalImage.ImageName}.{animalImage.FileFormat}";

            // This call creates a local CloudBlobContainer object, but does not make a network call
            // to the Azure Storage Service. The container on the service that this object represents may
            // or may not exist at this point. If it does exist, the properties will not yet have been
            // popluated on this object.
            CloudBlockBlob blockblob = container.GetBlockBlobReference(animalImage.ImageBlob);
            animalImage.UploadBlobSASUrl = Utils.GenerateWriteSasUrl(blockblob);

            // upload data to Cosmos DB
            await CosmosDBClient.UpsertDocumentAsync(CosmosDBCollectionUri, animalImage);

            /* debugging only
            string blobContent = "Simulating uploading a faked image";
            CloudBlockBlob blob = new CloudBlockBlob(new Uri(animalImage.UploadBlobSASUrl));
            MemoryStream msWrite = new MemoryStream(Encoding.UTF8.GetBytes(blobContent));
            msWrite.Position = 0;
            using (msWrite)
            {
                await blob.UploadFromStreamAsync(msWrite);
            }       
             */

            //return blob url for uploading image
            return animalImage;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<AnimalImage> Get(string id)
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
    }
}
