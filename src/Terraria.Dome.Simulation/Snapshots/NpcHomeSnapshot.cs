namespace Terraria.Dome.Simulation;

public readonly record struct NpcHomeSnapshot(
  int NpcId,
  short TileX,
  short TileY,
  bool IsHomeless);
