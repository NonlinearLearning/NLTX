using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeSettingsValidationQuery
{
  public static TreeSettingsValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ushort treeType,
    Func<int, bool> isGroundValid)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(isGroundValid);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile tile = snapshot.GetTile(x, y);
    WorldTile left = GetTile(snapshot, x - 1, y);
    WorldTile right = GetTile(snapshot, x + 1, y);
    WorldTile below = GetTile(snapshot, x, y + 1);
    int belowType = below.IsActive ? below.Type : -1;
    bool groundValid = isGroundValid.Invoke(belowType);
    bool hasLeftTree = left.IsActive && left.Type == treeType;
    bool hasRightTree = right.IsActive && right.Type == treeType;
    bool supported = groundValid || belowType == treeType;
    return new TreeSettingsValidationResult(
      supported,
      !supported,
      treeType,
      belowType,
      groundValid,
      hasLeftTree,
      hasRightTree,
      tile.FrameX,
      tile.FrameY,
      true,
      true);
  }

  private static WorldTile GetTile(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
  }
}
