using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class VineFrameQuery
{
  public static VineFrameResult Evaluate(WorldGridSnapshot snapshot, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile vine = snapshot.GetTile(x, y);
    if (!vine.IsActive || y == 0)
    {
      return new VineFrameResult(false, true, null);
    }

    WorldTile support = snapshot.GetTile(x, y - 1);
    int supportTileType = support.IsActive && !HasBottomSlope(support) ? support.Type : -1;
    if (vine.Type == supportTileType)
    {
      return new VineFrameResult(true, false, null);
    }

    ushort? replacementTileType = GetReplacementTileType(vine.Type, supportTileType);
    if (replacementTileType is not null && replacementTileType != vine.Type)
    {
      return new VineFrameResult(false, false, replacementTileType);
    }

    return IsUnsupported(vine.Type, supportTileType)
      ? new VineFrameResult(false, true, null)
      : new VineFrameResult(true, false, null);
  }

  private static ushort? GetReplacementTileType(ushort vineTileType, int supportTileType)
  {
    return supportTileType switch
    {
      60 or 62 or 226 => 62,
      70 or 528 => 528,
      109 or 115 or 492 => 115,
      199 or 205 or 662 => 205,
      23 or 636 or 661 => 636,
      633 or 638 => 638,
      382 => 382,
      2 or 52 or 477 when vineTileType != 382 => 52,
      _ => null
    };
  }

  private static bool HasBottomSlope(WorldTile tile)
  {
    return tile.Slope is 3 or 4;
  }

  private static bool IsUnsupported(ushort vineTileType, int supportTileType)
  {
    if (supportTileType == -1)
    {
      return true;
    }

    return vineTileType switch
    {
      52 or 382 => supportTileType is not (2 or 192 or 477),
      62 => supportTileType is not (60 or 226 or 384),
      115 => supportTileType is not (109 or 492),
      205 => supportTileType is not (199 or 662),
      528 => supportTileType != 70,
      636 => supportTileType is not (23 or 661),
      638 => supportTileType != 633,
      _ => false
    };
  }
}
