using System;
using System.Collections.Generic;

namespace MMABettingModule.Classes
{
    public class Event
    {
        public enum ResultType { None, FightResult, GoTheDistance }

        public List<ISelection> AllBettingTypes { get; set; }
        public string Name { get; private set; }

        public string Winner { get; set; }
        public DateTime Date { get; set; }
        public GoTheDistance GoTheDistance { get; set; } = new GoTheDistance();

        public RoundBetting RoundBetting { get; set; } = new RoundBetting();

        public FightResult FightResult { get; set; } = new FightResult();

        public MethodOfVictory MethodOfVictory { get; set; } = new MethodOfVictory();
        public List<Fighter> Fighters { get; set; } = new List<Fighter>();

        public Event(string name, Fighter f1 = null, Fighter f2 = null)
        {
            Name = name;
            if (f1 != null)
            {
                Fighters.Add(f1);
            }

            if (f2 != null)
            {
                Fighters.Add(f2);
            }
        }
    }
}