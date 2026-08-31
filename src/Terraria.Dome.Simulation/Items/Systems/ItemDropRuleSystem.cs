using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemDropRuleSystem
{
  private readonly ItemDefinitionRegistry? _itemDefinitions;

  public ItemDropRuleSystem()
  {
  }

  public ItemDropRuleSystem(ItemDefinitionRegistry itemDefinitions)
  {
    _itemDefinitions = itemDefinitions ?? throw new ArgumentNullException(nameof(itemDefinitions));
  }

  public IReadOnlyList<CreateWorldItemCommand> Evaluate(
    WorldSeed seed,
    int sourceEntityId,
    long tick,
    IReadOnlyList<ItemDropDefinition> definitions,
    bool expertMode,
    bool masterMode,
    SimulationVector position,
    WorldSectionCoordinates section,
    int spawnSource,
    IReadOnlyDictionary<string, int>? conditionValues = null)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (sourceEntityId <= 0 || tick < 0 || spawnSource < 0 ||
        !float.IsFinite(position.X) || !float.IsFinite(position.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(sourceEntityId));
    }

    List<CreateWorldItemCommand> commands = new();
    HashSet<int> processedChains = [];
    for (int index = 0; index < definitions.Count; index++)
    {
      ItemDropDefinition definition = definitions[index];
      if (!IsEligible(definition, expertMode, masterMode, conditionValues))
      {
        continue;
      }

      if (definition.ChainId < 0)
      {
        commands.Add(CreateCommand(
          definition,
          seed,
          sourceEntityId,
          tick,
          index,
          position,
          section,
          spawnSource));
        continue;
      }

      if (!processedChains.Add(definition.ChainId))
      {
        continue;
      }

      int totalWeight = 0;
      for (int candidateIndex = 0; candidateIndex < definitions.Count; candidateIndex++)
      {
        ItemDropDefinition candidate = definitions[candidateIndex];
        if (candidate.ChainId == definition.ChainId &&
            IsEligible(candidate, expertMode, masterMode, conditionValues))
        {
          totalWeight = checked(totalWeight + candidate.Weight);
        }
      }

      int selection = (int)(GetRandom(
        seed,
        sourceEntityId,
        tick,
        definition.ChainId) % (uint)totalWeight);
      for (int candidateIndex = 0; candidateIndex < definitions.Count; candidateIndex++)
      {
        ItemDropDefinition candidate = definitions[candidateIndex];
        if (candidate.ChainId != definition.ChainId ||
            !IsEligible(candidate, expertMode, masterMode, conditionValues))
        {
          continue;
        }

        if (selection < candidate.Weight)
        {
          commands.Add(CreateCommand(
            candidate,
            seed,
            sourceEntityId,
            tick,
            candidateIndex,
            position,
            section,
            spawnSource));
          break;
        }

        selection -= candidate.Weight;
      }
    }

    return commands;
  }

  private static CreateWorldItemCommand CreateCommand(
    ItemDropDefinition definition,
    WorldSeed seed,
    int sourceEntityId,
    long tick,
    int definitionIndex,
    SimulationVector position,
    WorldSectionCoordinates section,
    int spawnSource)
  {
    uint random = GetRandom(seed, sourceEntityId, tick, definitionIndex);
    long quantityRange = (long)definition.MaximumQuantity - definition.MinimumQuantity + 1;
    int quantity = definition.MinimumQuantity + (int)(random % (ulong)quantityRange);
    return new CreateWorldItemCommand(
      new ItemStack(definition.ItemType, quantity),
      position,
      section,
      spawnSource);
  }

  private static uint GetRandom(
    WorldSeed seed,
    int sourceEntityId,
    long tick,
    int discriminator)
  {
    return unchecked((uint)(seed.Value ^ sourceEntityId * 1103515245 ^
      (int)tick * 486187739 ^ discriminator * 214013));
  }

  private bool IsEligible(
    ItemDropDefinition definition,
    bool expertMode,
    bool masterMode,
    IReadOnlyDictionary<string, int>? conditionValues)
  {
    return definition.ItemType != 0 && definition.MinimumQuantity > 0 &&
      definition.MaximumQuantity >= definition.MinimumQuantity && definition.Weight > 0 &&
      IsItemDefinitionCompatible(definition) &&
      (!definition.MasterOnly || masterMode) &&
      (!definition.ExpertOnly || expertMode || masterMode) &&
      AreConditionsMet(definition.Conditions, conditionValues);
  }

  private bool IsItemDefinitionCompatible(ItemDropDefinition definition)
  {
    return _itemDefinitions is null ||
      _itemDefinitions.TryGet(definition.ItemType, out ItemDefinition itemDefinition) &&
      definition.MaximumQuantity <= itemDefinition.StackLimit;
  }

  private static bool AreConditionsMet(
    IReadOnlyList<ItemDropCondition>? conditions,
    IReadOnlyDictionary<string, int>? conditionValues)
  {
    if (conditions is null)
    {
      return true;
    }

    for (int index = 0; index < conditions.Count; index++)
    {
      ItemDropCondition condition = conditions[index];
      bool matches = !string.IsNullOrWhiteSpace(condition.Key) &&
        conditionValues is not null &&
        conditionValues.TryGetValue(condition.Key, out int value) &&
        value >= condition.MinimumValue;
      if (condition.Invert)
      {
        matches = !matches;
      }

      if (!matches)
      {
        return false;
      }
    }

    return true;
  }
}
