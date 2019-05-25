using System.Collections.Generic;

namespace SoccerBettingModule.Classes
{
    public class MethodOfVictory : ISelection
    {
        public string Name { get; set; }
        public string Winner { get; set; }
        public List<Runner> Runners { get; set; }

        public MethodOfVictory()
        {
            Name = "Method of Victory";
            Runners = new List<Runner>();
        }

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}