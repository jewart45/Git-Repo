using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SportsBettingModule.Classes
{
    public class Graph
    {
        public Dictionary<DateTime, double> pointsDictionary1 { get; set; }

        public Dictionary<int, double> pointsDictionaryInt1 { get; set; }

        public Dictionary<DateTime, double> pointsDictionary2 { get; set; }

        public Dictionary<DateTime, double> pointsDictionary3 { get; set; }

        public enum GraphType { Oxy, Visiblox }

        public string AxisNameX { get; set; }

        public string AxisNameY { get; set; }

        public string AxisNameY2 { get; set; }
        public string AxisNameY3 { get; set; }

        public string Title { get; set; }

        public double IncrementX { get; set; }

        private List<DataPoint> PlotPoints1 { get; set; }
        private List<DataPoint> PlotPoints2 { get; set; }
        private List<DataPoint> PlotPoints3 { get; set; }

        public double IncrementY { get; set; }

        private readonly DateTime MinX;
        private readonly DateTime MaxX;
        private readonly int MinXInt;
        private readonly int MaxXInt;

        private readonly double MinY;
        private readonly double MaxY;

        private bool useDtAxis;

        public Type TypeX { get; private set; }

        public Type TypeY { get; private set; }

        public Type TypeY2 { get; private set; }
        public Type TypeY3 { get; private set; }

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

        public Graph(Dictionary<int, double> dictionary, string title, string axisNameX, string axisNameY)
        {
            pointsDictionaryInt1 = dictionary;

            MinXInt = dictionary.Keys.First();
            MaxXInt = dictionary.Keys.Last();
            MinY = dictionary.Values.Min();
            MaxY = dictionary.Values.Max();

            AxisNameX = axisNameX;
            AxisNameY = axisNameY;
            Title = title;

            PlotPoints1 = new List<DataPoint>();
            CreatePointList(dictionary);
        }

        public Graph(Dictionary<DateTime, double> dictionary1, Dictionary<DateTime, double> dictionary2, string title, string axisNameX, string axisNameY, string axisNameY2, Dictionary<DateTime, double> dictionary3 = null, string axisNameY3 = "")
        {
            pointsDictionary1 = dictionary1;
            pointsDictionary2 = dictionary2;
            pointsDictionary3 = dictionary3;

            MinX = dictionary1.Keys.First();
            MaxX = dictionary1.Keys.Last();
            //Take whichever max is greater
            MinY = dictionary1.Values.Min() > dictionary2.Values.Min() ? dictionary2.Values.Min() : dictionary1.Values.Min();
            MaxY = dictionary1.Values.Max() < dictionary2.Values.Max() ? dictionary2.Values.Max() : dictionary1.Values.Max();

            AxisNameX = axisNameX;
            AxisNameY = axisNameY;
            AxisNameY2 = axisNameY2;
            AxisNameY3 = axisNameY3;
            Title = title;

            PlotPoints1 = new List<DataPoint>();
            PlotPoints2 = new List<DataPoint>();
            PlotPoints3 = new List<DataPoint>();
            CreatePointList(dictionary1, dictionary2, dictionary3);
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

        private void CreatePointList(Dictionary<int, double> dict)
        {
            PlotPoints1.Clear();
            int i = 0;
            while (i < dict.Count)
            {
                PlotPoints1.Add(new DataPoint(dict.Keys.ToList()[i], dict.Values.ToList()[i]));
                i++;
            }
        }

        private void CreatePointList(Dictionary<DateTime, double> dict1, Dictionary<DateTime, double> dict2, Dictionary<DateTime, double> dict3 = null)
        {
            PlotPoints1.Clear();
            PlotPoints2.Clear();
            PlotPoints3.Clear();
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
            if (dict3 != null)
            {
                var count3 = dict3?.Count;
                i = 0;
                while (i < count3)
                {
                    PlotPoints3.Add(new DataPoint(DateTimeAxis.ToDouble(dict3.Keys.ToList()[i]), dict3.Values.ToList()[i]));
                    i++;
                }
            }
        }

        public PlotModel CreateOxyPlot()
        {
            useDtAxis = pointsDictionary1 != null;
            if (Title == "")
            {
                return null;
            }

            var plotView = new PlotModel();

            var xAxisDt = new DateTimeAxis();
            var xAxisInt = new LinearAxis();
            var yAxis = new LinearAxis
            {
                //xAxis = (DateTimeAxis)plotView.DefaultXAxis;
                Minimum = MinY - (MinY * 0.1),
                Maximum = MaxY + (MaxY * 0.1)
            };
            xAxisInt.Position = AxisPosition.Bottom;
            xAxisDt.Position = AxisPosition.Bottom;

            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dot;

            xAxisDt.StringFormat = "dd/MM hh:mm";
            if (useDtAxis)
            {
                xAxisDt.IntervalType = DateTimeIntervalType.Auto;
            }
            var series = new OxyPlot.Series.LineSeries();

            if (useDtAxis)
            {
                series = new OxyPlot.Series.LineSeries()
                {
                    TrackerFormatString = "Date: {hh:mm dd/MM/yy};Value: {4}",
                    ItemsSource = PlotPoints1,
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerFill = OxyPlot.OxyColors.OrangeRed,
                    Color = OxyPlot.OxyColors.OrangeRed,
                    Title = AxisNameY
                };
            }
            else
            {
                series = new OxyPlot.Series.LineSeries()
                {
                    ItemsSource = PlotPoints1,
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerFill = OxyPlot.OxyColors.OrangeRed,
                    Color = OxyPlot.OxyColors.OrangeRed,
                    Title = AxisNameY
                };
            }
            //foreach (PlotPoint point in PlotPoints)
            //    series.Points.Add(new OxyPlot.Series.ScatterPoint(point.X, point.Y));

            plotView.Series.Add(series);

            if (PlotPoints2?.Count != 0)
            {
                var series2 = new OxyPlot.Series.LineSeries
                {
                    TrackerFormatString = "Date: {2:M/d/yy};Value: {4}",
                    ItemsSource = PlotPoints2,
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerFill = OxyColors.CornflowerBlue,
                    Color = OxyColors.CornflowerBlue,
                    Title = AxisNameY2
                };
                plotView.IsLegendVisible = true;
                plotView.Series.Add(series2);
            }

            if (PlotPoints3?.Count != 0)
            {
                var series3 = new OxyPlot.Series.LineSeries
                {
                    TrackerFormatString = "Date: {2:M/d/yy};Value: {4}",
                    ItemsSource = PlotPoints3,
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerFill = OxyColors.Green,
                    Color = OxyColors.Green,
                    Title = AxisNameY3
                };
                plotView.IsLegendVisible = true;
                plotView.Series.Add(series3);
            }

            plotView.Title = Title;
            if (useDtAxis)
            {
                plotView.Axes.Add(xAxisDt);
            }
            else
            {
                plotView.Axes.Add(xAxisInt);
            }

            plotView.Axes.Add(yAxis);
            return plotView;
        }

        public DataTable ToDataTable()
        {
            DataTable table = new DataTable(Title);
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