using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TNCServicesPlatform.StorageAPI.Models;

namespace TNCServicesPlatform.StorageAPI.Controllers
{
    [Route("api/storage/")]
    public class StorageController_AzureStorageAPIController : Controller
    {
        [HttpPost]
        [Route("{type}/_upload")]
        public async Task<IActionResult> UploadFileToAzureBlob([FromBody] JObject item, [FromRoute] string type, [FromQuery] string source = "")
        {
            return this.Ok($"Uploaded {type} from {source}, content {item}");
        }

        // POST api/values
        [HttpPost]
        [Route("upload")]
        public async Task<AnimalImage> UploadImage([FromBody]AnimalImage animalImage)
        {
            /* CloudBlobClient blobClient = this.AppConfiguration.BlobStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(this.AppConfiguration.AnimalImageContainer);

            // Create a new container, if it does not exist
            container.CreateIfNotExists();

            // TODO: validate input
            animalImage.Id = Guid.NewGuid().ToString().ToLowerInvariant();
            animalImage.ImageName = animalImage.ImageName.ToLowerInvariant();
            animalImage.FileFormat = animalImage.FileFormat.ToLowerInvariant();
            animalImage.ImageBlob = animalImage.Id + "/" + animalImage.ImageName +
                "." + animalImage.FileFormat;
            CloudBlockBlob blockblob = container.GetBlockBlobReference(animalImage.ImageBlob);

            await this.AppConfiguration.CosmosDBClient.UpsertDocumentAsync(
                this.AppConfiguration.AnimalImageCollectionUri,
                animalImage);

            animalImage.UploadBlobSASUrl = Utils.GenerateWriteSasUrl(blockblob);
            */

            animalImage.UploadBlobSASUrl = "temp_url";

            return animalImage;
        }

    }
}
