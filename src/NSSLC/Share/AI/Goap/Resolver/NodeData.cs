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
    public struct NodeData
    {
        public int Index;

        // Cost of when using this node as a parent
        public float P;

        // Cost when performing this node
        public float G;

        // Heuristic
        public float H;
        public int ParentIndex;
        public Vector3 Position;

        public float F => this.G + this.H;
    }

}
