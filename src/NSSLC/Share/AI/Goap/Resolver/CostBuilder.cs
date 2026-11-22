// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System.Collections.Generic;
using System.Linq;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Resolver
{
    public class CostBuilder : ICostBuilder
    {
        private readonly List<IConnectable> actionIndexList;
        private float[] costList;

        public CostBuilder(List<IConnectable> actionIndexList)
        {
            this.actionIndexList = actionIndexList;
            this.costList = this.actionIndexList.Select(x => 1f).ToArray();
        }

        public ICostBuilder SetCost(IConnectable action, float cost)
        {
            var index = this.GetIndex(action);

            if (index == -1)
                return this;

            this.costList[index] = cost;

            return this;
        }

        private int GetIndex(IConnectable condition)
        {
            for (var i = 0; i < this.actionIndexList.Count; i++)
            {
                if (this.actionIndexList[i] == condition)
                    return i;
            }

            return -1;
        }

        public float[] Build()
        {
            return this.costList;
        }

        public void Clear()
        {
            for (var i = 0; i < this.costList.Length; i++)
            {
                this.costList[i] = 1f;
            }
        }
    }
}