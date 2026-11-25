using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Items;
using Terraria.Share.Relationships;

namespace Terraria.Share.Inventory;

/// <summary>
/// Rebuilds item supplied attribute contributions from authoritative inventory facts.
/// </summary>
/// <remarks>
/// Inventory owns slot and containment facts. This system owns the Equipment, Accessory and
/// Weapon source categories for entities that carry both InventoryComponent and
/// AttributeSetComponent. It never creates a Player-specific equipment or defense component.
/// </remarks>
public sealed class InventoryAttributeContributionSystem
    : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private static readonly AttributeSourceCategory[] InventorySourceCategories = [
    AttributeSourceCategory.Equipment,
    AttributeSourceCategory.Accessory,
    AttributeSourceCategory.Weapon
  ];

  private readonly ItemRegistry _definitions;
  private readonly QueryDescription _identityQuery;
  private readonly QueryDescription _inventoryQuery;

  public InventoryAttributeContributionSystem(
      ArchWorld world,
      ItemRegistry definitions)
      : base(world)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitions = definitions;
    _identityQuery = new QueryDescription().WithAll<EntityIdentityComponent>();
    _inventoryQuery = new QueryDescription()
        .WithAll<InventoryComponent, AttributeSetComponent>();
  }

  public override void Update(in EcsServerTickContext context)
  {
    ArchWorld world = context.World;
    ItemRegistry definitions = _definitions;
    Dictionary<EntityUuid, ArchEntity> identities = [];
    bool duplicateIdentity = false;

    world.Query(
        in _identityQuery,
        (ArchEntity entity, ref EntityIdentityComponent identity) =>
        {
          if (!identities.TryAdd(identity.Id, entity))
          {
            duplicateIdentity = true;
          }
        });

    if (duplicateIdentity)
    {
      return;
    }

    world.Query(
        in _inventoryQuery,
        (ArchEntity owner,
            ref InventoryComponent inventory,
            ref AttributeSetComponent attributes) =>
        {
          Rebuild(world, identities, definitions, inventory, attributes);
        });
  }

  private static void Rebuild(
      ArchWorld world,
      IReadOnlyDictionary<EntityUuid, ArchEntity> identities,
      ItemRegistry definitions,
      InventoryComponent inventory,
      AttributeSetComponent attributes)
  {
    List<AttributeModifier> pending = [];
    foreach (InventorySlot slot in inventory.Slots)
    {
      if (slot.IsEmpty || !TryGetDefinition(
              world,
              identities,
              definitions,
              slot,
              out Item definition))
      {
        if (!slot.IsEmpty)
        {
          return;
        }

        continue;
      }

      long sourceId = slot.Definition.Index + 1L;
      foreach (ItemAttributeContribution contribution in definition.AttributeContributions)
      {
        if (!IsContributionActive(slot.Definition.Kind, contribution.SourceCategory))
        {
          continue;
        }

        pending.Add(contribution.Contribution.ToModifier(
            contribution.SourceCategory,
            sourceId));
      }
    }

    foreach (AttributeSourceCategory category in InventorySourceCategories)
    {
      attributes.RemoveBySourceCategory(category);
    }

    foreach (AttributeModifier modifier in pending)
    {
      attributes.AddModifier(modifier);
    }
  }

  private static bool IsContributionActive(
      InventorySlotKind slotKind,
      AttributeSourceCategory sourceCategory)
  {
    return sourceCategory switch
    {
      AttributeSourceCategory.Equipment => slotKind == InventorySlotKind.Armor,
      AttributeSourceCategory.Accessory => slotKind == InventorySlotKind.Accessory,
      AttributeSourceCategory.Weapon => slotKind == InventorySlotKind.Weapon,
      _ => false
    };
  }

  private static bool TryGetDefinition(
      ArchWorld world,
      IReadOnlyDictionary<EntityUuid, ArchEntity> identities,
      ItemRegistry definitions,
      InventorySlot slot,
      out Item definition)
  {
    definition = null!;
    if (slot.Content is not ItemStack stack)
    {
      return false;
    }

    return !stack.IsEmpty && definitions.TryGet(stack.ItemId, out definition);
  }
}
