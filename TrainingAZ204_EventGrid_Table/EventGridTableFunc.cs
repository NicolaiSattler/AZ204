using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text;
using System;
using System.Threading.Tasks;
using TrainingAZ204.Core;
using Azure.Messaging.EventGrid;

namespace TrainingAZ204_EventGrid_Table
{
    public static class EventGridTableFunc
    {
        [FunctionName("ProcessPerson_EventGrid")]
        //[return: Table("persontable", Connection="AzureStorageAccount")] // cant find correct attribute??
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            var data = eventGridEvent.Data.ToString();
            var bytes = Convert.FromBase64String(data);
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
 
            log.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}
