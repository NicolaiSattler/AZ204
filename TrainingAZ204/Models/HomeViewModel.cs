using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainingAZ204.Core;

namespace TrainingAZ204.Models
{
    public class HomeViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Person> PersonCollection { get; set; }
    }
}
