using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses
{
    public class MarketplaceEvent
    {
        public string Name { get; private set; }

        public string Winner { get; set; }
        public DateTime Date { get; set; }

        public string MarketId { get; set; }

        public List<MarketplaceRunner> Runners { get; set; }

        public MarketplaceEvent(string name, string marketId = null)
        {
            Name = name;
            Runners = new List<MarketplaceRunner>();
            MarketId = marketId;
        }

        public MarketplaceEvent(string name, DateTime date, string marketId = null)
        {
            Name = name;
            Runners = new List<MarketplaceRunner>();
            Date = date;
            MarketId = marketId;
        }
    }
}