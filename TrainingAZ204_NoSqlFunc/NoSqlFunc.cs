using System;
using System.Text;
using System.Threading.Tasks;
using Kevsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using TrainingAZ204.Core;

namespace TrainingAZ204_NoSqlFunc
{
    public static class NoSqlFunc
    {
        [FunctionName("ProcessPersonQueue")]
        public static async Task Run([QueueTrigger("personqueue", Connection = "AzureStorageAccount")]string item,
                               [MongoDb(CollectionName = "persons", 
                                        ConnectionStringSetting = "CosmosDbConnection", 
                                        DatabaseName = "TrainingAZ204", 
                                        IdType = typeof(string), ReadOnly = false)] 
                                    IAsyncCollector<Registration> registrations,
                               ILogger log)
        {
            //Does not work yet -.-
            var bytes = Convert.FromBase64String(item);
            var items = Encoding.UTF8.GetString(bytes).Split(';');
            await registrations.AddAsync(new Registration
            {
                _id = new ObjectId(),
                FirstName = items[0],
                LastName = items[1],
                ImageGuid = items[2]
            });
        }
    }
}
