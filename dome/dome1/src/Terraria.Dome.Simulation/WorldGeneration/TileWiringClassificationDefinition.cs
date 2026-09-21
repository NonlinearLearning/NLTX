namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileWiringClassificationDefinition(
  ushort TileType,
  bool IsMechanism,
  bool IgnoreWhenValidatingTraps,
  bool IsTrigger);
