using System;

namespace SportsBettingModule.Classes
{
    public class ExchangeBet
    {
        public double Odds { get; set; }

        public double OddsDecimal { get; set; }

        public DateTime EventStart { get; set; }

        public DateTime BetMadeTime { get; set; }

        public string EventName { get; set; }

        public string SelectionID { get; set; }

        public string Selection { get; set; }

        public bool Win { get; set; }
    }
}