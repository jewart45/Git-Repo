using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttaGlance
{
    [Serializable]
    public class Event
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public Event(DateTime start, DateTime en, string subject = "", string desc = "")
        {
            Start = start;
            End = en;
            Subject = subject;
            Description = desc;
        }

        public bool CheckIfDayIntersects(DateTime day)
        {
            var date = day.Date;

            //if earlier than start
            if(DateTime.Compare(Start.Date, date) < 0)
            {
                return false;
            }
            else if(DateTime.Compare(End, date) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
