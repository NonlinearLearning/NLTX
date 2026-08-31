using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record TilePresenceScanResult(
  IReadOnlySet<ushort> ActiveTileTypes,
  IReadOnlySet<ushort> WallTypes,
  int ActiveTileCount,
  int WorldTileCount,
  int StartX,
  int EndExclusiveX,
  int StartY,
  int EndExclusiveY);
