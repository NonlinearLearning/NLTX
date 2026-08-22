namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TreeGrowthDispatch(
  TreeGrowthDispatchKind Kind,
  LegacyTreeProfileKind? ProfileKind);
