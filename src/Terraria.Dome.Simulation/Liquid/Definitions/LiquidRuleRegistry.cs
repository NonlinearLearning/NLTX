using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Liquid.Components;

namespace Terraria.Dome.Simulation.Liquid.Definitions;

public sealed class LiquidRuleRegistry
{
  private readonly IReadOnlyDictionary<LiquidType, LiquidRuleDefinition> _definitions;
  private readonly IReadOnlyDictionary<(LiquidType First, LiquidType Second), LiquidMergeRule>
    _mergeRules;

  private LiquidRuleRegistry(
    IReadOnlyDictionary<LiquidType, LiquidRuleDefinition> definitions,
    IReadOnlyDictionary<(LiquidType First, LiquidType Second), LiquidMergeRule> mergeRules)
  {
    _definitions = definitions;
    _mergeRules = mergeRules;
  }

  public LiquidRuleDefinition Get(LiquidType type)
  {
    if (!_definitions.TryGetValue(type, out LiquidRuleDefinition definition))
    {
      throw new ArgumentOutOfRangeException(nameof(type));
    }

    return definition;
  }

  public bool TryGetMerge(
    LiquidType firstType,
    LiquidType secondType,
    out LiquidMergeRule rule)
  {
    if (!Enum.IsDefined(firstType) || !Enum.IsDefined(secondType) || firstType == secondType)
    {
      rule = default;
      return false;
    }

    return _mergeRules.TryGetValue((firstType, secondType), out rule);
  }

  public static LiquidRuleRegistry CreateDefault()
  {
    Dictionary<LiquidType, LiquidRuleDefinition> definitions = new()
    {
      [LiquidType.Water] = new(LiquidType.Water, LiquidGravity.Down, 32, false),
      [LiquidType.Lava] = new(LiquidType.Lava, LiquidGravity.Down, 16, true),
      [LiquidType.Honey] = new(LiquidType.Honey, LiquidGravity.Down, 8, false),
      [LiquidType.Shimmer] = new(LiquidType.Shimmer, LiquidGravity.Down, 16, false)
    };
    Dictionary<(LiquidType First, LiquidType Second), LiquidMergeRule> mergeRules = new()
    {
      [(LiquidType.Water, LiquidType.Lava)] = new(
        LiquidType.Water,
        LiquidType.Lava,
        LiquidType.Lava,
        56),
      [(LiquidType.Water, LiquidType.Honey)] = new(
        LiquidType.Water,
        LiquidType.Honey,
        LiquidType.Honey,
        229),
      [(LiquidType.Water, LiquidType.Shimmer)] = new(
        LiquidType.Water,
        LiquidType.Shimmer,
        LiquidType.Shimmer,
        659),
      [(LiquidType.Lava, LiquidType.Water)] = new(
        LiquidType.Lava,
        LiquidType.Water,
        LiquidType.Water,
        56),
      [(LiquidType.Lava, LiquidType.Honey)] = new(
        LiquidType.Lava,
        LiquidType.Honey,
        LiquidType.Honey,
        230),
      [(LiquidType.Lava, LiquidType.Shimmer)] = new(
        LiquidType.Lava,
        LiquidType.Shimmer,
        LiquidType.Shimmer,
        659),
      [(LiquidType.Honey, LiquidType.Water)] = new(
        LiquidType.Honey,
        LiquidType.Water,
        LiquidType.Water,
        229),
      [(LiquidType.Honey, LiquidType.Lava)] = new(
        LiquidType.Honey,
        LiquidType.Lava,
        LiquidType.Lava,
        230),
      [(LiquidType.Honey, LiquidType.Shimmer)] = new(
        LiquidType.Honey,
        LiquidType.Shimmer,
        LiquidType.Shimmer,
        659),
      [(LiquidType.Shimmer, LiquidType.Water)] = new(
        LiquidType.Shimmer,
        LiquidType.Water,
        LiquidType.Water,
        659),
      [(LiquidType.Shimmer, LiquidType.Lava)] = new(
        LiquidType.Shimmer,
        LiquidType.Lava,
        LiquidType.Lava,
        659),
      [(LiquidType.Shimmer, LiquidType.Honey)] = new(
        LiquidType.Shimmer,
        LiquidType.Honey,
        LiquidType.Honey,
        659)
    };
    return new LiquidRuleRegistry(definitions, mergeRules);
  }
}
