// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System;
using Terraria.Share.Ai.Htn.Conditions;

namespace Terraria.Share.Ai.Htn.Debug
{

    public struct DecomposedCompoundTaskEntry : IDecompositionLogEntry<ITask>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Depth { get; set; }
        public ConsoleColor Color { get; set; }
        public ITask Entry { get; set; }
    }
}
