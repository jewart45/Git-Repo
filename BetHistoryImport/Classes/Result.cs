using System;

namespace BetHistoryImport.Classes
{
    public class Result
    {
        public string EventName { get; set; }

        public string Winner { get; set; }

        public string RawInput { get; set; }

        public bool Success { get; private set; }

        public Result(string eventName, string winner)
        {
            RawInput = eventName;
            var ok = ProcessWinner(winner);
            ok = ok && ProcessEventName(eventName);

            Success = ok;
        }

        private bool ProcessEventName(string input)
        {
            string[] p = input.Split(new char[] { '/', '-' });
            if (p.Length > 2)
            {
                EventName = p[1].Trim();
                return true;
            }
            else
            {
                EventName = input;
                return false;
            }
        }

        private bool ProcessWinner(string input)
        {
            var k = input.Split(new string[] { "Winner(s): " }, StringSplitOptions.None);

            if (k.Length > 1)
            {
                Winner = k[1].Trim();
                return true;
            }
            else
            {
                Winner = input;
                return false;
            }
        }
    }
}