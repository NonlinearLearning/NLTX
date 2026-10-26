// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System;
using System.Collections.Generic;
using Terraria.Ai.Htn.Compounds;
using Terraria.Ai.Htn.Conditions;
using Terraria.Ai.Htn.Debug;
using Terraria.Ai.Htn.Factory;

namespace Terraria.Ai.Htn
{

    public struct PartialPlanEntry
    {
        public ICompoundTask Task;
        public int TaskIndex;
    }
}
