namespace Terraria.Player;

public readonly record struct PlayerAbilityAndPresentationPropertiesSnapshot(
  bool UsingBiomeTorches,
  bool UsingSuperCart,
  float BowEffectiveDamage,
  float GunEffectiveDamage,
  float SpecialistEffectiveDamage,
  bool CanUseBootFlyingAbilities,
  bool CanUseWingAbilities,
  bool ShouldNotDraw,
  int TalkNpc,
  bool IsLockedToATile,
  bool PortalPhysicsEnabled,
  bool MountFishronSpecial);
