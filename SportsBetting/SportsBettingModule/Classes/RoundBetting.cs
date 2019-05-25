using System.Collections.Generic;

namespace SportsBettingModule.Classes
{
    public class RoundBetting : ISelection
    {
        public string Name { get; set; }
        public string Winner { get; set; }
        public List<Runner> Runners { get; set; }

        public RoundBetting(Runner run1, Runner run2)
        {
            Name = "Round Betting";
            Runners = new List<Runner>
            {
                run1,
                run2
            };
        }

        public RoundBetting()
        {
            Name = "Round Betting";
            Runners = new List<Runner>();
        }

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}