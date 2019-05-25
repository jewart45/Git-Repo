using LoginClientLib;
using Microsoft.Win32;
using MMABettingModule.Classes;
using MMADatabase;
using MMADatabase.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MMABettingModule
{
    public partial class MainWindow : Window
    {
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

            using (MMADatabaseModel db = new MMADatabaseModel())
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
            using (MMADatabaseModel db = new MMADatabaseModel())
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
                using (MMADatabaseModel db = new MMADatabaseModel())
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
                using (MMADatabaseModel db = new MMADatabaseModel())
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
                                .Where(x => x.DateTaken.AddDays(1).Date == fightDate.Date || x.DateTaken.Date == fightDate.Date)
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
                                    FightName = record.FightName,
                                    Winner = record.Winner
                                });
                            }
                        }
                    }

                    dataTable.Columns.Add("Fight Date", typeof(DateTime));
                    dataTable.Columns.Add("Fight Name", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Odds", typeof(double));
                    dataTable.Columns.Add("Winner", typeof(bool));

                    foreach (FighterFinalOddsExport i in finalOdds)
                    {
                        dataTable.Rows.Add(i.FightDate, i.FightName, i.Name, i.Odds, i.Winner);
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
                using (MMADatabaseModel db = new MMADatabaseModel())
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
            using (MMADatabaseModel db = new MMADatabaseModel())
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

        private void listBetsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (BettingInfoListed.Count <= 0)
            {
                LoadingScreen(true);

                if (BettingInfoListed == null)
                {
                    BettingInfoListed = new List<Event>();
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
            else
            {
                InvokeUI(() =>
                {
                    DisplayBettingInfo();
                    ShowWindow(ListBetsGrid);
                });
            }
        }

        private void LogAllchk_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chkSender = sender as CheckBox;

            if (!(bool)chkSender.IsChecked)
            {
                OddsLogger.RemoveAllLoggingItems();
            }
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
                    string selectionName = chk.Tag.ToString().Split('|')[0];
                    string eventName = chk.Tag.ToString().Split('|')[1];
                    Runner f = BettingInfoAvailable.Where(x => x.Name == eventName).First().FightResult.Runners.Where(x => x.Name == selectionName).FirstOrDefault();

                    if (f != null)
                    {
                        bool logged = false;
                        switch (settings.EventType)
                        {
                            case "Fight Result":
                                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                                break;

                            case "Go The Distance?":
                                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.GoTheDistance.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                                break;

                            case "Round Betting":
                                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.RoundBetting.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                                break;

                            case "Method of Victory":
                                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.MethodOfVictory.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                                break;

                            default:
                                logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chkSender.IsChecked);
                                break;
                        }

                        if (logged != chkSender.IsChecked)
                        {
                            DisplayError(this, "Could not log, was not able to connect");
                        }

                        chk.IsChecked = logged;
                    }
                    else
                    {
                        //If not available just change it anyway
                        chk.IsChecked = (bool)chkSender.IsChecked;
                    }
                }
            }
        }

        private void ResultTypeSelectorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultTypeSelectorCombo.SelectedIndex == -1)
            {
                return;
            }

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

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //Just fuck up the session token
            char[] k = loginClient.SessionToken.ToCharArray();
            k[3] = 'g';
            loginClient.SessionToken = k.ToString();
            marketMessenger.Initialise(loginClient.SessionToken);
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
                        using (MMADatabaseModel db = new MMADatabaseModel())
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
                marketMessenger.SetMarketFilter(eventTypeString);
                myGuiProperties.ResultTypes = marketMessenger.GetEventTypes(eventTypeString);

                InvokeUI(() =>
                {
                    ResultTypeSelectorCombo.SelectedIndex = 0;
                    LoadingScreen(false);
                });
            });
        }

        private void LogNowBtn_Click(object sender, RoutedEventArgs e) => OddsLogger.LogOddsNow(settings.EventType, myGuiProperties.Virtualise);

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
                    Event f = BettingInfoAvailable.Where(x => x.Name == EventSelector.Text && x.Name == SelectionSelector.SelectedItem.ToString()).FirstOrDefault();
                    if (f != null)
                    {
                        oxyPlotView.Model = CreateGraphView(f).CreateOxyPlot();
                    }
                    myGuiProperties.ResultTypes = marketMessenger.GetEventTypes("Mixed Martial Arts");
                    if (ResultTypeSelectorCombo.Items.Contains("Fight Result"))
                    {
                        foreach (var i in ResultTypeSelectorCombo.Items)
                        {
                            if ((string)i == "Fight Result")
                            {
                                ResultTypeSelectorCombo.SelectedItem = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        ResultTypeSelectorCombo.SelectedIndex = 0;
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

        private void TwelveHourRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReLogin();
            if (loginClient.SessionToken != null)
            {
                twelveHourRefreshTimer.Stop();
                twelveHourRefreshTimer.Start();
            }
            else
            {
                DisplayError(this, "Error using 12 hour refresh, please try again");
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e) => ShowWindow(SettingsGrid);

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

        private void AutoRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
       => InvokeUI(() =>
       {
           GetBettingInfo(settings.EventType);
           UpdateList();

           Event f = BettingInfoAvailable
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
                   Event tag = BettingInfoAvailable.Where(x => x.Name == chk.Tag.ToString()).FirstOrDefault();
                   //Add if not checked but all is checked
                   if (tag != null && !(bool)chk.IsChecked && (bool)LogAllchk.IsChecked)
                   {
                       List<OddsInfo> oddsInfos = tag.FightResult.ToOddsInfo(tag);
                       foreach (OddsInfo oi in oddsInfos)
                       {
                           OddsLogger.AddLoggingItem(oi);
                       }

                       //bool logged = ChangeLogging(f, true);
                       chk.IsChecked = true;
                   }
               }
           }
       });

        private void startLoggingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!OddsLogger.Logging)
            {
                OddsLogger.StartLoggingAsync(settings.EventType, myGuiProperties.Virtualise);
                StartLoggingBtn.IsChecked = true;
            }
            else
            {
                OddsLogger.StopLoggingAsync();
                StartLoggingBtn.IsChecked = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) => GetResultsFromXML();

        private void ButtonColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxColourItem colour = (ComboBoxColourItem)ButtonColorCombo.SelectedItem;
            myGuiProperties.ButtonBackgroundColour = colour.Value;
        }
    }
}