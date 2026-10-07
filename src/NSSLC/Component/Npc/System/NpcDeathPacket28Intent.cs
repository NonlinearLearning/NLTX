namespace Terraria.Npc;

public readonly record struct NpcDeathPacket28Intent(
  NpcInstanceId TargetInstanceId,
  NpcSlot TargetLegacySlot,
  int Damage);
