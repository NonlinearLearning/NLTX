using System;

using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public readonly record struct WorldInvasionProgressResult(
  bool IsAvailable,
  int Progress,
  int ProgressMax,
  int Icon);

public sealed class WorldInvasionProgressProjectionSystem
{
  public WorldInvasionProgressResult Resolve(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);

    if (progression.InvasionType is < 1 or > 4 ||
        progression.InvasionSizeStart <= 0 ||
        progression.InvasionSize < 0 ||
        progression.InvasionSize > progression.InvasionSizeStart)
    {
      return new WorldInvasionProgressResult(false, 0, 0, 0);
    }

    return new WorldInvasionProgressResult(
      true,
      progression.InvasionSizeStart - progression.InvasionSize,
      progression.InvasionSizeStart,
      progression.InvasionType + 3);
  }
}
