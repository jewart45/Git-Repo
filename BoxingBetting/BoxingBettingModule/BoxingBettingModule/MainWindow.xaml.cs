using BoxingBettingModule.Classes;
using BoxingDatabase;
using BoxingDatabase.Tables;
using CommonClasses;
using LoginClientLib;
using Marketplace;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BoxingBettingModule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MarketplaceMessenger marketMessenger;
        private Timer autoRefreshTimer;
        private List<FightEvent> BettingInfoAvailable;
        private List<FightEvent> BettingInfoListed;
        private List<string> LoggingFighters;
        private LoginClient loginClient;
        private GUIProperties myGuiProperties;
        private List<Button> NavigationButtons;
        private Dictionary<Grid, Button> NavigationSet;
        private Logger OddsLogger;
        private SettingsVariables settings;
        private List<Grid> Windows;
        private Timer twelveHourRefreshTimer;

        public MainWindow()
        {
            marketMessenger = new MarketplaceMessenger();
            Windows = new List<Grid>();
            NavigationButtons = new List<Button>();
            NavigationSet = new Dictionary<Grid, Button>();
            LoggingFighters = new List<string>();
            myGuiProperties = new GUIProperties();
            BettingInfoListed = new List<FightEvent>();
            myGuiProperties.NavigationVisibility = Visibility.Hidden;
            myGuiProperties.LoggedUserName = "No User";
            OddsLogger = new Logger(marketMessenger, myGuiProperties.LoggingInterval);
            settings = new SettingsVariables();
            //Set the inital event types
            myGuiProperties.ResultTypes = settings.PossibleEventTypes;
            autoRefreshTimer = new Timer(myGuiProperties.AutoRefreshInterval.TotalMilliseconds);

            twelveHourRefreshTimer = new Timer(1000 * 60 * 60 * 12);    //24 hours

            InitializeComponent();
            SetUpEvents();

            DataContext = myGuiProperties;
            Windows.Add(LoginGrid);
            Windows.Add(ListBetsGrid);
            Windows.Add(GraphsGrid);
            Windows.Add(SettingsGrid);
            Windows.Add(ListPastFightsGrid);

            NavigationSet.Add(LoginGrid, LoginBtn);
            NavigationSet.Add(ListBetsGrid, ListBetsBtn);
            NavigationSet.Add(GraphsGrid, GraphBetsBtn);
            NavigationSet.Add(SettingsGrid, SettingsBtn);
            NavigationSet.Add(ListPastFightsGrid, ShowPastFightsBtn);

            //Add Nav Buttons
            NavigationButtons.Add(LoginBtn);
            NavigationButtons.Add(GraphBetsBtn);
            NavigationButtons.Add(ListBetsBtn);
            NavigationButtons.Add(ShowPastFightsBtn);
            NavigationButtons.Add(SettingsBtn);

            //Set up Gui to start
            using (BoxingDatabaseModel db = new BoxingDatabaseModel())
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
        //        case Fighter.FighterProperty.FightDate:
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
            BettingInfoAvailable = new List<FightEvent>();

            List<MarketplaceEvent> EventList = new List<MarketplaceEvent>();
            List<MarketplaceEvent> EventListWithOdds = new List<MarketplaceEvent>();

            IDictionary<string, string> RunnerDictionary;

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
            EventListWithOdds = marketMessenger.GetAllOdds(EventList, eventType, false);

            CreateEventFramework(BettingInfoAvailable, EventListWithOdds, eventType);

            //EventDictionary = marketMessenger.GetEventSelectionIDs(eventType);
            //RunnerDictionary = marketMessenger.GetRunnerSelectionIDs(eventType);
            //OddsDictionary = marketMessenger.GetAllOddsOld(RunnerDictionary.Keys.ToList(), eventType);
            //DateDictionary = marketMessenger.GetAllDates(EventDictionary.GroupBy(x => x.Value)
            //    .Select(x => x.Key).ToList());

            //foreach (string t in settings.PossibleEventTypes)
            //{
            //CreateEventFramework(BettingInfoAvailable, EventDictionary, RunnerDictionary, OddsDictionary, DateDictionary, eventType);
            //}

            //Sort by fight date and then name of fight
            BettingInfoAvailable = BettingInfoAvailable.OrderBy(x => x.Name).OrderBy(x => x.Date).ToList();
        }

        private void CreateEventFramework(List<FightEvent> evList, List<MarketplaceEvent> marketList, string eventType)
        {
            if (eventType == "Match Odds" || eventType == "Fight Result")
            {
                foreach (MarketplaceEvent ev in marketList)
                {
                    //If not in eventList, add
                    if (!evList.Select(x => x.Name).ToList().Contains(ev.Name))
                    {
                        FightEvent e = new FightEvent(ev.Name)
                        {
                            Date = ev.Date
                        };
                        foreach (MarketplaceRunner runn in ev.Runners)
                        {
                            e.Fighters.Add(new Fighter(runn.Name, runn.SelectionID, runn.Odds != null ? runn.Odds : "0"));
                            e.FightResult.AddRunner(new Runner(runn.Name, runn.SelectionID, runn.Odds != null ? runn.Odds : "0"));
                        }

                        //e.Winner = ev.Winner;
                        evList.Add(e);
                    }
                }
            }
        }

        private void CreateEventFramework(List<FightEvent> evList, IDictionary<string, string> eventDictionary, IDictionary<string, string> runnerDictionary, IDictionary<string, string> oddsDictionary, IDictionary<string, string> dateDictionary, string eventType)
        {
            if (eventType == "Match Odds")
            {
                foreach (KeyValuePair<string, string> p in runnerDictionary)
                {
                    string selId = p.Key;
                    if (eventDictionary.ContainsKey(p.Key))
                    {
                        //If not in eventList, add
                        if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
                        {
                            FightEvent e = new FightEvent(eventDictionary[selId])
                            {
                                Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
                            };
                            evList.Add(e);
                        }

                        //Add fighters and add Fight Event info
                        FightEvent k = evList.Where(x => x.Name == eventDictionary[selId]).FirstOrDefault();
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
                            FightEvent e = new FightEvent(eventDictionary[selId])
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
                            FightEvent e = new FightEvent(eventDictionary[selId])
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
                            FightEvent e = new FightEvent(eventDictionary[selId])
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
            GetBettingInfo(settings.EventType);
            UpdateList();

            FightEvent f = BettingInfoAvailable
            .Where(x => x.Fighters.First().NameNoSpaces == EventSelector.Text && x.Fighters.First().Odds != "" && x.Name == SelectionSelector.SelectedItem.ToString())
            .FirstOrDefault();

            if (f != null)
            {
                oxyPlotView.Model = CreateGraphView(f).CreateOxyPlot();
            }

            //Log all new fights
            foreach (CheckBox chk in ListGrid.Children.OfType<CheckBox>())
            {
                if (chk.Tag != null)
                {
                    List<Runner> list = new List<Runner>();
                    var evnt = BettingInfoListed.Where(x => x.Name == chk.Tag.ToString().Split('|')[1]).ToList();
                    List<ISelection> selection = ChooseSelectionType(evnt);

                    foreach (List<Runner> l in selection.Select(y => y.Runners))
                    {
                        list.AddRange(l);
                    }
                    //TODO: Fix this it doesnt work very well -Josh
                    Runner r = list.Where(x => x.Name == chk.Tag.ToString().Split('|')[0]).FirstOrDefault();
                    bool logged = false;

                    if (!(bool)chk.IsChecked && (bool)LogAllchk.IsChecked)
                    {
                        logged = ChangeLogging(evnt.First(), r, true);
                    }

                    //FightEvent tag = BettingInfoAvailable.Where(x => x.Name == chk.Tag.ToString()).FirstOrDefault();
                    ////Add if not checked but all is checked
                    //if (tag != null && !(bool)chk.IsChecked && (bool)LogAllchk.IsChecked)
                    //{
                    //    List<OddsInfo> oddsInfos = tag.FightResult.ToOddsInfo(tag);
                    //    foreach (OddsInfo oi in oddsInfos)
                    //    {
                    //        OddsLogger.AddLoggingItem(oi);
                    //    }

                    //    //bool logged = ChangeLogging(f, true);
                    //    chk.IsChecked = true;
                    //}
                }
            }
        });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            List<FighterFinalOddsExport> finalOdds = new List<FighterFinalOddsExport>();
            DataTable dataTable = new DataTable("Results Export");

            List<string> fighters;
            List<string> fightEvents;

            Task.Run(() =>
            {
                using (BoxingDatabaseModel db = new BoxingDatabaseModel())
                {
                    fighters = db.oddsInfo
                    .GroupBy(x => x.Name)
                    .Select(t => t.Key)
                    .ToList();

                    foreach (string name in fighters)
                    {
                        fightEvents = db.oddsInfo
                        .Where(x => x.Name == name)
                        .GroupBy(x => x.FightName)
                        .Select(x => x.Key)
                        .ToList();

                        //Fighters may have been in multiple fights
                        foreach (string ev in fightEvents)
                        {
                            //Get Average Odds First
                            DateTime fightDate = db.oddsInfo
                            .Where(x => x.Name == name && x.FightName == ev)
                            .OrderByDescending(x => x.ID)
                            .Select(x => x.FightDate)
                            .First();
                            //Take the odds taken 1 day before the fight
                            List<OddsInfo> record1 = db.oddsInfo
                                .Where(x => x.Name == name && x.FightName == ev)
                                .ToList();
                            IOrderedEnumerable<OddsInfo> record2 = record1
                                .OrderBy(x => x.DateTaken);

                            //Get Median
                            OddsInfo record = record2.FirstOrDefault();

                            //Add to a list
                            if (record != null)
                            {
                                finalOdds.Add(new FighterFinalOddsExport
                                {
                                    FightDate = fightDate,
                                    Name = record.Name,
                                    Odds = record.OddsValue,
                                    FightName = record.FightName
                                });
                            }
                        }
                    }

                    dataTable.Columns.Add("Fight Date", typeof(DateTime));
                    dataTable.Columns.Add("Fight Name", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Odds", typeof(double));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.FightDate, i.FightName, i.Name, i.Odds);
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

        private void ButtonColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxColourItem colour = (ComboBoxColourItem)ButtonColorCombo.SelectedItem;
            myGuiProperties.ButtonBackgroundColour = colour.Value;
        }

        private bool ChangeLogging(FightEvent ev, Runner run, bool log)
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
                    if (oi.Name == run.NameNoSpaces)
                    {
                        success = OddsLogger.AddLoggingItem(oi);
                    }
                }
            }
            else
            {
                foreach (OddsInfo oi in ev.FightResult.ToOddsInfo(ev))
                {
                    if (oi.Name == run.NameNoSpaces)
                    {
                        success = !OddsLogger.RemoveLoggingItem(oi.Name, ev.Name, oi.EventType);
                    }
                }
            }
            return success;
        }

        private void CleanData(object sender, RoutedEventArgs e)
        {
            using (BoxingDatabaseModel db = new BoxingDatabaseModel())
            {
                DateTime dt = new DateTime(1, 1, 1);
                //  List<OddsInfo> l = db.oddsInfo.Where(x => Math.Abs(x.OddsValue) > 1200).ToList();
                List<OddsInfo> p = db.oddsInfo.Where(x => x.DateTaken == dt).ToList();
                //db.oddsInfo
                //    .RemoveRange(db.oddsInfo
                //    .Where(x => Math.Abs(x.OddsValue) > 1200));

                db.oddsInfo
                    .RemoveRange(db.oddsInfo.Where(x => x.DateTaken == dt));
                int k = db.SaveChanges();
                MessageBox.Show(k.ToString() + " Entries Removed");
            }
        }

        private void ClearListGridData()
        {
            ListGrid.Children.Clear();
            while (ListGrid.RowDefinitions.Count > 1)
            {
                ListGrid.RowDefinitions.RemoveAt(0);
            }

            BettingInfoListed.Clear();
        }

        private Graph CreateGraphView(FightEvent f)
        {
            using (BoxingDatabaseModel db = new BoxingDatabaseModel())
            {
                //Get all odds for the event
                var eventInfo = db.oddsInfo
                    .Where(x => x.FightName == f.Name)
                    .Select(t => new { t.Name, t.DateTaken, t.OddsValue });
                //Names of the fighters
                List<string> selections = eventInfo.GroupBy(fers => fers.Name).Select(p => p.Key).ToList();
                if (selections.Count() > 1)
                {
                    Dictionary<DateTime, double> f1 = eventInfo.Where(x => x.Name == selections[0]).GroupBy(x => x.DateTaken).ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    Dictionary<DateTime, double> f2 = eventInfo.Where(x => x.Name == selections[1]).GroupBy(x => x.DateTaken).ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    if (f1.Count == 0 || f2.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return new Graph(f1, f2, selections[0], selections[1], "Date", "Odds");
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

            using (BoxingDatabaseModel db = new BoxingDatabaseModel())
            {
                List<string> list = db.oddsInfo
                    .Where(x => x.FightDate > window.Start && x.FightDate < window.End && x.EventType == settings.EventType)
                    .GroupBy(x => x.FightName)
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
            List<string> EventNames = BettingInfoListed.Select(x => x.Name).ToList();
            try
            {
                double height = 25;
                Brush backgroundBrush = Brushes.Beige;

                for (int i = 0; i < BettingInfoAvailable.Count; i++)
                {
                    ISelection sel = ChooseSelectionType(BettingInfoAvailable[i]);
                    List<string> currentRunners = new List<string>();
                    List<Runner> currentEvent = BettingInfoListed
                              .Where(x => x.Name == BettingInfoAvailable[i].Name)
                              .Select(x => x.FightResult.Runners)
                              .FirstOrDefault();
                    if (currentEvent != null)
                    {
                        currentRunners = currentEvent.Select(f => f.Name)
                              .ToList();
                    }

                    if (!EventNames.Contains(BettingInfoAvailable[i].Name))
                    {
                        foreach (Runner item in sel.Runners)
                        {
                            //As long as runner isnt listed
                            if (!currentRunners.Contains(item.Name))
                            {
                                //Fights.Add(new Fight())
                                //Fight f = new Fight();
                                int numRows = ListGrid.RowDefinitions.Count;
                                RowDefinition rowdef = new RowDefinition
                                {
                                    Height = new GridLength(height)
                                };

                                //rowdef.SetValue(Grid.BackgroundProperty, backgroundBrush);

                                if (numRows % 2 == 1)
                                {
                                    if (backgroundBrush == Brushes.Beige)
                                    {
                                        backgroundBrush = Brushes.White;
                                    }
                                    else
                                    {
                                        backgroundBrush = Brushes.Beige;
                                    }
                                }
                                rowdef.SetValue(Panel.BackgroundProperty, backgroundBrush);

                                ListGrid.RowDefinitions.Insert(numRows, rowdef);
                                CheckBox toggleLoggingChk = new CheckBox();
                                toggleLoggingChk.SetValue(Grid.RowProperty, numRows);
                                toggleLoggingChk.SetValue(Grid.ColumnProperty, 0);
                                toggleLoggingChk.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                                toggleLoggingChk.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                toggleLoggingChk.Click += ToggleLoggingFighter;
                                toggleLoggingChk.Tag = item.Name + '|' + BettingInfoAvailable[i].Name;

                                Label date = new Label
                                {
                                    Content = BettingInfoAvailable.ElementAt(i).Date.ToString("MMMM-dd-HH:mm"),
                                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                                };
                                date.SetValue(Grid.RowProperty, numRows);
                                date.SetValue(Grid.ColumnProperty, 1);

                                Label fightEventName = new Label
                                {
                                    Content = BettingInfoAvailable.ElementAt(i).Name,
                                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                                };
                                //fighterName.VerticalAlignment = VerticalAlignment.Center;
                                fightEventName.SetValue(Grid.RowProperty, numRows);
                                fightEventName.SetValue(Grid.ColumnProperty, 2);

                                Label selection = new Label
                                {
                                    Content = item.Name,
                                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                                };
                                selection.SetValue(Grid.RowProperty, numRows);
                                selection.SetValue(Grid.ColumnProperty, 3);

                                Label Odds = new Label
                                {
                                    Content = item.Odds,
                                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                                };
                                //Odds.VerticalAlignment = VerticalAlignment.Center;
                                Odds.SetValue(Grid.RowProperty, numRows);
                                Odds.SetValue(Grid.ColumnProperty, 4);

                                Label Percent = new Label
                                {
                                    Content = item.PercentChance,
                                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                                };
                                //Odds.VerticalAlignment = VerticalAlignment.Center;
                                Percent.SetValue(Grid.RowProperty, numRows);
                                Percent.SetValue(Grid.ColumnProperty, 5);

                                Label Mult = new Label
                                {
                                    Content = item.Multiplier.ToString(),
                                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                                };
                                Mult.SetValue(Grid.RowProperty, numRows);
                                Mult.SetValue(Grid.ColumnProperty, 6);

                                Label Change = new Label
                                {
                                    Content = (item.Odds.ToDouble() - item.LastOdds.ToDouble()).ToString(),
                                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                                };
                                Change.SetValue(Grid.RowProperty, numRows);
                                Change.SetValue(Grid.ColumnProperty, 7);

                                Grid color = new Grid();
                                color.SetValue(Grid.RowProperty, numRows);
                                color.SetValue(Grid.ColumnSpanProperty, 100);
                                color.SetValue(Panel.BackgroundProperty, backgroundBrush);

                                ListGrid.Children.Add(color);
                                ListGrid.Children.Add(date);
                                ListGrid.Children.Add(selection);
                                ListGrid.Children.Add(fightEventName);
                                ListGrid.Children.Add(Odds);
                                ListGrid.Children.Add(toggleLoggingChk);
                                ListGrid.Children.Add(Percent);
                                ListGrid.Children.Add(Mult);
                                ListGrid.Children.Add(Change);
                            }
                        }
                        BettingInfoListed.Add(BettingInfoAvailable[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayError(marketMessenger, "Could not Fill Rows: " + ex.Message);
            }
        }

        private ISelection ChooseSelectionType(FightEvent ev)
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

        private List<ISelection> ChooseSelectionType(List<FightEvent> ev)
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

        private void DisplayPastFights()
        {
            LoadingScreen(true);
            Task.Run(() =>
            {
                List<string> pastFights;
                DateTime fightDate;
                Dictionary<string, int> PastFightsPointsDictionary = new Dictionary<string, int>();

                using (BoxingDatabaseModel db = new BoxingDatabaseModel())
                {
                    List<OddsInfo> oddsInfos = db.oddsInfo.ToList();

                    pastFights = oddsInfos
                        .OrderBy(x => x.FightDate)
                        .Select(x => x.FightName)
                        .Distinct()
                        .ToList();

                    foreach (var fight in pastFights)
                    {
                        var count = oddsInfos.Where(x => x.FightName == fight).Count();
                        PastFightsPointsDictionary.Add(fight, count);
                    }
                }
                InvokeUI(() =>
                {
                    try
                    {
                        double height = 25;
                        Brush backgroundBrush = Brushes.Beige;

                        for (int i = 0; i < pastFights.Count; i++)

                        {
                            using (BoxingDatabaseModel db = new BoxingDatabaseModel())
                            {
                                string s = pastFights[i];
                                List<OddsInfo> DataList = db.oddsInfo
                                    .Where(x => x.FightName == s)
                                    .ToList();
                                fightDate = DataList
                                    .OrderByDescending(x => x.DateTaken)
                                    .Select(x => x.FightDate)
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
                                Content = fightDate.Date.ToLongDateString(),
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                                VerticalAlignment = System.Windows.VerticalAlignment.Center
                            };
                            date.SetValue(Grid.RowProperty, numRows - 1);
                            date.SetValue(Grid.ColumnProperty, 1);

                            Label fightName = new Label
                            {
                                Content = pastFights[i],
                                HorizontalAlignment = HorizontalAlignment.Left
                            };
                            fightName.SetValue(Grid.RowProperty, numRows - 1);
                            fightName.SetValue(Grid.ColumnProperty, 2);

                            //List number of points for the fight
                            Label numPoints = new Label();
                            if (PastFightsPointsDictionary.ContainsKey(pastFights[i]))
                            {
                                numPoints = new Label
                                {
                                    Content = PastFightsPointsDictionary[pastFights[i]].ToString(),
                                    HorizontalAlignment = HorizontalAlignment.Left
                                };
                                numPoints.SetValue(Grid.RowProperty, numRows - 1);
                                numPoints.SetValue(Grid.ColumnProperty, 3);
                            }

                            Grid color = new Grid();
                            color.SetValue(Grid.RowProperty, numRows - 1);
                            color.SetValue(Grid.ColumnSpanProperty, 100);
                            color.SetValue(Panel.BackgroundProperty, backgroundBrush);

                            pastFightsGrid.Children.Add(color);
                            pastFightsGrid.Children.Add(removeFight);

                            pastFightsGrid.Children.Add(date);
                            pastFightsGrid.Children.Add(fightName);
                            pastFightsGrid.Children.Add(numPoints);
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
            using (BoxingDatabaseModel db = new BoxingDatabaseModel())
            {
                Dictionary<DateTime, double> dict = db.oddsInfo
                    .Where(x => x.Name == EventSelector.SelectedItem.ToString())
                    .Select(t => new { t.DateTaken, t.OddsValue })
                    .GroupBy(x => x.DateTaken)
                    .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                g = new Graph(dict, EventSelector.SelectedItem.ToString(), "Date", "Odds");
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel|*.csv"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                CreateCSVFile(g.ToDataTable(), saveFileDialog.FileName, false);
            }
        }

        private void ExportFinalOdds(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            List<FighterFinalOddsExport> finalOdds = new List<FighterFinalOddsExport>();
            DataTable dataTable = new DataTable("Results Export");

            Task.Run(() =>
            {
                using (BoxingDatabaseModel db = new BoxingDatabaseModel())
                {
                    List<string> fighters = db.oddsInfo.Where(x => x.FightDate < DateTime.Now).GroupBy(x => x.Name).Select(t => t.Key).ToList();

                    foreach (string name in fighters)
                    {
                        //Get Average Odds First
                        int count = db.oddsInfo.Where(x => x.Name == name).Count();

                        //Group by day and
                        Dictionary<int, IOrderedEnumerable<OddsInfo>> record1 = db.oddsInfo.Where(x => x.Name == name)
                            .GroupBy(x => x.DateTaken.Day).ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.OddsValue));
                        List<OddsInfo> dubList = new List<OddsInfo>();
                        Parallel.ForEach(record1, day =>
                         {
                             //Take median of each day
                             int c = day.Value.Count();
                             if (c > 20)
                             {
                                 List<long> k = day.Value.Select(x => x.OddsValue).ToList();
                             }
                             dubList.Add(day.Value.Skip(c / 2 - 1).First());
                         });
                        //Take median of each median day value
                        OddsInfo record = dubList.Skip(dubList.Count / 2).First();

                        finalOdds.Add(new FighterFinalOddsExport
                        {
                            FightDate = record.FightDate,
                            Name = record.Name,
                            Odds = record.OddsValue,
                            FightName = record.FightName
                        });
                    }

                    dataTable.Columns.Add("Fight Date", typeof(DateTime));
                    dataTable.Columns.Add("Fight Name", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Final Odds", typeof(double));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.FightDate, i.FightName, i.Name, i.Odds);
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

        private void ExportLastDayOdds(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);
            List<FighterFinalOddsExport> finalOdds = new List<FighterFinalOddsExport>();
            DataTable dataTable = new DataTable("Results Export");

            List<string> fighters;
            List<string> fightEvents;

            Task.Run(() =>
            {
                using (BoxingDatabaseModel db = new BoxingDatabaseModel())
                {
                    fighters = db.oddsInfo
                    //.Where(x => x.FightDate.CompareTo(DateTime.Now) < 0)
                    .GroupBy(x => x.Name)
                    .Select(t => t.Key)
                    .ToList();

                    foreach (string name in fighters)
                    {
                        fightEvents = db.oddsInfo
                        .Where(x => x.Name == name)
                        .GroupBy(x => x.FightName)
                        .Select(x => x.Key)
                        .ToList();

                        //Fighters may have been in multiple fights
                        foreach (string ev in fightEvents)
                        {
                            //Get Average Odds First
                            DateTime fightDate = db.oddsInfo
                            .Where(x => x.Name == name && x.FightName == ev)
                            .OrderByDescending(x => x.ID)
                            .Select(x => x.FightDate)
                            .First();
                            //Take the odds taken 1 day before the fight
                            List<OddsInfo> record1 = db.oddsInfo
                                .Where(x => x.Name == name && x.FightName == ev)
                                .ToList();
                            IOrderedEnumerable<OddsInfo> record2 = record1
                                .Where(x => x.DateTaken.AddDays(1).Date == fightDate.Date)
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
                                record2 = record1.Where(x => x.DateTaken.Date == fightDate.Date || x.DateTaken.AddDays(2).Date == fightDate.Date).OrderByDescending(x => x.OddsValue);
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
                                    FightDate = fightDate,
                                    Name = record.Name,
                                    Odds = record.OddsValue,
                                    FightName = record.FightName
                                });
                            }
                        }
                    }

                    dataTable.Columns.Add("Fight Date", typeof(DateTime));
                    dataTable.Columns.Add("Fight Name", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Odds", typeof(double));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.FightDate, i.FightName, i.Name, i.Odds);
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
                using (BoxingDatabaseModel db = new BoxingDatabaseModel())
                {
                    DateTime oneMonthAgo = DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0));
                    fighters = db.oddsInfo
                    .Where(x => x.FightDate.CompareTo(oneMonthAgo) > 0)
                    .GroupBy(x => x.Name)
                    .Select(t => t.Key)
                    .ToList();

                    foreach (string name in fighters)
                    {
                        fightEvents = db.oddsInfo
                        .Where(x => x.Name == name && x.FightDate.CompareTo(oneMonthAgo) > 0)
                        .GroupBy(x => x.FightName)
                        .Select(x => x.Key)
                        .ToList();

                        //Fighters may have been in multiple fights
                        foreach (string ev in fightEvents)
                        {
                            //Get Average Odds First
                            DateTime fightDate = db.oddsInfo
                            .Where(x => x.Name == name && x.FightName == ev)
                            .OrderByDescending(x => x.ID)
                            .Select(x => x.FightDate)
                            .First();
                            //Take the odds taken 1 day before the fight
                            List<OddsInfo> record1 = db.oddsInfo
                                .Where(x => x.Name == name && x.FightName == ev)
                                .ToList();
                            IOrderedEnumerable<OddsInfo> record2 = record1
                                .Where(x => x.DateTaken.AddDays(1).Date == fightDate.Date)
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
                                record2 = record1.Where(x => x.DateTaken.Date == fightDate.Date || x.DateTaken.AddDays(2).Date == fightDate.Date).OrderByDescending(x => x.OddsValue);
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
                                    FightDate = fightDate,
                                    Name = record.Name,
                                    Odds = record.OddsValue,
                                    FightName = record.FightName
                                });
                            }
                        }
                    }

                    dataTable.Columns.Add("Fight Date", typeof(DateTime));
                    dataTable.Columns.Add("Fight Name", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Odds", typeof(double));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.FightDate, i.FightName, i.Name, i.Odds);
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
            using (BoxingDatabaseModel db = new BoxingDatabaseModel())
            {
                if (EventSelector.SelectedItem != null)
                {
                    List<string> fights = db.oddsInfo
                        .Where(x => x.FightName == EventSelector.SelectedItem.ToString())
                        .OrderBy(o => o.DateTaken)
                        .GroupBy(x => x.Name)
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

        private void InsertOneFightToList(FightEvent fn, int i)
        {
            //rowdef.SetValue(Grid.BackgroundProperty, backgroundBrush);

            //////////////////

            double height = 25;
            Brush backgroundBrush = Brushes.Beige;

            RowDefinition rowdef;

            //Rn through each fighter in event
            ISelection s = ChooseSelectionType(fn);
            foreach (Runner run in s.Runners)
            {
                int numRows = ListGrid.RowDefinitions.Count;
                if (numRows % 2 == 0)
                {
                    if (backgroundBrush == Brushes.Beige)
                    {
                        backgroundBrush = Brushes.White;
                    }
                    else
                    {
                        backgroundBrush = Brushes.Beige;
                    }
                }
                rowdef = new RowDefinition
                {
                    Height = new GridLength(height)
                };
                rowdef.SetValue(Panel.BackgroundProperty, backgroundBrush);

                ListGrid.RowDefinitions.Insert(numRows, rowdef);
                CheckBox toggleLoggingChk = new CheckBox();
                toggleLoggingChk.SetValue(Grid.RowProperty, numRows);
                toggleLoggingChk.SetValue(Grid.ColumnProperty, 0);
                toggleLoggingChk.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                toggleLoggingChk.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                toggleLoggingChk.Click += ToggleLoggingFighter;
                toggleLoggingChk.Tag = run.Name;

                Label date = new Label
                {
                    Content = marketMessenger.GetDate(fn.Name).ToString("MMMM-dd-HH:mm"),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                date.SetValue(Grid.RowProperty, numRows);
                date.SetValue(Grid.ColumnProperty, 1);

                Label fightEventName = new Label
                {
                    Content = fn.Name,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                };
                //fighterName.VerticalAlignment = VerticalAlignment.Center;
                fightEventName.SetValue(Grid.RowProperty, numRows);
                fightEventName.SetValue(Grid.ColumnProperty, 2);

                Label selection = new Label
                {
                    Content = run.Name,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                };
                selection.SetValue(Grid.RowProperty, numRows);
                selection.SetValue(Grid.ColumnProperty, 3);

                Label Odds = new Label
                {
                    Content = run.Odds,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                //Odds.VerticalAlignment = VerticalAlignment.Center;
                Odds.SetValue(Grid.RowProperty, numRows);
                Odds.SetValue(Grid.ColumnProperty, 4);

                Label Percent = new Label
                {
                    Content = run.PercentChance,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                //Odds.VerticalAlignment = VerticalAlignment.Center;
                Percent.SetValue(Grid.RowProperty, numRows);
                Percent.SetValue(Grid.ColumnProperty, 5);

                Label Mult = new Label
                {
                    Content = run.Multiplier.ToString(),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                Mult.SetValue(Grid.RowProperty, numRows);
                Mult.SetValue(Grid.ColumnProperty, 6);

                Label Change = new Label
                {
                    Content = (run.LastOdds.ToDouble() - run.Odds.ToDouble()).ToString(),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                Change.SetValue(Grid.RowProperty, numRows);
                Change.SetValue(Grid.ColumnProperty, 7);

                Grid color = new Grid();
                color.SetValue(Grid.RowProperty, numRows);
                color.SetValue(Grid.ColumnSpanProperty, 100);
                color.SetValue(Panel.BackgroundProperty, backgroundBrush);

                ListGrid.Children.Add(color);
                ListGrid.Children.Add(date);
                ListGrid.Children.Add(selection);
                ListGrid.Children.Add(fightEventName);
                ListGrid.Children.Add(Odds);
                ListGrid.Children.Add(toggleLoggingChk);
                ListGrid.Children.Add(Percent);
                ListGrid.Children.Add(Mult);
                ListGrid.Children.Add(Change);

                //Log Selection
                ChangeLogging(fn, run, true);
                toggleLoggingChk.IsChecked = true;

                i++;
            }
            BettingInfoListed.Add(fn);
        }

        private void listBetsBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadingScreen(true);

            if (BettingInfoListed == null)
            {
                BettingInfoListed = new List<FightEvent>();
            }

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

            LoadingScreen(false);
        }

        private void LoadingScreen(bool v) => LoadingViewGrid.Visibility = v ? Visibility.Visible : Visibility.Hidden;

        private void LogAllchk_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chkSender = sender as CheckBox;
            List<Runner> list = new List<Runner>();
            foreach (List<Runner> l in BettingInfoAvailable.Select(y => y.FightResult.Runners))
            {
                list.AddRange(l);
            }

            foreach (CheckBox chk in ListGrid.Children.OfType<CheckBox>())
            {
                if (chk.Tag != null)
                {
                    //var f = BettingInfoAvailable.Where(x => x.Name == chk.Tag.ToString()).FirstOrDefault();
                    //if (f != null)
                    //{
                    //    bool logged = ChangeLogging(f, f.FightResult.Runners.First(), (bool)chkSender.IsChecked);
                    //    if (logged != (bool)chkSender.IsChecked) DisplayError(this, "Could not log, was not able to connect");
                    //    chk.IsChecked = logged;
                    //}
                    list.Clear();
                    var evnt = BettingInfoListed.Where(x => x.Name == chk.Tag.ToString().Split('|')[1]).ToList();
                    List<ISelection> selection = ChooseSelectionType(evnt);

                    foreach (List<Runner> l in selection.Select(y => y.Runners))
                    {
                        list.AddRange(l);
                    }
                    Runner f = list.Where(x => x.Name == chk.Tag.ToString().Split('|')[0]).FirstOrDefault();
                    bool logged = false;
                    logged = ChangeLogging(evnt.First(), f, (bool)chkSender.IsChecked);

                    // Runner f = list.Where(x => x.Name == chk.Tag.ToString().Split('|')[0]).FirstOrDefault();
                    //if (f != null)
                    //{
                    //    bool logged = false;
                    //    switch (settings.EventType)
                    //    {
                    //        case "Match Odds":
                    //            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                    //            break;

                    //        case "Go The Distance?":
                    //            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.GoTheDistance.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                    //            break;

                    //        case "Round Betting":
                    //            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.RoundBetting.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                    //            break;

                    //        case "Method of Victory":
                    //            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.MethodOfVictory.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                    //            break;

                    //        default:
                    //            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                    //            break;
                    //    }

                    if (logged != chkSender.IsChecked)
                    {
                        DisplayError(this, "Could not log, was not able to connect");
                    }

                    chk.IsChecked = logged;
                }
            }
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
                        //Set up rememberMe
                        CheckBox remember = LoggingGrid.Children.OfType<CheckBox>().FirstOrDefault();
                        using (BoxingDatabaseModel db = new BoxingDatabaseModel())
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
                marketMessenger.SetMarketFilter("Boxing");
                InvokeUI(() => LoadingScreen(false));
            });
        }

        private void LogNowBtn_Click(object sender, RoutedEventArgs e) => OddsLogger.LogOddsNow(settings.EventType);

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
                GetBettingInfo(settings.EventType);
                InvokeUI(() =>
                {
                    UpdateList();
                    FightEvent f = BettingInfoAvailable.Where(x => x.Name == EventSelector.Text && x.Name == SelectionSelector.SelectedItem.ToString()).FirstOrDefault();
                    if (f != null)
                    {
                        oxyPlotView.Model = CreateGraphView(f).CreateOxyPlot();
                    }
                    LoadingScreen(false);
                });
            });

            if ((bool)AutoRefreshChk.IsChecked)
            {
                //reset timer
                autoRefreshTimer.Stop();
                autoRefreshTimer.Start();
            }
        }

        private void RefreshFightNameList()
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
                using (BoxingDatabaseModel db = new BoxingDatabaseModel())
                {
                    string fightName = fight.Tag.ToString();
                    //Remove datapoints
                    IQueryable<OddsInfo> dataPoints = db.oddsInfo
                        .Where(x => x.FightName == fightName);
                    db.oddsInfo.RemoveRange(dataPoints);
                    pointsRemoved = db.SaveChanges();
                }
                MessageBox.Show(pointsRemoved.ToString() + " Points Removed");
                RefreshFightNameList();
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e) => ShowWindow(SettingsGrid);

        private void SetUpEvents()
        {
            OddsLogger.MarketplaceErrorOccured += ReLogin;
            autoRefreshTimer.Elapsed += AutoRefreshTimer_Elapsed;
            twelveHourRefreshTimer.Elapsed += TwentyFourHourRefreshTimer_Elapsed;
        }

        private void TwentyFourHourRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
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
                marketMessenger.SetMarketFilter("Boxing");

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
                RefreshFightNameList();
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
            foreach (CheckBox chk in ListGrid.Children.OfType<CheckBox>())
            {
                if (sender == chk)
                {
                    List<Runner> list = new List<Runner>();
                    var evnt = BettingInfoListed.Where(x => x.Name == chk.Tag.ToString().Split('|')[1]).ToList();
                    List<ISelection> selection = ChooseSelectionType(evnt);

                    foreach (List<Runner> l in selection.Select(y => y.Runners))
                    {
                        list.AddRange(l);
                    }
                    //TODO: Fix this it doesnt work very well -Josh
                    Runner f = list.Where(x => x.Name == chk.Tag.ToString().Split('|')[0]).FirstOrDefault();
                    bool logged = false;
                    logged = ChangeLogging(evnt.First(), f, (bool)chk.IsChecked);
                    //switch (settings.EventType)
                    //{
                    //    case "Match Odds":
                    //        logged = ChangeLogging(evnt.First(), f, (bool)chk.IsChecked);
                    //        break;

                    //    case "Go The Distance?":
                    //        logged = ChangeLogging(BettingInfoAvailable.Where(x => x.GoTheDistance.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                    //        break;

                    //    case "Round Betting":
                    //        logged = ChangeLogging(BettingInfoAvailable.Where(x => x.RoundBetting.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                    //        break;

                    //    case "Method of Victory":
                    //        logged = ChangeLogging(BettingInfoAvailable.Where(x => x.MethodOfVictory.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                    //        break;

                    //    default:
                    //        logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                    //        break;
                    //}

                    if (logged != chk.IsChecked)
                    {
                        DisplayError(this, "Could not log, was not able to connect");
                    }

                    chk.IsChecked = logged;
                    break;
                }
            }
        }

        private void UpdateGraph()
        {
            Graph g;

            using (BoxingDatabaseModel db = new BoxingDatabaseModel())
            {
                if (SelectionSelector.SelectedItem.ToString() != "All")
                {
                    Dictionary<DateTime, double> dict1 = db.oddsInfo
                        .Where(x => x.FightName == EventSelector.SelectedItem.ToString() && x.Name == SelectionSelector.SelectedItem.ToString())
                        .Select(t => new { t.DateTaken, t.OddsValue })
                        .GroupBy(x => x.DateTaken)
                        .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    g = new Graph(dict1, EventSelector.SelectedItem.ToString(), "Date", "Odds");
                    oxyPlotView.Model = g.CreateOxyPlot();
                }
                else if (SelectionSelector.Items.Count > 2)
                {
                    string selections = SelectionSelector.Items[0].ToString();
                    string selection1 = SelectionSelector.Items[1].ToString();
                    Dictionary<DateTime, double> dict1 = db.oddsInfo
                        .Where(x => x.FightName == EventSelector.SelectedItem.ToString() && x.Name == selection1)
                        .Select(t => new { t.DateTaken, t.OddsValue })
                        .GroupBy(x => x.DateTaken)
                        .ToDictionary(t => t.Key, t => (double)t.First().OddsValue);
                    string selection2 = SelectionSelector.Items[2].ToString();
                    Dictionary<DateTime, double> dict2 = db.oddsInfo
                        .Where(x => x.FightName == EventSelector.SelectedItem.ToString() && x.Name == selection2)
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
            try
            {
                //Clear Data from grid first
                //ClearListGridData();

                //DisplayBettingInfo();

                for (int i = 0; i < BettingInfoAvailable.Count; i++)
                {
                    if (BettingInfoListed.Select(x => x.Name).ToList().Contains(BettingInfoAvailable[i].Name))
                    {
                        ISelection select = ChooseSelectionType(BettingInfoAvailable.ElementAt(i));
                        foreach (Runner sel in select.Runners)
                        {
                            bool check = ListGrid.Children.OfType<Label>()
                                .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 2 && x.Content.ToString() == BettingInfoAvailable[i].Name)
                                .Count() > 0;
                            //If row no longer there, continue
                            if (!check)
                            {
                                continue;
                            }
                            List<int> rows = ListGrid.Children.OfType<Label>()
                                .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && x.Content.ToString() == sel.Name)
                                .Select(x => (int)x.GetValue(Grid.RowProperty)).ToList();

                            int targetRow = 0;
                            object p = ListGrid.Children.OfType<Label>()
                            .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 2 && x.Content.ToString() == BettingInfoAvailable[i].Name && rows.Contains((int)x.GetValue(Grid.RowProperty)))
                            .Select(x => x.GetValue(Grid.RowProperty)).FirstOrDefault();
                            if (p != null)
                            {
                                targetRow = (int)p;

                                ListGrid.Children.OfType<Label>()
                                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 1 && (int)x.GetValue(Grid.RowProperty) == targetRow)
                                    .First().Content = BettingInfoAvailable.ElementAt(i).Date.ToString("MMMM-dd-HH:mm");

                                ListGrid.Children.OfType<Label>()
                                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && (int)x.GetValue(Grid.RowProperty) == targetRow)
                                    .First().Content = sel.Name;

                                //Capture current odds for comparison

                                Label changeVal = ListGrid.Children.OfType<Label>()
                                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 7 && (int)x.GetValue(Grid.RowProperty) == targetRow)
                                    .First();

                                double currentOdds = ListGrid.Children.OfType<Label>()
                                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 4 && (int)x.GetValue(Grid.RowProperty) == targetRow)
                                    .First().Content.ToDouble();

                                changeVal.Content = (sel.Odds.ToDouble() - currentOdds).ToString();
                                changeVal.Foreground = changeVal.Content.ToDouble() >= 0 ? Brushes.ForestGreen : Brushes.DarkRed;

                                ListGrid.Children.OfType<Label>()
                                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 4 && (int)x.GetValue(Grid.RowProperty) == targetRow)
                                    .First().Content = sel.Odds;

                                ListGrid.Children.OfType<Label>()
                                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 5 && (int)x.GetValue(Grid.RowProperty) == targetRow)
                                    .First().Content = sel.PercentChance;

                                ListGrid.Children.OfType<Label>()
                                    .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 6 && (int)x.GetValue(Grid.RowProperty) == targetRow)
                                    .First().Content = sel.Multiplier.ToString();
                            }
                        }
                    }
                    else
                    {
                        InsertOneFightToList(BettingInfoAvailable[i], i);
                    }
                }

                //Lets remove fights that have happened

                RemoveFightsNotAvailable();

                //var pastFights = BettingInfoListed.Where(x=>BettingInfoAvailable.Select(x=>x.Name).Contains(x))
            }
            catch (Exception ex)
            {
                if (ex.Message != "Collection was modified; enumeration operation may not execute.")
                {
                    DisplayError(marketMessenger, "Could not Fill Rows: " + ex.Message);
                }
            }
        }

        private void RemoveFightsNotAvailable()
        {
            IEnumerable<string> availableNames = BettingInfoAvailable.Select(y => y.Name);
            IEnumerable<string> missingEventNames = BettingInfoListed.Select(x => x.Name).Where(x => !availableNames.Contains(x));
            if (missingEventNames.Count() > 0)
            {
                List<FightEvent> missingEvents = BettingInfoListed.Where(x => missingEventNames.Contains(x.Name)).ToList();

                foreach (FightEvent ev in missingEvents)
                {
                    ISelection select = ChooseSelectionType(ev);
                    foreach (Runner sel in select.Runners)
                    {
                        //int targetRow = (int)ListGrid.Children.OfType<Label>()
                        //           .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && x.Content.ToString() == sel.Name)
                        //           .First().GetValue(Grid.RowProperty);

                        List<int> rows = ListGrid.Children.OfType<Label>()
                                .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && x.Content.ToString() == sel.Name)
                                .Select(x => (int)x.GetValue(Grid.RowProperty)).ToList();

                        int targetRow = ListGrid.Children.OfType<Label>()
                        .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 2 && x.Content.ToString() == ev.Name && rows.Contains((int)x.GetValue(Grid.RowProperty)))
                        .Select(x => (int)x.GetValue(Grid.RowProperty)).FirstOrDefault();
                        if (targetRow > 0)
                        {
                            ChangeLogging(ev, sel, false);
                            RemoveRowFromListGrid(targetRow);
                        }
                    }
                }
                BettingInfoListed.RemoveAll(x => missingEventNames.Contains(x.Name));
            }
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
                        LogAllchk.IsChecked = false;
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
            List<UIElement> childList = new List<UIElement>();
            ListGrid.RowDefinitions.RemoveAt(rowVal);
            for (int i = 0; i < ListGrid.Children.Count; i++)
            {
                UIElement el = ListGrid.Children[i];
                int row = Grid.GetRow(el);
                if (row == rowVal)
                {
                    childList.Add(el);
                }
                else if (row > rowVal)
                {
                    el.SetValue(Grid.RowProperty, row - 1);
                }
            }
            foreach (UIElement elemen in childList)
            {
                ListGrid.Children.Remove(elemen);
            }
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //Just fuck up the session token
            char[] k = loginClient.SessionToken.ToCharArray();
            k[3] = 'g';
            loginClient.SessionToken = k.ToString();
            marketMessenger.Initialise(loginClient.SessionToken);
        }
    }
}