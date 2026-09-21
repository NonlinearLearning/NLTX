using Terraria.Combat.Items.Commands;
using Terraria.Content.Items;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Combat.Items;

public static class ItemCombatCommandBuilder
{
  public static bool TryBuild(
    ItemUseId useId,
    EntityReference userReference,
    ItemContentId itemContentId,
    InventorySlotIndex inventorySlot,
    ItemPersistentId itemPersistentId,
    ItemCapabilitySnapshot capability,
    ItemResourceUseQuery.ResourceAvailability resources,
    EntityReference? lockOnTargetReference,
    int? activeTagEffectTypeId,
    ItemNetworkId? networkId,
    out UseItemCombatCommand? command)
  {
    command = null;
    ArgumentNullException.ThrowIfNull(itemPersistentId);
    ArgumentNullException.ThrowIfNull(capability);

    if (!useId.IsValid || !userReference.IsValid || !itemContentId.IsValid ||
      !inventorySlot.IsValid ||
      (lockOnTargetReference is EntityReference target && !target.IsValid) ||
      (activeTagEffectTypeId is int tagEffectTypeId && tagEffectTypeId < 0) ||
      (networkId is ItemNetworkId id && !id.IsValid))
    {
      return false;
    }

    ItemResourceUseQuery.Result resourceUse = ItemResourceUseQuery.Evaluate(
      capability.ResourceRecovery,
      resources);
    if (!resourceUse.CanUse)
    {
      return false;
    }

    command = new UseItemCombatCommand(
      useId,
      userReference,
      itemContentId,
      inventorySlot,
      itemPersistentId,
      capability,
      resourceUse,
      lockOnTargetReference,
      activeTagEffectTypeId,
      networkId);
    return true;
  }
}
