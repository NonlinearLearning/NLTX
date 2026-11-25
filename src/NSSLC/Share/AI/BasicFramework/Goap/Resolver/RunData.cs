// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: synchronous .NET collections and System.Numerics; see SOURCES.md.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Terraria.Share.Ai.Goap.Resolver
{
    public struct RunData
    {
        public int[] StartIndex;

        public Vector3 AgentPosition;

        // Index = NodeIndex
        public bool[] IsEnabled;

        public bool[] IsExecutable;

        // Index = ConditionIndex
        public bool[] ConditionsMet;
        public Vector3[] Positions;
        public float[] Costs;
        public float DistanceMultiplier;
    }

}
