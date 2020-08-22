using CommonClasses;
using Marketplace;
using MMABettingModule.Classes;
using MMADatabase;
using MMADatabase.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MMABettingModule
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

        public TimeSpan logInterval { get; set; } = new TimeSpan(6, 0, 0);

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
            if (!fighterOddsList.Where(x => x.FightName == odds.FightName).Select(r => r.Name).ToList().Contains(odds.Name))
            {
                fighterOddsList.Add(odds);
            }

            return true;
        }

        public async void StartLoggingAsync(string eventType, bool virtualise)
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

                    while (true)
                    {
                        if (LoggingCancelFlag)
                        {
                            break;
                        }

                        if ((i + 1) % TimeSpan.FromHours(1).TotalSeconds == 0)
                        {
                            //fighterDictionary = marketMessenger.GetAllOddsOld(fighterOddsList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.FightDate) > 0).Select(x => x.SelectionID).ToList<string>(), eventType, virtualise);
                            EventList = marketMessenger.GetEventSelectionIDs(eventType, "All");
                            if (EventList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.Date) > 0).Count() > 0)
                            {
                                EventListWithOdds = marketMessenger.GetAllOdds(EventList.Where(x => DateTime.Now.AddDays(1).CompareTo(x.Date) > 0).ToList(), eventType, "All");
                                foreach (OddsInfo f in fighterOddsList)
                                {
                                    int k = DateTime.Now.Add(TimeSpan.FromDays(1)).CompareTo(f.FightDate);
                                }

                                error = LogSpecificOdds(EventListWithOdds, eventType, false);
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
                        else if (i < logInterval.TotalSeconds)
                        {
                            i++;
                        }
                        else
                        {
                            EventList = marketMessenger.GetEventSelectionIDs(eventType, "All");
                            EventListWithOdds = marketMessenger.GetAllOdds(EventList, eventType, "All");
                            error = LogSpecificOdds(EventListWithOdds, eventType, true);

                            //Reset Counter
                            if (error == "")
                            {
                                i = 0;
                            }
                            else
                            {
                                //Try again in 10 seconds
                                i = (int)(logInterval.TotalSeconds) - 10;
                                Console.WriteLine(error);
                            }
                        }
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                    //Increment Counter
                });
                Logging = false;
            }
            catch (Exception ex)
            {
                Logging = false;
                MessageBox.Show("Error Logging, Cancel and start over:  " + ex.ToString());
            }
        }

        private string LogSpecificOdds(List<MarketplaceEvent> EventListWithOdds, string eventType, bool zeroCheck)
        {
            string error = "";
            List<OddsInfo> listOfOddsToAdd = new List<OddsInfo>();
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
                foreach (MarketplaceRunner runner in ev.Runners)
                {
                    if (runner.Odds != "")
                    {
                        OddsInfo info = fighterOddsList.Where(x => x.FightName == ev.Name && x.Name == runner.Name.RemoveSpaces()).FirstOrDefault();
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
            using (MMADatabaseModel db = new MMADatabaseModel())
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
                    fighterDictionary = marketMessenger.GetAllOddsOld(fighterOddsList.Select(x => x.SelectionID).ToList<string>(), "Fight Result", false);
                    foreach (KeyValuePair<string, string> f in fighterDictionary)
                    {
                        if (f.Value != "")
                        {
                            OddsInfo info = fighterOddsList.Where(x => x.SelectionID == f.Key).First();
                            //Only take bets up to 30 mins before fight
                            if (info.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                            {
                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(f.Value));
                                info.DateTaken = DateTime.Now;
                            }
                        }
                    }
                    using (MMADatabaseModel db = new MMADatabaseModel())
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
            //IDictionary<string, string> fighterDictionary = marketMessenger.GetFighterDictionary("Fight Result");
        });

        public void LogOddsNow(string eventType, bool virtualise)
        {
            IDictionary<string, string> fighterDictionary;
            //Get Odds
            List<MarketplaceEvent> EventList = new List<MarketplaceEvent>();
            List<MarketplaceEvent> EventListWithOdds = new List<MarketplaceEvent>();
            List<OddsInfo> listOddsToAdd = new List<OddsInfo>();

            EventList = marketMessenger.GetEventSelectionIDs(eventType, "All");
            EventListWithOdds = marketMessenger.GetAllOdds(EventList, eventType, "All");
            fighterDictionary = marketMessenger.GetAllOddsOld(fighterOddsList.Select(x => x.SelectionID).ToList<string>(), eventType);
            foreach (MarketplaceEvent ev in EventListWithOdds)
            {
                foreach (MarketplaceRunner runner in ev.Runners)
                {
                    if (runner.Odds != "")
                    {
                        OddsInfo info = fighterOddsList.Where(x => x.FightName == ev.Name && x.Name == runner.Name.RemoveSpaces()).FirstOrDefault();
                        //Only take bets up to 30 mins before fight
                        if (info != null)
                        {
                            if (info.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0)
                            {
                                info.EventType = eventType;
                                info.OddsValue = PriceTradedToOdds(Convert.ToDouble(runner.Odds));
                                info.DateTaken = DateTime.Now;
                                listOddsToAdd.Add(info);
                            }
                        }
                    }
                }
            }
            using (MMADatabaseModel db = new MMADatabaseModel())
            {
                db.AddMultipleOdds(listOddsToAdd.Where(x => x.OddsValue != 0 && x.FightDate.AddMinutes(30).CompareTo(DateTime.Now) > 0).ToList());
                foreach (MarketplaceEvent ev in EventListWithOdds.Where(x => x.Winner != null))
                {
                    foreach (OddsInfo dbOdds in db.oddsInfo.Where(x => x.FightName == ev.Name))
                    {
                        dbOdds.Winner = dbOdds.Name == ev.Winner;
                    }
                }
                db.SaveChanges();
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

        internal void RemoveAllLoggingItems() => fighterOddsList.Clear();
    }
}