using CommonClasses;
using MMABettingModule.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MMABettingModule
{
    public partial class MainWindow : Window
    {
        public void GetBettingInfo(string eventType)
        {
            BettingInfoAvailable = new List<Event>();

            IDictionary<string, string> RunnerDictionary;
            //IDictionary<string, string> EventDictionary;
            //IDictionary<string, string> OddsDictionary;
            //IDictionary<string, string> DateDictionary;

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

            //EventDictionary = marketMessenger.GetEventSelectionIDs(eventType);
            //RunnerDictionary = marketMessenger.GetRunnerSelectionIDs(eventType);
            //OddsDictionary = marketMessenger.GetAllOddsOld(RunnerDictionary.Values.ToList(), eventType, myGuiProperties.Virtualise);
            //DateDictionary = marketMessenger.GetAllDates(EventDictionary.GroupBy(x => x.Value)
            //    .Select(x => x.Key).ToList());

            EventList = marketMessenger.GetEventSelectionIDs(eventType, true);
            EventListWithOdds = marketMessenger.GetAllOdds(EventList, eventType, myGuiProperties.Virtualise);

            //foreach (string t in settings.PossibleEventTypes)
            //{
            //CreateEventFramework(BettingInfoAvailable, EventDictionary, RunnerDictionary, OddsDictionary, DateDictionary, eventType);
            //}
            CreateEventFramework(BettingInfoAvailable, EventListWithOdds, eventType);

            //Sort by fight date and then name of fight
            BettingInfoAvailable = BettingInfoAvailable.OrderBy(x => x.Name).OrderBy(x => x.Date).ToList();
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

                            int targetRow = (int)ListGrid.Children.OfType<Label>()
                                .Where(x => (int)x.GetValue(Grid.ColumnProperty) == 3 && x.Content.ToString() == sel.Name)
                                .First().GetValue(Grid.RowProperty);

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
                    else
                    {
                        InsertOneFightToList(BettingInfoAvailable[i], i);
                    }
                }

                //Lets remove fights that have happened

                RemoveFightsNotAvailable();

                //Set the colours
                SetRowColours();
            }
            catch (Exception ex)
            {
                if (ex.Message != "Collection was modified; enumeration operation may not execute.")
                {
                    DisplayError(marketMessenger, "Could not Fill Rows: " + ex.Message);
                }
            }
        }

        private void InsertOneFightToList(Event fn, int i)
        {
            //rowdef.SetValue(Grid.BackgroundProperty, backgroundBrush);

            //////////////////

            double height = 25;

            RowDefinition rowdef;

            //Rn through each fighter in event
            ISelection s = ChooseSelectionType(fn);
            foreach (Runner run in s.Runners)
            {
                int numRows = ListGrid.RowDefinitions.Count;

                rowdef = new RowDefinition
                {
                    Height = new GridLength(height)
                };

                ListGrid.RowDefinitions.Insert(numRows, rowdef);
                CheckBox toggleLoggingChk = new CheckBox();
                toggleLoggingChk.SetValue(Grid.RowProperty, numRows);
                toggleLoggingChk.SetValue(Grid.ColumnProperty, 0);
                toggleLoggingChk.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                toggleLoggingChk.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                toggleLoggingChk.Click += ToggleLoggingFighter;
                toggleLoggingChk.Tag = run.Name + '|' + fn.Name;

                Label date = new Label
                {
                    Content = fn.Date.ToString("MMMM-dd-HH:mm"),
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

        private void RemoveFightsNotAvailable()
        {
            IEnumerable<string> availableNames = BettingInfoAvailable.Select(y => y.Name);
            IEnumerable<string> missingEventNames = BettingInfoListed.Select(x => x.Name).Where(x => !availableNames.Contains(x));
            if (missingEventNames.Count() > 0)
            {
                List<Event> missingEvents = BettingInfoListed.Where(x => missingEventNames.Contains(x.Name)).ToList();

                foreach (Event ev in missingEvents)
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

        private void DisplayBettingInfo()
        {
            List<string> EventNames = BettingInfoListed.Select(x => x.Name).ToList();
            try
            {
                double height = 25;

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
                                    Content = marketMessenger.GetDate(BettingInfoAvailable.ElementAt(i).Name).ToString("MMMM-dd-HH:mm"),
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

                SetRowColours();
            }
            catch (Exception ex)
            {
                DisplayError(marketMessenger, "Could not Fill Rows: " + ex.Message);
            }
        }

        private void SetRowColours()
        {
            //set background colour
            for (int i = 0; i < ListGrid.RowDefinitions.Count; i++)
            {
                var b = i % 2 > 0 ? Brushes.Beige : Brushes.White;
                ListGrid.RowDefinitions[i].SetValue(Panel.BackgroundProperty, b);
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

        private void ToggleLoggingFighter(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox chk in ListGrid.Children.OfType<CheckBox>())
            {
                if (sender == chk)
                {
                    List<Runner> list = new List<Runner>();
                    List<ISelection> selection = ChooseSelectionType(BettingInfoAvailable, chk.Tag.ToString().Split('|')[1]);

                    foreach (List<Runner> l in selection.Select(y => y.Runners))
                    {
                        list.AddRange(l);
                    }

                    Runner f = list.Where(x => x.Name == chk.Tag.ToString().Split('|')[0]).FirstOrDefault();
                    bool logged = false;
                    switch (settings.EventType)
                    {
                        case "Fight Result":
                            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                            break;

                        case "Go The Distance?":
                            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.GoTheDistance.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                            break;

                        case "Round Betting":
                            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.RoundBetting.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                            break;

                        case "Method of Victory":
                            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.MethodOfVictory.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                            break;

                        default:
                            logged = ChangeLogging(BettingInfoAvailable.Where(x => x.FightResult.Runners.Contains(f)).First(), f, (bool)chk.IsChecked);
                            break;
                    }

                    if (logged != chk.IsChecked)
                    {
                        DisplayError(this, "Could not log, was not able to connect");
                    }

                    chk.IsChecked = logged;
                    break;
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
            else if (eventType == "Go The Distance?")
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
                            e.GoTheDistance.AddRunner(new Runner(runn.Name, runn.SelectionID, runn.Odds != null ? runn.Odds : "0"));
                        }

                        e.Winner = ev.Winner;
                        evList.Add(e);
                    }
                }
            }
            else if (eventType == "Round Betting")
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
                            e.RoundBetting.AddRunner(new Runner(runn.Name, runn.SelectionID, runn.Odds != null ? runn.Odds : "0"));
                        }

                        e.Winner = ev.Winner;
                        evList.Add(e);
                    }
                }
            }
            else if (eventType == "Method of Victory")
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
                            e.MethodOfVictory.AddRunner(new Runner(runn.Name, runn.SelectionID, runn.Odds != null ? runn.Odds : "0"));
                        }

                        e.Winner = ev.Winner;
                        evList.Add(e);
                    }
                }
            }
        }

        //private void CreateEventFramework(List<FightEvent> evList, IDictionary<string, string> eventDictionary, IDictionary<string, string> runnerDictionary, IDictionary<string, string> oddsDictionary, IDictionary<string, string> dateDictionary, string eventType)
        //{
        //    if (eventType == "Fight Result")
        //    {
        //        foreach (KeyValuePair<string, string> p in runnerDictionary)
        //        {
        //            string selId = p.Value;
        //            if (eventDictionary.ContainsKey(p.Value))
        //            {
        //                //If not in eventList, add
        //                if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
        //                {
        //                    FightEvent e = new FightEvent(eventDictionary[selId])
        //                    {
        //                        Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
        //                    };
        //                    evList.Add(e);
        //                }

        //                //Add fighters and add Fight Event info
        //                FightEvent k = evList.Where(x => x.Name == eventDictionary[selId]).FirstOrDefault();
        //                evList.Where(x => x.Name == eventDictionary[selId])
        //                    .First().Fighters.Add(new Fighter(p.Key, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
        //                evList.Where(x => x.Name == eventDictionary[selId])
        //                    .First().FightResult.AddRunner(new Runner(p.Key, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
        //            }
        //        }
        //    }
        //    else if (eventType == "Go The Distance?")
        //    {
        //        foreach (KeyValuePair<string, string> p in runnerDictionary)
        //        {
        //            string selId = p.Value;
        //            if (eventDictionary.ContainsKey(p.Value))
        //            {
        //                if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
        //                {
        //                    FightEvent e = new FightEvent(eventDictionary[selId])
        //                    {
        //                        Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
        //                    };
        //                    evList.Add(e);
        //                }

        //                //Add GoTheDistance
        //                evList.Where(x => x.Name == eventDictionary[selId])
        //                    .First().GoTheDistance.AddRunner(new Runner(p.Key, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
        //            }
        //        }
        //    }
        //    else if (eventType == "Round Betting")
        //    {
        //        foreach (KeyValuePair<string, string> p in runnerDictionary)
        //        {
        //            string selId = p.Value;
        //            if (eventDictionary.ContainsKey(p.Value))
        //            {
        //                if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
        //                {
        //                    FightEvent e = new FightEvent(eventDictionary[selId])
        //                    {
        //                        Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
        //                    };
        //                    evList.Add(e);
        //                }

        //                //Add GoTheDistance
        //                evList.Where(x => x.Name == eventDictionary[selId])
        //                    .First().RoundBetting.AddRunner(new Runner(p.Key, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
        //            }
        //        }
        //    }
        //    else if (eventType == "Method of Victory")
        //    {
        //        foreach (KeyValuePair<string, string> p in runnerDictionary)
        //        {
        //            string selId = p.Value;
        //            if (eventDictionary.ContainsKey(p.Value))
        //            {
        //                if (!evList.Select(x => x.Name).ToList().Contains(eventDictionary[selId]))
        //                {
        //                    FightEvent e = new FightEvent(eventDictionary[selId])
        //                    {
        //                        Date = Convert.ToDateTime(dateDictionary[eventDictionary[selId]])
        //                    };
        //                    evList.Add(e);
        //                }

        //                //Add Method of Victory
        //                evList.Where(x => x.Name == eventDictionary[selId])
        //                    .First().MethodOfVictory.AddRunner(new Runner(p.Key, selId, oddsDictionary.ContainsKey(selId) ? oddsDictionary[selId] : "0"));
        //            }
        //        }
        //    }
        //}
    }
}