using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyOreTierSelectionPolicy
{
  private const ushort CopperAlternativeTileType = 166;
  private const ushort GoldAlternativeTileType = 169;
  private const ushort IronAlternativeTileType = 167;
  private const ushort SilverAlternativeTileType = 168;

  public static LegacyOreTierSelection Select(
    bool isDontStarveWorld,
    bool isDrunkWorld,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    SavedOreTierDefaults defaults = SavedOreTierDefaults.Version4;
    ushort copper = random.Next(2) == 0 ? CopperAlternativeTileType : defaults.CopperTileType;
    ushort iron = SelectIronOrGoldTier(
      isDontStarveWorld, isDrunkWorld, IronAlternativeTileType, defaults.IronTileType, random);
    ushort silver = random.Next(2) == 0 ? SilverAlternativeTileType : defaults.SilverTileType;
    ushort gold = SelectIronOrGoldTier(
      isDontStarveWorld, isDrunkWorld, GoldAlternativeTileType, defaults.GoldTileType, random);
    return new LegacyOreTierSelection(copper, iron, silver, gold);
  }

  private static ushort SelectIronOrGoldTier(
    bool isDontStarveWorld,
    bool isDrunkWorld,
    ushort alternativeTileType,
    ushort defaultTileType,
    LegacyPassRandomState random)
  {
    if ((!isDontStarveWorld || isDrunkWorld) && random.Next(2) == 0)
    {
      return alternativeTileType;
    }

    return defaultTileType;
  }
}
