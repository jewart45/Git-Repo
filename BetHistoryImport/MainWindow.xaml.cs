using BetHistoryImport.Classes;
using Newtonsoft.Json;
using SportsDatabaseSqlite;
using SportsDatabaseSqlite.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetHistoryImport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GUIProperties myGuiProperties;
        private bool bail;
        private System.Timers.Timer timer;

        public List<SelectionDisplay> AllSelections { get; private set; } = new List<SelectionDisplay>();

        public MainWindow()
        {
            InitializeComponent();

            myGuiProperties = new GUIProperties();

            DataContext = myGuiProperties;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var filePath = string.Empty;

            using (System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\Users\\jewar\\Downloads\\BASIC\\2020";
                //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                //openFileDialog.FilterIndex = 2;
                //openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    myGuiProperties.FilePath = openFileDialog.SelectedPath;
                }
            }
        }

        private void GetEventInfo(string filePath)
        {
            Task.Run(async () =>
            {
                string[] files = Directory.GetFiles(filePath, "*", SearchOption.AllDirectories);
                //Read the contents of the file into a stream
                var fileContent = string.Empty;
                foreach (var file in files)
                {
                    //bail out of running
                    if (bail)
                    {
                        break;
                    }
                    StreamReader reader = new StreamReader(file);
                    fileContent = reader.ReadToEnd();
                    var oddsToAdd = await BuildEventHistoryAsync(fileContent);
                    var oddsSummed = new List<OddsInfoMedian>();

                    foreach (var o in oddsToAdd.Select(x => x.SelectionName).Distinct())
                    {
                        var odds = oddsToAdd.Where(x => x.SelectionName == o);

                        var odd = odds.First();
                        //median value
                        var median = oddsToAdd
                        .Where(x => x.SelectionName == o)
                        .Select(x => x.OddsValue)
                        .OrderBy(x => x)
                        .Skip(odds.Count() / 2)
                        .First();
                        oddsSummed.Add(new OddsInfoMedian()
                        {
                            DateTaken = odd.DateTaken,
                            CountryCode = odd.CountryCode,
                            EventDate = odd.EventDate,
                            EventName = odd.EventName,
                            EventTypeId = odd.EventTypeId,
                            ResultType = odd.ResultType,
                            MarketID = odd.MarketID,
                            OddsMedian = median,
                            SelectionID = odd.SelectionID,
                            SelectionName = odd.SelectionName,
                            Winner = odd.Winner
                        });
                    }
                    if (oddsToAdd.Count > 0)
                    {
                        using (var db = new SportsDatabaseModel())
                        {
                            db.oddsInfo.AddRange(oddsToAdd);
                            db.oddsInfoMed.AddRange(oddsSummed);
                            await db.SaveChangesAsync();
                        }
                        //await Task.Delay(1000);
                        //Add to Logged
                    }
                }
                InvokeUI(() =>
                {

                    bailBtn.Visibility = Visibility.Hidden;
                    RunGetInfoBtn.Visibility = Visibility.Visible;
                });
            });
        }

        private void UpdateList()
        {
            InvokeUI(() =>
            {
                
                //dataGrid.ItemsSource = AllSelections;
            });
        }

        public static string OddsToChance(string Odds)
        {
            if (Odds == "-") return "0";
            double chance;
            double oddsDbl = Convert.ToDouble(Odds);
            if (oddsDbl > 0) chance = 100 / (oddsDbl + 100);
            else
            {
                oddsDbl *= -1;
                chance = oddsDbl / (100 + oddsDbl);
            }
            return (chance * 100).ToString("#.##") + "%";
        }

        public static long PriceTradedtoOdds(double val)
        {
            double line;
            //val = val * 100;
            //Set + or -

            string prefix = val >= 2 ? "+" : "-";
            if (val == 0)
                return 0;
            else if (val >= 2)
            {
                line = val * 100 - 100;
                return (long)line;
            }
            else
            {
                line = -1 * (100 / (val - 1));
                return (long)line;
            }
        }

        private async Task<List<OddsInfo>> BuildEventHistoryAsync(string fileContent)
        {
            List<OddsInfo> oddsInfo = new List<OddsInfo>();
            //Null check
            if (fileContent == "")
                return oddsInfo;

            var content = fileContent.Split('\n');
            List<ImportPoint> objList = new List<ImportPoint>();
            content.Where(x => x.Contains("Hey"));
            List<ImportEvent> evList = new List<ImportEvent>();
            foreach (var cont in content.Where(x => x.Contains("marketDefinition")))
            {
                var evToAdd = JsonConvert.DeserializeObject<ImportEvent>(cont);
                evList.Add(evToAdd);
            };

            var startTimeEv = evList.Find(x => x.Mc.First(y=>y.MarketDefinition != null).MarketDefinition.InPlay);
            if (startTimeEv == null || (startTimeEv.Mc.First(y => y.MarketDefinition != null).MarketDefinition.EventTypeId != myGuiProperties.EventTypeId || myGuiProperties.EventTypeId == 0) || (startTimeEv.Mc.First(y=>y.MarketDefinition != null).MarketDefinition.MarketType != myGuiProperties.ResultType || myGuiProperties.ResultType == "") || !startTimeEv.Mc.First(y => y.MarketDefinition != null).MarketDefinition.EventName.Contains('@'))
                return oddsInfo;

            var startTime = startTimeEv.Clk;

            var endEv = JsonConvert.DeserializeObject<ImportEvent>(content.ToList().Find(x => x.Contains(@"""status"":""CLOSED"",")));
            var evDef = endEv.Mc.First(y => y.MarketDefinition != null).MarketDefinition;
            //build event

            if (evDef.Runners.Where(x => x.Status == "WINNER").FirstOrDefault() == null)
                return oddsInfo;

            var ev = new Event(evDef.EventName, evDef.EventId.ToString())
            {
                Date = evDef.MarketTime.LocalDateTime,
                Winner = evDef.Runners.Where(x => x.Status == "WINNER").FirstOrDefault().Name,
            };

            //set up the result
            var result = new OtherResult(evDef.MarketType, evDef.EventId.ToString());
            foreach (var r in evDef.Runners)
            {
                result.AddRunner(new RunnerSel(r.Name, r.Id, r.Status == "WINNER", "0"));
            }
            ev.AddOtherResult(result);

            foreach (var line in content.Where(x => !x.Contains("marketDefinition") && x != ""))
            {
                var converted = JsonConvert.DeserializeObject<ImportPoint>(line);
                if (converted.Clk > startTime)
                    break;
                for (int i = 0; i < converted.Mc.First(y => y.Rc != null).Rc.Count(); i++)
                {
                    var runner = ev.OtherResults.First().Runners.Find(x => x.SelectionID == converted.Mc.First(y => y.Rc != null).Rc[i].Id);
                    if(runner != null)
                    {
                        oddsInfo.Add(new OddsInfo()
                        {
                            DateTaken = evDef.MarketTime.LocalDateTime.AddMinutes(i),
                            CountryCode = evDef.CountryCode,
                            EventDate = evDef.MarketTime.LocalDateTime,
                            EventTypeId = evDef.EventTypeId,
                            EventName = evDef.EventName,
                            ResultType = evDef.MarketType,
                            MarketID = evDef.EventId.ToString(),
                            OddsValue = PriceTradedtoOdds(converted.Mc.First(y => y.Rc != null).Rc[i].Ltp),
                            SelectionID = converted.Mc.First(y => y.Rc != null).Rc[i].Id.ToString(),
                            SelectionName = runner.Name,
                            Winner = runner.Winner
                        });
                    }
                    
                }

                objList.Add(converted);
            };

            await UpdateNewInfo(ev);

            return oddsInfo;
        }

        private void InvokeUI(Action a) => Application.Current.Dispatcher.Invoke(a);

        private async Task UpdateNewInfo(Event ev)
        {
            var sels = new List<SelectionDisplay>();
            foreach (var runner in ev.OtherResults.First().Runners)
            {
                sels.Add(new SelectionDisplay()
                {
                    Date = ev.Date,
                    DecimalOdds = runner.Multiplier,
                    Odds = Convert.ToDouble(runner.Odds),
                    EventName = ev.Name,
                    Percentage = runner.PercentChance,
                    ResultType = ev.OtherResults.First().Name,
                    Selected = false,
                    SelectionName = runner.Name,
                    Winner = runner.Winner
                });
            }
            InvokeUI(() =>
            {
                foreach(var item in sels)
                {
                    dataGrid.Items.Add(item);

                }
            });
            await Task.Delay(200);

            //Update UI
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void filteredListLbl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void allSelectionsLbl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void CompetitionTypeSelectorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void RunGetInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            bail = false;
            bailBtn.Visibility = Visibility.Visible;
            RunGetInfoBtn.Visibility = Visibility.Hidden;
            //RunListUpdater();
            GetEventInfo(myGuiProperties.FilePath);
        }

        private void RunListUpdater()
        {
            timer = new System.Timers.Timer(1000);
            timer.Interval = 1000 * 5;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) => UpdateList();

        private void bailBtn_Click(object sender, RoutedEventArgs e)
        {
            RunGetInfoBtn.Visibility = Visibility.Visible;
            bailBtn.Visibility = Visibility.Hidden;
            bail = true;
            timer?.Stop();
        }
    }
}