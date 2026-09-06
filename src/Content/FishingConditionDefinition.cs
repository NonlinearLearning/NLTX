using System.Collections.Immutable;

namespace Terraria.Content;

public sealed record FishingConditionDefinition(
  FishingConditionKind Kind,
  float? MinimumValue = null,
  float? MaximumValue = null,
  ImmutableArray<int> ContentTypeIds = default,
  bool Required = true);
