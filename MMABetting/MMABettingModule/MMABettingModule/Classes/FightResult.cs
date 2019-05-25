using System.Collections.Generic;

namespace MMABettingModule.Classes
{
    public class FightResult : ISelection
    {
        public string Name { get; set; }
        public List<Runner> Runners { get; set; }

        public FightResult(Runner run1, Runner run2)
        {
            Name = "Fight Result";
            Runners = new List<Runner>();
            Runners.Add(run1);
            Runners.Add(run2);
        }

        public FightResult()
        {
            Name = "Fight Result";
            Runners = new List<Runner>();
        }

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}