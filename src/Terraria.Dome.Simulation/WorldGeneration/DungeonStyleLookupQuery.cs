using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class DungeonStyleLookupQuery
{
  public static bool TryFindByTile(
    IReadOnlyList<DungeonStyleLookupEntry> styles,
    int tileType,
    out DungeonStyleDefinition style)
  {
    for (int index = 0; index < styles.Count; index++)
    {
      DungeonStyleLookupEntry entry = styles[index];
      if (entry.Style.TileIsInStyle(tileType))
      {
        style = entry.Style;
        return true;
      }

      for (int subIndex = 0; subIndex < entry.SubStyles.Count; subIndex++)
      {
        DungeonStyleDefinition subStyle = entry.SubStyles[subIndex];
        if (subStyle.TileIsInStyle(tileType))
        {
          style = subStyle;
          return true;
        }
      }
    }

    style = default;
    return false;
  }

  public static bool TryFindByWall(
    IReadOnlyList<DungeonStyleLookupEntry> styles,
    int wallType,
    out DungeonStyleDefinition style)
  {
    for (int index = 0; index < styles.Count; index++)
    {
      DungeonStyleLookupEntry entry = styles[index];
      if (entry.Style.WallIsInStyle(wallType))
      {
        style = entry.Style;
        return true;
      }

      for (int subIndex = 0; subIndex < entry.SubStyles.Count; subIndex++)
      {
        DungeonStyleDefinition subStyle = entry.SubStyles[subIndex];
        if (subStyle.WallIsInStyle(wallType))
        {
          style = subStyle;
          return true;
        }
      }
    }

    style = default;
    return false;
  }
}
