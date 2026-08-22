namespace Terraria.Dome.Simulation.WorldGeneration;

public static class DungeonChestQuery
{
  private const ushort ChestTileType = 21;
  private const ushort LockedBiomeChestTileType = 467;
  private const int FirstBiomeChestStyle = 23;
  private const int LastBiomeChestStyle = 27;
  private const int LockedBiomeChestStyle = 13;

  public static bool IsLockedBiomeChest(ushort chestType, int chestStyle)
  {
    return chestType switch
    {
      ChestTileType => chestStyle >= FirstBiomeChestStyle && chestStyle <= LastBiomeChestStyle,
      LockedBiomeChestTileType => chestStyle == LockedBiomeChestStyle,
      _ => false
    };
  }
}
