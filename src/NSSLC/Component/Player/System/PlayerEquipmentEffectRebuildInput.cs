namespace Terraria.Player;

public readonly record struct PlayerEquipmentEffectRebuildInput(
  bool ArmorEffectDrawShadow,
  bool ArmorEffectDrawShadowSubtle,
  bool ArmorEffectDrawOutlines,
  bool ArmorEffectDrawShadowLokis,
  bool ArmorEffectDrawShadowBasilisk,
  bool ArmorEffectDrawOutlinesForbidden,
  bool ArmorEffectDrawShadowEocShield,
  bool SocialShadowRocketBoots,
  bool SocialGhost,
  bool AshWoodBonus,
  bool SocialIgnoreLight);
