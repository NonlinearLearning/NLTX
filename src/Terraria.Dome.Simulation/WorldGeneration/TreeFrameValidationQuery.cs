using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeFrameValidationQuery
{
  public static TreeFrameValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ushort treeType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile tile = snapshot.GetTile(x, y);
    WorldTile left = GetTile(snapshot, x - 1, y);
    WorldTile right = GetTile(snapshot, x + 1, y);
    WorldTile below = GetTile(snapshot, x, y + 1);
    bool hasLeftTree = left.IsActive && left.Type == treeType;
    bool hasRightTree = right.IsActive && right.Type == treeType;
    int groundType = NormalizeGroundType(below);
    bool supported = groundType == 2 || groundType == treeType;
    short suggestedFrameX = tile.FrameX;
    short suggestedFrameY = tile.FrameY;
    int frameRemainder = tile.FrameY % 66;
    if (IsBranchFrame(tile))
    {
      if (hasLeftTree && hasRightTree)
      {
        suggestedFrameX = 110;
        suggestedFrameY = (short)(66 + frameRemainder);
      }
      else if (hasLeftTree)
      {
        suggestedFrameX = 88;
        suggestedFrameY = (short)frameRemainder;
      }
      else if (hasRightTree)
      {
        suggestedFrameX = 66;
        suggestedFrameY = (short)(66 + frameRemainder);
      }
      else
      {
        suggestedFrameX = 0;
        suggestedFrameY = (short)frameRemainder;
      }
    }

    return new TreeFrameValidationResult(
      supported,
      !supported,
      tile.FrameX,
      tile.FrameY,
      suggestedFrameX,
      suggestedFrameY,
      groundType,
      hasLeftTree,
      hasRightTree,
      true,
      true);
  }

  private static WorldTile GetTile(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
  }

  private static int NormalizeGroundType(WorldTile tile)
  {
    if (!tile.IsActive)
    {
      return -1;
    }

    return tile.Type is 23 or 60 or 70 or 109 or 147 or 199 or 234 or 477 or 492 or 661 or 662
      ? 2
      : tile.Type;
  }

  private static bool IsBranchFrame(WorldTile tile)
  {
    return (tile.FrameX is 66 or 110 or 132) ||
      ((tile.FrameX is 0 or 66 or 88) && tile.FrameY is >= 132 and <= 176);
  }
}
