using SoccerDatabase.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerBettingModule.Classes
{
    public static class Converters
    {
        public static double ToDouble(this object s)
        {
            try
            {
                return Convert.ToDouble(s);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static List<OddsInfo> ToOddsInfo(this ISelection f, Event ev)
        {
            List<OddsInfo> list = new List<OddsInfo>();
            foreach (Runner run in f.Runners)
            {
                list.Add(run.Odds != "-" ?
                new OddsInfo() { EventDate = ev.Date, EventName = ev.Name, SelectionName = run.Name, OddsValue = (long)Convert.ToDecimal(run.Odds), Percent = (long)Convert.ToDecimal(run.PercentChanceDecimal), SelectionID = run.SelectionID, EventType = f.Name }
                : new OddsInfo() { EventDate = ev.Date, EventName = ev.Name, SelectionName = run.Name, OddsValue = 0, Percent = 0, SelectionID = run.SelectionID, EventType = f.Name });
            }
            if (f.Winner != null)
            {
                list.Find(x => x.SelectionName == f.Winner).Winner = true;
                foreach (OddsInfo k in list.Where(x => x.SelectionName != f.Winner))
                {
                    k.Winner = false;
                }
            }

            return list;
        }

        public static long PercentToDouble(this string percent)
        {
            if (percent == "-")
            {
                return 0;
            }

            long chance;
            long oddsDbl = (long)Convert.ToDecimal(percent);
            if (oddsDbl > 0)
            {
                chance = 100 / (oddsDbl + 100);
            }
            else
            {
                oddsDbl *= -1;
                chance = oddsDbl / (100 + oddsDbl);
            }
            return chance;
        }

        public static string Name(this Fighter f) => f.Name;

        public static string NameNoSpaces(this Fighter f) => f.NameNoSpaces;

        public static string RemoveSpaces(this string s) => s.Replace(" ", "");
    }
}