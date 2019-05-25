using System;

namespace SportsDatabaseSqlite.Tables
{
    public partial class OddsInfo
    {
        public long ID { get; set; }
        public DateTime DateTaken { get; set; }
        public string SelectionName { get; set; }
        public long OddsValue { get; set; }
        public long Percent { get; set; }
        public DateTime EventDate { get; set; }
        public string EventName { get; set; }
        public string SelectionID { get; set; }
        public string EventType { get; set; }
        public bool Winner { get; set; }
    }
}