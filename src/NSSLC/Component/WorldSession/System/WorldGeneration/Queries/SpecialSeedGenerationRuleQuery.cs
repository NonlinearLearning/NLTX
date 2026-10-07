using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class SpecialSeedGenerationRuleQuery
{
  public static SpecialSeedGenerationRuleFlagsSnapshot Snapshot(
    SpecialSeedGenerationRuleFlagsComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
