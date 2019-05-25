using System;

namespace SoccerBettingModule.Classes
{
    internal class DateSelection
    {
        public enum ComboSelection { Upcoming, LastWeek, LastMonth, All }

        public DateTime Start;
        public DateTime End;
    }
}