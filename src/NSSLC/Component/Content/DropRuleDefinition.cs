using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Terraria.Content;

public sealed record DropRuleDefinition(
  string RuleId,
  ImmutableArray<int> ItemTypeIds,
  int ChanceNumerator,
  int ChanceDenominator,
  int MinimumStack,
  int MaximumStack,
  bool RequiresExpertMode = false,
  bool RequiresWorldEvent = false,
  bool RequiresKiller = false,
  DropRuleKind Kind = DropRuleKind.Common,
  ImmutableArray<DropConditionDefinition> Conditions = default,
  ImmutableArray<DropRuleReference> SuccessChains = default,
  ImmutableArray<DropRuleReference> FailureChains = default,
  DropRuleReference? ExpertVariant = null,
  DropRuleReference? NormalVariant = null,
  DropLuckPolicy LuckPolicy = DropLuckPolicy.None);
