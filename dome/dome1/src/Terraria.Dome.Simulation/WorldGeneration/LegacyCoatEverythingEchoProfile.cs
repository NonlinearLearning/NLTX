using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyCoatEverythingEchoProfile(
  IReadOnlySet<ushort> SelectiveTileTypes,
  IReadOnlySet<ushort> BoulderTileTypes,
  IReadOnlySet<ushort> SolidTileTypes);
