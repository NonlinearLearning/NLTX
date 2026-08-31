using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Player.Definitions;

public enum LegacySentryArmorSetClass : byte
{
  Squire,
  Apprentice,
  Huntress,
  Monk
}

public enum LegacySentryArmorSetTier : byte
{
  Tier2,
  Tier3
}

public readonly record struct LegacySentryArmorSetDefinition(
  ushort HeadItemType,
  ushort BodyItemType,
  ushort LegItemType,
  LegacySentryArmorSetClass SetClass,
  LegacySentryArmorSetTier Tier,
  int CapacityBonus);

public static class LegacySentryArmorSetRegistry
{
  private static readonly FrozenDictionary<
    (ushort HeadItemType, ushort BodyItemType, ushort LegItemType),
    LegacySentryArmorSetDefinition> _definitions =
    new Dictionary<
      (ushort HeadItemType, ushort BodyItemType, ushort LegItemType),
      LegacySentryArmorSetDefinition>
    {
      [(3800, 3801, 3802)] = new(
        3800,
        3801,
        3802,
        LegacySentryArmorSetClass.Squire,
        LegacySentryArmorSetTier.Tier2,
        1),
      [(3797, 3798, 3799)] = new(
        3797,
        3798,
        3799,
        LegacySentryArmorSetClass.Apprentice,
        LegacySentryArmorSetTier.Tier2,
        1),
      [(3803, 3804, 3805)] = new(
        3803,
        3804,
        3805,
        LegacySentryArmorSetClass.Huntress,
        LegacySentryArmorSetTier.Tier2,
        1),
      [(3806, 3807, 3808)] = new(
        3806,
        3807,
        3808,
        LegacySentryArmorSetClass.Monk,
        LegacySentryArmorSetTier.Tier2,
        1),
      [(3871, 3872, 3873)] = new(
        3871,
        3872,
        3873,
        LegacySentryArmorSetClass.Squire,
        LegacySentryArmorSetTier.Tier3,
        1),
      [(3874, 3875, 3876)] = new(
        3874,
        3875,
        3876,
        LegacySentryArmorSetClass.Apprentice,
        LegacySentryArmorSetTier.Tier3,
        1),
      [(3877, 3878, 3879)] = new(
        3877,
        3878,
        3879,
        LegacySentryArmorSetClass.Huntress,
        LegacySentryArmorSetTier.Tier3,
        1),
      [(3880, 3881, 3882)] = new(
        3880,
        3881,
        3882,
        LegacySentryArmorSetClass.Monk,
        LegacySentryArmorSetTier.Tier3,
        1)
    }.ToFrozenDictionary();

  private static readonly IReadOnlyList<ItemDefinition> _supplementalDefinitions =
    Array.AsReadOnly(
    [
      CreateBodyDefinition(3798, 200),
      CreateLegDefinition(3799, 144),
      CreateBodyDefinition(3801, 201),
      CreateLegDefinition(3802, 145),
      CreateBodyDefinition(3804, 202),
      CreateLegDefinition(3805, 146),
      CreateBodyDefinition(3807, 203),
      CreateLegDefinition(3808, 148),
      CreateBodyDefinition(3872, 204),
      CreateLegDefinition(3873, 152),
      CreateBodyDefinition(3875, 205),
      CreateLegDefinition(3876, 153),
      CreateBodyDefinition(3878, 206),
      CreateLegDefinition(3879, 154),
      CreateBodyDefinition(3881, 207),
      CreateLegDefinition(3882, 156)
    ]);

  public static IReadOnlyDictionary<
    (ushort HeadItemType, ushort BodyItemType, ushort LegItemType),
    LegacySentryArmorSetDefinition> Definitions => _definitions;

  public static IReadOnlyList<ItemDefinition> SupplementalDefinitions => _supplementalDefinitions;

  public static bool TryGet(
    ushort headItemType,
    ushort bodyItemType,
    ushort legItemType,
    out LegacySentryArmorSetDefinition definition)
  {
    if (headItemType == 0 || bodyItemType == 0 || legItemType == 0)
    {
      definition = default;
      return false;
    }

    return _definitions.TryGetValue(
      (headItemType, bodyItemType, legItemType),
      out definition);
  }

  private static ItemDefinition CreateBodyDefinition(ushort itemType, int bodySlot)
  {
    return new ItemDefinition(
      itemType,
      1,
      Equipment: new ItemEquipmentDefinition(
        ItemEquipmentSlot.Body,
        BodySlot: bodySlot));
  }

  private static ItemDefinition CreateLegDefinition(ushort itemType, int legSlot)
  {
    return new ItemDefinition(
      itemType,
      1,
      Equipment: new ItemEquipmentDefinition(
        ItemEquipmentSlot.Legs,
        LegSlot: legSlot));
  }
}
