using System.Collections.Generic;
using System.Linq;

namespace BetHistoryImport.Classes
{
    public class GoTheDistance : ISelection
    {
        public string Name { get; set; }
        public string Winner { get; set; }
        public string Yes => Runners.Where(x => x.Name == "Yes").FirstOrDefault().Odds;
        public string No => Runners.Where(x => x.Name == "No").FirstOrDefault().Odds;
        public List<RunnerSel> Runners { get; set; }

        public GoTheDistance(RunnerSel run1, RunnerSel run2)
        {
            Name = "Go The Distance?";
            Runners = new List<RunnerSel>
            {
                run1,
                run2
            };
        }

        public GoTheDistance()
        {
            Name = "Go The Distance?";
            Runners = new List<RunnerSel>();
        }

        public void AddRunner(RunnerSel runner) => Runners.Add(runner);
    }
}