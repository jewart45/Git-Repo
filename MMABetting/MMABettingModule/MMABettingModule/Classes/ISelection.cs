using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMABettingModule.Classes
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