namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonStyleDefinition
{
  public DungeonStyleDefinition(
    int brickTileType,
    int? brickGrassTileType,
    int brickCrackedTileType,
    int brickWallType,
    int windowGlassWallType,
    int windowClosedGlassWallType,
    int windowEdgeWallType)
  {
    BrickTileType = brickTileType;
    BrickGrassTileType = brickGrassTileType;
    BrickCrackedTileType = brickCrackedTileType;
    BrickWallType = brickWallType;
    WindowGlassWallType = windowGlassWallType;
    WindowClosedGlassWallType = windowClosedGlassWallType;
    WindowEdgeWallType = windowEdgeWallType;
  }

  public int BrickTileType { get; }

  public int? BrickGrassTileType { get; }

  public int BrickCrackedTileType { get; }

  public int BrickWallType { get; }

  public int WindowGlassWallType { get; }

  public int WindowClosedGlassWallType { get; }

  public int WindowEdgeWallType { get; }

  public bool TileIsInStyle(int tileType, bool includeCracked = true)
  {
    if (BrickGrassTileType.HasValue && tileType == BrickGrassTileType.Value)
    {
      return true;
    }

    if (includeCracked && tileType == BrickCrackedTileType)
    {
      return true;
    }

    return tileType == BrickTileType;
  }

  public bool WallIsInStyle(int wallType, bool includeWindows = false)
  {
    if (includeWindows &&
        (wallType == WindowGlassWallType ||
         wallType == WindowEdgeWallType ||
         wallType == WindowClosedGlassWallType))
    {
      return true;
    }

    return wallType == BrickWallType;
  }
}
