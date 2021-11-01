using Azure;
using Azure.Data.Tables;
using System;

namespace TrainingAZ204.Core
{
    [Serializable]
    public class Person: ITableEntity
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageGuid { get; set; }
    }
}
