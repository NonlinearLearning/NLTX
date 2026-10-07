namespace Terraria.Npc;

public readonly record struct NpcStrikeResult(
  NpcCombatResult CombatResult,
  NpcKnockbackResult KnockbackResult);
