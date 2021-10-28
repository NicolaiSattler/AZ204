using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace TrainingAZ204_SqlTableFunc
{
    public static class SqlTableFunc
    {
        private const string INSERT_QUERY = "INSERT INTO person (RowId, PartitionID, FirstName, LastName, ImageGuid) " +
                                            "VALUES ({0},{1},{2},{3},{4})";

        [FunctionName("ProcessPerson")]
        public static async Task Run([QueueTrigger("personqueue", Connection = "ConnectionString")]string item, ILogger log)
        {
            var bytes = Convert.FromBase64String(item);
            var items = Encoding.UTF8.GetString(bytes).Split(';');
            var firstName = items[0];
            var lastName = items[1];
            var imageGuid = items[2];

            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                try
                {
                    connection.Open();

                    var query = string.Format(INSERT_QUERY, Guid.NewGuid(), lastName[0], firstName, lastName, imageGuid);
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
