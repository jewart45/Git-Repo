using System;

namespace BetHistoryImport.Classes
{
    public class SelectionDisplay
    {
        public bool Selected { get; set; }
        public DateTime Date { get; set; }
        public string EventName { get; set; }
        public string SelectionName { get; set; }

        public string ResultType { get; set; }

        public string Percentage { get; set; }

        public double DecimalOdds { get; set; }
        public double Odds { get; set; }
        public double Change { get; set; }
    }
}