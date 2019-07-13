using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttaGlance
{
    public class Calendar
    {
        public string Id { get; set; } 
        public string Name { get; set; } 

        public List<Event> EventsList { get; set; } = new List<Event>();
        public Calendar(string id = null, string name = null)
        {
            Name = name ?? "User";
            Id = id ?? "no Id";
        }


    }
}
