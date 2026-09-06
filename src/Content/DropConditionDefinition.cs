using System.Collections.Immutable;

namespace Terraria.Content;

public sealed record DropConditionDefinition(
  DropConditionKind Kind,
  string? Parameter = null,
  int? IntegerValue = null,
  bool? BooleanValue = null,
  ImmutableArray<ContentReference> ReferencedContentIds = default);
