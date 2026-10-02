using Terraria.Content.Items;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Combat.Items.Commands;

public sealed class UseItemCombatCommand
{
  internal UseItemCombatCommand(
    ItemUseId useId,
    EntityReference userReference,
    ItemContentId itemContentId,
    InventorySlotIndex inventorySlot,
    ItemPersistentId itemPersistentId,
    ItemCapabilitySnapshot capability,
    ItemResourceUseQuery.Result resourceUse,
    EntityReference? lockOnTargetReference,
    int? activeTagEffectTypeId,
    ItemNetworkId? networkId)
  {
    UseId = useId;
    UserReference = userReference;
    ItemContentId = itemContentId;
    InventorySlot = inventorySlot;
    ItemPersistentId = itemPersistentId;
    Combat = capability.Combat;
    ProjectileUse = capability.ProjectileUse;
    ResourceRecovery = capability.ResourceRecovery;
    DamageClass = capability.DamageClass;
    ResourceUse = resourceUse;
    LockOnTargetReference = lockOnTargetReference;
    ActiveTagEffectTypeId = activeTagEffectTypeId;
    NetworkId = networkId;
  }

  public int? ActiveTagEffectTypeId { get; }

  public ItemCombatCapabilityDefinition Combat { get; }

  public ItemDamageClassCapability DamageClass { get; }

  public InventorySlotIndex InventorySlot { get; }

  public ItemContentId ItemContentId { get; }

  public ItemPersistentId ItemPersistentId { get; }

  public EntityReference? LockOnTargetReference { get; }

  public ItemNetworkId? NetworkId { get; }

  public ItemProjectileUseDefinition ProjectileUse { get; }

  public ItemResourceUseQuery.Result ResourceUse { get; }

  public ItemResourceRecoveryDefinition ResourceRecovery { get; }

  public ItemUseId UseId { get; }

  public EntityReference UserReference { get; }
}
