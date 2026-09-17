namespace Terraria.Player;

public readonly record struct PlayerCombatProcSnapshot(
  float LifeSteal,
  float GhostDmg,
  int EocDash,
  int EocHit,
  int InfernoCounter,
  int StarCloakCooldown,
  bool OnHitDodge,
  bool OnHitRegen,
  bool OnHitPetal,
  bool OnHitTitaniumStorm,
  int TitaniumStormCooldown,
  bool HasTitaniumStormBuff,
  int PetalTimer,
  int BoneGloveTimer,
  int PhantomPhoneixCounter);
