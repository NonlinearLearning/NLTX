namespace Terraria.Content;

public sealed record RecipeConditionDefinition(
  string Kind,
  string? Parameter = null,
  int? IntegerValue = null,
  bool? BooleanValue = null);
