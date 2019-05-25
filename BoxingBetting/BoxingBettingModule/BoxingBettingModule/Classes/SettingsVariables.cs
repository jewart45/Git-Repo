using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxingBettingModule.Classes
{
    internal class SettingsVariables
    {
        public List<string> PossibleEventTypes = new List<string> { "Match Odds", "Go The Distance?", "Round Betting", "Method of Victory" };
        public string EventType;

        public SettingsVariables()
        {
            EventType = "Match Odds";
        }
    }
}