﻿using System;
using System.Collections.Generic;

namespace SportsBettingModule.Classes
{
    public class OtherResult : ISelection
    {
        public string EventName { get; set; }
        public string Name { get; set; }
        public string MarketId { get; set; }
        public string Winner { get; set; }
        public List<Runner> Runners { get; set; } = new List<Runner>();
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

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}