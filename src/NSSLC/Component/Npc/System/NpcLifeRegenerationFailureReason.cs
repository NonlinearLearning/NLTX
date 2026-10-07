namespace Terraria.Npc;

public enum NpcLifeRegenerationFailureReason : byte
{
  None,
  DamageProtected,
  InvalidInput,
  ProjectileSnapshotRequired,
  TownNpcDamageMultiplierRequired,
  InvalidTownNpcDamageMultiplier,
  ArithmeticOverflow,
}
