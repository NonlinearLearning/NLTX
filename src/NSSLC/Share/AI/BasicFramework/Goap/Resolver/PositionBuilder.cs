// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System.Collections.Generic;
using System.Linq;
using Terraria.Share.Ai.Goap.Core;

using System.Numerics;

namespace Terraria.Share.Ai.Goap.Resolver
{
    public class PositionBuilder : IPositionBuilder
    {
        private readonly List<IConnectable> actionIndexList;
        private Vector3[] executableList;

        public PositionBuilder(List<IConnectable> actionIndexList)
        {
            this.actionIndexList = actionIndexList;
            this.executableList = this.actionIndexList.Select(x => GraphResolverJob.InvalidPosition).ToArray();
        }

        public IPositionBuilder SetPosition(IConnectable action, Vector3? position)
        {
            var index = this.GetIndex(action);

            if (index == -1)
                return this;

            this.executableList[index] = position ?? GraphResolverJob.InvalidPosition;

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

        public Vector3[] Build()
        {
            return this.executableList;
        }

        public void Clear()
        {
            for (var i = 0; i < this.executableList.Length; i++)
            {
                this.executableList[i] = GraphResolverJob.InvalidPosition;
            }
        }
    }
}