using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Liquid.Components;

namespace Terraria.Dome.Simulation.Liquid.Definitions;

public sealed class TileObjectLiquidRuleRegistry
{
  private readonly Dictionary<ushort, List<TileObjectLiquidRule>> _rulesByTileType = new();

  public TileObjectLiquidRuleRegistry(IEnumerable<TileObjectLiquidRule> rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    foreach (TileObjectLiquidRule rule in rules)
    {
      ArgumentNullException.ThrowIfNull(rule);
      if (!_rulesByTileType.TryGetValue(rule.TileType, out List<TileObjectLiquidRule>? tileRules))
      {
        tileRules = [];
        _rulesByTileType.Add(rule.TileType, tileRules);
      }

      tileRules.Add(rule);
    }
  }

  public static TileObjectLiquidRuleRegistry Empty { get; } = new([]);

  public bool HasRules(ushort tileType)
  {
    return _rulesByTileType.ContainsKey(tileType);
  }

  public bool TryGet(ushort tileType, LiquidType liquidType, short originFrameX,
    out TileObjectLiquidRule rule)
  {
    if (_rulesByTileType.TryGetValue(tileType, out List<TileObjectLiquidRule>? tileRules))
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
