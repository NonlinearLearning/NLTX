using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Items.Definitions;

public static class LegacySentryEquipmentRegistry
{
  private static readonly FrozenDictionary<ushort, ItemEquipmentDefinition> _definitions =
    new Dictionary<ushort, ItemEquipmentDefinition>
    {
      [3381] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 1),
      [3797] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 1),
      [3800] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 1),
      [3803] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 1),
      [3806] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 1),
      [3871] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 2),
      [3874] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 2),
      [3877] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 2),
      [3880] = new(ItemEquipmentSlot.Head, SentryCapacityBonus: 2)
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<ushort, ItemEquipmentDefinition> Definitions => _definitions;

  public static IReadOnlyList<ItemDefinition> CreateDefinitions()
  {
    ItemDefinition[] definitions = new ItemDefinition[_definitions.Count];
    int index = 0;
    foreach (KeyValuePair<ushort, ItemEquipmentDefinition> entry in _definitions)
    {
      definitions[index] = new ItemDefinition(
        entry.Key,
        StackLimit: 1,
        Equipment: entry.Value);
      index++;
    }

    return Array.AsReadOnly(definitions);
  }

  public static bool TryGet(
    ushort itemType,
    out ItemEquipmentDefinition definition)
  {
    return _definitions.TryGetValue(itemType, out definition);
  }
}
