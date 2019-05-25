using System;

namespace SportsDatabaseSqlite.Tables
{
    public partial class ResultLog
    {
        public long ID { get; set; }
        public string Reference { get; set; }

        public long SelectionId { get; set; }

        public string MarketId { get; set; }

        public string Odds { get; set; }

        public double OddsDecimal { get; set; }

        public double AmountWagered { get; set; }

        public double AmountMatched { get; set; }

        public double AmountWon { get; set; }

        public DateTime EventStart { get; set; }

        public DateTime BetMadeTime { get; set; }
        public string SelectionName { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public bool Winner { get; set; }
    }
}