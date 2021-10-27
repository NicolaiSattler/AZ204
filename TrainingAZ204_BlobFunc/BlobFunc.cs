using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace TrainingAZ204_BlobFunc
{
    public static class BlobFunc
    {
        private const int Dimension = 50;

        [FunctionName("ProcessImage")]

        public static async Task Run([BlobTrigger("personblob/{name}", Connection = "ConnectionString")]Stream myBlob, string name, ILogger log)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var blobRef = client.GetContainerReference("personblobtn");

            await blobRef.CreateIfNotExistsAsync();

            using (var stream = new MemoryStream())
            using(var image = Image.Load(myBlob))
            {
                var blockBlob = blobRef.GetBlockBlobReference(name);
                image.Mutate(x => x.Resize(Dimension, Dimension));

                await image.SaveAsJpegAsync(stream);

                stream.Position = 0;
                await blockBlob.UploadFromStreamAsync(stream);
            }

        }
    }
}
