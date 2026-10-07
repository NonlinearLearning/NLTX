using System.Collections.Immutable;

namespace Terraria.Content;

public sealed record RecipeGroupDefinition(
  int GroupId,
  string? PersistentId,
  ImmutableArray<int> ValidItemTypeIds,
  int? PreferredItemTypeId,
  int FakeItemId,
  string? DisplayNameKey = null,
  int? DecraftItemTypeId = null);
