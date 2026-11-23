// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System.Collections.Generic;
using Terraria.Ai.Htn.Compounds;
using Terraria.Ai.Htn.Conditions;

namespace Terraria.Ai.Htn
{
    public interface ITask
    {
        /// <summary>
        ///     Used for debugging and identification purposes
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     The parent of this task in the hierarchy
        /// </summary>
        ICompoundTask Parent { get; set; }

        /// <summary>
        ///     The conditions that must be satisfied for this task to pass as valid.
        /// </summary>
        List<ICondition> Conditions { get; }

        /// <summary>
        ///     Add a new condition to the task.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        ITask AddCondition(ICondition condition);

        /// <summary>
        ///     Check the task's preconditions, returns true if all preconditions are valid.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        bool IsValid(IContext ctx);

        DecompositionStatus OnIsValidFailed(IContext ctx);
    }
}
