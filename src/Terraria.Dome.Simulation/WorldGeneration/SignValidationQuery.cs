using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SignValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int SignWidth = 2;
  private const int SignHeight = 2;
  private const ushort BottomMountedSignType = 85;

  public static SignValidationResult Evaluate(
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

    WorldTile source = snapshot.GetTile(x, y);
    int frameColumn = source.FrameX / TileFrameWidth;
    int frameRow = source.FrameY / TileFrameWidth;
    int originX = x - frameColumn % SignWidth;
    int originY = y - frameRow % SignHeight;
    int style = frameColumn / SignWidth;
    int attachmentStyle = frameColumn % 5;
    if (originX < 1 || originY < 1 ||
        originX + SignWidth >= snapshot.Metadata.Width - 1 ||
        originY + SignHeight >= snapshot.Metadata.Height - 1)
    {
      return new SignValidationResult(false, true, originX, originY, attachmentStyle, false);
    }
    bool valid = true;
    for (int offsetX = 0; offsetX < SignWidth; offsetX++)
    {
      for (int offsetY = 0; offsetY < SignHeight; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        WorldTile tile = snapshot.Metadata.IsInside(tileX, tileY)
          ? snapshot.GetTile(tileX, tileY)
          : default;
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)(style * 36 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(frameRow / SignHeight * 36 + offsetY * TileFrameWidth));
      }
    }

    bool hasAttachment = tileType == BottomMountedSignType
      ? HasBottomSupport(snapshot, tileDefinitions, originX, originY)
      : HasOrientedAttachment(
        snapshot,
        tileDefinitions,
        originX,
        originY,
        attachmentStyle);
    valid &= hasAttachment;
    return new SignValidationResult(
      valid,
      !valid,
      originX,
      originY,
      attachmentStyle,
      hasAttachment);
  }

  private static bool HasBottomSupport(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry definitions,
    int x,
    int y)
  {
    return TileStateQuery.IsSolidAllowingBottomSlope(snapshot, definitions, x, y + SignHeight) &&
      TileStateQuery.IsSolidAllowingBottomSlope(snapshot, definitions, x + 1, y + SignHeight);
  }

  private static bool HasOrientedAttachment(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry definitions,
    int x,
    int y,
    int attachmentStyle)
  {
    return attachmentStyle switch
    {
      0 => TileStateQuery.CanAttachToTop(snapshot, definitions, x, y + SignHeight) &&
        TileStateQuery.CanAttachToTop(snapshot, definitions, x + 1, y + SignHeight),
      1 => TileStateQuery.CanAttachToBottom(snapshot, definitions, x, y - 1) &&
        TileStateQuery.CanAttachToBottom(snapshot, definitions, x + 1, y - 1),
      2 => TileStateQuery.CanAttachToRight(snapshot, definitions, x - 1, y) &&
        TileStateQuery.CanAttachToRight(snapshot, definitions, x - 1, y + 1),
      3 => TileStateQuery.CanAttachToLeft(snapshot, definitions, x + SignWidth, y) &&
        TileStateQuery.CanAttachToLeft(snapshot, definitions, x + SignWidth, y + 1),
      4 => HasWall(snapshot, x, y),
      _ => false
    };
  }

  private static bool HasWall(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.GetTile(x, y).WallType > 0 &&
      snapshot.GetTile(x + 1, y).WallType > 0 &&
      snapshot.GetTile(x, y + 1).WallType > 0 &&
      snapshot.GetTile(x + 1, y + 1).WallType > 0;
  }
}
