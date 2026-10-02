namespace Terraria.Content.Items;

public sealed class ItemCapabilitySnapshot
{
  public ItemCapabilitySnapshot(
    ItemCombatCapabilityDefinition combat,
    ItemProjectileUseDefinition projectileUse,
    ItemResourceRecoveryDefinition resourceRecovery,
    ItemDamageClassCapability damageClass,
    ItemPresentationDefinition presentation)
  {
    ArgumentNullException.ThrowIfNull(combat);
    ArgumentNullException.ThrowIfNull(projectileUse);
    ArgumentNullException.ThrowIfNull(resourceRecovery);
    ArgumentNullException.ThrowIfNull(damageClass);
    ArgumentNullException.ThrowIfNull(presentation);

    Combat = combat;
    ProjectileUse = projectileUse;
    ResourceRecovery = resourceRecovery;
    DamageClass = damageClass;
    Presentation = presentation;
  }

  public ItemCombatCapabilityDefinition Combat { get; }

  public ItemDamageClassCapability DamageClass { get; }

  public ItemPresentationDefinition Presentation { get; }

  public ItemProjectileUseDefinition ProjectileUse { get; }

  public ItemResourceRecoveryDefinition ResourceRecovery { get; }
}
