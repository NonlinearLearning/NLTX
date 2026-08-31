using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Loot;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Events;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcLootDefinition(
  int LootTableId,
  ushort ItemType,
  int MinimumQuantity,
  int MaximumQuantity);

public sealed class NpcLootDefinitionRegistry
{
  private readonly ItemDefinitionRegistry? _itemDefinitions;
  private readonly IReadOnlyDictionary<int, NpcLootDefinition> _definitions;

  public NpcLootDefinitionRegistry(IEnumerable<NpcLootDefinition> definitions)
    : this(definitions, null)
  {
  }

  public NpcLootDefinitionRegistry(
    IEnumerable<NpcLootDefinition> definitions,
    ItemDefinitionRegistry? itemDefinitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _itemDefinitions = itemDefinitions;
    Dictionary<int, NpcLootDefinition> entries = new();
    foreach (NpcLootDefinition definition in definitions)
    {
      if (definition.LootTableId <= 0 || definition.ItemType == 0 ||
          definition.MinimumQuantity <= 0 ||
          definition.MaximumQuantity < definition.MinimumQuantity ||
          !IsItemDefinitionCompatible(definition) ||
          !entries.TryAdd(definition.LootTableId, definition))
      {
        throw new ArgumentException("NPC loot definition is invalid or duplicated.", nameof(definitions));
      }
    }

    _definitions = entries;
  }

  public bool TryGet(int lootTableId, out NpcLootDefinition definition)
  {
    return _definitions.TryGetValue(lootTableId, out definition);
  }

  private bool IsItemDefinitionCompatible(NpcLootDefinition definition)
  {
    return _itemDefinitions is null ||
      _itemDefinitions.TryGet(definition.ItemType, out ItemDefinition itemDefinition) &&
      definition.MaximumQuantity <= itemDefinition.StackLimit;
  }
}

public sealed class NpcLootSystem
{
  private readonly NpcLootDefinitionRegistry _definitions;
  private readonly LootTable _lootTable;

  public NpcLootSystem(NpcLootDefinitionRegistry definitions, WorldSeed seed)
  {
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _lootTable = new LootTable(seed);
  }

  public ItemStack Roll(int lootTableId, int replicationId)
  {
    if (replicationId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(replicationId));
    }

    if (!_definitions.TryGet(lootTableId, out NpcLootDefinition definition))
    {
      throw new KeyNotFoundException($"NPC loot table {lootTableId} is not registered.");
    }

    ItemStack result = _lootTable.Roll(lootTableId, replicationId, definition.ItemType,
      definition.MinimumQuantity, definition.MaximumQuantity);
    return result;
  }

  public NpcLootCommand CreateDrop(NpcDeathEvent death)
  {
    if (!death.Npc.IsValid || death.LootTableId <= 0 || death.Tick < 0 ||
        !float.IsFinite(death.Position.X) || !float.IsFinite(death.Position.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(death));
    }

    ItemStack stack = Roll(death.LootTableId, death.Npc.Value);
    return new NpcLootCommand(
      death.Npc,
      new CreateWorldItemCommand(
        stack,
        death.Position,
        death.Section,
        death.Npc.Value));
  }

  public bool IsRegistered(int lootTableId)
  {
    return _definitions.TryGet(lootTableId, out _);
  }
}
