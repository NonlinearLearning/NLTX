namespace Terraria.Npc;

public readonly record struct NpcDeathPhaseSpawnIntent(
  NpcInstanceId SourceNpcInstanceId,
  NpcTypeId NpcType,
  int PositionX,
  int PositionY,
  float Ai3);
