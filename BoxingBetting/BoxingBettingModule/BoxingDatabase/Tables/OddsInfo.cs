using System;

namespace BoxingDatabase.Tables
{
    public partial class OddsInfo
    {
        public long ID { get; set; }
        public DateTime DateTaken { get; set; }
        public string Name { get; set; }
        public long OddsValue { get; set; }
        public long Percent { get; set; }
        public DateTime FightDate { get; set; }
        public string FightName { get; set; }
        public string SelectionID { get; set; }
        public string EventType { get; set; }
    }
}