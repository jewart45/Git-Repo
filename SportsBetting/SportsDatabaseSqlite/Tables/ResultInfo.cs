using System;

namespace SportsDatabaseSqlite.Tables
{
    public partial class EventsLookup
    {
        public long ID { get; set; }
        public DateTime DateTaken { get; set; }
        public DateTime EventDate { get; set; }
        public string MarketId { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public bool Winner { get; set; }
    }
}