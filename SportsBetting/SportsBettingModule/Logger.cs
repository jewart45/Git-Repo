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
        public MarketplaceMessenger marketMessenger { get; set; }
        private List<OddsInfo> oddsList { get; set; }

        private List<Fighter> FightersAvailable { get; set; }
        public Task LoggingTask_Results { get; private set; }

        // Declare the delegate (if using non-generic pattern).
        public delegate void RaiseError(object sender, string error);

        // Declare the event.
        public event RaiseError MarketplaceErrorOccured;

        public Task LoggingTask_OverUnder { get; private set; }
        private bool LoggingCancelFlag { get; set; }
        public bool Logging { get; private set; }

        public TimeSpan logInterval { get; set; } = new TimeSpan(6, 0, 0);
        public TimeSpan shortLogInterval { get; set; } = new TimeSpan(0, 20, 0);

        public Logger(MarketplaceMessenger messenger, TimeSpan t)
        {
            oddsList = new List<OddsInfo>();
            logInterval = t;
            marketMessenger = messenger;
        }

        //public async Task StartLoggingAsync(TimeSpan timeSpan) => await LoggingTask(timeSpan);

        public int RemoveLoggingItem(string selId, string marketId)
        {
            var number = oddsList.RemoveAll(x => x.SelectionID == selId && x.MarketID == marketId);
            return number;
        }

   

        public int RemoveLoggingItem(string itemName, string EventName, string eventType)
        {
            var num = oddsList.RemoveAll(x => x.SelectionName == itemName && x.EventName == EventName && x.ResultType == eventType);

            return num;
        }

        public void FillResults(List<OddsInfo> resultsList)
        {
            foreach (OddsInfo res in resultsList)
            {
                OddsInfo f = oddsList
                    .Where(x => x.EventName == res.EventName && x.SelectionName == res.SelectionName)
                    .FirstOrDefault();
                if (f != null)
                {
                    f.Winner = res.Winner;
                }

                using (SportsDatabaseModel db = new SportsDatabaseModel())
                {
                    List<OddsInfo> k = db.oddsInfo
                        .Where(x => x.EventName == res.EventName && x.SelectionName == res.SelectionName)
                        .ToList();

                    for (int i = 0; i < k.Count; i++)
                    {
                        k[i].Winner = res.Winner;
                    }

                    db.SaveChanges();
                }
            }
        }

        public bool AddLoggingItem(OddsInfo odds)
        {
            if (oddsList.Where(x => x.MarketID == odds.MarketID && x.SelectionID == odds.SelectionID).FirstOrDefault() == null)
            {
                oddsList.Add(odds);
            }

            return true;
        }

        private string LogSpecificOdds(List<MarketplaceEvent> EventListWithOdds, string bettingResultType, bool zeroCheck)
        {
            string error = "";
            List<OddsInfo> listOfOddsToAdd = new List<OddsInfo>();
            List<EventsLookup> listOfEventsToAdd = new List<EventsLookup>();
            try
            {
                if (EventListWithOdds.Count == 0)
                {
                    if (MarketplaceErrorOccured != null)
                    {
                        MarketplaceErrorOccured(this, null);
                    }
                    //Try again in 10 seconds

                    error = "dictionary empty ";
                    Console.WriteLine(error);
                }
                else
                {
                    error = "";
                }
            }
            catch (Exception)
            {
                if (MarketplaceErrorOccured != null)
                {
                    MarketplaceErrorOccured(this, null);
                }

                //Try again
                error = "dictionary empty";
            }

            //Get Odds
            foreach (MarketplaceEvent ev in EventListWithOdds)
            {
                listOfEventsToAdd.Add(ev.ToResultInfo());
                foreach (MarketplaceRunner runner in ev.Runners)
                {
                    if (runner.Odds != "")
                    {
                        OddsInfo info = oddsList.Where(x => x.EventName == ev.Name && x.SelectionName == runner.Name && x.ResultType == ev.ResultType).FirstOrDefault();
                        //Only take bets up to 30 mins before fight
                        if (info != null)
                        {
                            if (info.EventDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                            {
                                info.ResultType = ev.ResultType;
                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(runner.Odds));
                                info.DateTaken = DateTime.Now;
                                listOfOddsToAdd.Add(info);
                            }
                        }
                    }
                }
            }
            using (var db = new SportsDatabaseModel())
            {
                db.AddMultipleOdds(listOfOddsToAdd.Where(x => x.OddsValue != 0 && x.EventDate.CompareTo(DateTime.Now.AddMinutes(30)) > 0).ToList());
                db.AddEventLookups(listOfEventsToAdd);
            }
            //Log Results
            if (OrderingActive)
            {
                MakeBetsOnCurrentOdds(bettingResultType, EventListWithOdds.Where(x => x.Date.CompareTo(DateTime.Now.AddMinutes(30)) > 0).ToList());
            }

            return error;
        }

        public async void StartLoggingAsync(string bettingResultType, string competition)
        {
            LoggingCancelFlag = false;

            Logging = true;

            try
            {
                await Task.Run(async () =>
                {
                    string error = "";
                    MessageBox.Show("Logging Started");
                    List<MarketplaceEvent> EventList = new List<MarketplaceEvent>();
                    List<MarketplaceEvent> EventListWithOdds = new List<MarketplaceEvent>();
                    int i = 0;
                    int errorCount = 0;
                    while (true)
                    {
                        if (LoggingCancelFlag)
                        {
                            break;
                        }
                        if ((i + 1) % shortLogInterval.TotalSeconds == 0)
                        {
                            //fighterDictionary = marketMessenger.GetAllOddsOld(fighterOddsList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.FightDate) > 0).Select(x => x.SelectionID).ToList<string>(), eventType, virtualise);

                            //EventList = marketMessenger.GetEventSelectionIDs(eventType, competition);
                            //EventListWithOdds = marketMessenger.GetSpecificOdds(EventList.Where(x => fighterOddsList.Select(y => y.EventName).Contains(x.Name)).ToList());

                            if (EventList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.Date) > 0).Count() > 0)
                            {
                                EventListWithOdds = marketMessenger.GetSpecificOdds(EventList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.Date) > 0 && oddsList.Select(y => y.EventName).Contains(x.Name)).ToList());

                                error = LogSpecificOdds(EventListWithOdds, bettingResultType, false);
                                //increment
                                //Reset Counter
                                if (error == "" && EventList.Count > 0)
                                {
                                    i++;
                                }
                                else
                                {
                                    //Try again in 10 seconds
                                    i = (int)(logInterval.TotalSeconds) - 10;
                                    Console.WriteLine(error);
                                }
                            }
                            else
                            {
                                i++;
                            }
                        }
                       
                        else if(i > logInterval.TotalSeconds)
                        {
                            List<OddsInfo> listOfOddsToAdd = new List<OddsInfo>();
                            try
                            {
                                //EventList = marketMessenger.GetEventSelectionIDs(eventType, competition);
                                var eventsToCheck = EventList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.Date) < 0 && oddsList.Select(y => y.EventName).Contains(x.Name)).ToList();
                                EventListWithOdds = marketMessenger.GetSpecificOdds(eventsToCheck);
                                if (EventListWithOdds.Count == 0 && eventsToCheck.Count > 0)
                                {
                                    if (MarketplaceErrorOccured != null)
                                    {
                                        MarketplaceErrorOccured(this, null);
                                    }
                                    //Try again in 10 seconds
                                    i = (int)(logInterval.TotalSeconds) - 10;
                                    if (error != "")
                                    {
                                        errorCount++;
                                    }

                                    error = "dictionary empty this many times: " + errorCount.ToString();
                                    Console.WriteLine(error);
                                }
                                else
                                {
                                    error = "";
                                    errorCount = 0;
                                }
                            }
                            catch (Exception)
                            {
                                if (MarketplaceErrorOccured != null)
                                {
                                    MarketplaceErrorOccured(this, null);
                                }

                                //Try again
                                i = (int)(logInterval.TotalSeconds) - 10;
                                error = "dictionary empty";
                                EventList = new List<MarketplaceEvent>();
                                EventListWithOdds = new List<MarketplaceEvent>();
                            }

                            //Get Odds
                            foreach (MarketplaceEvent ev in EventListWithOdds)
                            {
                                foreach (MarketplaceRunner runner in ev.Runners)
                                {
                                    if (runner.Odds != "")
                                    {
                                        OddsInfo info = oddsList.Where(x => x.EventName == ev.Name && x.SelectionName == runner.Name).FirstOrDefault();
                                        //Only take bets up to 30 mins before fight
                                        if (info != null)
                                        {
                                            if (info.EventDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                                            {
                                                info.ResultType = ev.ResultType;
                                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(runner.Odds));
                                                info.DateTaken = DateTime.Now;
                                                listOfOddsToAdd.Add(info);
                                            }
                                        }
                                    }
                                }
                            }

                            using (var db = new SportsDatabaseModel())
                            {
                                db.AddMultipleOdds(listOfOddsToAdd.Where(x => x.OddsValue != 0 && x.EventDate.CompareTo(DateTime.Now.AddMinutes(30)) > 0).ToList());
                            }
                            if (OrderingActive)
                            {
                                MakeBetsOnCurrentOdds(bettingResultType, EventListWithOdds.Where(x => x.Date.CompareTo(DateTime.Now.AddMinutes(30)) > 0).ToList());
                            }

                            //Reset Counter
                            if (error == "")
                            {
                                i = 0;
                            }
                        }
                        else
                        {
                            i++;
                        }
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                    //Increment Counter

                    //IDictionary<string, string> fighterDictionary = marketMessenger.GetFighterDictionary("Match Odds");
                });
                Logging = false;
            }
            catch (Exception ex)
            {
                Logging = false;
                MessageBox.Show("Error Logging, Cancel and start over:  " + ex.ToString());
            }
        }

        public async void StopLoggingAsync()
        {
            LoggingCancelFlag = true;
            await Task.Run(() =>
            {
                //Wait for logging to complete
                while (Logging)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            });
            MessageBox.Show("Logging Ended");
            LoggingTask_Results?.Dispose();
            LoggingTask_OverUnder?.Dispose();
        }

        
        public void LogOddsNow(string resultType, string competition)
        {
            IDictionary<string, string> fighterDictionary;
            //Get Odds
            List<MarketplaceEvent> EventList = new List<MarketplaceEvent>();
            List<MarketplaceEvent> EventListWithOdds = new List<MarketplaceEvent>();

            EventList = marketMessenger.GetEventSelectionIDs(resultType, competition);
            EventListWithOdds = marketMessenger.GetAllOdds(EventList.Where(x => oddsList.Select(y => y.EventName).Contains(x.Name)).ToList(), resultType, competition);
            fighterDictionary = marketMessenger.GetAllOddsOld(oddsList.Select(x => x.SelectionID).ToList<string>(), resultType);
            foreach (MarketplaceEvent ev in EventListWithOdds)
            {
                foreach (MarketplaceRunner runner in ev.Runners)
                {
                    if (runner.Odds != "" && oddsList.Select(x => x.SelectionName).Contains(runner.Name))
                    {
                        OddsInfo info = oddsList.Where(x => x.EventName == ev.Name && x.SelectionName == runner.Name).FirstOrDefault();
                        //Only take bets up to 30 mins before fight
                        if (info != null)
                        {
                            if (info.EventDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                            {
                                info.ResultType = resultType;
                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(runner.Odds));
                                info.DateTaken = DateTime.Now;
                            }
                        }
                    }
                }
            }
            using (SportsDatabaseModel db = new SportsDatabaseModel())
            {
                db.AddMultipleOdds(oddsList.Where(x => x.OddsValue != 0 && x.EventDate.AddMinutes(30).CompareTo(DateTime.Now) > 0).ToList());
                foreach (MarketplaceEvent ev in EventListWithOdds.Where(x => x.Winner != null))
                {
                    foreach (OddsInfo dbOdds in db.oddsInfo.Where(x => x.EventName == ev.Name))
                    {
                        dbOdds.Winner = dbOdds.SelectionName == ev.Winner;
                    }
                }
                db.SaveChanges();
            }

            if (OrderingActive)
            {
                MakeBetsOnCurrentOdds(resultType, EventListWithOdds.Where(x => x.Date.CompareTo(DateTime.Now.AddMinutes(30)) > 0).ToList());
            }
        }

        private long PriceTradedToOdds(double val)
        {
            double line;
            //val = val * 100;
            //Set + or -

            string prefix = val >= 2 ? "+" : "-";
            if (val == 0)
            {
                return 0;
            }

            if (val >= 2)
            {
                line = val * 100 - 100;
                return (long)Convert.ToDecimal(prefix + line.ToString("000"));
            }
            else
            {
                line = 100 / (val - 1);
                return (long)Convert.ToDecimal(prefix + line.ToString("000"));
            }
        }
    }
}