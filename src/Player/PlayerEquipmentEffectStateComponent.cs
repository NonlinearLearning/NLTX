namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-727, P09-728, P09-729, P09-730, P09-731, P09-732, P09-733, P09-734, P09-735, P09-737, P09-738
// crossSubsystemOwner: effect rebuild order and renderer consumption remain integration-review
public sealed class PlayerEquipmentEffectStateComponent
{
  public bool ArmorEffectDrawShadow { get; internal set; }

  public bool ArmorEffectDrawShadowSubtle { get; internal set; }

  public bool ArmorEffectDrawOutlines { get; internal set; }

  public bool ArmorEffectDrawShadowLokis { get; internal set; }

  public bool ArmorEffectDrawShadowBasilisk { get; internal set; }

  public bool ArmorEffectDrawOutlinesForbidden { get; internal set; }

  public bool ArmorEffectDrawShadowEocShield { get; internal set; }

  public bool SocialShadowRocketBoots { get; internal set; }

  public bool SocialGhost { get; internal set; }

  public bool AshWoodBonus { get; internal set; }

  public bool SocialIgnoreLight { get; internal set; }
}
