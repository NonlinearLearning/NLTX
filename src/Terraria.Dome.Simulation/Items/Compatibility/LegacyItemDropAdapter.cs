using System;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Items.Compatibility;

public readonly record struct LegacyItemDropRecord(
  int ItemType,
  int MinimumQuantity = 1,
  int MaximumQuantity = 1,
  ushort PrefixId = 0,
  ushort VariantId = 0);

public static class LegacyItemDropAdapter
{
  public static ItemDropDefinition ToDefinition(LegacyItemDropRecord record)
  {
    ValidateRecord(record);
    return new ItemDropDefinition(
      (ushort)record.ItemType,
      record.MinimumQuantity,
      record.MaximumQuantity);
  }

  public static bool TryCreateCommand(
    LegacyItemDropRecord record,
    int quantity,
    SimulationVector position,
    WorldSectionCoordinates section,
    int spawnSource,
    out CreateWorldItemCommand command)
  {
    command = default;
    try
    {
      ItemDropDefinition definition = ToDefinition(record);
      if (quantity < definition.MinimumQuantity || quantity > definition.MaximumQuantity ||
          spawnSource < 0)
      {
        return false;
      }

      command = new CreateWorldItemCommand(
        new ItemStack(definition.ItemType, quantity),
        position,
        section,
        spawnSource,
        new ItemInstanceStateComponent(
          PrefixId: record.PrefixId,
          VariantId: record.VariantId));
      return true;
    }
    catch (ArgumentOutOfRangeException)
    {
      return false;
    }
  }

  private static void ValidateRecord(LegacyItemDropRecord record)
  {
    if (record.ItemType <= 0 || record.ItemType > ushort.MaxValue ||
        record.MinimumQuantity <= 0 || record.MaximumQuantity < record.MinimumQuantity)
    {
      throw new ArgumentOutOfRangeException(nameof(record));
    }
  }
}
