namespace Terraria.Player;

public readonly record struct PlayerCombatProcHitEffects(
  float GhostDamage,
  float LifeStealCost,
  bool OnHitDodge,
  bool OnHitRegen,
  bool OnHitPetal,
  bool OnHitTitaniumStorm);
