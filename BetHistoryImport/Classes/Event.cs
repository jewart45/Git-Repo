using CommonClasses;
using System;
using System.Collections.Generic;

namespace BetHistoryImport.Classes
{
    public class Event
    {
        public List<ISelection> AllBettingTypes { get; set; }
        public string Name { get; private set; }

        public string Winner { get; set; }
        public string ResultType { get; set; }

        public string Id { get; set; }
        public DateTime Date { get; set; }
        public GoTheDistance GoTheDistance { get; set; } = new GoTheDistance();

        public List<OtherResult> OtherResults { get; set; } = new List<OtherResult>();

        public MatchResult MatchResult { get; set; } = new MatchResult();

        public MethodOfVictory MethodOfVictory { get; set; } = new MethodOfVictory();
        public List<Fighter> Fighters { get; set; } = new List<Fighter>();

        public Event(string name, string id = null, string resultType = null, Fighter f1 = null, Fighter f2 = null)
        {
            Name = name;
            ResultType = resultType;
            Id = id;

            if (f1 != null)
            {
                Fighters.Add(f1);
            }

            if (f2 != null)
            {
                Fighters.Add(f2);
            }
        }

        internal void Update(MarketplaceEvent x)
        {
            Date = x.Date;
            //Fighters.Find(x=> x.Name == )
        }

        public void AddOtherResult(OtherResult res)
        {
            res.Date = Date;
            res.EventName = Name;
            OtherResults.Add(res);
        }
    }
}