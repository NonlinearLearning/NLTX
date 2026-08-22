using System;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public static class CactusFrameCommandSystem
{
  public static bool TryCreateCommand(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    long sequence,
    out TileChangeCommand command)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentOutOfRangeException.ThrowIfNegative(sequence);
    CactusFrameResult result = CactusFrameQuery.Evaluate(snapshot, x, y);
    if (result.ShouldKill)
    {
      command = new TileChangeCommand(sequence, x, y, TileChangeKind.Kill, TileType: 0);
      return true;
    }

    command = default;
    return false;
  }
}
