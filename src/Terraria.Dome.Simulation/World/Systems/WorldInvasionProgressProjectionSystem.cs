using System;

using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public readonly record struct WorldInvasionProgressResult(
  bool IsAvailable,
  int Progress,
  int ProgressMax,
  int Icon,
  int Wave);

public sealed class WorldInvasionProgressProjectionSystem
{
  public WorldInvasionProgressResult Resolve(WorldProgressionState progression)
  {
    return Resolve(progression, progressWave: 0);
  }

  public WorldInvasionProgressResult Resolve(
    WorldProgressionState progression,
    int progressWave)
  {
    ArgumentNullException.ThrowIfNull(progression);

    if (progression.InvasionType is < 1 or > 4 ||
        progression.InvasionSizeStart <= 0 ||
        progression.InvasionSize < 0 ||
        progression.InvasionSize > progression.InvasionSizeStart)
    {
      return new WorldInvasionProgressResult(false, 0, 0, 0, 0);
    }

    return new WorldInvasionProgressResult(
      true,
      progression.InvasionSizeStart - progression.InvasionSize,
      progression.InvasionSizeStart,
      progression.InvasionType + 3,
      progressWave);
  }
}
