// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Runtime
{
    public class KeyResolver : IKeyResolver
    {
        public string GetKey(ICondition condition)
        {
            return condition.WorldKey.Name + this.GetText(condition.Comparison);
        }

        public string GetKey(IEffect effect)
        {
            return effect.WorldKey.Name + this.GetText(effect.Increase);
        }

        public bool AreConflicting(IEffect effect, ICondition condition)
        {
            if (effect.WorldKey.Name != condition.WorldKey.Name)
                return false;

            if (this.GetText(effect.Increase) == this.GetText(condition.Comparison))
                return false;

            return true;
        }

        private string GetText(bool value)
        {
            if (value)
                return "_increase";

            return "_decrease";
        }

        private string GetText(Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.GreaterThan:
                case Comparison.GreaterThanOrEqual:
                    return "_increase";
                case Comparison.SmallerThan:
                case Comparison.SmallerThanOrEqual:
                    return "_decrease";
            }

            throw new Exception($"Comparison type {comparison} not supported");
        }
    }
}
