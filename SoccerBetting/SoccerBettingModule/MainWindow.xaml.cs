﻿using CommonClasses;
using LoginClientLib;
using Marketplace;
using Microsoft.Win32;
using SoccerBettingModule.Classes;
using SoccerDatabase;
using SoccerDatabase.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace SoccerBettingModule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MarketplaceMessenger marketMessenger;
        private Timer autoRefreshTimer;
        private List<Event> BettingInfoAvailable;
        private List<Event> BettingInfoListed;
        private List<string> LoggingFighters;
        private LoginClient loginClient;
        private GUIProperties myGuiProperties;
        private List<Button> NavigationButtons;
        private Dictionary<Grid, Button> NavigationSet;
        private Logger OddsLogger;
        private SettingsVariables settings;
        private List<Grid> Windows;
        private Timer twelveHourRefreshTimer;
        private List<SelectionDisplay> AllSelections = new List<SelectionDisplay>();
        private Timer getResultsTimer;
        private const string resultsFilePath = @"C:\Users\jewar\source\repos\SoccerBetting\SoccerBettingModule\Resources\results.xml";

        public MainWindow()
        {
            marketMessenger = new MarketplaceMessenger();
            Windows = new List<Grid>();
            NavigationButtons = new List<Button>();
            NavigationSet = new Dictionary<Grid, Button>();
            LoggingFighters = new List<string>();
            myGuiProperties = new GUIProperties();
            BettingInfoListed = new List<Event>();
            myGuiProperties.NavigationVisibility = Visibility.Hidden;
            myGuiProperties.LoggedUserName = "No User";
            OddsLogger = new Logger(marketMessenger, myGuiProperties.LoggingInterval);
            settings = new SettingsVariables();
            //Set the inital event types
            myGuiProperties.ResultTypes = settings.PossibleEventTypes;
            autoRefreshTimer = new Timer(myGuiProperties.AutoRefreshInterval.TotalMilliseconds);

            twelveHourRefreshTimer = new Timer(1000 * 60 * 60 * 12);    //12 hours
            getResultsTimer = new Timer(1000 * 60 * 60 * 5);    //12 hours

            InitializeComponent();
            SetUpEvents();

            DataContext = myGuiProperties;
            Windows.Add(LoginGrid);
            Windows.Add(ListBetsGrid);
            Windows.Add(GraphsGrid);
            Windows.Add(SettingsGrid);
            Windows.Add(ListPastFightsGrid);
            Windows.Add(ResultsGrid);

            NavigationSet.Add(LoginGrid, LoginBtn);
            NavigationSet.Add(ListBetsGrid, ListBetsBtn);
            NavigationSet.Add(GraphsGrid, GraphBetsBtn);
            NavigationSet.Add(SettingsGrid, SettingsBtn);
            NavigationSet.Add(ListPastFightsGrid, ShowPastFightsBtn);
            NavigationSet.Add(ResultsGrid, ResultsBtn);

            //Add Nav Buttons
            NavigationButtons.Add(LoginBtn);
            NavigationButtons.Add(GraphBetsBtn);
            NavigationButtons.Add(ListBetsBtn);
            NavigationButtons.Add(ShowPastFightsBtn);
            NavigationButtons.Add(SettingsBtn);
            NavigationButtons.Add(ResultsBtn);

            //Set up Gui to start
            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                User user = db.users.Where(x => x.Remember).FirstOrDefault();
                if (user != null)
                {
                    usernameTxtBox.Text = user.Username;
                    passTxtBox.Password = user.Password;
                    chkRememberMe.IsChecked = true;
                }
            }
            ShowWindow(LoginGrid);
        }

        public void CreateCSVFile(DataTable dt, string strFilePath, bool append)
        {
            StreamWriter sw = new StreamWriter(strFilePath, append);

            int iColCount = dt.Columns.Count;
            for (int i = 0; i < iColCount; i++)
            {
                sw.Write(dt.Columns[i]);
                if (i < iColCount - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);

            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        sw.Write(dr[i].ToString());
                    }
                    if (i < iColCount - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        public void DisplayError(object sender, string msg) => MessageBox.Show(msg, "Error From: " + sender.ToString(), MessageBoxButton.OK);

        //public List<FightEvent> FillBettingInfo(List<FightEvent> eventList, IDictionary<string, string> dictionary, Fighter.FighterProperty property)
        //{
        //    //TODO: fill betting info needs doing
        //    switch (property)
        //    {
        //        case Fighter.FighterProperty.EventDate:
        //            foreach (FightEvent f in eventList)
        //            {
        //                if (dictionary.ContainsKey(f.Name))
        //                    f.Date = Convert.ToDateTime(dictionary[f.Name]);
        //            }
        //            break;

        //        case Fighter.FighterProperty.Odds:
        //            foreach (FightEvent f in eventList)
        //            {
        //                if (dictionary.ContainsKey(f.SelectionID) && dictionary[f.SelectionID] != "")
        //                {
        //                    f.PriceTradedtoOdds(Convert.ToDouble(dictionary[f.SelectionID]));
        //                }
        //            }
        //            break;

        //        case Fighter.FighterProperty.SelectionID:
        //            foreach (FightEvent f in eventList)
        //            {
        //                if (dictionary[f.Name] != null)
        //                    f.SelectionID = dictionary[f.Name];
        //            }
        //            break;

        //        case Fighter.FighterProperty.Name:
        //            foreach (System.Collections.Generic.KeyValuePair<string, string> fe in dictionary)
        //            {
        //                if (!eventList.Select(x => x.Name).Contains(fe.Key))
        //                    eventList.Add(new FightEvent(fe.Key, fe.Value));
        //            }
        //            break;

        //        case Fighter.FighterProperty.FightEventName:
        //            foreach (System.Collections.Generic.KeyValuePair<string, string> fe in dictionary)
        //            {
        //                if (!eventList.Select(x => x.Name).Contains(fe.Key))
        //                    eventList.Add(new FightEvent(fe.Key, fe.Value));
        //            }
        //            break;

        //        default:
        //            break;
        //    }
        //    return eventList;
        //}

        public void GetBettingInfo(string eventType)
        {
            BettingInfoAvailable = new List<Event>();

            IDictionary<string, string> RunnerDictionary;

            List<MarketplaceEvent> EventList = new List<MarketplaceEvent>();
            List<MarketplaceEvent> EventListWithOdds = new List<MarketplaceEvent>();

            try
            {
                RunnerDictionary = marketMessenger.GetBettingDictionary(eventType);
            }
            catch (Exception)
            {
                ReLogin();
                //Try Again
                RunnerDictionary = marketMessenger.GetBettingDictionary(eventType);
            }

            EventList = marketMessenger.GetEventSelectionIDs(eventType, true);
            EventListWithOdds = marketMessenger.GetAllOdds(EventList, eventType, myGuiProperties.Virtualize);
            
            CreateEventFramework(BettingInfoAvailable, EventListWithOdds, eventType);
         

            //Sort by fight date and then name of fight
            BettingInfoAvailable = BettingInfoAvailable.OrderBy(x => x.Name).OrderBy(x => x.Date).ToList();

            foreach(var ev in BettingInfoAvailable)
            {
                var selection = ChooseSelectionType(ev);
                foreach (var run in selection.Runners)
                {
                    var selDisplay = AllSelections.Where(x => x.EventName == ev.Name && x.SelectionName == run.Name && x.ResultType == settings.EventType).FirstOrDefault();
                    if (selDisplay != null)
                    {
                        selDisplay.Change = run.Odds.ToDouble() - selDisplay.Odds;
                        selDisplay.Odds = run.Odds.ToDouble();
                        selDisplay.Date = ev.Date;
                    }
                    else
                    {
                        var sd = new SelectionDisplay
                        {
                            Change = 0,
                            Date = ev.Date,
                            DecimalOdds = run.Multiplier,
                            EventName = ev.Name,
                            Odds = run.Odds.ToDouble(),
                            Percentage = run.PercentChance,
                            ResultType = settings.EventType,
                            Selected = true,
                            SelectionName = run.Name
                        };

                        AllSelections.Add(sd);

                        ChangeLogging(ev, run, true);

                    }

                   
                }
                
            }
            var selsNotAvail = AllSelections.Where(x => !BettingInfoAvailable.Select(y => y.Name).ToList().Contains(x.EventName) && x.ResultType == settings.EventType);

            foreach (var se in selsNotAvail)
            {
                OddsLogger.RemoveLoggingItem(se.SelectionName, se.EventName, se.ResultType);
            }

            AllSelections.RemoveAll(x => !BettingInfoAvailable.Select(y => y.Name).ToList().Contains(x.EventName) && x.ResultType == settings.EventType);

            myGuiProperties.SelectionToDisplay = AllSelections.Where(x => x.ResultType == settings.EventType).OrderBy(x => x.Date).ToList();
        }

        private void CreateEventFramework(List<Event> evList, IDictionary<string, string> eventDictionary, IDictionary<string, string> runnerDictionary, IDictionary<string, string> oddsDictionary, IDictionary<string, string> dateDictionary, string eventType)
        {
            if (eventType == "Match Odds" || eventType == "Fight Result")
            {
                foreach (KeyValuePair<string, string> p in runnerDictionary)
                {
                    string selId = p.Key;
                    if (eventDictionary.ContainsKey(p.Key))
                    {
                        //If not in eventList, add
                        if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
                        {
                            Event e = new Event(eventDictionary[selId])
                            {
                                Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
                            };
                            evList.Add(e);
                        }

                        //Add fighters and add Fight Event info
                        Event k = evList.Where(x => x.Name == eventDictionary[selId]).FirstOrDefault();
                        evList.Where(x => x.Name == eventDictionary[selId])
                            .First().Fighters.Add(new Fighter(p.Value, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
                        evList.Where(x => x.Name == eventDictionary[selId])
                            .First().FightResult.AddRunner(new Runner(p.Value, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
                    }
                }
            }
            else if (eventType == "Go The Distance?")
            {
                foreach (KeyValuePair<string, string> p in runnerDictionary)
                {
                    string selId = p.Key;
                    if (eventDictionary.ContainsKey(p.Key))
                    {
                        if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
                        {
                            Event e = new Event(eventDictionary[selId])
                            {
                                Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
                            };
                            evList.Add(e);
                        }

                        //Add GoTheDistance
                        evList.Where(x => x.Name == eventDictionary[selId])
                            .First().GoTheDistance.AddRunner(new Runner(p.Value, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
                    }
                }
            }
            else if (eventType == "Round Betting")
            {
                foreach (KeyValuePair<string, string> p in runnerDictionary)
                {
                    string selId = p.Key;
                    if (eventDictionary.ContainsKey(p.Key))
                    {
                        if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
                        {
                            Event e = new Event(eventDictionary[selId])
                            {
                                Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
                            };
                            evList.Add(e);
                        }

                        //Add GoTheDistance
                        evList.Where(x => x.Name == eventDictionary[selId])
                            .First().RoundBetting.AddRunner(new Runner(p.Value, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
                    }
                }
            }
            else if (eventType == "Method of Victory")
            {
                foreach (KeyValuePair<string, string> p in runnerDictionary)
                {
                    string selId = p.Key;
                    if (eventDictionary.ContainsKey(p.Key))
                    {
                        if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
                        {
                            Event e = new Event(eventDictionary[selId])
                            {
                                Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
                            };
                            evList.Add(e);
                        }

                        //Add Method of Victory
                        evList.Where(x => x.Name == eventDictionary[selId])
                            .First().MethodOfVictory.AddRunner(new Runner(p.Value, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
                    }
                }
            }
        }

        private void CreateEventFramework(List<Event> evList, List<MarketplaceEvent> marketList, string eventType)
        {
            if (eventType == "Match Odds" || eventType == "Fight Result")
            {
                foreach (MarketplaceEvent ev in marketList)
                {
                    //If not in eventList, add
                    if (!evList.Select(x => x.Name).ToList().Contains(ev.Name))
                    {
                        Event e = new Event(ev.Name)
                        {
                            Date = ev.Date
                        };
                        foreach (MarketplaceRunner runn in ev.Runners)
                        {
                            e.Fighters.Add(new Fighter(runn.Name, runn.SelectionID, runn.Odds != null ? runn.Odds : "0"));
                            e.FightResult.AddRunner(new Runner(runn.Name, runn.SelectionID, runn.Odds != null ? runn.Odds : "0"));
                        }

                        e.Winner = ev.Winner;
                        evList.Add(e);
                    }
                }
            }
            //else if (eventType == "Go The Distance?")
            //{
            //    foreach (KeyValuePair<string, string> p in runnerDictionary)
            //    {
            //        string selId = p.Key;
            //        if (eventDictionary.ContainsKey(p.Key))
            //        {
            //            if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
            //            {
            //                Event e = new Event(eventDictionary[selId])
            //                {
            //                    Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
            //                };
            //                evList.Add(e);
            //            }

            //            //Add GoTheDistance
            //            evList.Where(x => x.Name == eventDictionary[selId])
            //                .First().GoTheDistance.AddRunner(new Runner(p.Value, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
            //        }
            //    }
            //}
            //else if (eventType == "Round Betting")
            //{
            //    foreach (KeyValuePair<string, string> p in runnerDictionary)
            //    {
            //        string selId = p.Key;
            //        if (eventDictionary.ContainsKey(p.Key))
            //        {
            //            if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
            //            {
            //                Event e = new Event(eventDictionary[selId])
            //                {
            //                    Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
            //                };
            //                evList.Add(e);
            //            }

            //            //Add GoTheDistance
            //            evList.Where(x => x.Name == eventDictionary[selId])
            //                .First().RoundBetting.AddRunner(new Runner(p.Value, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
            //        }
            //    }
            //}
            //else if (eventType == "Method of Victory")
            //{
            //    foreach (KeyValuePair<string, string> p in runnerDictionary)
            //    {
            //        string selId = p.Key;
            //        if (eventDictionary.ContainsKey(p.Key))
            //        {
            //            if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
            //            {
            //                Event e = new Event(eventDictionary[selId])
            //                {
            //                    Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
            //                };
            //                evList.Add(e);
            //            }

            //            //Add Method of Victory
            //            evList.Where(x => x.Name == eventDictionary[selId])
            //                .First().MethodOfVictory.AddRunner(new Runner(p.Value, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
            //        }
            //    }
            //}
        }

        private void AutoRefreshChk_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)AutoRefreshChk.IsChecked)
            {
                autoRefreshTimer.Start();
            }
            else
            {
                autoRefreshTimer.Stop();
            }
        }

        private void InvokeUI(Action a) => Application.Current.Dispatcher.Invoke(a);

        private void AutoRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        => InvokeUI(() =>
        {
            MainMessage("Refreshing List...");
            GetBettingInfo(settings.EventType);
            UpdateList();
            FindAndFillResults();
            Event f = BettingInfoAvailable
            .Where(x => x.Fighters.First().NameNoSpaces == EventSelector.Text && x.Fighters.First().Odds != "" && x.Name == SelectionSelector.SelectedItem.ToString())
            .FirstOrDefault();

            if (f != null)
            {
                oxyPlotView.Model = CreateGraphView(f).CreateOxyPlot();
            }

            ////Log all new fights
            //foreach (CheckBox chk in ListGrid.Children.OfType<CheckBox>())
            //{
            //    if (chk.Tag != null)
            //    {
            //        string runner = chk.Tag.ToString().Split('|')[0];

            //        string eventName = chk.Tag.ToString().Split('|')[1];

            //        Event tag = BettingInfoAvailable.Where(x => x.Name == eventName).FirstOrDefault();
            //        //Add if not checked but all is checked
            //        if (tag != null && !(bool)chk.IsChecked && (bool)LogAllchk.IsChecked)
            //        {
            //            List<OddsInfo> oddsInfos = tag.FightResult.ToOddsInfo(tag);
            //            foreach (OddsInfo oi in oddsInfos)
            //            {
            //                OddsLogger.AddLoggingItem(oi);
            //            }

            //            //bool logged = ChangeLogging(f, true);
            //            chk.IsChecked = true;
            //        }
            //    }
            //}

            MainMessage("");
        });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            //{
            //    foreach (var d in db.results)
            //    {
            //        d.AmountWon = 0;
            //    }
            //    db.SaveChanges();
            //}
        }

        private void ButtonColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxColourItem colour = (ComboBoxColourItem)ButtonColorCombo.SelectedItem;
            myGuiProperties.ButtonBackgroundColour = colour.Value;
        }

        private bool ChangeLogging(Event ev, Runner run, bool log)
        {
            bool success = false;
            if (log)
            {
                List<OddsInfo> oddsInfos;
                switch (settings.EventType)
                {
                    case "Match Odds":
                        oddsInfos = ev.FightResult.ToOddsInfo(ev);
                        break;

                    case "Go The Distance?":
                        oddsInfos = ev.GoTheDistance.ToOddsInfo(ev);
                        break;

                    case "Round Betting":
                        oddsInfos = ev.RoundBetting.ToOddsInfo(ev);

                        break;

                    case "Method of Victory":
                        oddsInfos = ev.MethodOfVictory.ToOddsInfo(ev);
                        break;

                    default:
                        oddsInfos = ev.FightResult.ToOddsInfo(ev);
                        break;
                }

                foreach (OddsInfo oi in oddsInfos)
                {
                    if (oi.SelectionName == run.Name)
                    {
                        success = OddsLogger.AddLoggingItem(oi);
                    }
                }
            }
            else
            {
                foreach (OddsInfo oi in ev.FightResult.ToOddsInfo(ev))
                {
                    if (oi.SelectionName == run.Name)
                    {
                        success = !OddsLogger.RemoveLoggingItem(oi.SelectionName, ev.Name, oi.EventType);
                    }
                }
            }
            return success;
        }

        private void CleanData(object sender, RoutedEventArgs e)
        {
            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                DateTime dt = new DateTime(1, 1, 1);
                List<OddsInfo> l = db.oddsInfo.Where(x => Math.Abs(x.OddsValue) > 1200).ToList();
                List<OddsInfo> p = db.oddsInfo.Where(x => x.DateTaken == dt).ToList();
                //db.oddsInfo
                //    .RemoveRange(db.oddsInfo
                //    .Where(x => Math.Abs(x.OddsValue) > 1200));

                MessageBoxResult res = MessageBox.Show("This will remove " + p.Count.ToString() + " entries. Do you wish to continue?", "Confirmation", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    db.oddsInfo
                    .RemoveRange(db.oddsInfo.Where(x => x.DateTaken == dt));
                    int k = db.SaveChanges();
                    MessageBox.Show(k.ToString() + " Entries Removed");
                }
            }
        }

        private void ExportFirstOdds(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            List<FighterFinalOddsExport> finalOdds = new List<FighterFinalOddsExport>();
            DataTable dataTable = new DataTable("Results Export");

            List<string> fighters;
            List<string> fightEvents;

            Task.Run(() =>
            {
                using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                {
                    DateTime oneWeekAgo = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0));

                    fighters = db.oddsInfo
                    .Where(x => x.EventDate.CompareTo(oneWeekAgo) > 0)
                    .GroupBy(x => x.SelectionName)
                    .Select(t => t.Key)
                    .ToList();

                    foreach (string name in fighters)
                    {
                        fightEvents = db.oddsInfo
                        .Where(x => x.SelectionName == name && x.EventDate.CompareTo(oneWeekAgo) > 0)
                        .GroupBy(x => x.EventName)
                        .Select(x => x.Key)
                        .ToList();

                        //Fighters may have been in multiple fights
                        foreach (string ev in fightEvents)
                        {
                            //Get Average Odds First
                            DateTime EventDate = db.oddsInfo
                            .Where(x => x.SelectionName == name && x.EventName == ev && x.DateTaken.CompareTo(x.EventDate) < 0)
                            .OrderByDescending(x => x.DateTaken)
                            .Select(x => x.EventDate)
                            .FirstOrDefault();

                            if (EventDate == new DateTime())
                            {
                                continue;
                            }

                            OddsInfo record = db.oddsInfo
                                .Where(x => x.SelectionName == name && x.EventName == ev && x.DateTaken.CompareTo(x.EventDate) < 0)
                                .OrderBy(x => x.DateTaken).FirstOrDefault();

                            //Add to a list
                            if (record != null)
                            {
                                finalOdds.Add(new FighterFinalOddsExport
                                {
                                    EventDate = EventDate,
                                    Name = record.SelectionName,
                                    Odds = record.OddsValue,
                                    EventName = record.EventName,
                                    Winner = record.Winner ? "W" : "L"
                                });
                            }
                        }
                    }

                    dataTable.Columns.Add("Event Date", typeof(DateTime));
                    dataTable.Columns.Add("Event Name", typeof(string));
                    dataTable.Columns.Add("Selection", typeof(string));
                    dataTable.Columns.Add("Odds", typeof(double));
                    dataTable.Columns.Add("Winner", typeof(string));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.EventDate, i.EventName, i.Name, i.Odds, i.Winner);
                    }
                }

                InvokeUI(() =>
                {
                    LoadingScreen(false);
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel|*.csv"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        CreateCSVFile(dataTable, saveFileDialog.FileName, false);
                    }
                });
            });
        }

        private void RemoveEventsWithoutWinner(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);

            List<string> EventNames;

            Task.Run(() =>
            {
                using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                {
                    DateTime now = DateTime.Now;
                    DateTime oneWeekEarlier = now.Subtract(TimeSpan.FromDays(7));
                    DateTime twelveHoursAgo = now.Subtract(TimeSpan.FromHours(12));

                    EventNames = db.oddsInfo
                    .Where(x => x.EventDate.CompareTo(oneWeekEarlier) > 0 && x.EventDate.CompareTo(twelveHoursAgo) < 0)
                    .GroupBy(x => x.EventName)
                    .Select(t => t.Key)
                    .ToList();
                    int j = 0;
                    foreach (string name in EventNames)
                    {
                        //If there are events with no winner in database
                        //if (name == "Sportive de Tunis v Guadalajara")
                        //{
                        //    var k = db.oddsInfo.Where(x => x.EventName == name).ToList();
                        //}
                        if (db.oddsInfo.Where(x => x.EventName == name && x.Winner == true).FirstOrDefault() == null)
                        {
                            var eventLogs = db.oddsInfo.Where(x => x.EventName == name);
                            db.oddsInfo.RemoveRange(eventLogs);
                            j++;
                        }
                    }

                    int i = db.SaveChanges();
                    MessageBox.Show(j.ToString() + " events removed.");
                }

                InvokeUI(() =>
                {
                    LoadingScreen(false);
                });
            });
        }

        private void ClearListGridData() => myGuiProperties.SelectionToDisplay = new List<SelectionDisplay>();

        private Graph CreateGraphView(Event f)
        {
            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                //Get all odds for the event
                var eventInfo = db.oddsInfo
                    .Where(x => x.EventName == f.Name)
                    .Select(t => new { t.SelectionName, t.DateTaken, t.OddsValue });
                //Names of the fighters
                List<string> selections = eventInfo.GroupBy(fers => fers.SelectionName).Select(p => p.Key).ToList();
                if (selections.Count() > 1)
                {
                    Dictionary<DateTime, double> f1 = eventInfo.Where(x => x.SelectionName == selections[0]).GroupBy(x => x.DateTaken).ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    Dictionary<DateTime, double> f2 = eventInfo.Where(x => x.SelectionName == selections[1]).GroupBy(x => x.DateTaken).ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    Dictionary<DateTime, double> f3 = new Dictionary<DateTime, double>();
                    if (selections.Count > 2)
                    {
                        f3 = eventInfo.Where(x => x.SelectionName == selections[2]).GroupBy(x => x.DateTaken).ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    }

                    if (f1.Count == 0 || f2.Count == 0 || f3.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return new Graph(f1, f2, selections[0], selections[1], "Date", "Odds", f3, selections.Count > 2 ? selections[2] : null);
                    }
                }
                else
                {
                    DisplayError(this, "Unable to create graph");
                    return null;
                }
            }
        }

        private void DateSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateSelection window = new DateSelection();
            switch (DateSelector.SelectedItem)
            {
                case DateSelection.ComboSelection.Upcoming:
                    window.Start = DateTime.Now;
                    window.End = DateTime.Now.AddMonths(12);
                    break;

                case DateSelection.ComboSelection.LastWeek:
                    window.Start = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
                    window.End = DateTime.Now;
                    break;

                case DateSelection.ComboSelection.LastMonth:
                    window.Start = DateTime.Today.AddMonths(-1);
                    window.End = DateTime.Now;
                    break;

                case DateSelection.ComboSelection.All:
                    window.Start = new DateTime(0);
                    window.End = window.End = DateTime.Now.AddMonths(12);
                    break;

                default:
                    window.Start = DateTime.Now;
                    window.End = DateTime.Now.AddMonths(12);
                    break;
            }

            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                List<string> list = db.oddsInfo
                    .Where(x => x.EventDate > window.Start && x.EventDate < window.End && x.EventType == settings.EventType)
                    .GroupBy(x => x.EventName)
                    .Select(t => t.Key)
                    .OrderBy(f => f)
                    .ToList();
                EventSelector.Items.Clear();
                SelectionSelector.Items.Clear();
                if (list.Count != 0)
                {
                    foreach (string item in list)
                    {
                        EventSelector.Items.Add(item);
                    }
                }
            }
            EventSelector.SelectedIndex = -1;
            oxyPlotView.Model = null;
            ExportBtn.IsEnabled = false;
        }

        private void DisplayBettingInfo()
        {
            myGuiProperties.SelectionToDisplay = AllSelections.Where(x => x.ResultType == settings.EventType).ToList();

            //List<string> EventNames = BettingInfoListed.Select(x => x.Name).ToList();
            //try
            //{
            //    double height = 25;
            //    Brush backgroundBrush = Brushes.Beige;

            //    for (int i = 0; i < BettingInfoAvailable.Count; i++)
            //    {
            //        ISelection sel = ChooseSelectionType(BettingInfoAvailable[i]);
            //        List<string> currentRunners = new List<string>();
            //        List<Runner> currentEvent = BettingInfoListed
            //                  .Where(x => x.Name == BettingInfoAvailable[i].Name)
            //                  .Select(x => x.FightResult.Runners)
            //                  .FirstOrDefault();
            //        if (currentEvent != null)
            //        {
            //            currentRunners = currentEvent.Select(f => f.Name)
            //                  .ToList();
            //        }

            //        if (!EventNames.Contains(BettingInfoAvailable[i].Name))
            //        {
            //            foreach (Runner item in sel.Runners)
            //            {
            //                //As long as runner isnt listed
            //                if (!currentRunners.Contains(item.Name))
            //                {
            //                    //Fights.Add(new Fight())
            //                    //Fight f = new Fight();
            //                    int numRows = ListGrid.RowDefinitions.Count;
            //                    RowDefinition rowdef = new RowDefinition
            //                    {
            //                        Height = new GridLength(height)
            //                    };

            //                    //rowdef.SetValue(Grid.BackgroundProperty, backgroundBrush);

            //                    if (numRows % 2 == 1)
            //                    {
            //                        if (backgroundBrush == Brushes.Beige)
            //                        {
            //                            backgroundBrush = Brushes.White;
            //                        }
            //                        else
            //                        {
            //                            backgroundBrush = Brushes.Beige;
            //                        }
            //                    }
            //                    rowdef.SetValue(Panel.BackgroundProperty, backgroundBrush);

            //                    ListGrid.RowDefinitions.Insert(numRows, rowdef);
            //                    CheckBox toggleLoggingChk = new CheckBox();
            //                    toggleLoggingChk.SetValue(Grid.RowProperty, numRows);
            //                    toggleLoggingChk.SetValue(Grid.ColumnProperty, 0);
            //                    toggleLoggingChk.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //                    toggleLoggingChk.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            //                    toggleLoggingChk.Click += ToggleLoggingFighter;
            //                    toggleLoggingChk.Tag = item.Name + '|' + BettingInfoAvailable[i].Name;

            //                    Label date = new Label
            //                    {
            //                        Content = marketMessenger.GetDate(BettingInfoAvailable.ElementAt(i).Name).ToString("MMMM-dd-HH:mm"),
            //                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            //                        VerticalAlignment = System.Windows.VerticalAlignment.Center
            //                    };
            //                    date.SetValue(Grid.RowProperty, numRows);
            //                    date.SetValue(Grid.ColumnProperty, 1);

            //                    Label fightEventName = new Label
            //                    {
            //                        Content = BettingInfoAvailable.ElementAt(i).Name,
            //                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            //                    };
            //                    //fighterName.VerticalAlignment = VerticalAlignment.Center;
            //                    fightEventName.SetValue(Grid.RowProperty, numRows);
            //                    fightEventName.SetValue(Grid.ColumnProperty, 2);

            //                    Label selection = new Label
            //                    {
            //                        Content = item.Name,
            //                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            //                    };
            //                    selection.SetValue(Grid.RowProperty, numRows);
            //                    selection.SetValue(Grid.ColumnProperty, 3);

            //                    Label Odds = new Label
            //                    {
            //                        Content = item.Odds,
            //                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            //                    };
            //                    //Odds.VerticalAlignment = VerticalAlignment.Center;
            //                    Odds.SetValue(Grid.RowProperty, numRows);
            //                    Odds.SetValue(Grid.ColumnProperty, 4);

            //                    Label Percent = new Label
            //                    {
            //                        Content = item.PercentChance,
            //                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            //                    };
            //                    //Odds.VerticalAlignment = VerticalAlignment.Center;
            //                    Percent.SetValue(Grid.RowProperty, numRows);
            //                    Percent.SetValue(Grid.ColumnProperty, 5);

            //                    Label Mult = new Label
            //                    {
            //                        Content = item.Multiplier.ToString(),
            //                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            //                    };
            //                    Mult.SetValue(Grid.RowProperty, numRows);
            //                    Mult.SetValue(Grid.ColumnProperty, 6);

            //                    Label Change = new Label
            //                    {
            //                        Content = (item.Odds.ToDouble() - item.LastOdds.ToDouble()).ToString(),
            //                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            //                    };
            //                    Change.SetValue(Grid.RowProperty, numRows);
            //                    Change.SetValue(Grid.ColumnProperty, 7);

            //                    Grid color = new Grid();
            //                    color.SetValue(Grid.RowProperty, numRows);
            //                    color.SetValue(Grid.ColumnSpanProperty, 100);
            //                    color.SetValue(Panel.BackgroundProperty, backgroundBrush);

            //                    ListGrid.Children.Add(color);
            //                    ListGrid.Children.Add(date);
            //                    ListGrid.Children.Add(selection);
            //                    ListGrid.Children.Add(fightEventName);
            //                    ListGrid.Children.Add(Odds);
            //                    ListGrid.Children.Add(toggleLoggingChk);
            //                    ListGrid.Children.Add(Percent);
            //                    ListGrid.Children.Add(Mult);
            //                    ListGrid.Children.Add(Change);
            //                }
            //            }
            //            BettingInfoListed.Add(BettingInfoAvailable[i]);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    DisplayError(marketMessenger, "Could not Fill Rows: " + ex.Message);
            //}
        }

        private ISelection ChooseSelectionType(Event ev)
        {
            ISelection sel;

            switch (settings.EventType)
            {
                case "Match Odds":
                    sel = ev.FightResult;
                    break;

                case "Go The Distance?":
                    sel = ev.GoTheDistance;
                    break;

                case "Round Betting":
                    sel = ev.RoundBetting;
                    break;

                case "Method of Victory":
                    sel = ev.MethodOfVictory;
                    break;

                default:
                    sel = ev.FightResult;
                    break;
            }
            return sel;
        }

        private List<ISelection> ChooseSelectionType(List<Event> ev)
        {
            List<ISelection> sel;

            switch (settings.EventType)
            {
                case "Match Odds":
                    sel = ev.Select(x => x.FightResult).ToList<ISelection>();
                    break;

                case "Go The Distance?":
                    sel = ev.Select(x => x.GoTheDistance).ToList<ISelection>();
                    break;

                case "Round Betting":
                    sel = ev.Select(x => x.RoundBetting).ToList<ISelection>();
                    break;

                case "Method of Victory":
                    sel = ev.Select(x => x.MethodOfVictory).ToList<ISelection>();
                    break;

                default:
                    sel = ev.Select(x => x.FightResult).ToList<ISelection>();
                    break;
            }
            return sel;
        }

        private List<ISelection> ChooseSelectionType(List<Event> ev, string eventName)
        {
            List<ISelection> sel;

            switch (settings.EventType)
            {
                case "Match Odds":
                    sel = ev.Where(x => x.Name == eventName).Select(x => x.FightResult).ToList<ISelection>();
                    break;

                case "Go The Distance?":
                    sel = ev.Where(x => x.Name == eventName).Select(x => x.GoTheDistance).ToList<ISelection>();
                    break;

                case "Round Betting":
                    sel = ev.Where(x => x.Name == eventName).Select(x => x.RoundBetting).ToList<ISelection>();
                    break;

                case "Method of Victory":
                    sel = ev.Where(x => x.Name == eventName).Select(x => x.MethodOfVictory).ToList<ISelection>();
                    break;

                default:
                    sel = ev.Where(x => x.Name == eventName).Select(x => x.FightResult).ToList<ISelection>();
                    break;
            }
            return sel;
        }

        private void DisplayPastFights()
        {
            LoadingScreen(true);
            Task.Run(() =>
            {
                List<string> pastFights;
                DateTime EventDate;

                using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                {
                    List<OddsInfo> oddsInfos = db.oddsInfo.ToList();

                    pastFights = oddsInfos
                        .OrderBy(x => x.EventDate)
                        .Select(x => x.EventName)
                        .Distinct()
                        .ToList();
                }
                InvokeUI(() =>
                {
                    try
                    {
                        double height = 25;
                        Brush backgroundBrush = Brushes.Beige;

                        for (int i = 0; i < pastFights.Count; i++)

                        {
                            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                            {
                                string s = pastFights[i];
                                List<OddsInfo> DataList = db.oddsInfo
                                    .Where(x => x.EventName == s)
                                    .ToList();
                                EventDate = DataList
                                    .OrderByDescending(x => x.DateTaken)
                                    .Select(x => x.EventDate)
                                    .Last();
                            }
                            int numRows = pastFightsGrid.RowDefinitions.Count;
                            RowDefinition rowdef = new RowDefinition
                            {
                                Height = new GridLength(height)
                            };

                            if (backgroundBrush == Brushes.Beige)
                            {
                                backgroundBrush = Brushes.White;
                            }
                            else
                            {
                                backgroundBrush = Brushes.Beige;
                            }

                            rowdef.SetValue(Panel.BackgroundProperty, backgroundBrush);

                            pastFightsGrid.RowDefinitions.Insert(numRows - 1, rowdef);

                            Button removeFight = new Button
                            {
                                Content = "Remove"
                            };
                            removeFight.SetValue(Grid.RowProperty, numRows - 1);
                            removeFight.SetValue(Grid.ColumnProperty, 0);
                            removeFight.Width = 75;
                            removeFight.Height = 20;
                            removeFight.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                            removeFight.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                            removeFight.Click += RemoveFightFromDb;
                            removeFight.Tag = pastFights[i];

                            Label date = new Label
                            {
                                Content = EventDate.Date.ToLongDateString(),
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                                VerticalAlignment = System.Windows.VerticalAlignment.Center
                            };
                            date.SetValue(Grid.RowProperty, numRows - 1);
                            date.SetValue(Grid.ColumnProperty, 1);

                            Label EventName = new Label
                            {
                                Content = pastFights[i],
                                HorizontalAlignment = HorizontalAlignment.Left
                            };
                            EventName.SetValue(Grid.RowProperty, numRows - 1);
                            EventName.SetValue(Grid.ColumnProperty, 2);

                            Grid color = new Grid();
                            color.SetValue(Grid.RowProperty, numRows - 1);
                            color.SetValue(Grid.ColumnSpanProperty, 100);
                            color.SetValue(Panel.BackgroundProperty, backgroundBrush);

                            pastFightsGrid.Children.Add(color);
                            pastFightsGrid.Children.Add(removeFight);

                            pastFightsGrid.Children.Add(date);
                            pastFightsGrid.Children.Add(EventName);
                        }
                        LoadingScreen(false);
                    }
                    catch (Exception ex)
                    {
                        LoadingScreen(false);
                        DisplayError(marketMessenger, "Could not Fill Rows: " + ex.Message);
                    }
                });
            });
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (OddsLogger.Logging)
            {
                MessageBoxResult exitChoice = MessageBox.Show("Are you sure you wish to exit? Logging will end", "Exit Application", MessageBoxButton.YesNo);
                if (exitChoice == MessageBoxResult.Yes)
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            //Create Graph data
            Graph g;
            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                if (db.oddsInfo.Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == SelectionSelector.SelectedItem.ToString()).FirstOrDefault() != null)
                {
                    Dictionary<DateTime, double> dict = db.oddsInfo
                  .Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == SelectionSelector.SelectedItem.ToString())
                  .Select(t => new { t.DateTaken, t.OddsValue })
                  .GroupBy(x => x.DateTaken)
                  .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    g = new Graph(dict, EventSelector.SelectedItem.ToString(), "Date", "Odds");

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel|*.csv"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        CreateCSVFile(g.ToDataTable(), saveFileDialog.FileName, false);
                    }
                }
            }
        }

        private void ExportFinalOdds(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            List<FighterFinalOddsExport> finalOdds = new List<FighterFinalOddsExport>();
            DataTable dataTable = new DataTable("Results Export");

            Task.Run(() =>
            {
                using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                {
                    List<string> fighters = db.oddsInfo.Where(x => x.EventDate < DateTime.Now).GroupBy(x => x.SelectionName).Select(t => t.Key).ToList();

                    foreach (string name in fighters)
                    {
                        //Get Average Odds First
                        int count = db.oddsInfo.Where(x => x.SelectionName == name).Count();

                        //Group by day and
                        Dictionary<int, IOrderedEnumerable<OddsInfo>> record1 = db.oddsInfo.Where(x => x.SelectionName == name)
                            .GroupBy(x => x.DateTaken.Day).ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.OddsValue));
                        List<OddsInfo> dubList = new List<OddsInfo>();
                        Parallel.ForEach(record1, day =>
                         {
                             //Take median of each day
                             int c = day.Value.Count();
                             if (c > 0)
                             {
                                 List<long> k = day.Value.Select(x => x.OddsValue).ToList();
                             }
                             dubList.Add(day.Value.Skip(c / 2 - 1).First());
                         });
                        //Take median of each median day value
                        OddsInfo record = dubList.Skip(dubList.Count / 2).First();

                        finalOdds.Add(new FighterFinalOddsExport
                        {
                            EventDate = record.EventDate,
                            Name = record.SelectionName,
                            Odds = record.OddsValue,
                            EventName = record.EventName
                        });
                    }

                    dataTable.Columns.Add("Event Date", typeof(DateTime));
                    dataTable.Columns.Add("Event Name", typeof(string));
                    dataTable.Columns.Add("SelectionName", typeof(string));
                    dataTable.Columns.Add("Final Odds", typeof(double));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.EventDate, i.EventName, i.Name, i.Odds);
                    }
                }
                InvokeUI(() =>
                {
                    LoadingScreen(false);
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel|*.csv"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        CreateCSVFile(dataTable, saveFileDialog.FileName, false);
                    }
                });
            });
        }

        private void ExportLastWeekOdds(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            List<FighterFinalOddsExport> finalOdds = new List<FighterFinalOddsExport>();
            DataTable dataTable = new DataTable("Results Export");

            List<string> fighters;
            List<string> fightEvents;

            Task.Run(() =>
            {
                using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                {
                    DateTime oneWeekAgo = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));

                    fighters = db.oddsInfo
                    .Where(x => x.EventDate.CompareTo(oneWeekAgo) > 0)
                    .GroupBy(x => x.SelectionName)
                    .Select(t => t.Key)
                    .ToList();

                    foreach (string name in fighters)
                    {
                        fightEvents = db.oddsInfo
                        .Where(x => x.SelectionName == name && x.EventDate.CompareTo(oneWeekAgo) > 0)
                        .GroupBy(x => x.EventName)
                        .Select(x => x.Key)
                        .ToList();

                        //Fighters may have been in multiple fights
                        foreach (string ev in fightEvents)
                        {
                            //Get Average Odds First
                            DateTime EventDate = db.oddsInfo
                            .Where(x => x.SelectionName == name && x.EventName == ev && x.DateTaken.CompareTo(x.EventDate) < 0)
                            .OrderByDescending(x => x.ID)
                            .Select(x => x.EventDate)
                            .FirstOrDefault();

                            if (EventDate == new DateTime())
                            {
                                continue;
                            }

                            List<OddsInfo> record1 = db.oddsInfo
                                .Where(x => x.SelectionName == name && x.EventName == ev && x.DateTaken.CompareTo(x.EventDate) < 0)
                                .ToList();
                            IOrderedEnumerable<OddsInfo> record2 = record1
                                // .Where(x => x.DateTaken.AddDays(1).Date == EventDate.Date)
                                .OrderByDescending(x => x.OddsValue);

                            int recordCount = record2.Count();

                            //Get Median
                            OddsInfo record = null;
                            if (recordCount != 0)
                            {
                                record = record2.Skip(recordCount / 2).First();
                            }
                            //Add to a list
                            if (record != null)
                            {
                                finalOdds.Add(new FighterFinalOddsExport
                                {
                                    EventDate = EventDate,
                                    Name = record.SelectionName,
                                    Odds = record.OddsValue,
                                    EventName = record.EventName,
                                    Winner = record.Winner ? "W" : "L"
                                });
                            }
                        }
                    }

                    dataTable.Columns.Add("Event Date", typeof(DateTime));
                    dataTable.Columns.Add("Event Name", typeof(string));
                    dataTable.Columns.Add("Selection", typeof(string));
                    dataTable.Columns.Add("Odds", typeof(double));
                    dataTable.Columns.Add("Winner", typeof(string));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.EventDate, i.EventName, i.Name, i.Odds, i.Winner);
                    }
                }

                InvokeUI(() =>
                {
                    LoadingScreen(false);
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel|*.csv"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        CreateCSVFile(dataTable, saveFileDialog.FileName, false);
                    }
                });
            });
        }

        private void ExportLastDayLastMonthOdds(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            List<FighterFinalOddsExport> finalOdds = new List<FighterFinalOddsExport>();
            DataTable dataTable = new DataTable("Results Export");

            List<string> fighters;
            List<string> fightEvents;

            Task.Run(() =>
            {
                using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                {
                    DateTime oneMonthAgo = DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0));
                    fighters = db.oddsInfo
                    .Where(x => x.EventDate.CompareTo(oneMonthAgo) > 0)
                    .GroupBy(x => x.SelectionName)
                    .Select(t => t.Key)
                    .ToList();

                    foreach (string name in fighters)
                    {
                        fightEvents = db.oddsInfo
                        .Where(x => x.SelectionName == name && x.EventDate.CompareTo(oneMonthAgo) > 0)
                        .GroupBy(x => x.EventName)
                        .Select(x => x.Key)
                        .ToList();

                        //Fighters may have been in multiple fights
                        foreach (string ev in fightEvents)
                        {
                            //Get Average Odds First
                            DateTime EventDate = db.oddsInfo
                            .Where(x => x.SelectionName == name && x.EventName == ev)
                            .OrderByDescending(x => x.ID)
                            .Select(x => x.EventDate)
                            .First();
                            //Take the odds taken 1 day before the fight
                            List<OddsInfo> record1 = db.oddsInfo
                                .Where(x => x.SelectionName == name && x.EventName == ev)
                                .ToList();
                            IOrderedEnumerable<OddsInfo> record2 = record1
                                .Where(x => x.DateTaken.AddDays(1).Date == EventDate.Date)
                                .OrderByDescending(x => x.OddsValue);

                            int recordCount = record2.Count();

                            //Get Median
                            OddsInfo record = null;
                            if (recordCount != 0)
                            {
                                record = record2.Skip(recordCount / 2).First();
                            }
                            //If there are no values from the day before, take the day of and 2 days before together
                            else
                            {
                                record2 = record1
                                .Where(x => x.DateTaken.Date == EventDate.Date || x.DateTaken.AddDays(2).Date == EventDate.Date)
                                .OrderByDescending(x => x.OddsValue);
                                recordCount = record2.Count();
                                if (recordCount != 0)
                                {
                                    record = record2.Skip(recordCount / 2).First();
                                }
                            }
                            //Add to a list
                            if (record != null)
                            {
                                finalOdds.Add(new FighterFinalOddsExport
                                {
                                    EventDate = EventDate,
                                    Name = record.SelectionName,
                                    Odds = record.OddsValue,
                                    EventName = record.EventName
                                });
                            }
                        }
                    }

                    dataTable.Columns.Add("Event Date", typeof(DateTime));
                    dataTable.Columns.Add("Event Name", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Odds", typeof(double));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.EventDate, i.EventName, i.Name, i.Odds);
                    }
                }

                InvokeUI(() =>
                {
                    LoadingScreen(false);
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel|*.csv"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        CreateCSVFile(dataTable, saveFileDialog.FileName, false);
                    }
                });
            });
        }

        /// <summary>
        /// Handles the SelectionChanged event of the FighterSelector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void FighterSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainMessage("Generating Graph...");
            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                if (EventSelector.SelectedItem != null)
                {
                    List<string> fights = db.oddsInfo
                        .Where(x => x.EventName == EventSelector.SelectedItem.ToString())
                        .OrderBy(o => o.DateTaken)
                        .GroupBy(x => x.SelectionName)
                        .Select(x => x.Key)
                        .ToList();

                    //Clear list and add all fights to it
                    SelectionSelector.Items.Clear();
                    SelectionSelector.Items.Add("All");

                    foreach (string item in fights)
                    {
                        SelectionSelector.Items.Add(item);
                    }
                    SelectionSelector.SelectedIndex = 0;
                }
                else
                {
                    SelectionSelector.SelectedIndex = -1;
                }
            }
            if (EventSelector.SelectedIndex != -1)
            {
                UpdateGraph();
            }
            MainMessage("");
        }

        private void SelectionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionSelector.SelectedIndex != -1)
            {
                UpdateGraph();
            }
        }

        private void graphBetsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GraphsGrid.Visibility == Visibility.Visible)
            {
                return;
            }

            if (DateSelector.Items.Count == 0)
            {
                foreach (DateSelection.ComboSelection E in Enum.GetValues(typeof(DateSelection.ComboSelection)))
                {
                    DateSelector.Items.Add(E);
                }
            }
            DateSelector.SelectedIndex = 0;
            ShowWindow(GraphsGrid);
        }

        private void InsertOneFightToList(Event fn, int i)
        {
            //rowdef.SetValue(Grid.BackgroundProperty, backgroundBrush);

            ////////////////////

            //double height = 25;
            //Brush backgroundBrush = Brushes.Beige;

            //RowDefinition rowdef;

            ////Rn through each fighter in event
            //ISelection s = ChooseSelectionType(fn);
            //foreach (Runner run in s.Runners)
            //{
            //    int numRows = ListGrid.RowDefinitions.Count;
            //    if (numRows % 2 == 0)
            //    {
            //        if (backgroundBrush == Brushes.Beige)
            //        {
            //            backgroundBrush = Brushes.White;
            //        }
            //        else
            //        {
            //            backgroundBrush = Brushes.Beige;
            //        }
            //    }
            //    rowdef = new RowDefinition
            //    {
            //        Height = new GridLength(height)
            //    };
            //    rowdef.SetValue(Panel.BackgroundProperty, backgroundBrush);

            //    ListGrid.RowDefinitions.Insert(numRows, rowdef);
            //    CheckBox toggleLoggingChk = new CheckBox();
            //    toggleLoggingChk.SetValue(Grid.RowProperty, numRows);
            //    toggleLoggingChk.SetValue(Grid.ColumnProperty, 0);
            //    toggleLoggingChk.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //    toggleLoggingChk.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            //    toggleLoggingChk.Click += ToggleLoggingFighter;
            //    toggleLoggingChk.Tag = run.Name + '|' + fn.Name;

            //    Label date = new Label
            //    {
            //        Content = fn.Date.ToString("MMMM-dd-HH:mm"),
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            //        VerticalAlignment = System.Windows.VerticalAlignment.Center
            //    };
            //    date.SetValue(Grid.RowProperty, numRows);
            //    date.SetValue(Grid.ColumnProperty, 1);

            //    Label fightEventName = new Label
            //    {
            //        Content = fn.Name,
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            //    };
            //    //fighterName.VerticalAlignment = VerticalAlignment.Center;
            //    fightEventName.SetValue(Grid.RowProperty, numRows);
            //    fightEventName.SetValue(Grid.ColumnProperty, 2);

            //    Label selection = new Label
            //    {
            //        Content = run.Name,
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            //    };
            //    selection.SetValue(Grid.RowProperty, numRows);
            //    selection.SetValue(Grid.ColumnProperty, 3);

            //    Label Odds = new Label
            //    {
            //        Content = run.Odds,
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            //    };
            //    //Odds.VerticalAlignment = VerticalAlignment.Center;
            //    Odds.SetValue(Grid.RowProperty, numRows);
            //    Odds.SetValue(Grid.ColumnProperty, 4);

            //    Label Percent = new Label
            //    {
            //        Content = run.PercentChance,
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            //    };
            //    //Odds.VerticalAlignment = VerticalAlignment.Center;
            //    Percent.SetValue(Grid.RowProperty, numRows);
            //    Percent.SetValue(Grid.ColumnProperty, 5);

            //    Label Mult = new Label
            //    {
            //        Content = run.Multiplier.ToString(),
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            //    };
            //    Mult.SetValue(Grid.RowProperty, numRows);
            //    Mult.SetValue(Grid.ColumnProperty, 6);

            //    Label Change = new Label
            //    {
            //        Content = (run.LastOdds.ToDouble() - run.Odds.ToDouble()).ToString(),
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            //    };
            //    Change.SetValue(Grid.RowProperty, numRows);
            //    Change.SetValue(Grid.ColumnProperty, 7);

            //    Grid color = new Grid();
            //    color.SetValue(Grid.RowProperty, numRows);
            //    color.SetValue(Grid.ColumnSpanProperty, 100);
            //    color.SetValue(Panel.BackgroundProperty, backgroundBrush);

            //    ListGrid.Children.Add(color);
            //    ListGrid.Children.Add(date);
            //    ListGrid.Children.Add(selection);
            //    ListGrid.Children.Add(fightEventName);
            //    ListGrid.Children.Add(Odds);
            //    ListGrid.Children.Add(toggleLoggingChk);
            //    ListGrid.Children.Add(Percent);
            //    ListGrid.Children.Add(Mult);
            //    ListGrid.Children.Add(Change);

            //    //Log Selection
            //    ChangeLogging(fn, run, true);
            //    toggleLoggingChk.IsChecked = true;

            //    i++;
            //}
            //BettingInfoListed.Add(fn);
        }

        private void listBetsBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);

            if (BettingInfoListed == null)
            {
                BettingInfoListed = new List<Event>();
            }
            else if (myGuiProperties.SelectionToDisplay.Count == 0)
            {
                try
                {
                    Task.Run(() =>
                    {
                        GetBettingInfo(settings.EventType);
                        InvokeUI(() =>
                        {
                            DisplayBettingInfo();
                            ShowWindow(ListBetsGrid);
                        });
                    });
                }
                catch (Exception)
                {
                    ReLogin();
                    try
                    {
                        Task.Run(() =>
                        {
                            GetBettingInfo(settings.EventType);
                            InvokeUI(() =>
                            {
                                DisplayBettingInfo();
                                ShowWindow(ListBetsGrid);
                            });
                        });
                    }
                    catch (Exception ex)
                    {
                        DisplayError(marketMessenger, "Could not Fill Betting Info: " + ex.Message);
                    }
                }
            }
            else
            {
                ShowWindow(ListBetsGrid);
            }

            LoadingScreen(false);
        }

        private void LoadingScreen(bool v) => LoadingViewGrid.Visibility = v ? Visibility.Visible : Visibility.Hidden;

        private void LogAllchk_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chkSender = sender as CheckBox;

            myGuiProperties.SelectionToDisplay.ForEach(x => x.Selected = (bool)chkSender.IsChecked);

            
            foreach (var sel in myGuiProperties.SelectionToDisplay)
            {
                var s = ChooseSelectionType(BettingInfoAvailable.Where(x => x.Name == sel.EventName).First());
                Runner f = s.Runners.Where(x => x.Name == sel.EventName).FirstOrDefault();

                if (f != null)
                {
                    switch (settings.EventType)
                    {
                        case "Match Odds":
                            ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f) && x.Name == sel.EventName).First(), f, (bool)chkSender.IsChecked);
                            break;

                        case "Go The Distance?":
                            ChangeLogging(BettingInfoAvailable.Where(x => x.GoTheDistance.Runners.Contains(f) && x.Name == sel.EventName).First(), f, (bool)chkSender.IsChecked);
                            break;

                        case "Round Betting":
                            ChangeLogging(BettingInfoAvailable.Where(x => x.RoundBetting.Runners.Contains(f) && x.Name == sel.EventName).First(), f, (bool)chkSender.IsChecked);
                            break;

                        case "Method of Victory":
                            ChangeLogging(BettingInfoAvailable.Where(x => x.MethodOfVictory.Runners.Contains(f) && x.Name == sel.EventName).First(), f, (bool)chkSender.IsChecked);
                            break;

                        default:
                           ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f) && x.Name == sel.EventName).First(), f, (bool)chkSender.IsChecked);
                            break;
                    }
                    
                }
            }

            //foreach (CheckBox chk in ListGrid.Children.OfType<CheckBox>())
            //{
            //    if (chk.Tag != null)
            //    {
            //        //var f = BettingInfoAvailable.Where(x => x.Name == chk.Tag.ToString()).FirstOrDefault();
            //        //if (f != null)
            //        //{
            //        //    bool logged = ChangeLogging(f, f.FightResult.Runners.First(), (bool)chkSender.IsChecked);
            //        //    if (logged != (bool)chkSender.IsChecked) DisplayError(this, "Could not log, was not able to connect");
            //        //    chk.IsChecked = logged;
            //        //}
            //        string selectionName = chk.Tag.ToString().Split('|')[0];
            //        string eventName = chk.Tag.ToString().Split('|')[1];
            //        var sel = ChooseSelectionType(BettingInfoAvailable.Where(x => x.Name == eventName).First());
            //        Runner f = sel.Runners.Where(x => x.Name == selectionName).FirstOrDefault();

            //        if (f != null)
            //        {
            //            bool logged = false;
            //            switch (settings.EventType)
            //            {
            //                case "Match Odds":
            //                    logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Select(z => z.Name).ToList().Contains(selectionName) && x.Name == eventName).First(), f, (bool)chkSender.IsChecked);
            //                    break;

            //                case "Go The Distance?":
            //                    logged = ChangeLogging(BettingInfoAvailable.Where(x => x.GoTheDistance.Runners.Contains(f) && x.Name == eventName).First(), f, (bool)chkSender.IsChecked);
            //                    break;

            //                case "Round Betting":
            //                    logged = ChangeLogging(BettingInfoAvailable.Where(x => x.RoundBetting.Runners.Contains(f) && x.Name == eventName).First(), f, (bool)chkSender.IsChecked);
            //                    break;

            //                case "Method of Victory":
            //                    logged = ChangeLogging(BettingInfoAvailable.Where(x => x.MethodOfVictory.Runners.Contains(f) && x.Name == eventName).First(), f, (bool)chkSender.IsChecked);
            //                    break;

            //                default:
            //                    logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f) && x.Name == eventName).First(), f, (bool)chkSender.IsChecked);
            //                    break;
            //            }

            //            if (logged != chkSender.IsChecked)
            //            {
            //                DisplayError(this, "Could not log, was not able to connect");
            //            }

            //            chk.IsChecked = logged;
            //        }
            //        else
            //        {
            //            //If not available just change it anyway
            //            chk.IsChecked = (bool)chkSender.IsChecked;
            //        }
            //    }
            //}
        }

        private void LoggingIntervalTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            TextBox txtBox = sender as TextBox;
            if (key == Key.Return)
            {
                TimeSpan.TryParse(txtBox.Text, out TimeSpan span);
                if (span != null)
                {
                    OddsLogger.logInterval = span;
                    txtBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                }
            }
        }

        private void ShortLoggingIntervalTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            TextBox txtBox = sender as TextBox;
            if (key == Key.Return)
            {
                TimeSpan.TryParse(txtBox.Text, out TimeSpan span);
                if (span != null)
                {
                    OddsLogger.shortLogInterval = span;
                    txtBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                }
            }
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e) => ShowWindow(LoginGrid);

        private void loginUserBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            string username = usernameTxtBox.Text;
            string password = passTxtBox.Password.ToString();
            if (username == "" || password == "")
            {
                LoginUserBtn.Background = Brushes.Red;
                return;
            }

            Task.Run(() =>
            {
                loginClient = new LoginClient(username, password);
                if (loginClient.SessionToken == null)
                {
                    InvokeUI(() =>
                    {
                        DisplayError(loginClient, "Could not log in");
                        LoginUserBtn.Background = Brushes.Red;
                        LoginBtn.Background = Brushes.Red;
                        LoadingScreen(false);
                    });
                    return;
                }
                else
                {
                    InvokeUI(() =>
                    {
                        LoginUserBtn.Background = Brushes.Green;
                        LoginBtn.Background = Brushes.Green;
                        myGuiProperties.LoggedUserName = username;

                        twelveHourRefreshTimer.Start();
                        getResultsTimer.Start();
                        //Set up rememberMe
                        CheckBox remember = LoggingGrid.Children.OfType<CheckBox>().FirstOrDefault();
                        using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                        {
                            User user = new User() { Username = username, Password = password };
                            db.ChangeRemember(user, (bool)remember.IsChecked);
                        }
                    });
                }

                myGuiProperties.UserLoggedIn = true;
                myGuiProperties.NavigationVisibility = Visibility.Visible;
                //mainMessage.Content = "Login Successful";
                marketMessenger.Initialise(loginClient.SessionToken);
                marketMessenger.SetMarketFilter("Soccer");
                var bal = marketMessenger.GetAccountBalance();
                myGuiProperties.CurrentBalance = bal.CurrentAvailable;
                myGuiProperties.CurrentExposure = bal.Exposure;
                myGuiProperties.TotalBalance = bal.Total;
                InvokeUI(() => LoadingScreen(false));
            });
        }

        private void LogNowBtn_Click(object sender, RoutedEventArgs e)
        {
            MainMessage("Logging...");
            OddsLogger.LogOddsNow(settings.EventType);
            MainMessage("");
        }

        private void LogoutUser(object sender, RoutedEventArgs e)
        {
            OddsLogger.StopLoggingAsync();
            StartLoggingBtn.Background = Brushes.Red;
            myGuiProperties.NavigationVisibility = Visibility.Hidden;
            myGuiProperties.LoggedUserName = "No User";
            LoginUserBtn.Background = Brushes.Red;
            LoginBtn.Background = Brushes.Red;
            ShowWindow(LoginGrid);
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            Task.Run(() =>
            {
                MainMessage("Refreshing List...");
                GetBettingInfo(settings.EventType);
                //myGuiProperties.ResultTypes = marketMessenger.GetEventTypes("Soccer");
                var bal = marketMessenger.GetAccountBalance();
                myGuiProperties.CurrentBalance = bal.CurrentAvailable;
                myGuiProperties.CurrentExposure = bal.Exposure;
                myGuiProperties.TotalBalance = bal.Total;

                FindAndFillResults();
                InvokeUI(() =>
                {
                    UpdateList();
                    Event f = BettingInfoAvailable.Where(x => x.Name == EventSelector.Text && x.Name == SelectionSelector.SelectedItem.ToString()).FirstOrDefault();
                    if (f != null)
                    {
                        oxyPlotView.Model = CreateGraphView(f).CreateOxyPlot();
                    }
                    ResultTypeSelectorCombo.SelectedIndex = 0;
                    LoadingScreen(false);
                    MainMessage("");
                });
            });

            if ((bool)AutoRefreshChk.IsChecked)
            {
                //reset timer
                autoRefreshTimer.Stop();
                autoRefreshTimer.Start();
            }
        }

        private void FindAndFillResults()
        {
            List<Event> winnerList = BettingInfoAvailable.Where(x => x.Winner != null).ToList();
            List<OddsInfo> oddsList = new List<OddsInfo>();
            foreach (Event w in winnerList)
            {
                ISelection p = ChooseSelectionType(w);
                oddsList.AddRange(p.ToOddsInfo(w));
            }
            OddsLogger.FillResults(oddsList);
        }

        private void RefreshEventNameList()
        {
            LoadingScreen(true);
            InvokeUI(() =>
            {
                pastFightsGrid.Children.Clear();
                pastFightsGrid.RowDefinitions.RemoveRange(1, pastFightsGrid.RowDefinitions.Count - 1);
                DisplayPastFights();
                LoadingScreen(false);
            });
        }

        private void GetResultsFromXML(string filePath)
        {
#pragma warning disable IDE0022 // Use expression body for methods
            Task.Run(() =>
            {
                string url = "http://rss.betfair.com/RSS.aspx?format=rss&sportID=1";
                XmlReaderSettings p = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Parse
                };
                XmlReader reader = XmlReader.Create(url, p);

                int i = 0;
                while (i < 10)
                {
                    try
                    {
                        var feed = SyndicationFeed.Load(reader);
                        reader.Close();
                        List<Result> results = new List<Result>();
                        foreach (SyndicationItem item in feed.Items)
                        {
                            if (item.Title.Text.Contains("Match Odds settled"))
                            {
                                if (!results.Select(x => x.RawInput).ToList().Contains(item.Title.Text))
                                {
                                    results.Add(new Result(item.Title.Text, item.Summary.Text));
                                }
                            }
                        }
                        if (results.Count > 0)
                        {
                            ProcessResults(results);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        reader.Close();
                        i++;
                        if (i >= 10)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            });
#pragma warning restore IDE0022 // Use expression body for methods
        }

        private void ProcessResults(List<Result> results)
        {
            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                foreach (var res in results)
                {
                    if (res.Success)
                    {
                        string EventName = res.EventName;

                        //Remove datapoints
                        var dataPoints = db.oddsInfo
                            .Where(x => x.EventName == res.EventName);
                        foreach (var point in dataPoints)
                        {
                            point.Winner = point.SelectionName.Trim().ToLower() == res.Winner.ToLower();
                        }
                    }
                    else
                    {
                    }
                }
                db.SaveChanges();
            }
        }

        private void FillBetResults()
        {
            var commissionRate = marketMessenger.GetCommissionRate();
            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                var settledBets = marketMessenger.GetSettledBets(TimeSpan.FromDays(1));

                foreach (var bet in settledBets)
                {
                    var res = db.results.Where(x => x.MarketId == bet.MarketId).FirstOrDefault();
                    if (res != null)
                    {
                        res.AmountMatched = bet.MatchedAmount;
                        res.AmountWon = bet.Profit;
                        res.Reference = bet.Profit > 0 ? (bet.Profit * commissionRate).ToString("0.00") : "0.00";
                        res.Winner = bet.Profit > 0;
                    }
                    else
                    {
                    }
                }

                db.SaveChanges();
            }
        }

        private void RemoveFightFromDb(object sender, RoutedEventArgs e)
        {
            Button fight = sender as Button;
            string message = "Are you sure you wish to remove this fight record from the database?";
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            // Show message box
            MessageBoxResult result = MessageBox.Show(message, "", buttons);
            if (result == MessageBoxResult.Yes)
            {
                int pointsRemoved;
                using (SoccerDatabaseModel db = new SoccerDatabaseModel())
                {
                    string EventName = fight.Tag.ToString();
                    //Remove datapoints
                    IQueryable<OddsInfo> dataPoints = db.oddsInfo
                        .Where(x => x.EventName == EventName);
                    db.oddsInfo.RemoveRange(dataPoints);
                    pointsRemoved = db.SaveChanges();
                }
                MessageBox.Show(pointsRemoved.ToString() + " Points Removed");
                RefreshEventNameList();
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e) => ShowWindow(SettingsGrid);

        private void SetUpEvents()
        {
            OddsLogger.MarketplaceErrorOccured += ReLogin;
            dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            autoRefreshTimer.Elapsed += AutoRefreshTimer_Elapsed;
            twelveHourRefreshTimer.Elapsed += TwentyFourHourRefreshTimer_Elapsed;
            getResultsTimer.Elapsed += GetResultsTimer_Elapsed;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if((string)e.Column.Header == "Selected")
            {
                e.Column.IsReadOnly = false;
                
            }
            else
            {
                e.Column.IsReadOnly = true;
            }
        }

        private void GetResultsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetResultsFromXML(resultsFilePath);
            FillBetResults();
            getResultsTimer.Stop();
            getResultsTimer.Start();
        }

        private void TwentyFourHourRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //GetResultsFromXML(resultsFilePath);
            ReLogin();
            if (loginClient.SessionToken != null)
            {
                twelveHourRefreshTimer.Stop();
                twelveHourRefreshTimer.Start();
            }
            else
            {
                DisplayError(this, "Error using 24 hour refresh, please try again");
            }
        }

        private void ReLogin(object sender = null, string error = null)
        {
            loginClient.GetNewSessionToken();
            if (loginClient.SessionToken != null)
            {
                marketMessenger.Initialise(loginClient.SessionToken);
                marketMessenger.SetMarketFilter("Soccer");

                marketMessenger.GetBettingDictionary(settings.EventType);
            }
            if (error != null)
            {
                DisplayError(OddsLogger, error);
            }

            myGuiProperties.ReloginTimes++;

            //MessageBox.Show("ReLogin Tried, Session token = " + loginClient.SessionToken != null ? "Successful" : "Unsuccessful");
        }

        private void ShowPastFightsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pastFightsGrid.RowDefinitions.Count > 2)
            {
                RefreshEventNameList();
            }
            else
            {
                DisplayPastFights();
            }
            //Show the Window
            ShowWindow(ListPastFightsGrid);
        }

        private void ShowWindow(Grid gridName)
        {
            //LoadingScreen(false);
            foreach (Button b in NavigationButtons)
            {
                b.FontWeight = System.Windows.FontWeights.Normal;
                b.Background = Brushes.WhiteSmoke;
            }
            foreach (Grid window in Windows)
            {
                if (window == gridName)
                {
                    window.Visibility = Visibility.Visible;
                }
                else
                {
                    window.Visibility = Visibility.Hidden;
                }
            }
            if (NavigationSet.ContainsKey(gridName))
            {
                Button btn = NavigationSet[gridName];
                btn.FontWeight = System.Windows.FontWeights.Bold;
                btn.Background = Brushes.Gray;
            }
        }

        private void startLoggingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!OddsLogger.Logging)
            {
                OddsLogger.StartLoggingAsync(settings.EventType);
                StartLoggingBtn.IsChecked = true;
            }
            else
            {
                OddsLogger.StopLoggingAsync();
                StartLoggingBtn.IsChecked = false;
            }
        }

        private void ToggleLoggingFighter(object sender, RoutedEventArgs e)
        {
            //foreach (CheckBox chk in ListGrid.Children.OfType<CheckBox>())
            //{
            //    if (sender == chk)
            //    {
            //        List<Runner> list = new List<Runner>();
            //        List<ISelection> selection = ChooseSelectionType(BettingInfoAvailable, chk.Tag.ToString().Split('|')[1]);

            //        foreach (List<Runner> l in selection.Select(y => y.Runners))
            //        {
            //            list.AddRange(l);
            //        }

            //        Runner f = list.Where(x => x.Name == chk.Tag.ToString().Split('|')[0]).FirstOrDefault();
            //        bool logged = false;
            //        switch (settings.EventType)
            //        {
            //            case "Match Odds":
            //                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
            //                break;

            //            case "Go The Distance?":
            //                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.GoTheDistance.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
            //                break;

            //            case "Round Betting":
            //                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.RoundBetting.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
            //                break;

            //            case "Method of Victory":
            //                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.MethodOfVictory.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
            //                break;

            //            default:
            //                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
            //                break;
            //        }

            //        if (logged != chk.IsChecked)
            //        {
            //            DisplayError(this, "Could not log, was not able to connect");
            //        }

            //        chk.IsChecked = logged;
            //        break;
            //    }
            //}
        }

        private void UpdateGraph()
        {
            Graph g;

            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                if (SelectionSelector.SelectedItem.ToString() != "All")
                {
                    Dictionary<DateTime, double> dict1 = db.oddsInfo
                        .Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == SelectionSelector.SelectedItem.ToString())
                        .Select(t => new { t.DateTaken, t.OddsValue })
                        .GroupBy(x => x.DateTaken)
                        .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    g = new Graph(dict1, EventSelector.SelectedItem.ToString(), "Date", "Odds");
                    oxyPlotView.Model = g.CreateOxyPlot();
                }
                else if (SelectionSelector.Items.Count > 3)
                {
                    string selections = SelectionSelector.Items[0].ToString();
                    string selection1 = SelectionSelector.Items[1].ToString();
                    Dictionary<DateTime, double> dict1 = db.oddsInfo
                        .Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == selection1)
                        .Select(t => new { t.DateTaken, t.OddsValue })
                        .GroupBy(x => x.DateTaken)
                        .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    string selection2 = SelectionSelector.Items[2].ToString();
                    Dictionary<DateTime, double> dict2 = db.oddsInfo
                        .Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == selection2)
                        .Select(t => new { t.DateTaken, t.OddsValue })
                        .GroupBy(x => x.DateTaken)
                        .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    string selection3 = SelectionSelector.Items[3].ToString();
                    Dictionary<DateTime, double> dict3 = db.oddsInfo
                        .Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == selection3)
                        .Select(t => new { t.DateTaken, t.OddsValue })
                        .GroupBy(x => x.DateTaken)
                        .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    g = new Graph(dict1, dict2, EventSelector.SelectedItem.ToString(), "Date", SelectionSelector.Items[1].ToString(), SelectionSelector.Items[2].ToString(), dict3, SelectionSelector.Items[3].ToString());
                    oxyPlotView.Model = g.CreateOxyPlot();
                }
                else if (SelectionSelector.Items.Count > 2)
                {
                    string selections = SelectionSelector.Items[0].ToString();
                    string selection1 = SelectionSelector.Items[1].ToString();
                    Dictionary<DateTime, double> dict1 = db.oddsInfo
                        .Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == selection1)
                        .Select(t => new { t.DateTaken, t.OddsValue })
                        .GroupBy(x => x.DateTaken)
                        .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    string selection2 = SelectionSelector.Items[2].ToString();
                    Dictionary<DateTime, double> dict2 = db.oddsInfo
                        .Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == selection2)
                        .Select(t => new { t.DateTaken, t.OddsValue })
                        .GroupBy(x => x.DateTaken)
                        .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    g = new Graph(dict1, dict2, EventSelector.SelectedItem.ToString(), "Date", SelectionSelector.Items[1].ToString(), SelectionSelector.Items[2].ToString());
                    oxyPlotView.Model = g.CreateOxyPlot();
                }
            }

            ExportBtn.IsEnabled = true;
        }

        private void UpdateList()
        {
            //try
            //{
            //    //Clear Data from grid first
            //    //ClearListGridData();

            //    //DisplayBettingInfo();

            //    for (int i = 0; i < BettingInfoAvailable.Count; i++)
            //    {
            //        if (BettingInfoListed.Select(x => x.Name).ToList().Contains(BettingInfoAvailable[i].Name))
            //        {
            //            ISelection select = ChooseSelectionType(BettingInfoAvailable.ElementAt(i));
            //            foreach (Runner sel in select.Runners)
            //            {
            //                bool check = ListGrid.Children.OfType<Label>()
            //                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 2 && x.Content.ToString() == BettingInfoAvailable[i].Name)
            //                    .Count() > 0;
            //                //If row no longer there, continue
            //                if (!check)
            //                {
            //                    continue;
            //                }
            //                List<int> rows = ListGrid.Children.OfType<Label>()
            //                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && x.Content.ToString() == sel.Name)
            //                    .Select(x => (int)x.GetValue(Grid.RowProperty)).ToList();

            //                int targetRow = 0;
            //                object p = ListGrid.Children.OfType<Label>()
            //                .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 2 && x.Content.ToString() == BettingInfoAvailable[i].Name && rows.Contains((int)x.GetValue(Grid.RowProperty)))
            //                .Select(x => x.GetValue(Grid.RowProperty)).FirstOrDefault();
            //                if (p != null)
            //                {
            //                    targetRow = (int)p;

            //                    ListGrid.Children.OfType<Label>()
            //                        .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 1 && (int)x.GetValue(Grid.RowProperty) == targetRow)
            //                        .First().Content = BettingInfoAvailable.ElementAt(i).Date.ToString("MMMM-dd-HH:mm");

            //                    ListGrid.Children.OfType<Label>()
            //                        .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && (int)x.GetValue(Grid.RowProperty) == targetRow)
            //                        .First().Content = sel.Name;

            //                    //Capture current odds for comparison

            //                    Label changeVal = ListGrid.Children.OfType<Label>()
            //                        .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 7 && (int)x.GetValue(Grid.RowProperty) == targetRow)
            //                        .First();

            //                    double currentOdds = ListGrid.Children.OfType<Label>()
            //                        .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 4 && (int)x.GetValue(Grid.RowProperty) == targetRow)
            //                        .First().Content.ToDouble();

            //                    changeVal.Content = (sel.Odds.ToDouble() - currentOdds).ToString();
            //                    changeVal.Foreground = changeVal.Content.ToDouble() >= 0 ? Brushes.ForestGreen : Brushes.DarkRed;

            //                    ListGrid.Children.OfType<Label>()
            //                        .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 4 && (int)x.GetValue(Grid.RowProperty) == targetRow)
            //                        .First().Content = sel.Odds;

            //                    ListGrid.Children.OfType<Label>()
            //                        .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 5 && (int)x.GetValue(Grid.RowProperty) == targetRow)
            //                        .First().Content = sel.PercentChance;

            //                    ListGrid.Children.OfType<Label>()
            //                        .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 6 && (int)x.GetValue(Grid.RowProperty) == targetRow)
            //                        .First().Content = sel.Multiplier.ToString();
            //                }
            //            }
            //        }
            //        else
            //        {
            //            InsertOneFightToList(BettingInfoAvailable[i], i);
            //        }
            //    }

            //    //Lets remove fights that have happened

            //    RemoveFightsNotAvailable();

            //    //var pastFights = BettingInfoListed.Where(x=>BettingInfoAvailable.Select(x=>x.Name).Contains(x))
            //}
            //catch (Exception ex)
            //{
            //    if (ex.Message != "Collection was modified; enumeration operation may not execute.")
            //    {
            //        DisplayError(marketMessenger, "Could not Fill Rows: " + ex.Message);
            //    }
            //}
        }

        private void RemoveFightsNotAvailable()
        {
            //IEnumerable<string> availableNames = BettingInfoAvailable.Select(y => y.Name);
            //IEnumerable<string> missingEventNames = BettingInfoListed.Select(x => x.Name).Where(x => !availableNames.Contains(x));
            //if (missingEventNames.Count() > 0)
            //{
            //    List<Event> missingEvents = BettingInfoListed.Where(x => missingEventNames.Contains(x.Name)).ToList();

            //    foreach (Event ev in missingEvents)
            //    {
            //        ISelection select = ChooseSelectionType(ev);
            //        foreach (Runner sel in select.Runners)
            //        {
            //            //int targetRow = (int)ListGrid.Children.OfType<Label>()
            //            //           .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && x.Content.ToString() == sel.Name)
            //            //           .First().GetValue(Grid.RowProperty);

            //            List<int> rows = ListGrid.Children.OfType<Label>()
            //                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && x.Content.ToString() == sel.Name)
            //                    .Select(x => (int)x.GetValue(Grid.RowProperty)).ToList();

            //            int targetRow = ListGrid.Children.OfType<Label>()
            //            .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 2 && x.Content.ToString() == ev.Name && rows.Contains((int)x.GetValue(Grid.RowProperty)))
            //            .Select(x => (int)x.GetValue(Grid.RowProperty)).FirstOrDefault();
            //            if (targetRow > 0)
            //            {
            //                ChangeLogging(ev, sel, false);
            //                RemoveRowFromListGrid(targetRow);
            //            }
            //        }
            //    }
            //    BettingInfoListed.RemoveAll(x => missingEventNames.Contains(x.Name));
            //}
        }

        private void ViewBalance(object sender, RoutedEventArgs e)
        {
            //TODO: Implement ViewBalance
        }

        private void ResultTypeSelectorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadingScreen(true);
            object result = ResultTypeSelectorCombo.SelectedItem;
            settings.EventType = result.ToString();

            if (loginClient != null)
            {
                //Lets update the listGrid
                Task.Run(() =>
                {
                    GetBettingInfo(settings.EventType);
                    Task.Delay(500);
                    InvokeUI(() =>
                    {
                        ClearListGridData();
                        DisplayBettingInfo();
                        DateSelector.SelectedIndex = -1;
                        //LogAllchk.IsChecked = false;
                        LoadingScreen(false);
                    });
                });
            }
            else
            {
                LoadingScreen(false);
            }
            if ((bool)AutoRefreshChk.IsChecked)
            {
                //reset timer
                autoRefreshTimer.Stop();
                autoRefreshTimer.Start();
            }
        }

        /// <summary>
        /// Handles the KeyDown event of the AutoRefreshIntervalTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void AutoRefreshIntervalTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            TextBox txtBox = sender as TextBox;
            if (key == Key.Return)
            {
                TimeSpan.TryParse(txtBox.Text, out TimeSpan span);
                if (span != null)
                {
                    autoRefreshTimer.Interval = span.TotalMilliseconds;
                    txtBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                }
            }
        }

        private void RemoveRowFromListGrid(int rowVal)
        {
            //List<UIElement> childList = new List<UIElement>();
            //ListGrid.RowDefinitions.RemoveAt(rowVal);
            //for (int i = 0; i < ListGrid.Children.Count; i++)
            //{
            //    UIElement el = ListGrid.Children[i];
            //    int row = Grid.GetRow(el);
            //    if (row == rowVal)
            //    {
            //        childList.Add(el);
            //    }
            //    else if (row > rowVal)
            //    {
            //        el.SetValue(Grid.RowProperty, row - 1);
            //    }
            //}
            //foreach (UIElement elemen in childList)
            //{
            //    ListGrid.Children.Remove(elemen);
            //}
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //Just fuck up the session token
            char[] k = loginClient.SessionToken.ToCharArray();
            k[3] = 'g';
            loginClient.SessionToken = k.ToString();
            marketMessenger.Initialise(loginClient.SessionToken);
        }

        private void OverLevelTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double.TryParse(OverLevelTextBox.Text, out double span);
            if (span != 0)
            {
                OddsLogger.MinBetLevel = span;
            }
        }

        private void UnderLevelTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double.TryParse(UnderLevelTextBox.Text, out double span2);
            if (span2 != 0)
            {
                OddsLogger.MaxBetLevel = span2;
            }
        }

        private void BetAmountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double.TryParse(BetAmountTextBox.Text, out double span);
            if (span != 0)
            {
                OddsLogger.BetAmount = span;
            }
        }

        private void ResultsBtn_Click(object sender, RoutedEventArgs e)
        {
            Graph g;

            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                Dictionary<int, double> dict1 = new Dictionary<int, double>();
                Dictionary<int, double> dict2 = new Dictionary<int, double>();
                var l = db.results
                    //.Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == SelectionSelector.SelectedItem.ToString())
                    //.Select(t => new { t.EventStart, t.AmountWon })
                    .OrderBy(x => x.EventStart)
                    .ToList();

                if (l.Count > 0)
                {
                    double sum = 0;
                    if ((bool)ResultsByDateChk.IsChecked)
                    {
                        for (int i = 0; i < l.Count; i++)
                        {
                            sum += (l[i].AmountWon - Convert.ToDouble(l[i].Reference));
                            if (i > 20)
                            {
                                dict2.Add(i, sum / i.ToDouble());
                            }
                        }
                        //.ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                        g = new Graph(dict2, "Betting Results", "Date", "Odds");
                        oxyPlotViewResults.Model = g.CreateOxyPlot();
                    }
                    else
                    {
                        for (int i = 0; i < l.Count; i++)
                        {
                            sum += (l[i].AmountWon - Convert.ToDouble(l[i].Reference));
                            dict1.Add(i, sum);
                        }

                        //.ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                        g = new Graph(dict1, "Betting Results", "Bet", "Odds");
                        oxyPlotViewResults.Model = g.CreateOxyPlot();
                    }
                }

                ShowWindow(ResultsGrid);
            }
        }

        private void ResultsNowBtn_Click(object sender, RoutedEventArgs e) =>
            //LoadingScreen(true);
            Task.Run(() =>
            {
                MainMessage("Getting Results from XML...");
                GetResultsFromXML(resultsFilePath);
                MainMessage("Filling Results...");
                FillBetResults();
                MainMessage("");
            });//LoadingScreen(false);

        private void MainMessage(string v) => InvokeUI(() =>
                                            {
                                                myGuiProperties.MainMessage = v;
                                            });

        private void ActiveOrderingChk_Click(object sender, RoutedEventArgs e) => OddsLogger.OrderingActive = (bool)ActiveOrderingChk.IsChecked;

        private void ResultsByNameChk_Click(object sender, RoutedEventArgs e)
        {
            Graph g;

            using (SoccerDatabaseModel db = new SoccerDatabaseModel())
            {
                Dictionary<int, double> dict1 = new Dictionary<int, double>();
                Dictionary<int, double> dict2 = new Dictionary<int, double>();
                var l = db.results
                    //.Where(x => x.EventName == EventSelector.SelectedItem.ToString() && x.SelectionName == SelectionSelector.SelectedItem.ToString())
                    //.Select(t => new { t.EventStart, t.AmountWon })
                    .OrderBy(x => x.EventStart)
                    .ToList();
                double sum = 0;
                if ((bool)ResultsByDateChk.IsChecked)
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        sum += (l[i].AmountWon - Convert.ToDouble(l[i].Reference));
                        if (i > 20)
                        {
                            dict2.Add(i, sum / i.ToDouble());
                        }
                    }
                    //.ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    g = new Graph(dict2, "Betting Results", "Date", "Odds");
                    oxyPlotViewResults.Model = g.CreateOxyPlot();
                }
                else
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        sum += (l[i].AmountWon - Convert.ToDouble(l[i].Reference));
                        dict1.Add(i, sum);
                    }
                    //.ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    g = new Graph(dict1, "Betting Results", "Bet", "Odds");
                    oxyPlotViewResults.Model = g.CreateOxyPlot();
                }
            }
        }

        private void FillingBetResultsClick(object sender, RoutedEventArgs e) => FillBetResults();

        private void BetLimitTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double.TryParse(BetLimitTextBox.Text, out double span);
            if (span != 0)
            {
                OddsLogger.BetLimit = span;
            }
        }
    }
}