using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingAZ204.Core
{
    public class Registration
    {
        public ObjectId _id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageGuid { get; set; }
    }
}
