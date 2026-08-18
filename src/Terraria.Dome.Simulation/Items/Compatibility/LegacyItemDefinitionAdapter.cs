using System;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Compatibility;

public readonly record struct LegacyItemDefinitionRecord(
  int ItemType,
  int MaxStack,
  int HealthRestore = 0,
  int UseCooldownTicks = 0,
  string NameKey = "",
  short TileType = -1,
  ItemEquipmentDefinition? Equipment = null);

public static class LegacyItemDefinitionAdapter
{
  public static ItemDefinition ToDefinition(LegacyItemDefinitionRecord record)
  {
    if (record.ItemType <= 0 || record.ItemType > ushort.MaxValue || record.MaxStack <= 0 ||
        record.HealthRestore < 0 || record.UseCooldownTicks < 0 || record.TileType < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(record));
    }

    ItemUseDefinition? use = record.UseCooldownTicks > 0
      ? new ItemUseDefinition(CooldownTicks: record.UseCooldownTicks)
      : null;
    ItemPlacementDefinition? placement = record.TileType >= 0
      ? new ItemPlacementDefinition(TileType: record.TileType)
      : null;
    ItemIdentityDefinition? identity = string.IsNullOrEmpty(record.NameKey)
      ? null
      : new ItemIdentityDefinition(NameKey: record.NameKey);
    return new ItemDefinition(
      (ushort)record.ItemType,
      record.MaxStack,
      record.HealthRestore,
      record.UseCooldownTicks,
      identity,
      use,
      Placement: placement,
      Equipment: record.Equipment);
  }
}
