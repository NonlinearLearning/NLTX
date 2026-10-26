// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System.Collections.Generic;
using System.Linq;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Resolver
{
    public class ExecutableBuilder : IExecutableBuilder
    {
        private readonly List<IConnectable> actionIndexList;
        private bool[] executableList;

        public ExecutableBuilder(List<IConnectable> actionIndexList)
        {
            this.actionIndexList = actionIndexList;
            this.executableList = this.actionIndexList.Select(x => false).ToArray();
        }

        public IExecutableBuilder SetExecutable(IConnectable action, bool executable)
        {
            var index = this.GetIndex(action);

            if (index == -1)
                return this;

            this.executableList[index] = executable;

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
            for (var i = 0; i < this.executableList.Length; i++)
            {
                this.executableList[i] = false;
            }
        }

        public bool[] Build()
        {
            return this.executableList;
        }
    }
}