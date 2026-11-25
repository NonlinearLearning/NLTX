// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

namespace Terraria.Share.Ai.Htn.Compounds
{
    /// <summary>
    ///     The Decompose All interface is a tag to signify that this compound task type intends to
    ///     decompose all its subtasks.
    ///     For a task to support Pause Plan tasks, needed for partial planning, it must be
    ///     a decompose-all compound task type.
    /// </summary>
    public interface IDecomposeAll : ICompoundTask
    {
    }
}