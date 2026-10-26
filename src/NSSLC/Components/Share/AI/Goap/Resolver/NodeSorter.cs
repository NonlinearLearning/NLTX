// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: synchronous .NET collections and System.Numerics; see SOURCES.md.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Terraria.Ai.Goap.Resolver
{
    public struct NodeSorter : IComparer<NodeData>
    {
        public int Compare(NodeData x, NodeData y)
        {
            return x.F.CompareTo(y.F);
        }
    }

}
