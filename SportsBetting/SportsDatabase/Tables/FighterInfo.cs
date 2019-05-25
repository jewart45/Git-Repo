using System;

namespace SportsDatabase.Tables
{
    public partial class FighterInfo
    {
        public long ID { get; set; }

        public enum FighterProperty { Name, EventDate, WC, Odds, PercentChance, LastOdds, SelectionID, FightEventName }

        public string Name { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }

        public enum WeightClass { BW, FW, LW, WW, MW, LHW, HW, WSW, WBW, WFW, None }

        public static WeightClass WC { get; private set; }
        public string Odds { get; private set; }
        public string PercentChance { get; set; }
        public string LastOdds { get; set; }
        public string SelectionID { get; set; }
        public string NameNoSpaces { get; set; }
    }
}