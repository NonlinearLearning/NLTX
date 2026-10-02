namespace Terraria.Content;

public sealed record DropRuleCatalogEntry(
  int? NpcNetId,
  DropRuleDefinition Rule,
  int Priority,
  string SourceKey);
