using System.Collections.Generic;

namespace BoxingBettingModule.Classes
{
    public class RoundBetting : ISelection
    {
        public string Name { get; set; }
        public List<Runner> Runners { get; set; }

        public RoundBetting(Runner run1, Runner run2)
        {
            Name = "Round Betting";
            Runners = new List<Runner>();
            Runners.Add(run1);
            Runners.Add(run2);
        }

        public RoundBetting()
        {
            Name = "Round Betting";
            Runners = new List<Runner>();
        }

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}