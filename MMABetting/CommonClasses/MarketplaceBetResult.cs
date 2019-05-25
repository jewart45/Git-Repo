using System;

namespace CommonClasses
{
    public class MarketplaceBetResult
    {
        public string Name { get; private set; }

        public bool Win { get; set; }

        public string EventId { get; set; }

        public DateTime SettledDate { get; set; }

        public DateTime PlacedDate { get; set; }

        public double Profit { get; set; }

        public double MatchedAmount { get; set; }

        public double Commission { get; set; }

        public string MarketId { get; set; }

        public MarketplaceBetResult(string name, string marketId = null)
        {
            Name = name;
            MarketId = marketId;
        }
    }
}