using LoginClientLib;
using Marketplace;
using MMABettingModule.Classes;
using MMADatabase;
using MMADatabase.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MMABettingModule
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
        private const string eventTypeString = "Mixed Martial Arts";

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

            InitializeComponent();
            ResultTypeSelectorCombo.SelectedIndex = 0;
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
            using (MMADatabaseModel db = new MMADatabaseModel())
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

        private void InvokeUI(Action a) => Application.Current.Dispatcher.Invoke(a);

        private bool ChangeLogging(Event ev, Runner run, bool log)
        {
            bool success = false;
            if (log)
            {
                List<OddsInfo> oddsInfos;
                switch (settings.EventType)
                {
                    case "Fight Result":
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

        private ISelection ChooseSelectionType(Event ev)
        {
            ISelection sel;

            switch (settings.EventType)
            {
                case "Fight Result":
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
                case "Fight Result":
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

        private void LoadingScreen(bool v) => LoadingViewGrid.Visibility = v ? Visibility.Visible : Visibility.Hidden;

        private void SetUpEvents()
        {
            OddsLogger.MarketplaceErrorOccured += ReLogin;
            autoRefreshTimer.Elapsed += AutoRefreshTimer_Elapsed;
            twelveHourRefreshTimer.Elapsed += TwelveHourRefreshTimer_Elapsed;
        }

        private void ReLogin(object sender = null, string error = null)
        {
            loginClient.GetNewSessionToken();
            if (loginClient.SessionToken != null)
            {
                marketMessenger.Initialise(loginClient.SessionToken);
                marketMessenger.SetMarketFilter(eventTypeString);

                marketMessenger.GetBettingDictionary(settings.EventType);
            }
            if (error != null)
            {
                DisplayError(OddsLogger, error);
            }

            myGuiProperties.ReloginTimes++;

            //MessageBox.Show("ReLogin Tried, Session token = " + loginClient.SessionToken != null ? "Successful" : "Unsuccessful");
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

        private void ViewBalance(object sender, RoutedEventArgs e)
        {
            //TODO: Implement ViewBalance
        }
    }
}