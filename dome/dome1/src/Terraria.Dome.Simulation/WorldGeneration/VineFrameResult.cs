namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct VineFrameResult(
  bool ShouldKeep,
  bool ShouldKill,
  ushort? ReplacementTileType);
