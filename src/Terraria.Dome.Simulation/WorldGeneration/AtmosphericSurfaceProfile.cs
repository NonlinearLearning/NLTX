namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct AtmosphericSurfaceProfile(
  int WorldHeight,
  int WorldSurfaceY,
  int RockLayerY,
  bool IsRemixWorld);
