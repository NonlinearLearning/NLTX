using System;

namespace Terraria.Attributes;

/// <summary>
/// Central classification and admission rules for attribute keys.
/// </summary>
public static class AttributeKindMetadata
{
  /// <summary>Returns the architectural category of an attribute key.</summary>
  public static AttributeKindCategory GetCategory(AttributeKind attribute)
  {
    return attribute switch
    {
      AttributeKind.Shield => AttributeKindCategory.ResourcePool,
      AttributeKind.MaxShield => AttributeKindCategory.Capacity,
      AttributeKind.MaximumLife => AttributeKindCategory.Capacity,
      AttributeKind.PotionCooldownTicks => AttributeKindCategory.Cooldown,
      AttributeKind.RestorationCooldownTicks => AttributeKindCategory.Cooldown,
      AttributeKind.MushroomCooldownTicks => AttributeKindCategory.Cooldown,
      AttributeKind.BreathCapacityTicks => AttributeKindCategory.Capacity,
      AttributeKind.MountKind => AttributeKindCategory.Identity,
      AttributeKind.StatusFlag => AttributeKindCategory.StatusFlag,
      AttributeKind.ProjectileHitImmunityTicks => AttributeKindCategory.Policy,
      _ when Enum.IsDefined(attribute) => AttributeKindCategory.ComputedScalar,
      _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute,
          "Unknown attribute kind.")
    };
  }

  /// <summary>
  /// Determines whether a key may receive an ordinary numeric contribution.
  /// </summary>
  public static bool AllowsNumericContribution(AttributeKind attribute)
  {
    AttributeKindCategory category = GetCategory(attribute);
    return category is AttributeKindCategory.ComputedScalar
        or AttributeKindCategory.Capacity
        or AttributeKindCategory.Policy;
  }

  /// <summary>Throws when a key is not valid for an ordinary attribute contribution.</summary>
  public static void ValidateNumericContribution(AttributeKind attribute)
  {
    if (!AllowsNumericContribution(attribute))
    {
      throw new ArgumentException(
          $"Attribute '{attribute}' is {GetCategory(attribute)} state and requires a dedicated owner.",
          nameof(attribute));
    }
  }
}
