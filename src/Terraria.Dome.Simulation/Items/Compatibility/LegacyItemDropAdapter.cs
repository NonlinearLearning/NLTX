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

  public static ItemDropDefinition ToDefinition(
    LegacyItemDropRecord record,
    ItemDefinitionRegistry itemDefinitions)
  {
    ArgumentNullException.ThrowIfNull(itemDefinitions);
    ItemDropDefinition definition = ToDefinition(record);
    if (!itemDefinitions.TryGet(definition.ItemType, out ItemDefinition itemDefinition) ||
        definition.MaximumQuantity > itemDefinition.StackLimit)
    {
      throw new ArgumentOutOfRangeException(
        nameof(record),
        "Legacy item drop definition is unknown or exceeds the item stack limit.");
    }

    return definition;
  }

  public static bool TryCreateCommand(
    LegacyItemDropRecord record,
    int quantity,
    SimulationVector position,
    WorldSectionCoordinates section,
    int spawnSource,
    out CreateWorldItemCommand command)
  {
    return TryCreateCommandCore(
      record,
      quantity,
      position,
      section,
      spawnSource,
      itemDefinitions: null,
      out command);
  }

  public static bool TryCreateCommand(
    LegacyItemDropRecord record,
    int quantity,
    SimulationVector position,
    WorldSectionCoordinates section,
    int spawnSource,
    ItemDefinitionRegistry itemDefinitions,
    out CreateWorldItemCommand command)
  {
    ArgumentNullException.ThrowIfNull(itemDefinitions);
    return TryCreateCommandCore(
      record,
      quantity,
      position,
      section,
      spawnSource,
      itemDefinitions,
      out command);
  }

  private static bool TryCreateCommandCore(
    LegacyItemDropRecord record,
    int quantity,
    SimulationVector position,
    WorldSectionCoordinates section,
    int spawnSource,
    ItemDefinitionRegistry? itemDefinitions,
    out CreateWorldItemCommand command)
  {
    command = default;
    try
    {
      ItemDropDefinition definition = itemDefinitions is null
        ? ToDefinition(record)
        : ToDefinition(record, itemDefinitions);
      if (quantity < definition.MinimumQuantity || quantity > definition.MaximumQuantity ||
          spawnSource < 0 || !float.IsFinite(position.X) || !float.IsFinite(position.Y))
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
