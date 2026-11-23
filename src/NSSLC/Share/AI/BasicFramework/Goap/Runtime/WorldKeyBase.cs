// Adapted from CrashKonijn/GOAP WorldKeyBase; see LICENSE.txt and SOURCES.md.
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Runtime;

public abstract class WorldKeyBase : IWorldKey {
  // Distinct types get distinct graph keys and sensor slots.
  public string Name => GetType().FullName!;
}
