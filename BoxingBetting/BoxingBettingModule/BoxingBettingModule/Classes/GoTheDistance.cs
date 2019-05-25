using System.Collections.Generic;
using System.Linq;

namespace BoxingBettingModule.Classes
{
    public class GoTheDistance : ISelection
    {
        public string Name { get; set; }
        public string Yes => Runners.Where(x => x.Name == "Yes").FirstOrDefault().Odds;
        public string No => Runners.Where(x => x.Name == "No").FirstOrDefault().Odds;
        public List<Runner> Runners { get; set; }

        public GoTheDistance(Runner run1, Runner run2)
        {
            Name = "Go The Distance?";
            Runners = new List<Runner>();
            Runners.Add(run1);
            Runners.Add(run2);
        }

        public GoTheDistance()
        {
            Name = "Go The Distance?";
            Runners = new List<Runner>();
        }

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}