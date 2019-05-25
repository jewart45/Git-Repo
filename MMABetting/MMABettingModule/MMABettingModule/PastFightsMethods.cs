using MMADatabase;
using MMADatabase.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MMABettingModule
{
    public partial class MainWindow : Window
    {
        private void DisplayPastFights()
        {
            LoadingScreen(true);
            Task.Run(() =>
            {
                List<string> pastFights;
                DateTime fightDate;
                Dictionary<string, int> PastFightsPointsDictionary = new Dictionary<string, int>();
                Dictionary<string, DateTime> LastDatePointsDictionary = new Dictionary<string, DateTime>();

                using (MMADatabaseModel db = new MMADatabaseModel())
                {
                    List<OddsInfo> oddsInfos = db.oddsInfo.ToList();

                    pastFights = oddsInfos
                        .OrderBy(x => x.FightDate)
                        .Select(x => x.FightName)
                        .Distinct()
                        .ToList();

                    foreach (string fight in pastFights)
                    {
                        int count = oddsInfos.Where(x => x.FightName == fight).Count();
                        PastFightsPointsDictionary.Add(fight, count);

                        DateTime d = oddsInfos
                        .Where(x => x.FightName == fight)
                        .OrderByDescending(x => x.DateTaken)
                        .Select(x => x.DateTaken)
                        .FirstOrDefault();

                        LastDatePointsDictionary.Add(fight, d);
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
                            using (MMADatabaseModel db = new MMADatabaseModel())
                            {
                                string s = pastFights[i];
                                List<OddsInfo> DataList = db.oddsInfo
                                    .Where(x => x.FightName == s)
                                    .ToList();
                                fightDate = DataList
                                    .OrderBy(x => x.DateTaken)
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

                            Label lastDate = new Label();
                            if (LastDatePointsDictionary.ContainsKey(pastFights[i]))
                            {
                                lastDate = new Label
                                {
                                    Content = LastDatePointsDictionary[pastFights[i]].ToString(),
                                    HorizontalAlignment = HorizontalAlignment.Left
                                };
                                lastDate.SetValue(Grid.RowProperty, numRows - 1);
                                lastDate.SetValue(Grid.ColumnProperty, 4);
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
                            pastFightsGrid.Children.Add(lastDate);
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
                using (MMADatabaseModel db = new MMADatabaseModel())
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
    }
}