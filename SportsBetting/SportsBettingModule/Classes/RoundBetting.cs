using System.Collections.Generic;

namespace SportsBettingModule.Classes
{
    public class OtherResult : ISelection
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Winner { get; set; }
        public List<Runner> Runners { get; set; } = new List<Runner>();

        public OtherResult(string name, string id = null)
        {
            Name = name;
            Id = id;
        }

        public void AddRunner(Runner runner) => Runners.Add(runner);
    }
}