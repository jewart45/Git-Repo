﻿using System.Collections.Generic;

namespace BetHistoryImport.Classes
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
        List<RunnerSel> Runners { get; set; }

        /// <summary>
        /// Adds the runner.
        /// </summary>
        /// <param name="runner">The runner.</param>
        void AddRunner(RunnerSel runner);
    }
}