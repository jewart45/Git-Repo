using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMABettingModule.Classes
{
    public class Runner
    {
        public string Name { get; set; }
        public string SelectionID { get; set; }
        public string Odds { get; set; }
        public string NameNoSpaces => Name.Replace(" ", "");

        public string PercentChance => OddsToChance(Odds);

        public long PercentChanceDecimal => OddsToChanceLong(Odds);

        public double Multiplier { get; private set; }
        public string LastOdds { get; private set; }

        public Runner(string name, string selectionId, string odds)
        {
            Name = name;
            SelectionID = selectionId;
            PriceTradedtoOdds(Convert.ToDouble(odds));
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

        public void PriceTradedtoOdds(double val)
        {
            Multiplier = val;
            LastOdds = Odds;

            double line;
            //val = val * 100;
            //Set + or -

            string prefix = val >= 2 ? "+" : "-";
            if (val == 0) Odds = "-";
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
}