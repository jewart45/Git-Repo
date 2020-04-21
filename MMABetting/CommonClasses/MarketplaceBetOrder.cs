using System;

namespace CommonClasses
{
    public class MarketplaceBetOrder
    {
        public string Reference { get; private set; }

        public string SelectionName { get; set; }

        public long SelectionId { get; set; }

        public string MarketId { get; set; }

        public string Odds { get; set; }

        public string EventName { get; set; }
        public string ResultType { get; set; }

        public double OddsDecimal { get; set; }

        public double LastTradedOddsDecimal { get; set; }

        public DateTime EventStart { get; set; }

        public DateTime BetMadeTime { get; set; }

        public MarketplaceBetOrder(string reff, string marketId, string selectionId, double? odds, string selectionName)
        {
            Reference = reff;
            MarketId = marketId;
            SelectionId = (long)Convert.ToDecimal(selectionId);
            OddsDecimal = (double)odds;
            PriceTradedtoOdds((double)odds);
            SelectionName = selectionName;
        }

        public void PriceTradedtoOdds(double val)
        {
            double line;
            //val = val * 100;
            //Set + or -

            string prefix = val >= 2 ? "+" : "-";
            if (val == 0)
            {
                Odds = "-";
            }
            else if (val >= 2)
            {
                line = val * 100 - 100;
                Odds = prefix + line.ToString("000");
            }
            else
            {
                line = 100 / (val - 1);
                Odds = prefix + line.ToString("000");
            }
        }
    }

    public class MarketplaceAccountBalance
    {
        public double CurrentAvailable { get; set; }

        public double Exposure { get; set; }

        public double Total => CurrentAvailable - Exposure;
    }
}