// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System.Collections.Generic;

namespace Terraria.Ai.Goap.Core
{
    public interface IGraph
    {
        List<INode> RootNodes { get; set; }
        List<INode> ChildNodes { get; set; }
        INode[] AllNodes { get; }
        INode[] UnconnectedNodes { get; }
    }
}