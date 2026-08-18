using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Items.Definitions;

public static class ItemDefinitionCompiler
{
  public static void Validate(
    ItemDefinition definition,
    IReadOnlySet<ushort>? knownItemTypes = null)
  {
    if (definition.ItemType == 0)
    {
      throw new ArgumentException("Item type zero is reserved for an empty stack.");
    }

    if (definition.StackLimit <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(definition), "Stack limit must be positive.");
    }

    if (definition.HealthRestore < 0 || definition.UseCooldownTicks < 0 ||
        definition.Width < 0 || definition.Height < 0 || definition.Value < 0 ||
        definition.Rarity < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(definition),
        "Base item dimensions, value, rarity and use values cannot be negative.");
    }

    if (definition.Identity is ItemIdentityDefinition identity &&
        identity.NameKey is null)
    {
      throw new ArgumentException("Identity name keys cannot be null.", nameof(definition));
    }

    if (definition.Use is ItemUseDefinition use)
    {
      if (use.UseTime < 0 || use.UseAnimation < 0 || use.CooldownTicks < 0 ||
          use.HealthRestore < 0 || use.ManaRestore < 0 || use.ManaCost < 0 ||
          use.ShootSpeed < 0)
      {
        throw new ArgumentException("Item use values cannot be negative.", nameof(definition));
      }

      if (use.UseAnimation > 0 && use.UseTime > use.UseAnimation)
      {
        throw new ArgumentException("Use time cannot exceed use animation.", nameof(definition));
      }

      if (use.ConsumesAmmo && use.AmmoType == 0)
      {
        throw new ArgumentException("Ammo-consuming items require an ammo type.", nameof(definition));
      }

      ValidateReference(use.ShootType, knownItemTypes, nameof(use.ShootType));
      ValidateReference(use.AmmoType, knownItemTypes, nameof(use.AmmoType));
    }

    if (definition.Combat is ItemCombatDefinition combat)
    {
      if (combat.Damage < 0 || combat.Knockback < 0 || combat.CriticalChance < 0 ||
          combat.ArmorPenetration < 0 || combat.ProjectileSpeed < 0)
      {
        throw new ArgumentException("Combat values cannot be negative.", nameof(definition));
      }

      if (combat.ConsumesAmmo && combat.AmmoType == 0)
      {
        throw new ArgumentException("Ammo-consuming combat items require an ammo type.", nameof(definition));
      }

      ValidateReference(combat.ProjectileType, knownItemTypes, nameof(combat.ProjectileType));
      ValidateReference(combat.AmmoType, knownItemTypes, nameof(combat.AmmoType));
    }

    if (definition.Use is ItemUseDefinition useDefinition &&
        definition.Combat is ItemCombatDefinition combatDefinition &&
        useDefinition.ConsumesAmmo && combatDefinition.ConsumesAmmo &&
        useDefinition.AmmoType != combatDefinition.AmmoType)
    {
      throw new ArgumentException(
        "Use and combat definitions cannot require different ammunition types.",
        nameof(definition));
    }

    if (definition.Placement is ItemPlacementDefinition placement &&
        (placement.TileType < -1 || placement.WallType < -1 ||
         placement.PlacementHasNegativeValues()))
    {
      throw new ArgumentException("Placement values are invalid.", nameof(definition));
    }

    if (definition.Placement is ItemPlacementDefinition placementDefinition &&
        placementDefinition.TileType >= 0 && placementDefinition.WallType >= 0)
    {
      throw new ArgumentException(
        "An item placement definition cannot provide both a tile and a wall type.",
        nameof(definition));
    }

    if (definition.Recovery is ItemRecoveryDefinition recovery &&
        (recovery.Health < 0 || recovery.Mana < 0 || recovery.BuffDurationTicks < 0))
    {
      throw new ArgumentException("Recovery values cannot be negative.", nameof(definition));
    }

    if (definition.Recovery is ItemRecoveryDefinition recoveryDefinition &&
        ((recoveryDefinition.BuffType == 0 && recoveryDefinition.BuffDurationTicks > 0) ||
         (recoveryDefinition.BuffType != 0 && recoveryDefinition.BuffDurationTicks <= 0)))
    {
      throw new ArgumentException(
        "Buff recovery requires both a buff type and a positive duration.",
        nameof(definition));
    }

    if (definition.Equipment is ItemEquipmentDefinition equipment)
    {
      if (equipment.Defense < 0)
      {
        throw new ArgumentException("Equipment defense cannot be negative.", nameof(definition));
      }

      if (equipment.Slot == ItemEquipmentSlot.None &&
          (equipment.Accessory || equipment.Vanity || equipment.Social || equipment.Defense != 0))
      {
        throw new ArgumentException("Equipment attributes require an equipment slot.", nameof(definition));
      }
    }
  }

  private static void ValidateReference(
    ushort itemType,
    IReadOnlySet<ushort>? knownItemTypes,
    string parameterName)
  {
    if (itemType != 0 && knownItemTypes is not null && !knownItemTypes.Contains(itemType))
    {
      throw new ArgumentException($"Unknown item reference: {itemType}.", parameterName);
    }
  }
}

internal static class ItemPlacementDefinitionExtensions
{
  public static bool PlacementHasNegativeValues(this ItemPlacementDefinition placement)
  {
    return placement.PlaceStyle < 0 || placement.TileBoost < 0;
  }
}
