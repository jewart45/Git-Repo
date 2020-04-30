using System;

namespace SportsDatabaseSqlite.Tables
{
    public partial class OddsInfoMedian
    {
        public long ID { get; set; }
        public DateTime DateTaken { get; set; }
        public string SelectionName { get; set; }
        public string CountryCode { get; set; }
        public long OddsMedian { get; set; }
        public decimal Percent { get; set; }
        public DateTime EventDate { get; set; }
        public string EventName { get; set; }
        public long EventTypeId { get; set; }
        public string SelectionID { get; set; }
        public string MarketID { get; set; }
        public string ResultType { get; set; }
        public bool Winner { get; set; }
    }
}