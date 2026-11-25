// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using Terraria.Share.Ai.Goap.Core;

namespace Terraria.Share.Ai.Goap.Runtime
{
    public class ValueCondition : IValueCondition
    {
        public IWorldKey WorldKey { get; set; }
        public Comparison Comparison { get; set; }
        public int Amount { get; set; }

        public ValueCondition()
        {
        }

        public ValueCondition(IWorldKey worldKey, Comparison comparison, int amount)
        {
            this.WorldKey = worldKey;
            this.Comparison = comparison;
            this.Amount = amount;
        }
    }
}