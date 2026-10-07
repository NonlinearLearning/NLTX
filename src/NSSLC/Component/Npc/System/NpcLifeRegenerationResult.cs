namespace Terraria.Npc;

public readonly record struct NpcLifeRegenerationResult(
  bool Computed,
  NpcLifeRegenerationFailureReason FailureReason,
  int LifeRegenerationAfter,
  int LifeRegenerationCountBefore,
  int LifeRegenerationCountAfter,
  int HealingPoints,
  int EffectiveDamagePerSecond,
  int DamagePerEvent,
  int DamageEventCount,
  bool EelWhipDotBehaviorUnknown)
{
  public bool HasHealing => HealingPoints > 0;

  public bool HasDamage => DamageEventCount > 0;
}
