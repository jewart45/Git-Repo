using System.Collections.Generic;

namespace BetHistoryImport.Classes
{
    public class MethodOfVictory : ISelection
    {
        public string Name { get; set; }
        public string Winner { get; set; }
        public List<RunnerSel> Runners { get; set; }

        public MethodOfVictory()
        {
            Name = "Method of Victory";
            Runners = new List<RunnerSel>();
        }

        public void AddRunner(RunnerSel runner) => Runners.Add(runner);
    }
}