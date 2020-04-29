using CommonClasses;
using SportsDatabaseSqlite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetHistoryImport.Classes
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

        public static bool CompareList(this IEnumerable<string> list1, IEnumerable<string> list2)
        {
            var res = true;
            if (list1.Count() != list2.Count())
                return false;
            foreach(var entry in list1)
            {
                res &= list2.Contains(entry);
            }
            return res;
        }

        public static MarketplaceEvent ToMarketplaceEvent(this ISelection f, Event ev)
        {
            var result = ev.OtherResults.Find(x => x.Name == f.Name);
            MarketplaceEvent mEv = new MarketplaceEvent(result.Name, result.MarketId, result.Name) { Date = ev.Date, Winner = result.Winner };
            foreach(var r in f.Runners)
            {
                mEv.Runners.Add(new MarketplaceRunner(r.Name, r.SelectionID, r.Odds == "-" ? "0" : r.Odds));
            }

            if(ev.Winner != null)
            {
                //Todo: Implement this
            }

            return mEv;
        }

        

        public static List<OddsInfo> ToOddsInfo(this ISelection f, Event ev)
        {
            List<OddsInfo> list = new List<OddsInfo>();
            foreach(var result in ev.OtherResults)
            {
                foreach (RunnerSel run in f.Runners)
                {
                    list.Add(run.Odds != "-" ?
                    new OddsInfo() { EventDate = ev.Date, EventName = ev.Name, SelectionName = run.Name, OddsValue = (long)Convert.ToDecimal(run.Odds), Percent = (long)Convert.ToDecimal(run.PercentChanceDecimal), SelectionID = run.SelectionID, ResultType = f.Name, MarketID = result.MarketId}
                    : new OddsInfo() { EventDate = ev.Date, EventName = ev.Name, SelectionName = run.Name, OddsValue = 0, Percent = 0, SelectionID = run.SelectionID, ResultType = f.Name, MarketID = result.MarketId });
                }
                if (f.Winner != null)
                {
                    list.Find(x => x.SelectionName == f.Winner).Winner = true;
                    foreach (OddsInfo k in list.Where(x => x.SelectionName != f.Winner))
                    {
                        k.Winner = false;
                    }
                }
            }
           

            return list;
        }

        public static EventsLookup ToResultInfo(this MarketplaceEvent ev) => new EventsLookup
        {
            DateTaken = DateTime.Now,
            EventDate = ev.Date,
            EventName = ev.Name,
            ResultType = ev.ResultType,
            MarketId = ev.MarketId
        };

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