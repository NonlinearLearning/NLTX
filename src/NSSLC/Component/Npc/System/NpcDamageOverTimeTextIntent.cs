namespace Terraria.Npc;

public readonly record struct NpcDamageOverTimeTextIntent(
  NpcInstanceId SourceInstanceId,
  NpcDamageOverTimeTextBounds Bounds,
  int Amount,
  bool Dramatic,
  bool IsDamageOverTime);
