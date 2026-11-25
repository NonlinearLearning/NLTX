// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using Terraria.Share.Ai.Goap.Core;

using System.Numerics;

namespace Terraria.Share.Ai.Goap.Resolver
{
    public interface IPositionBuilder
    {
        IPositionBuilder SetPosition(IConnectable action, Vector3? position);
        Vector3[] Build();
        void Clear();
    }
}