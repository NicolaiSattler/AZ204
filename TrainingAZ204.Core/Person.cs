using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace TrainingAZ204.Core
{
    [Serializable]
    public class Person: TableEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageGuid { get; set; }
    }
}
