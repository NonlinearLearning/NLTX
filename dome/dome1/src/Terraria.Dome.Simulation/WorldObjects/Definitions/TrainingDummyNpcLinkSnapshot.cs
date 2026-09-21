namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TrainingDummyNpcLinkSnapshot(
  bool IsActive,
  int NpcType,
  int AiTileX,
  int AiTileY);
