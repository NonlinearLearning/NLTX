using System.Collections.Immutable;

namespace Terraria.Content;

public sealed record FishingDropRuleDefinition(
  string RuleId,
  int ChanceNumerator,
  int ChanceDenominator,
  ImmutableArray<int> ItemTypeIds,
  bool IsQuestFish,
  bool StopFurtherRulesOnMatch,
  FishingRarity Rarity = FishingRarity.Common,
  ImmutableArray<FishingConditionDefinition> Conditions = default);
