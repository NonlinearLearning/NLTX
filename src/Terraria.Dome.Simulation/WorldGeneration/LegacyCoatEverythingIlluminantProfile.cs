using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyCoatEverythingIlluminantProfile(
  IReadOnlySet<ushort> SelectiveTileTypes);
