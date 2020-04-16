using System.Collections.Generic;

namespace SportsBettingModule.Classes
{
    internal class SettingsVariables
    {
        public List<string> PossibleEventTypes = new List<string> { "Match Odds", "Go The Distance?", "Round Betting", "Method of Victory" };
        public List<string> PossibleSports = new List<string> { "Mixed Martial Arts", "Soccer", "Boxing", "Basketball" };
        public string EventType;
        public string Competition;
        public string Sport;

        public SettingsVariables()
        {
            EventType = "Match Odds";
            Competition = "";
            Sport = PossibleSports[0];
        }
    }
}