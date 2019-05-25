using System.Collections.Generic;

namespace SoccerBettingModule.Classes
{
    public class MatchResult : ISelection
    {
        public string Name { get; set; }
        public List<Runner> Runners { get; set; }
        public string Winner { get; set; }

        public MatchResult(Runner run1, Runner run2)
        {
            Name = "Match Odds";
            Runners = new List<Runner>
            {
                run1,
                run2
            };
        }

        public MatchResult()
        {
            Name = "Match Odds";
            Runners = new List<Runner>();
        }

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}