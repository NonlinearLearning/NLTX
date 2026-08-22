using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileBreakabilitySurvivalQuery
{
  private const ushort DisplayDollTileType = 470;
  private const ushort HatRackTileType = 475;

  public static TileBreakabilitySurvivalResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    bool chestCanBeDestroyed,
    bool displayDollCanBreak,
    bool hatRackCanBreak)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile tile = snapshot.GetTile(x, y);
    bool isChestLike = LegacyChestOriginQuery.TryGetOrigin(
      tile,
      x,
      y,
      out int originX,
      out int originY);
    bool isDisplayDoll = tile.Type == DisplayDollTileType;
    bool isHatRack = tile.Type == HatRackTileType;
    bool shouldSurvive = (isChestLike && !chestCanBeDestroyed) ||
      (isDisplayDoll && !displayDollCanBreak) ||
      (isHatRack && !hatRackCanBreak);
    return new TileBreakabilitySurvivalResult(
      shouldSurvive,
      isChestLike,
      isDisplayDoll,
      isHatRack,
      originX,
      originY);
  }
}
