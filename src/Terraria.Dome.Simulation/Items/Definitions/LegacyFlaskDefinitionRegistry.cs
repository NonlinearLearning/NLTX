using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct LegacyFlaskDefinition(
  ushort ItemType,
  ushort BuffType,
  int Value)
{
  public int BuffDurationTicks => ItemDefinition.FlaskDurationTicks;
}

public static class LegacyFlaskDefinitionRegistry
{
  private const int FlaskHeight = 24;
  private const int FlaskRarity = 4;
  private const int FlaskUseAnimation = 17;
  private const int FlaskUseStyle = 9;
  private const int FlaskUseTime = 17;
  private const int FlaskWidth = 14;
  private const int PartyFlaskValue = 300;
  private const int StandardFlaskValue = 500;

  private static readonly FrozenDictionary<ushort, LegacyFlaskDefinition> _definitions =
    new Dictionary<ushort, LegacyFlaskDefinition>
    {
      [1340] = new(1340, 71, StandardFlaskValue),
      [1353] = new(1353, 73, StandardFlaskValue),
      [1354] = new(1354, 74, StandardFlaskValue),
      [1355] = new(1355, 75, StandardFlaskValue),
      [1356] = new(1356, 76, StandardFlaskValue),
      [1357] = new(1357, 77, StandardFlaskValue),
      [1358] = new(1358, 78, PartyFlaskValue),
      [1359] = new(1359, 79, StandardFlaskValue)
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<ushort, LegacyFlaskDefinition> Definitions => _definitions;

  public static bool IsFlask(ushort itemType)
  {
    return _definitions.ContainsKey(itemType);
  }

  public static bool TryGet(
    ushort itemType,
    out LegacyFlaskDefinition definition)
  {
    return _definitions.TryGetValue(itemType, out definition);
  }

  public static bool TryGetBuffType(ushort itemType, out ushort buffType)
  {
    if (!_definitions.TryGetValue(itemType, out LegacyFlaskDefinition definition))
    {
      buffType = 0;
      return false;
    }

    buffType = definition.BuffType;
    return true;
  }

  public static IReadOnlyList<ItemDefinition> CreateDefinitions()
  {
    ItemDefinition[] definitions = new ItemDefinition[_definitions.Count];
    int index = 0;
    foreach (LegacyFlaskDefinition flask in _definitions.Values)
    {
      definitions[index] = new ItemDefinition(
        flask.ItemType,
        ItemDefinition.CommonMaxStack,
        Use: new ItemUseDefinition(
          UseStyle: FlaskUseStyle,
          UseTime: FlaskUseTime,
          UseAnimation: FlaskUseAnimation,
          Consumable: true,
          UseTurn: true),
        Recovery: new ItemRecoveryDefinition(
          BuffType: flask.BuffType,
          BuffDurationTicks: flask.BuffDurationTicks,
          Consumable: true),
        Width: FlaskWidth,
        Height: FlaskHeight,
        Value: flask.Value,
        Rarity: FlaskRarity);
      index++;
    }

    return Array.AsReadOnly(definitions);
  }
}
