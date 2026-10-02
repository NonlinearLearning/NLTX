namespace Terraria.Content;

public sealed record ProjectileCapabilitiesDefinition(
  bool IsArrow,
  bool IsBobber,
  bool IsMinion,
  bool IsSentry,
  bool IsHook = false)
{
  public bool IsCounterweight { get; init; }

  public float MinionSlots { get; init; }

  public bool IsOwnerHitCheck { get; init; }

  public bool UsesOwnerMeleeHitCooldown { get; init; }

  public bool UsesOwnerLight { get; init; }

  public bool NoEnchantments { get; init; }

  public bool NoEnchantmentVisuals { get; init; }
}
