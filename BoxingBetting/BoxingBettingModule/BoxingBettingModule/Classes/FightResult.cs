using System.Collections.Generic;

namespace BoxingBettingModule.Classes
{
    public class FightResult : ISelection
    {
        public string Name { get; set; }
        public List<Runner> Runners { get; set; }

        public FightResult(Runner run1, Runner run2)
        {
            Name = "Match Odds";
            Runners = new List<Runner>
            {
                run1,
                run2
            };
        }

        public FightResult()
        {
            Name = "Match Odds";
            Runners = new List<Runner>();
        }

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}