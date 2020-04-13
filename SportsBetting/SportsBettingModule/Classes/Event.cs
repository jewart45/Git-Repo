using System;
using System.Collections.Generic;

namespace SportsBettingModule.Classes
{
    public class Event
    {
        public List<ISelection> AllBettingTypes { get; set; }
        public string Name { get; private set; }

        public string Winner { get; set; }
        public string ResultType { get; set; }
        public DateTime Date { get; set; }
        public GoTheDistance GoTheDistance { get; set; } = new GoTheDistance();

        public RoundBetting RoundBetting { get; set; } = new RoundBetting();

        public MatchResult FightResult { get; set; } = new MatchResult();

        public MethodOfVictory MethodOfVictory { get; set; } = new MethodOfVictory();
        public List<Fighter> Fighters { get; set; } = new List<Fighter>();

        public Event(string name, string resultType = null, Fighter f1 = null, Fighter f2 = null)
        {
            Name = name;
            ResultType = resultType;

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