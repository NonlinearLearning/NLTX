using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record TileSolidityOverrideProjection(
  bool Solid,
  IReadOnlyList<ushort> BoulderTileTypes,
  IReadOnlyList<ushort> CrackedBrickTileTypes,
  bool LegacyRegistryMutationDeferred);
