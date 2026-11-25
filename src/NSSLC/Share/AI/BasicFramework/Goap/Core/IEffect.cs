// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
namespace Terraria.Share.Ai.Goap.Core
{
    public interface IEffect
    {
        public IWorldKey WorldKey { get; }
        public bool Increase { get; }
    }
}