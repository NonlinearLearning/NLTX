// Adapted from CrashKonijn/GOAP ResolveHandle; see LICENSE.txt and SOURCES.md.
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Resolver;

public sealed class JobResult {
  public IConnectable[] Actions { get; init; } = [];
  public IGoal? Goal { get; init; }
}
