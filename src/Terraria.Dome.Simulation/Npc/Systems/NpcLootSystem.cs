using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Loot;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcLootDefinition(
  int LootTableId,
  ushort ItemType,
  int MinimumQuantity,
  int MaximumQuantity);

public sealed class NpcLootDefinitionRegistry
{
  private readonly IReadOnlyDictionary<int, NpcLootDefinition> _definitions;

  public NpcLootDefinitionRegistry(IEnumerable<NpcLootDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    Dictionary<int, NpcLootDefinition> entries = new();
    foreach (NpcLootDefinition definition in definitions)
    {
      if (definition.LootTableId <= 0 || definition.ItemType == 0 ||
          definition.MinimumQuantity <= 0 ||
          definition.MaximumQuantity < definition.MinimumQuantity ||
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
    if (!_definitions.TryGet(lootTableId, out NpcLootDefinition definition))
    {
      throw new KeyNotFoundException($"NPC loot table {lootTableId} is not registered.");
    }

    ItemStack result = _lootTable.Roll(lootTableId, replicationId, definition.ItemType,
      definition.MinimumQuantity, definition.MaximumQuantity);
    return result;
  }

  public bool IsRegistered(int lootTableId)
  {
    return _definitions.TryGet(lootTableId, out _);
  }
}
