using System;

namespace BetHistoryImport.Classes
{
    public class Fighter
    {
        public enum FighterProperty { Name, WC, Odds, PercentChance, LastOdds, SelectionID }

        public string Name { get; private set; }

        public enum WeightClass { BW, FW, LW, WW, MW, LHW, HW, WSW, WBW, WFW, None }

        public static WeightClass WC { get; private set; }
        public string Odds { get; private set; }

        public double Multiplier { get; private set; }
        public string PercentChance => OddsToChance(Odds);

        public long PercentChanceDecimal => OddsToChanceLong(Odds);
        public string LastOdds { get; private set; }
        public string SelectionID { get; set; }
        public string NameNoSpaces => Name.Replace(" ", "");

        public Fighter(string name, string selectionId)
        {
            Name = name;
            SelectionID = selectionId;
            Multiplier = 0;
            Odds = "0";
            LastOdds = "0";
        }

        public Fighter(string name, string selectionId, string odds)
        {
            Name = name;
            SelectionID = selectionId;
            Multiplier = 0;
            PriceTradedtoOdds(Convert.ToDouble(odds));
            LastOdds = "0";
        }

        public Fighter(string name)
        {
            SelectionID = "";
            Name = name;
            Multiplier = 0;
            Odds = "0";
            LastOdds = "0";
        }

        public void PriceTradedtoOdds(double val)
        {
            Multiplier = val;
            LastOdds = Odds;

            double line;
            //val = val * 100;
            //Set + or -

            string prefix = val >= 2 ? "+" : "-";
            if (val == 0) Odds = "-";
            if (val >= 2)
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

        public string OddsToChance(string Odds)
        {
            if (Odds == "-") return "0";
            double chance;
            double oddsDbl = Convert.ToDouble(Odds);
            if (oddsDbl > 0) chance = 100 / (oddsDbl + 100);
            else
            {
                oddsDbl *= -1;
                chance = oddsDbl / (100 + oddsDbl);
            }
            return (chance * 100).ToString("#.##") + "%";
        }

        public long OddsToChanceLong(string Odds)
        {
            if (Odds == "-") return 0;
            long chance;
            long oddsDbl = (long)Convert.ToDecimal(Odds);
            if (oddsDbl > 0) chance = 100 / (oddsDbl + 100);
            else
            {
                oddsDbl *= -1;
                chance = oddsDbl / (100 + oddsDbl);
            }
            return chance;
        }
    }
}