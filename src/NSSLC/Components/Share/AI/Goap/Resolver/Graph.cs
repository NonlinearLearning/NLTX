// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System.Collections.Generic;
using System.Linq;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Resolver
{
    public class Graph : IGraph
    {
        public List<INode> RootNodes { get; set; } = new();
        public List<INode> ChildNodes { get; set; } = new();
        public INode[] AllNodes => this.RootNodes.Union(this.ChildNodes).ToArray();
        public INode[] UnconnectedNodes { get; set; }
    }
}