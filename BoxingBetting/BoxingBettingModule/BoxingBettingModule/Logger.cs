using BoxingBettingModule.Classes;
using BoxingDatabase;
using BoxingDatabase.Tables;
using CommonClasses;
using Marketplace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BoxingBettingModule
{
    public class Logger
    {
        public MarketplaceMessenger marketMessenger { get; set; }
        private List<OddsInfo> fighterOddsList { get; set; }

        private List<Fighter> FightersAvailable { get; set; }
        public Task LoggingTask_Results { get; private set; }

        // Declare the delegate (if using non-generic pattern).
        public delegate void RaiseError(object sender, string error);

        // Declare the event.
        public event RaiseError MarketplaceErrorOccured;

        public Task LoggingTask_OverUnder { get; private set; }
        private bool LoggingCancelFlag { get; set; }
        public bool Logging { get; private set; }

        public TimeSpan logInterval { get; set; } = new TimeSpan(0, 30, 0);

        public Logger(MarketplaceMessenger messenger, TimeSpan t)
        {
            fighterOddsList = new List<OddsInfo>();
            logInterval = t;
            marketMessenger = messenger;
        }

        //public async Task StartLoggingAsync(TimeSpan timeSpan) => await LoggingTask(timeSpan);

        public bool RemoveLoggingItem(string itemName, string fightName, string eventType)
        {
            OddsInfo fighterItem = fighterOddsList.Where(x => x.Name == itemName && x.FightName == fightName && x.EventType == eventType).FirstOrDefault();
            if (fighterItem != null)
            {
                fighterOddsList.Remove(fighterItem);
            }

            return true;
        }

        public bool AddLoggingItem(OddsInfo odds)
        {
            if (fighterOddsList.Where(x => x.FightName == odds.FightName && x.Name == odds.Name).FirstOrDefault() == null)
            {
                fighterOddsList.Add(odds);
            }

            return true;
        }

        public async void StartLoggingAsync(string eventType)
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
                    IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
                    List<OddsInfo> oddsToAdd = new List<OddsInfo>();
                    int i = 0;
                    int errorCount = 0;
                    while (true)
                    {
                        if (LoggingCancelFlag)
                        {
                            break;
                        }

                        if ((i + 1) % TimeSpan.FromHours(1).TotalSeconds == 0)
                        {
                            EventList = marketMessenger.GetEventSelectionIDs(eventType, true);
                            EventListWithOdds = marketMessenger.GetAllOdds(EventList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.Date) > 0).ToList(), eventType, false);
                            //fighterDictionary = marketMessenger.GetAllOddsOld(fighterOddsList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.FightDate) > 0).Select(x => x.SelectionID).ToList<string>(), eventType, virtualise);

                            error = LogSpecificOdds(EventListWithOdds, eventType, false);
                            //increment
                            //Reset Counter
                            if (error == "")
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
                        else if (i < logInterval.TotalSeconds)
                        {
                            i++;
                        }
                        else
                        {
                            try
                            {
                                EventList = marketMessenger.GetEventSelectionIDs(eventType, true);
                                EventListWithOdds = marketMessenger.GetAllOdds(EventList, eventType, false);
                                if (EventListWithOdds.Count == 0)
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
                                //fighterDictionary = marketMessenger.GetAllOddsOld(fighterOddsList.Select(x => x.SelectionID).ToList<string>(), eventType);
                                //if (fighterDictionary.Count == 0)
                                //{
                                //    if (MarketplaceErrorOccured != null)
                                //        MarketplaceErrorOccured(this, null);
                                //    //Try again in 10 seconds
                                //    i = (int)(logInterval.TotalSeconds) - 10;
                                //    if (error != "") errorCount++;
                                //    error = "dictionary empty this many times: " + errorCount.ToString();
                                //    Console.WriteLine(error);
                                //}
                                //else
                                //{
                                //    error = "";
                                //    errorCount = 0;
                                //}
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
                                fighterDictionary = new Dictionary<string, string>();
                            }

                            oddsToAdd.Clear();
                            //Get Odds
                            foreach (MarketplaceEvent ev in EventListWithOdds)
                            {
                                foreach (MarketplaceRunner runner in ev.Runners)
                                {
                                    if (runner.Odds != "")
                                    {
                                        OddsInfo info = fighterOddsList.Where(x => x.FightName == ev.Name && x.Name == runner.Name.Replace(" ", "")).FirstOrDefault();
                                        //Only take bets up to 30 mins before fight
                                        if (info != null)
                                        {
                                            if (info.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                                            {
                                                info.EventType = eventType;
                                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(runner.Odds));
                                                info.DateTaken = DateTime.Now;
                                                oddsToAdd.Add(info);
                                            }
                                        }
                                    }
                                }
                            }

                            using (var db = new BoxingDatabaseModel())
                            {
                                db.AddMultipleOdds(oddsToAdd.Where(x => x.OddsValue != 0 && x.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0).ToList());
                            }

                            //Reset Counter
                            if (error == "")
                            {
                                i = 0;
                            }
                        }
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                        //    foreach (var f in fighterDictionary)
                        //    {
                        //        if (f.Value != "")
                        //        {
                        //            var info = fighterOddsList.Where(x => x.SelectionID == f.Key).First();
                        //            //Only take bets up to 30 mins before fight
                        //            if (info.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                        //            {
                        //                info.EventType = eventType;
                        //                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(f.Value));
                        //                info.DateTaken = DateTime.Now;
                        //            }
                        //        }
                        //    }
                        //    using (var db = new BoxingDatabaseModel())
                        //    {
                        //        db.AddMultipleOdds(fighterOddsList.Where(x => x.OddsValue != 0 && x.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0).ToList());
                        //    }
                        //    //Reset Counter
                        //    if (error == "")
                        //    {
                        //        i = 0;
                        //    }
                        //}
                        //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
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

        private string LogSpecificOdds(List<MarketplaceEvent> events, string eventType, bool zeroCheck)
        {
            string error = "";
            List<OddsInfo> listOfOddsToAdd = new List<OddsInfo>();
            try
            {
                if (events.Count == 0 && zeroCheck && fighterOddsList.Count <= 0)
                {
                    if (MarketplaceErrorOccured != null)
                    {
                        MarketplaceErrorOccured(this, null);
                    }
                }
            }
            catch (Exception)
            {
                if (MarketplaceErrorOccured != null)
                {
                    MarketplaceErrorOccured(this, null);
                }

                error = "dictionary empty";
            }

            //Get Odds
            //foreach (KeyValuePair<string, string> f in fighterDictionary)
            //{
            //    if (f.Value != "")
            //    {
            //        OddsInfo info = fighterOddsList.Where(x => x.SelectionID == f.Key).First();
            //        //Only take bets up to 30 mins before fight
            //        if (info.FightDate.CompareTo(DateTime.Now.AddMinutes(30)) > 0)
            //        {
            //            info.EventType = eventType;
            //            info.OddsValue = PriceTradedToOdds(Convert.ToDouble(f.Value));
            //            info.DateTaken = DateTime.Now;
            //            listOfOddsToAdd.Add(info);
            //        }
            //    }
            //}

            foreach (MarketplaceEvent ev in events)
            {
                foreach (MarketplaceRunner runner in ev.Runners)
                {
                    if (runner.Odds != "")
                    {
                        OddsInfo info = fighterOddsList.Where(x => x.FightName == ev.Name && x.Name == runner.Name).FirstOrDefault();
                        //Only take bets up to 30 mins before fight
                        if (info != null)
                        {
                            if (info.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                            {
                                info.EventType = eventType;
                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(runner.Odds));
                                info.DateTaken = DateTime.Now;
                                listOfOddsToAdd.Add(info);
                            }
                        }
                    }
                }
            }
            using (var db = new BoxingDatabaseModel())
            {
                db.AddMultipleOdds(listOfOddsToAdd.Where(x => x.OddsValue != 0 && x.FightDate.CompareTo(DateTime.Now.AddMinutes(30)) > 0).ToList());
            }
            return error;
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

        private Task LogResultOdds() => new Task(() =>
        {
            IDictionary<string, string> fighterDictionary;
            int i = 0;
            while (true)
            {
                if (LoggingCancelFlag)
                {
                    break;
                }

                if (i < logInterval.TotalSeconds)
                {
                    i++;
                }
                else
                {
                    //Get Odds
                    fighterDictionary = marketMessenger.GetAllOddsOld(fighterOddsList.Select(x => x.SelectionID).ToList<string>(), "Match Odds");
                    foreach (var f in fighterDictionary)
                    {
                        if (f.Value != "")
                        {
                            var info = fighterOddsList.Where(x => x.SelectionID == f.Key).First();
                            //Only take bets up to 30 mins before fight
                            if (info.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                            {
                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(f.Value));
                                info.DateTaken = DateTime.Now;
                            }
                        }
                    }
                    using (var db = new BoxingDatabaseModel())
                    {
                        db.AddMultipleOdds(fighterOddsList.Where(x => x.OddsValue != 0 && x.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0).ToList());
                    }
                    //Reset Counter
                    i = 0;
                }
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            //Increment Counter
            Logging = false;
            //IDictionary<string, string> fighterDictionary = marketMessenger.GetFighterDictionary("Match Odds");
        });

        public void LogOddsNow(string eventType)
        {
            //Get Odds

            List<MarketplaceEvent> EventList = new List<MarketplaceEvent>();
            List<MarketplaceEvent> EventListWithOdds = new List<MarketplaceEvent>();

            EventList = marketMessenger.GetEventSelectionIDs(eventType, true);
            EventListWithOdds = marketMessenger.GetAllOdds(EventList, eventType, false);

            foreach (MarketplaceEvent ev in EventListWithOdds)
            {
                foreach (MarketplaceRunner runner in ev.Runners)
                {
                    if (runner.Odds != "")
                    {
                        OddsInfo info = fighterOddsList.Where(x => x.FightName == ev.Name && x.Name == runner.Name).FirstOrDefault();
                        //Only take bets up to 30 mins before fight
                        if (info != null)
                        {
                            if (info.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                            {
                                info.EventType = eventType;
                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(runner.Odds));
                                info.DateTaken = DateTime.Now;
                            }
                        }
                    }
                }
            }
            using (var db = new BoxingDatabaseModel())
            {
                db.AddMultipleOdds(fighterOddsList.Where(x => x.OddsValue != 0 && x.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0).ToList());
            }

            //fighterDictionary = marketMessenger.GetAllOddsOld(fighterOddsList.Select(x => x.SelectionID).ToList<string>(), eventType);
            //foreach (KeyValuePair<string, string> f in fighterDictionary)
            //{
            //    if (f.Value != "")
            //    {
            //        var info = fighterOddsList.Where(x => x.SelectionID == f.Key).First();
            //        info.OddsValue = PriceTradedToOdds(Convert.ToDouble(f.Value));
            //        info.DateTaken = DateTime.Now;
            //        info.EventType = eventType;
            //    }
            //    else
            //    {
            //        var info = fighterOddsList.Where(x => x.SelectionID == f.Key).First();
            //        info.DateTaken = DateTime.Now;
            //        info.OddsValue = 0;
            //    }
            //}
            //using (var db = new BoxingDatabaseModel())
            //{
            //    db.AddMultipleOdds(fighterOddsList.Where(x => x.OddsValue != 0).ToList());
            //}
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