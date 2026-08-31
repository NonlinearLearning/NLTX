using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Player.Definitions;

namespace Terraria.Dome.Simulation.Items.Definitions;

public static class ItemDefinitionCompiler
{
  private const short CartTrackTileType = ItemPlacementDefinition.CartTrackTileType;

  public static void Validate(
    ItemDefinition definition,
    IReadOnlySet<ushort>? knownItemTypes = null)
  {
    if (definition.ItemType == 0)
    {
      throw new ArgumentException("Item type zero is reserved for an empty stack.");
    }

    if (definition.StackLimit <= 0 || definition.StackLimit > ItemDefinition.CommonMaxStack)
    {
      throw new ArgumentOutOfRangeException(nameof(definition), "Stack limit must be positive.");
    }

    if (definition.HealthRestore < 0 || definition.UseCooldownTicks < 0 ||
        definition.Width < 0 || definition.Height < 0 || definition.Value < 0 ||
        definition.Rarity < 0 || definition.Alpha < 0 || definition.Alpha > 255 ||
        !float.IsFinite(definition.Scale) || definition.Scale <= 0)
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

    if (definition.Identity is ItemIdentityDefinition identityDefinition &&
        identityDefinition.BuyOnce && !identityDefinition.Buy)
    {
      throw new ArgumentException(
        "Buy-once items must also be marked as buyable.", nameof(definition));
    }

    if (definition.Use is ItemUseDefinition use)
    {
      if (use.UseStyle < 0 || use.UseTime < 0 || use.UseAnimation < 0 ||
          use.ReuseDelayTicks < 0 || use.CooldownTicks < 0 ||
          use.HealthRestore < 0 || use.ManaRestore < 0 || use.ManaCost < 0 ||
          !float.IsFinite(use.ShootSpeed) || use.ShootSpeed < 0)
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

      if (use.ShootsEveryUse && use.ShootType == 0)
      {
        throw new ArgumentException(
          "Items that shoot every use require a projectile type.", nameof(definition));
      }

      if (use.ShootSpeed > 0 && use.ShootType == 0)
      {
        throw new ArgumentException(
          "Projectile speed requires a projectile type.", nameof(definition));
      }

      ValidateReference(use.AmmoType, knownItemTypes, nameof(use.AmmoType));
    }

    if (definition.Combat is ItemCombatDefinition combat)
    {
      if (combat.Damage < 0 || combat.Knockback < 0 || combat.CriticalChance < 0 ||
          combat.ArmorPenetration < 0 || combat.BonusTagDamage < 0 ||
          !float.IsFinite(combat.ProjectileSpeed) ||
          combat.ProjectileSpeed < 0)
      {
        throw new ArgumentException("Combat values cannot be negative.", nameof(definition));
      }

      if (!Enum.IsDefined(combat.DamageClass))
      {
        throw new ArgumentException("The item damage class is invalid.", nameof(definition));
      }

      if (combat.ConsumesAmmo && combat.AmmoType == 0)
      {
        throw new ArgumentException("Ammo-consuming combat items require an ammo type.", nameof(definition));
      }

      if (combat.ProjectileSpeed > 0 && combat.ProjectileType == 0)
      {
        throw new ArgumentException(
          "Combat projectile speed requires a projectile type.", nameof(definition));
      }

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

    if (definition.Dd2Summon && !definition.IsSentry)
    {
      throw new ArgumentException(
        "DD2 summon metadata requires a sentry item.",
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

    if (definition.Placement is ItemPlacementDefinition emptyPlacement &&
        emptyPlacement.TileType < 0 && emptyPlacement.WallType < 0)
    {
      throw new ArgumentException(
        "A placement definition requires a tile or wall type.", nameof(definition));
    }

    if (definition.Placement is ItemPlacementDefinition cartTrackPlacement &&
        cartTrackPlacement.CartTrack &&
        (cartTrackPlacement.TileType != CartTrackTileType || cartTrackPlacement.WallType >= 0))
    {
      throw new ArgumentException(
        $"CartTrack placement requires tile type {CartTrackTileType} and no wall type.",
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
      if (!Enum.IsDefined(equipment.Slot) || equipment.Defense < 0 || equipment.LifeRegen < 0 ||
          equipment.ManaIncrease < 0 || equipment.SentryCapacityBonus < 0 ||
          equipment.HeadSlot < -1 || equipment.BodySlot < -1 ||
          equipment.LegSlot < -1 || equipment.HandOnSlot < -1 || equipment.HandOffSlot < -1 ||
          equipment.BackSlot < -1 || equipment.FrontSlot < -1 || equipment.ShoeSlot < -1 ||
          equipment.WaistSlot < -1 || equipment.WingSlot < -1 || equipment.ShieldSlot < -1 ||
          equipment.NeckSlot < -1 || equipment.FaceSlot < -1 || equipment.BalloonSlot < -1 ||
          equipment.BeardSlot < -1 || equipment.VoiceSlot < -1)
      {
        throw new ArgumentException(
          "Equipment numeric values cannot be negative.",
          nameof(definition));
      }

      if (equipment.Slot == ItemEquipmentSlot.None &&
          (equipment.Accessory || equipment.Vanity || equipment.Social || equipment.Defense != 0 ||
           equipment.LifeRegen != 0 || equipment.ManaIncrease != 0 ||
           equipment.SentryCapacityBonus != 0 || equipment.HeadSlot >= 0 ||
           equipment.BodySlot >= 0 || equipment.LegSlot >= 0 || equipment.HandOnSlot >= 0 ||
           equipment.HandOffSlot >= 0 || equipment.BackSlot >= 0 || equipment.FrontSlot >= 0 ||
           equipment.ShoeSlot >= 0 || equipment.WaistSlot >= 0 || equipment.WingSlot >= 0 ||
           equipment.ShieldSlot >= 0 || equipment.NeckSlot >= 0 || equipment.FaceSlot >= 0 ||
           equipment.BalloonSlot >= 0 || equipment.BeardSlot >= 0 || equipment.VoiceSlot > 0 ||
           equipment.HasVanityEffects))
      {
        throw new ArgumentException("Equipment attributes require an equipment slot.", nameof(definition));
      }
    }

    if (definition.Extractinator is ItemExtractinatorDefinition extractinator &&
        !ExtractinatorRuleRegistry.IsSupportedMode(extractinator.ExtractionMode))
    {
      throw new ArgumentException("The Extractinator mode is invalid.", nameof(definition));
    }

    if (definition.Tools is ItemToolDefinition tools &&
        (tools.PickPower < 0 || tools.AxePower < 0 || tools.HammerPower < 0 ||
         tools.TileWandPower < 0))
    {
      throw new ArgumentOutOfRangeException(nameof(definition), "Tool powers cannot be negative.");
    }

    if (definition.Gathering is ItemGatheringDefinition gathering &&
        (gathering.FishingPolePower < 0 || gathering.BaitPower < 0))
    {
      throw new ArgumentOutOfRangeException(
        nameof(definition), "Gathering powers cannot be negative.");
    }

    if (definition.Summoning is ItemSummoningDefinition summoning &&
        (summoning.NpcType < 0 || summoning.NpcType > ushort.MaxValue ||
         !MountTypeRegistry.IsValid(summoning.MountType)))
    {
      throw new ArgumentOutOfRangeException(
        nameof(definition), "Summoning references must be valid item metadata IDs.");
    }

    if (definition.Appearance is ItemAppearanceDefinition appearance && appearance.HairDye < -1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(definition), "Hair dye metadata must use -1 or a non-negative identifier.");
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
