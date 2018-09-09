using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TNCServicesPlatform.DataModel.Interfaces;
using TNCServicesPlatform.StorageAPI.Common;
using TNCServicesPlatform.StorageAPI.Models;

namespace TNCServicesPlatform.StorageAPI.Controllers
{
    [Route("api/storage/")]
    public class StorageController : Controller
    {
        private const string CosmosDBName = "goldenmonkey";
        private const string CosmosDBCollectionName = "animalimagesDemo";
        private const string BlobStorageContainerName = "animalimages";

        private const string LocationCollectionName = "animalLocation";

        // TODO: use config
        private string BlobStorageCS;
        private string CosmosDBKey;
        private string CosmosDBUrl = "https://tncdb4test.documents.azure.com:443";

        private readonly CloudStorageAccount BlobStorageAccount;
        private readonly DocumentClient CosmosDBClient;
        private readonly Uri CosmosDBCollectionUri;
        private readonly Uri LocationCollectionUri;

        private readonly IKeyVaultAccessModel _kv;

        // Initialize controller with depenedncy injection -  kvInstance singleton
        public  StorageController(IKeyVaultAccessModel kvInstance)
        {
            _kv = kvInstance;

            BlobStorageCS = _kv.GetKeyByName("BlobStorageCS").Result.Value;
            CosmosDBKey = _kv.GetKeyByName("CosmosDBKey").Result.Value;

            BlobStorageAccount = CloudStorageAccount.Parse(BlobStorageCS);
            CosmosDBClient = new DocumentClient(new Uri(CosmosDBUrl), CosmosDBKey);
            CosmosDBCollectionUri = UriFactory.CreateDocumentCollectionUri(CosmosDBName, CosmosDBCollectionName);
            LocationCollectionUri = UriFactory.CreateDocumentCollectionUri(CosmosDBName, LocationCollectionName);



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
                animalImage.ImageName = animalImage.ImageName;
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
        //By Jiacheng Zhu
        //reference:https://docs.microsoft.com/en-us/azure/cosmos-db/sql-api-get-started
        [HttpGet]
        [Route("Tags")]
        public IEnumerable<string> GetTags()
        {
            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

                IQueryable<AnimalImage> animalImageQuery = CosmosDBClient.CreateDocumentQuery<AnimalImage>(
                        UriFactory.CreateDocumentCollectionUri(CosmosDBName, CosmosDBCollectionName), queryOptions)
                    .Where(f => f.Tag != null);


                return new List<AnimalImage>(animalImageQuery).Select(a => a.Tag);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        //By Jiacheng Zhu
        //reference:https://docs.microsoft.com/en-us/azure/cosmos-db/sql-api-get-started
        [HttpGet]
        [Route("GetByTags/{tag}")]
        public IEnumerable<string> GetidByTag(string tag)
        {
            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

                IQueryable<AnimalImage> animalImageQuery = CosmosDBClient.CreateDocumentQuery<AnimalImage>(
                        UriFactory.CreateDocumentCollectionUri(CosmosDBName, CosmosDBCollectionName), queryOptions)
                    .Where(f => f.Tag == tag);



                return new List<AnimalImage>(animalImageQuery).Select(a => a.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPut]
        public async  Task<AnimalImage> CreateUpdateRecordByName([FromBody]AnimalImage image)
        {
            //DocumentCollection coll = await CosmosDBClient.CreateDocumentCollectionIfNotExistsAsync(new Uri(CosmosDBUrl),
            //    new DocumentCollection { Id = CosmosDBCollectionName },
            //    new RequestOptions { OfferThroughput = 10000 });

            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            var ifexist =CosmosDBClient.CreateDocumentQuery<AnimalImage>(
                UriFactory.CreateDocumentCollectionUri(CosmosDBName, CosmosDBCollectionName), queryOptions).Where(u => u.ImageName == image.ImageName);
            //if (!ifexist.Any())
            //{
            //    return await UploadImage2(image);
            //}
            var result = new List<AnimalImage>(ifexist);
            if (result.Count==0)
            {
                return await UploadImage2(image);
            }
            else
            {
	            result[0].Tag = image.Tag;
                result[0].LocationName = image.LocationName;
                await CosmosDBClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(CosmosDBName, CosmosDBCollectionName, result[0].Id),result[0]);
                CloudBlobClient blobClient = BlobStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(BlobStorageContainerName);
                CloudBlockBlob blockblob = container.GetBlockBlobReference(result[0].ImageBlob);
                result[0].UploadBlobSASUrl = Utils.GenerateWriteSasUrl(blockblob);
                return result[0];
            }
        }

        [HttpPut]
        [Route("location")]
        public async Task<AnimalLocation> CreateUpdateLocationByName([FromBody] AnimalLocation location)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            var ifexist = CosmosDBClient.CreateDocumentQuery<AnimalLocation>(
                UriFactory.CreateDocumentCollectionUri(CosmosDBName, LocationCollectionName), queryOptions).Where(u => u.Name == location.Name);
            var result = new List<AnimalLocation>(ifexist);
            if (result.Count == 0)
            {
                location.Id= Guid.NewGuid().ToString().ToLowerInvariant();
                await CosmosDBClient.UpsertDocumentAsync(LocationCollectionUri, location);
                return location;
            }
            else
            {
                result[0].Latitude = location.Longtitude;
                result[0].Longtitude = location.Latitude;
                await CosmosDBClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(CosmosDBName, CosmosDBCollectionName, result[0].Id), result[0]);
                return result[0];
            }
        }
        [HttpPost]
        [Route("_echo")]
        public string Echo([FromBody] string item)
        {
            return "Hello World " + item;
        }

        /*
        [HttpPost]
        [Route("UploadFiles")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<String> Upload([FromBody]IFormFile file)
        {
            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            try
            {
                var fileName = file.FileName;
                Uri uri = await this.UploadToAzure(fileName, file);
                string photoUrl = uri.ToString();
                return photoUrl;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return "TNC uploading failed.";
            }
        }

        const string STORAGE_ACCOUNT = "AZURE_STORAGE_CONNECTION_STRING";
        private async Task<Uri> UploadToAzure(string filename, IFormFile file)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable(STORAGE_ACCOUNT));
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");

            // Create the container if it doesn't already exist.
            await container.CreateIfNotExistsAsync();

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);

            // Create or overwrite the "myblob" blob with contents from a local file.
            await blockBlob.UploadFromStreamAsync(file.OpenReadStream());
            return blockBlob.Uri;
        }
        */
    }
}
