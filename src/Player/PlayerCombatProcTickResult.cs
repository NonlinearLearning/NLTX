namespace Terraria.Player;

public readonly record struct PlayerCombatProcTickResult(
  float GhostDmg,
  float LifeSteal,
  int EocDash,
  int EocHit,
  int InfernoCounter,
  int StarCloakCooldown,
  int TitaniumStormCooldown,
  int PetalTimer,
  int BoneGloveTimer);
