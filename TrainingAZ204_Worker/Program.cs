using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using TrainingAZ204.Core;

namespace TrainingAZ204_Worker
{
    class Program
    {
        private static CloudQueue Queue { get; set; }
        private static CloudTable Table { get; set; }

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
            //queuename must be lowercase 
            Queue =queueClient.GetQueueReference("personqueue");
            Table = tableClient.GetTableReference("persontable");

            await Queue.CreateIfNotExistsAsync();
            await Table.CreateIfNotExistsAsync();
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
                var person = new Person(firstName, lastName);
                person.PartitionKey = person.LastName[0].ToString();
                person.RowKey = Guid.NewGuid().ToString();

                await Table.ExecuteAsync(TableOperation.Insert(person));
                await Queue.DeleteAsync();
            }
        }
    }
}
