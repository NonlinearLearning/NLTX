using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class LegacyEvilReplacementDefinitions
{
  private static readonly IReadOnlySet<ushort> DefaultCrackedBrickTileTypes = new HashSet<ushort>
  {
    481, 482, 483
  };
  private static readonly IReadOnlySet<ushort> DefaultDungeonTileTypes = new HashSet<ushort>
  {
    41, 43, 44, 677, 678, 679
  };
  private static readonly IReadOnlySet<ushort> DefaultDungeonWallTypes = new HashSet<ushort>
  {
    7, 8, 9, 94, 95, 96, 97, 98, 99
  };

  public LegacyEvilReplacementDefinitions(
    IReadOnlySet<ushort> dungeonTileTypes,
    IReadOnlySet<ushort> crackedBrickTileTypes,
    IReadOnlySet<ushort> dungeonWallTypes)
  {
    ArgumentNullException.ThrowIfNull(dungeonTileTypes);
    ArgumentNullException.ThrowIfNull(crackedBrickTileTypes);
    ArgumentNullException.ThrowIfNull(dungeonWallTypes);
    DungeonTileTypes = dungeonTileTypes;
    CrackedBrickTileTypes = crackedBrickTileTypes;
    DungeonWallTypes = dungeonWallTypes;
  }

  public IReadOnlySet<ushort> CrackedBrickTileTypes { get; }

  public IReadOnlySet<ushort> DungeonTileTypes { get; }

  public IReadOnlySet<ushort> DungeonWallTypes { get; }

  public static LegacyEvilReplacementDefinitions CreateDefault()
  {
    return new LegacyEvilReplacementDefinitions(
      DefaultDungeonTileTypes,
      DefaultCrackedBrickTileTypes,
      DefaultDungeonWallTypes);
  }
}

public static class LegacyEvilReplacementQuery
{
  public static bool CanReplace(WorldTile tile, LegacyEvilReplacementDefinitions definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (tile.IsActive && (definitions.DungeonTileTypes.Contains(tile.Type) ||
                          definitions.CrackedBrickTileTypes.Contains(tile.Type)))
    {
      return false;
    }

    return !definitions.DungeonWallTypes.Contains(tile.WallType);
  }
}
