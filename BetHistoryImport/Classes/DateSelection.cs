using System;

namespace BetHistoryImport.Classes
{
    internal class DateSelection
    {
        public enum ComboSelection { Upcoming, LastWeek, LastMonth, All }

        public DateTime Start;
        public DateTime End;
    }
}