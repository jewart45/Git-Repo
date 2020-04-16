using System.Collections.Generic;

namespace SportsBettingModule.Classes
{
    public interface ISelection
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the winner.
        /// </summary>
        /// <value>
        /// The winner.
        /// </value>
        string Winner { get; set; }

        /// <summary>
        /// Gets or sets the runners.
        /// </summary>
        /// <value>
        /// The runners.
        /// </value>
        List<Runner> Runners { get; set; }

        /// <summary>
        /// Adds the runner.
        /// </summary>
        /// <param name="runner">The runner.</param>
        void AddRunner(Runner runner);
    }
}