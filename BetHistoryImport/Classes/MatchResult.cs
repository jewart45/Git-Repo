using System.Collections.Generic;

namespace BetHistoryImport.Classes
{
    public class MatchResult : ISelection
    {
        public string Name { get; set; }
        public List<RunnerSel> Runners { get; set; }
        public string Winner { get; set; }

        public MatchResult(RunnerSel run1, RunnerSel run2)
        {
            Name = "Match Odds";
            Runners = new List<RunnerSel>
            {
                run1,
                run2
            };
        }

        public MatchResult()
        {
            Name = "Match Odds";
            Runners = new List<RunnerSel>();
        }

        public void AddRunner(RunnerSel runner) => Runners.Add(runner);
    }
}