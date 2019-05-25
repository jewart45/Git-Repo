using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;

namespace MMABettingModule.Classes
{
    public class Graph
    {
        public Dictionary<DateTime, double> pointsDictionary1 { get; set; }

        public Dictionary<DateTime, double> pointsDictionary2 { get; set; }

        public enum GraphType { Oxy, Visiblox }

        public string AxisNameX { get; set; }

        public string AxisNameY { get; set; }

        public string AxisNameY2 { get; set; }

        public string Title { get; set; }

        public double IncrementX { get; set; }

        private List<DataPoint> PlotPoints1 { get; set; }
        private List<DataPoint> PlotPoints2 { get; set; }

        public double IncrementY { get; set; }

        private DateTime MinX;
        private DateTime MaxX;

        private double MinY;
        private double MaxY;

        public Type TypeX { get; private set; }

        public Type TypeY { get; private set; }

        public Type TypeY2 { get; private set; }

        public Graph(Dictionary<DateTime, double> dictionary, string title, string axisNameX, string axisNameY)
        {
            pointsDictionary1 = dictionary;

            MinX = dictionary.Keys.First();
            MaxX = dictionary.Keys.Last();
            MinY = dictionary.Values.Min();
            MaxY = dictionary.Values.Max();

            AxisNameX = axisNameX;
            AxisNameY = axisNameY;
            Title = title;

            PlotPoints1 = new List<DataPoint>();
            CreatePointList(dictionary);
        }

        public Graph(Dictionary<DateTime, double> dictionary1, Dictionary<DateTime, double> dictionary2, string title, string axisNameX, string axisNameY, string axisNameY2)
        {
            pointsDictionary1 = dictionary1;
            pointsDictionary2 = dictionary2;

            MinX = dictionary1.Keys.First();
            MaxX = dictionary1.Keys.Last();
            //Take whichever max is greater
            MinY = dictionary1.Values.Min() > dictionary2.Values.Min() ? dictionary2.Values.Min() : dictionary1.Values.Min();
            MaxY = dictionary1.Values.Max() < dictionary2.Values.Max() ? dictionary2.Values.Max() : dictionary1.Values.Max();

            AxisNameX = axisNameX;
            AxisNameY = axisNameY;
            AxisNameY2 = axisNameY2;
            Title = title;

            PlotPoints1 = new List<DataPoint>();
            PlotPoints2 = new List<DataPoint>();
            CreatePointList(dictionary1, dictionary2);
        }

        private void CreatePointList(Dictionary<DateTime, double> dict)
        {
            PlotPoints1.Clear();
            int i = 0;
            while (i < dict.Count)
            {
                PlotPoints1.Add(new DataPoint(DateTimeAxis.ToDouble(dict.Keys.ToList()[i]), dict.Values.ToList()[i]));
                i++;
            }
        }

        private void CreatePointList(Dictionary<DateTime, double> dict1, Dictionary<DateTime, double> dict2)
        {
            PlotPoints1.Clear();
            PlotPoints2.Clear();
            //var count1 = dict1.Count > dict2.Count ? dict2.Count : dict1.Count;
            var count1 = dict1.Count;
            int i = 0;
            while (i < count1)
            {
                PlotPoints1.Add(new DataPoint(DateTimeAxis.ToDouble(dict1.Keys.ToList()[i]), dict1.Values.ToList()[i]));
                i++;
            }
            var count2 = dict2.Count;
            i = 0;
            while (i < count2)
            {
                PlotPoints2.Add(new DataPoint(DateTimeAxis.ToDouble(dict2.Keys.ToList()[i]), dict2.Values.ToList()[i]));
                i++;
            }
        }

        public PlotModel CreateOxyPlot()
        {
            if (Title == "") return null;
            var plotView = new PlotModel();
            var xAxis = new DateTimeAxis();
            var yAxis = new LinearAxis();
            //xAxis = (DateTimeAxis)plotView.DefaultXAxis;
            yAxis.Minimum = MinY - 75;
            yAxis.Maximum = MaxY + 75;
            xAxis.Position = AxisPosition.Bottom;

            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dot;

            xAxis.StringFormat = "dd/MM hh:mm";
            xAxis.IntervalType = DateTimeIntervalType.Auto;

            var series = new OxyPlot.Series.LineSeries();
            series.TrackerFormatString = "Date: {2:M/d/yy};Value: {4}";
            series.ItemsSource = PlotPoints1;
            series.StrokeThickness = 2;
            series.MarkerType = MarkerType.Circle;
            series.MarkerSize = 3;
            series.MarkerFill = OxyPlot.OxyColors.OrangeRed;
            series.Color = OxyPlot.OxyColors.OrangeRed;
            series.Title = AxisNameY;
            //foreach (PlotPoint point in PlotPoints)
            //    series.Points.Add(new OxyPlot.Series.ScatterPoint(point.X, point.Y));

            plotView.Series.Add(series);

            if (PlotPoints2 != null)
            {
                var series2 = new OxyPlot.Series.LineSeries();
                series2.TrackerFormatString = "Date: {2:M/d/yy};Value: {4}";
                series2.ItemsSource = PlotPoints2;
                series2.StrokeThickness = 2;
                series2.MarkerType = MarkerType.Circle;
                series2.MarkerSize = 3;
                series2.MarkerFill = OxyColors.CornflowerBlue;
                series2.Color = OxyColors.CornflowerBlue;
                series2.Title = AxisNameY2;
                plotView.IsLegendVisible = true;
                plotView.Series.Add(series2);
            }

            plotView.Title = Title;

            plotView.Axes.Add(xAxis);
            plotView.Axes.Add(yAxis);
            return plotView;
        }

        public DataTable ToDataTable()
        {
            DataTable table = new DataTable(this.Title);
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Odds", typeof(double));

            foreach (KeyValuePair<DateTime, double> i in pointsDictionary1)
            {
                table.Rows.Add(i.Key, i.Value);
            }
            return table;
        }
    }
}