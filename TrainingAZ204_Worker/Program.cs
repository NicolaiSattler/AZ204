using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TrainingAZ204.Core;

namespace TrainingAZ204_Worker
{
    class Program
    {
        private const int HEIGHT = 50;
        private const int WIDTH = 50;

        private static CloudQueue Queue { get; set; }
        private static CloudTable Table { get; set; }
        private static CloudBlobContainer Blob { get; set; }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            await InitializeAsync();
            await ExecuteAsync();
        }

        private static async Task InitializeAsync()
        {
            var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=trainingnicolaistorage;AccountKey=j9yf9yBgZs5qf0QgIE1M+ZTQU7IAXXT+4w9fyQms8xoT1vnsI8r41aakLy9EkK7DxMk7uLMSZvpMCchAaMJyng==;EndpointSuffix=core.windows.net");
            var queueClient = account.CreateCloudQueueClient();
            var tableClient = account.CreateCloudTableClient();
            var blobClient = account.CreateCloudBlobClient();
            //queuename must be lowercase 
            Queue =queueClient.GetQueueReference("personqueue");
            Table = tableClient.GetTableReference("persontable");
            Blob = blobClient.GetContainerReference("personblob");

            await Queue.CreateIfNotExistsAsync();
            await Table.CreateIfNotExistsAsync();
            await Blob.CreateIfNotExistsAsync();
        }
        private static async Task<string> CreateThumpNailImageAsync(string imageName)
        {
            var blockBlob = Blob.GetBlockBlobReference(imageName);
            using(var largImageStream = new MemoryStream())
            using(var thumpNailStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(largImageStream);

                largImageStream.Position = 0;

                using(var image = Image.Load(largImageStream))
                {
                    
                }


            }

        }

        private static async Task ExecuteAsync()
        {
            var message = await Queue.GetMessageAsync();

            if (message != null && !string.IsNullOrEmpty(message.AsString))
            {
                var bytes = Convert.FromBase64String(message.AsString);
                var items = Encoding.UTF8.GetString(bytes).Split(';');
                var firstName = items[0];
                var lastName = items[1];
                var imageUrl = items[2];
                var person = new Person { FirstName = firstName, LastName = lastName, ImageGuid = imageUrl };
                person.PartitionKey = person.LastName[0].ToString();
                person.RowKey = Guid.NewGuid().ToString();

                await Table.ExecuteAsync(TableOperation.Insert(person));
                await Queue.DeleteAsync();
            }
        }
    }
}
