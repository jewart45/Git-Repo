using CommonClasses;
using MMABettingModule.Classes;
using MMADatabase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace MMABettingModule
{
    public partial class MainWindow : Window
    {
        public List<MarketplaceBetResult> GetSettledBets(TimeSpan span) => marketMessenger.GetSettledBets(span);

        private void ProcessResults(List<Result> results)
        {
            using (var db = new MMADatabaseModel())
            {
                foreach (var res in results)
                {
                    if (res.Success)
                    {
                        var dataPoints = db.oddsInfo
                            .Where(x => x.FightName == res.EventName);
                        foreach (var point in dataPoints)
                        {
                            point.Winner = point.Name.Trim().ToLower() == res.Winner.Replace(" ", "").ToLower();
                        }
                    }
                    else
                    {
                    }
                }
                db.SaveChanges();
            }
        }

        private void GetResultsFromXML()
        {
            string url = "http://rss.betfair.com/RSS.aspx?format=rss&sportID=26420387";
            XmlReaderSettings p = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse
            };

            Task.Run(() =>
            {
                int i = 0;
                XmlReader reader = XmlReader.Create(url, p);
                while (i < 10)
                {
                    try
                    {
                        var feed = SyndicationFeed.Load(reader);
                        reader.Close();
                        List<Result> results = new List<Result>();
                        foreach (SyndicationItem item in feed.Items)
                        {
                            if (item.Title.Text.Contains("Fight Result settled"))
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
                    System.Threading.Thread.Sleep(200);
                }
            });
        }
    }
}