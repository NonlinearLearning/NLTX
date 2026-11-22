// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable


using Terraria.Ai.Htn.Compounds;

namespace Terraria.Ai.Htn
{
    public interface IDomain
    {
        TaskRoot Root { get; }
        void Add(ICompoundTask parent, ITask subtask);
        void Add(ICompoundTask parent, Slot slot);
    }
}
