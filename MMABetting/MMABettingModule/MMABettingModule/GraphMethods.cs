using MMABettingModule.Classes;
using MMADatabase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace MMABettingModule
{
    public partial class MainWindow : Window
    {
        private Graph CreateGraphView(Event f)
        {
            using (MMADatabaseModel db = new MMADatabaseModel())
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

        private void UpdateGraph()
        {
            Graph g;

            using (MMADatabaseModel db = new MMADatabaseModel())
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
    }
}