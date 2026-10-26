// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable


using System.Collections.Generic;

namespace Terraria.Ai.Htn.Compounds
{
    public interface ICompoundTask : ITask
    {
        List<ITask> Subtasks { get; }
        ICompoundTask AddSubtask(ITask subtask);

        /// <summary>
        ///     Decompose the task onto the tasks to process queue, mind it's depth first
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        DecompositionStatus Decompose(IContext ctx, int startIndex, out Queue<ITask> result);
    }
}
