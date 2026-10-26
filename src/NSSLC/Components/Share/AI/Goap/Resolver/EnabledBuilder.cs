// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System.Collections.Generic;
using System.Linq;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Resolver
{
    public class EnabledBuilder : IEnabledBuilder
    {
        private readonly List<IConnectable> actionIndexList;
        private bool[] enabledList;

        public EnabledBuilder(List<IConnectable> actionIndexList)
        {
            this.actionIndexList = actionIndexList;
            this.enabledList = this.actionIndexList.Select(x => true).ToArray();
        }

        public IEnabledBuilder SetEnabled(IConnectable action, bool executable)
        {
            var index = this.GetIndex(action);

            if (index == -1)
                return this;

            this.enabledList[index] = executable;

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

        public void Clear()
        {
            for (var i = 0; i < this.enabledList.Length; i++)
            {
                this.enabledList[i] = true;
            }
        }

        public bool[] Build()
        {
            return this.enabledList;
        }
    }
}