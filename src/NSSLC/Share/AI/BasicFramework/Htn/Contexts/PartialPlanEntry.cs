// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System;
using System.Collections.Generic;
using Terraria.Share.Ai.Htn.Compounds;
using Terraria.Share.Ai.Htn.Conditions;
using Terraria.Share.Ai.Htn.Debug;
using Terraria.Share.Ai.Htn.Factory;

namespace Terraria.Share.Ai.Htn
{

    public struct PartialPlanEntry
    {
        public ICompoundTask Task;
        public int TaskIndex;
    }
}
