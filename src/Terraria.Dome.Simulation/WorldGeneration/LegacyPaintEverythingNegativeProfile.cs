using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyPaintEverythingNegativeProfile(
  IReadOnlySet<ushort> DungeonTileTypes,
  IReadOnlySet<ushort> CrackedBrickTileTypes,
  IReadOnlySet<ushort> DungeonWallTypes,
  IReadOnlySet<ushort> CloudTileTypes,
  IReadOnlySet<ushort> VineTileTypes,
  IReadOnlySet<ushort> CeilingNeighborTileTypes);
