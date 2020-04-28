using System;
using System.CodeDom;
using System.Collections.Generic;

namespace CommonClasses
{
    public class MarketplaceEvent
    {
        public string Name { get; set; }

        public string Winner { get; set; }
        public DateTime Date { get; set; }

        public string ResultType { get; set; }
        public string Competition { get; set; }

        public string MarketId { get; private set; }

        public List<MarketplaceRunner> Runners { get; set; }

        public MarketplaceEvent(string name, string marketId = null, string resultType = null)
        {
            Name = name;
            Runners = new List<MarketplaceRunner>();
            ResultType = resultType;
            MarketId = marketId;
        }

        

        public MarketplaceEvent(string name, DateTime date, string marketId = null, string resultType = null)
        {
            Name = name;
            Runners = new List<MarketplaceRunner>();
            Date = date;
            MarketId = marketId;
            ResultType = resultType;
        }

        public bool Compare(MarketplaceEvent incoming)
        {
            bool result = false;
            if (this.Date != incoming.Date || this.Name != incoming.Name)
                return false;
            else if (this.Runners.Count != incoming.Runners.Count)
                return false;
            else
            {
                foreach(MarketplaceRunner r in this.Runners)
                {
                    var t = incoming.Runners.Find(x => x.SelectionID == r.SelectionID);
                    if (t == null)
                        return false;
                    else if (t.Odds != r.Odds || t.Name != r.Name)
                        return false;

                }
            }
            return true;
        }

        public MarketplaceEvent Copy()
        {
            var ev = new MarketplaceEvent(Name, MarketId, ResultType);
            foreach (var r in Runners)
            {
                ev.Runners.Add(new MarketplaceRunner(r.Name, r.SelectionID, r.Odds));
            }
            ev.Date = Date;
            ev.Competition = Competition;
            ev.Winner = Winner;

            return ev;
        }
    }
}