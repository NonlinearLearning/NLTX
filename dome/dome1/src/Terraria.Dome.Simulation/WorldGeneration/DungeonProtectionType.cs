namespace Terraria.Dome.Simulation.WorldGeneration;

public enum DungeonProtectionType
{
  None,
  Tiles,
  Walls,
  TilesAndWalls
}

public static class DungeonProtectionTypeQuery
{
  public static bool ProtectsTiles(DungeonProtectionType protectionType)
  {
    return protectionType is DungeonProtectionType.Tiles or DungeonProtectionType.TilesAndWalls;
  }

  public static bool ProtectsWalls(DungeonProtectionType protectionType)
  {
    return protectionType is DungeonProtectionType.Walls or DungeonProtectionType.TilesAndWalls;
  }
}
