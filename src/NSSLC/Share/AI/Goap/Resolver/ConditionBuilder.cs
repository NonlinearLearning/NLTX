// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System.Collections.Generic;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Resolver
{
    public class ConditionBuilder : IConditionBuilder
    {
        private readonly List<ICondition> conditionIndexList;
        private bool[] conditionsMetList;

        public ConditionBuilder(List<ICondition> conditionIndexList)
        {
            this.conditionIndexList = conditionIndexList;
            this.conditionsMetList = new bool[this.conditionIndexList.Count];
        }

        public IConditionBuilder SetConditionMet(ICondition condition, bool met)
        {
            var index = this.GetIndex(condition);

            if (index == -1)
                return this;

            this.conditionsMetList[index] = met;

            return this;
        }

        private int GetIndex(ICondition condition)
        {
            for (var i = 0; i < this.conditionIndexList.Count; i++)
            {
                if (this.conditionIndexList[i] == condition)
                    return i;
            }

            return -1;
        }

        public bool[] Build()
        {
            return this.conditionsMetList;
        }

        public void Clear()
        {
            for (var i = 0; i < this.conditionsMetList.Length; i++)
            {
                this.conditionsMetList[i] = false;
            }
        }
    }
}