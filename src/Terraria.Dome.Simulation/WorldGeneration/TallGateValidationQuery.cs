using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TallGateValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int GateHeight = 5;
  private const ushort ClosedTallGateTileType = 388;
  private const ushort OpenTallGateTileType = 389;

  public static TallGateValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort tileType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (tileType is not (ClosedTallGateTileType or OpenTallGateTileType))
    {
      return new TallGateValidationResult(false, true, y, 0, false);
    }

    WorldTile source = snapshot.GetTile(x, y);
    int frameRow = source.FrameY / TileFrameWidth;
    int originY = y - frameRow % GateHeight;
    int styleBand = source.FrameX / TileFrameWidth;
    bool valid = true;
    for (int offsetY = 0; offsetY < GateHeight; offsetY++)
    {
      int tileY = originY + offsetY;
      WorldTile tile = snapshot.Metadata.IsInside(x, tileY)
        ? snapshot.GetTile(x, tileY)
        : default;
      valid &= tile.IsActive && tile.Type == tileType &&
        tile.FrameX == checked((short)(styleBand * TileFrameWidth)) &&
        tile.FrameY == checked((short)(offsetY * TileFrameWidth));
    }

    bool hasVerticalAnchors = snapshot.Metadata.IsInside(x, originY - 1) &&
      snapshot.Metadata.IsInside(x, originY + GateHeight) &&
      TileStateQuery.CanAttachToTop(snapshot, tileDefinitions, x, originY - 1) &&
      TileStateQuery.CanAttachToBottom(snapshot, tileDefinitions, x, originY + GateHeight);
    valid &= hasVerticalAnchors;
    return new TallGateValidationResult(valid, !valid, originY, styleBand, hasVerticalAnchors);
  }
}
