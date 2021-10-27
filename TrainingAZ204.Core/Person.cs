using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace TrainingAZ204.Core
{
    [Serializable]
    public class Person: TableEntity
    {

        [JsonProperty]
        public string FirstName { get; }

        [JsonProperty]
        public string LastName { get; }

        public Person() {  }
        public Person(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
