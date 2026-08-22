using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileAnchorValidationQuery
{
  public static TileAnchorValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int originX,
    int originY,
    int width,
    int height,
    int mode,
    TileAnchorKind anchor)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    int checkedTiles = 0;
    int failedTiles = 0;
    int checkedSides = 0;
    bool valid = true;
    if ((mode & 1) != 0)
    {
      checkedSides |= 1;
      for (int offsetY = 0; offsetY < height; offsetY++)
      {
        valid &= Check(
          snapshot,
          tileDefinitions,
          originX - 1,
          originY + offsetY,
          anchor,
          ref checkedTiles,
          ref failedTiles);
        valid &= Check(
          snapshot,
          tileDefinitions,
          originX + width,
          originY + offsetY,
          anchor,
          ref checkedTiles,
          ref failedTiles);
      }
    }

    if ((mode & 2) != 0)
    {
      checkedSides |= 2;
      for (int offsetX = 0; offsetX < width; offsetX++)
      {
        valid &= Check(
          snapshot,
          tileDefinitions,
          originX + offsetX,
          originY - 1,
          anchor,
          ref checkedTiles,
          ref failedTiles);
        valid &= Check(
          snapshot,
          tileDefinitions,
          originX + offsetX,
          originY + height,
          anchor,
          ref checkedTiles,
          ref failedTiles);
      }
    }

    return new TileAnchorValidationResult(valid, !valid, checkedSides, checkedTiles, failedTiles);
  }

  public static bool IsValid(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    TileAnchorKind anchor)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    WorldTile tile = snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
    return EvaluateTile(tile, snapshot, tileDefinitions, x, y, anchor);
  }

  private static bool Check(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry definitions,
    int x,
    int y,
    TileAnchorKind anchor,
    ref int checkedTiles,
    ref int failedTiles)
  {
    checkedTiles++;
    WorldTile tile = snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
    bool valid = EvaluateTile(tile, snapshot, definitions, x, y, anchor);
    if (!valid)
    {
      failedTiles++;
    }

    return valid;
  }

  private static bool EvaluateTile(
    WorldTile tile,
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry definitions,
    int x,
    int y,
    TileAnchorKind anchor)
  {
    if ((anchor & TileAnchorKind.EmptyTile) != 0 && !tile.IsActive)
    {
      return true;
    }

    if (!snapshot.Metadata.IsInside(x, y))
    {
      return false;
    }

    bool valid = false;
    if ((anchor & TileAnchorKind.SolidTile) != 0)
    {
      valid |= TileStateQuery.CanAttachToBottom(snapshot, definitions, x, y);
    }

    if ((anchor & TileAnchorKind.SolidBottom) != 0)
    {
      valid |= TileStateQuery.IsSolidAllowingBottomSlope(tile, definitions);
    }

    if ((anchor & TileAnchorKind.SolidWithTop) != 0)
    {
      valid |= TileStateQuery.CanAttachToTop(snapshot, definitions, x, y);
    }

    if ((anchor & TileAnchorKind.SolidSide) != 0)
    {
      valid |= TileStateQuery.CanAttachToLeft(snapshot, definitions, x, y) ||
        TileStateQuery.CanAttachToRight(snapshot, definitions, x, y);
    }

    return valid;
  }
}
