using System;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public static class VineFrameCommandSystem
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
    VineFrameResult result = VineFrameQuery.Evaluate(snapshot, x, y);
    if (result.ShouldKill)
    {
      command = new TileChangeCommand(sequence, x, y, TileChangeKind.Kill, TileType: 0);
      return true;
    }

    if (result.ReplacementTileType is ushort replacementTileType)
    {
      WorldTile sourceTile = snapshot.GetTile(x, y);
      command = new TileChangeCommand(
        sequence,
        x,
        y,
        TileChangeKind.UpdateTileType,
        replacementTileType,
        FrameX: sourceTile.FrameX,
        FrameY: sourceTile.FrameY);
      return true;
    }

    command = default;
    return false;
  }
}
