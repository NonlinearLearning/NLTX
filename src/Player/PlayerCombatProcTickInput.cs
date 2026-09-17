namespace Terraria.Player;

public readonly record struct PlayerCombatProcTickInput(
  float GhostDmg,
  float LifeSteal,
  int EocDash,
  int EocHit,
  int InfernoCounter,
  int StarCloakCooldown,
  int TitaniumStormCooldown,
  int PetalTimer,
  int BoneGloveTimer,
  bool ExpertMode);
