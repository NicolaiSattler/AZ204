using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TrainingAZ204.Core;

namespace TrainingAZ204_QueueFunc
{
    public static class TableFunc
    {
        [FunctionName("ProcessPerson")]
        public static async Task Run([QueueTrigger("personqueue", Connection = "ConnectionString")]string item, ILogger log)
        {
            var bytes = Convert.FromBase64String(item);
            var items = Encoding.UTF8.GetString(bytes).Split(';');
            var firstName = items[0];
            var lastName = items[1];
            var imageGuid = items[2];

            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference("persontable");

            await table.CreateIfNotExistsAsync();

            var person = new Person { FirstName = firstName, LastName = lastName, ImageGuid = imageGuid };
            person.PartitionKey = person.LastName[0].ToString();
            person.RowKey = Guid.NewGuid().ToString();

            await table.ExecuteAsync(TableOperation.Insert(person));
        }
    }
}
