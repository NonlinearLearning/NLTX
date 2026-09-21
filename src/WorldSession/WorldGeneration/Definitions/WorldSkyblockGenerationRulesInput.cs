using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSkyblockGenerationRulesInput
{
  public WorldSkyblockGenerationRulesInput(
    WorldSkyblockGenerationScanSnapshot scan,
    IReadOnlySet<ushort> dungeonTileTypes,
    IReadOnlySet<ushort> dungeonWallTypes,
    bool skyblockWorld)
  {
    ArgumentNullException.ThrowIfNull(dungeonTileTypes);
    ArgumentNullException.ThrowIfNull(dungeonWallTypes);
    Scan = scan;
    DungeonTileTypes = new HashSet<ushort>(dungeonTileTypes).ToFrozenSet();
    DungeonWallTypes = new HashSet<ushort>(dungeonWallTypes).ToFrozenSet();
    SkyblockWorld = skyblockWorld;
  }

  public WorldSkyblockGenerationScanSnapshot Scan { get; }

  public IReadOnlySet<ushort> DungeonTileTypes { get; }

  public IReadOnlySet<ushort> DungeonWallTypes { get; }

  public bool SkyblockWorld { get; }
}
