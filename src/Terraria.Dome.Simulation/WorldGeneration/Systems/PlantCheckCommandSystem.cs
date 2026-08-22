using System;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public static class PlantCheckCommandSystem
{
  public static bool TryCreateCommand(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    long sequence,
    out TileChangeCommand command)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentOutOfRangeException.ThrowIfNegative(sequence);

    int clampedX = Math.Clamp(x, 1, snapshot.Metadata.Width - 2);
    int clampedY = Math.Clamp(y, 1, snapshot.Metadata.Height - 2);
    PlantCheckResult result = PlantCheckQuery.Evaluate(
      snapshot,
      tileDefinitions,
      clampedX,
      clampedY);
    if (result.ShouldDestroy)
    {
      command = new TileChangeCommand(
        sequence,
        clampedX,
        clampedY,
        TileChangeKind.Kill,
        TileType: 0);
      return true;
    }

    if (result.ShouldConvert)
    {
      WorldTile sourceTile = snapshot.GetTile(clampedX, clampedY);
      command = new TileChangeCommand(
        sequence,
        clampedX,
        clampedY,
        TileChangeKind.UpdateTileType,
        (ushort)result.Conversion.TileType,
        FrameX: result.Conversion.FrameX,
        FrameY: sourceTile.FrameY);
      return true;
    }

    command = default;
    return false;
  }
}
