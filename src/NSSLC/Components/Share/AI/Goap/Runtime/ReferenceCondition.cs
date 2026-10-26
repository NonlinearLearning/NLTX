// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Runtime
{
    public class ReferenceCondition : IReferenceCondition
    {
        public IWorldKey WorldKey { get; set; }
        public Comparison Comparison { get; set; }
        public IWorldKey ValueKey { get; set; }

        public ReferenceCondition()
        {
        }

        public ReferenceCondition(IWorldKey worldKey, Comparison comparison, IWorldKey valueKey)
        {
            this.WorldKey = worldKey;
            this.Comparison = comparison;
            this.ValueKey = valueKey;
        }
    }
}