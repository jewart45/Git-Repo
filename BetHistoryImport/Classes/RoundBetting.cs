using System;
using System.Collections.Generic;

namespace BetHistoryImport.Classes
{
    public class OtherResult : ISelection
    {
        public string EventName { get; set; }
        public string Name { get; set; }
        public string MarketId { get; set; }
        public string Winner { get; set; }
        public List<RunnerSel> Runners { get; set; } = new List<RunnerSel>();
        public DateTime Date { get; internal set; }

        public OtherResult(string eventName, string name, string id = null, DateTime dt = new DateTime())
        {
            EventName = eventName;
            Name = name;
            MarketId = id;
            Date = dt;
        }


        public OtherResult(string name, string id = null)
        {
            Name = name;
            MarketId = id;
        }

        public void AddRunner(RunnerSel runner) => Runners.Add(runner);
    }
}