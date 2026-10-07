namespace Terraria.Npc;

public readonly record struct NpcSpawnEntityRequest(
  NpcTypeId Type,
  int PositionX,
  int PositionY,
  int StartIndex,
  float Ai0,
  float Ai1,
  float Ai2,
  float Ai3,
  int Target);
