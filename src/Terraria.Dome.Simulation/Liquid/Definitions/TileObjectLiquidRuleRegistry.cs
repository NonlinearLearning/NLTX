using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Dome.Simulation.Liquid.Components;

namespace Terraria.Dome.Simulation.Liquid.Definitions;

public sealed class TileObjectLiquidRuleRegistry
{
  private readonly IReadOnlyDictionary<ushort, IReadOnlyList<TileObjectLiquidRule>>
    _rulesByTileType;
  private readonly IReadOnlyList<TileObjectLiquidRule> _orderedRules;

  public TileObjectLiquidRuleRegistry(IEnumerable<TileObjectLiquidRule> rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    List<TileObjectLiquidRule> materialized = new(rules);
    Dictionary<ushort, List<TileObjectLiquidRule>> grouped = new();
    foreach (TileObjectLiquidRule rule in materialized)
    {
      ArgumentNullException.ThrowIfNull(rule);
      if (!grouped.TryGetValue(rule.TileType, out List<TileObjectLiquidRule>? tileRules))
      {
        tileRules = [];
        grouped.Add(rule.TileType, tileRules);
      }

      tileRules.Add(rule);
    }

    Dictionary<ushort, IReadOnlyList<TileObjectLiquidRule>> readOnlyGroups = new();
    foreach ((ushort tileType, List<TileObjectLiquidRule> tileRules) in grouped)
    {
      readOnlyGroups.Add(tileType, tileRules.AsReadOnly());
    }

    _rulesByTileType = new ReadOnlyDictionary<ushort, IReadOnlyList<TileObjectLiquidRule>>(
      readOnlyGroups);
    _orderedRules = materialized.AsReadOnly();
  }

  public static TileObjectLiquidRuleRegistry Empty { get; } = new([]);

  public IReadOnlyList<TileObjectLiquidRule> Rules => _orderedRules;

  public bool HasRules(ushort tileType)
  {
    return _rulesByTileType.ContainsKey(tileType);
  }

  public bool TryGet(ushort tileType, LiquidType liquidType, short originFrameX,
    out TileObjectLiquidRule rule)
  {
    if (_rulesByTileType.TryGetValue(tileType, out IReadOnlyList<TileObjectLiquidRule>? tileRules))
    {
      for (int index = 0; index < tileRules.Count; index++)
      {
        TileObjectLiquidRule candidate = tileRules[index];
        if (candidate.LiquidType == liquidType && candidate.MatchesOriginFrame(originFrameX))
        {
          rule = candidate;
          return true;
        }
      }
    }

    rule = null!;
    return false;
  }
}
