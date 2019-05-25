using CommonClasses;
using Marketplace;
using SportsBettingModule.Classes;
using SportsDatabaseSqlite;
using SportsDatabaseSqlite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SportsBettingModule
{
    public partial class Logger
    {
        public double BetAmount { get; set; } = 0;

        public double MinBetLevel { get; set; } = 3.00;
        public double MaxBetLevel { get; set; } = 3.60;

        public double BetLimit { get; set; } = 100;

        public bool OrderingActive { get; set; } = false;

        private void MakeBetsOnCurrentOdds(string eventType, List<MarketplaceEvent> list)
        {
            var bal = marketMessenger.GetAccountBalance();

            if (bal.CurrentAvailable <= BetLimit)
            {
                return;
            }

            var listWithOddsInRange = list.Where(x => x.Runners.Where(y => y.Odds.ToDouble() >= MinBetLevel && y.Odds.ToDouble() <= MaxBetLevel).FirstOrDefault() != null).ToList(); // && y.Name == "The Draw"

            var orders = marketMessenger.GetCurrentOffers(eventType, false, listWithOddsInRange);//.Where(x => x.Date.DayOfWeek != DayOfWeek.Saturday && x.Date.DayOfWeek != DayOfWeek.Sunday).ToList()
            var ordersToPlace = new List<MarketplaceBetOrder>();
            using (SportsDatabaseModel db = new SportsDatabaseModel())
            {
                ordersToPlace = orders.Where(x => db.results.Where(y => y.MarketId == x.MarketId && y.EventName == x.EventName && y.SelectionName == x.SelectionName).FirstOrDefault() == null && x.OddsDecimal >= MinBetLevel && x.OddsDecimal <= MaxBetLevel).ToList(); // && x.SelectionName == "The Draw"
            }

            var successfulOrdersList = marketMessenger.PlaceOrders(ordersToPlace, BetAmount);
            var successfullOrders = ordersToPlace.Where(x => successfulOrdersList.Contains(x.EventName));

            using (SportsDatabaseModel db = new SportsDatabaseModel())
            {
                foreach (var o in successfullOrders)
                {
                    db.results.Add(new ResultLog
                    {
                        BetMadeTime = DateTime.Now,
                        Odds = o.Odds,
                        SelectionId = o.SelectionId,
                        MarketId = o.MarketId,
                        OddsDecimal = o.OddsDecimal > o.LastTradedOddsDecimal ? o.OddsDecimal : o.LastTradedOddsDecimal,
                        EventStart = o.EventStart,
                        EventName = o.EventName,
                        EventType = eventType,
                        SelectionName = o.SelectionName,
                        AmountWagered = BetAmount
                    });
                    db.SaveChanges();
                }
            }
        }
    }
}