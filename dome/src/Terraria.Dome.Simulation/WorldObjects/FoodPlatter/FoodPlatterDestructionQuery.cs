using System;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldObjects.FoodPlatter;

public static class FoodPlatterDestructionQuery
{
  private const ushort FoodPlatterTileType = 520;

  public static FoodPlatterDestructionResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    FoodPlatterSnapshot platter)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(platter.TileX, platter.TileY))
    {
      throw new ArgumentOutOfRangeException(nameof(platter));
    }

    WorldTile tile = snapshot.GetTile(platter.TileX, platter.TileY);
    if (!tile.IsActive || tile.Type != FoodPlatterTileType ||
        TileStateQuery.IsSolidAllowingBottomSlope(
          snapshot,
          tileDefinitions,
          platter.TileX,
          platter.TileY + 1))
    {
      return default;
    }

    return new FoodPlatterDestructionResult(
      true,
      platter.Exists ? platter.EntityId : 0,
      platter.TileX,
      platter.TileY,
      platter.Exists ? platter.StoredItem : ItemStack.Empty);
  }
}
