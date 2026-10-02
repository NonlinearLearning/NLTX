using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Terraria.Content;

public sealed class DropRuleCatalog
{
  public DropRuleCatalog(
    IEnumerable<DropRuleCatalogEntry> globalEntries,
    IReadOnlyDictionary<int, IReadOnlyList<DropRuleCatalogEntry>> entriesByNpcNetId)
  {
    ArgumentNullException.ThrowIfNull(globalEntries);
    ArgumentNullException.ThrowIfNull(entriesByNpcNetId);
    GlobalEntries = globalEntries.ToImmutableArray();
    EntriesByNpcNetId = entriesByNpcNetId.ToFrozenDictionary(
      pair => pair.Key,
      pair => pair.Value.ToImmutableArray());
    RuleDefinitionsById = GlobalEntries
      .Concat(EntriesByNpcNetId.Values.SelectMany(static entries => entries))
      .Select(static entry => entry.Rule)
      .ToFrozenDictionary(rule => rule.RuleId);
  }

  public ImmutableArray<DropRuleCatalogEntry> GlobalEntries { get; }

  public FrozenDictionary<int, ImmutableArray<DropRuleCatalogEntry>> EntriesByNpcNetId { get; }

  public FrozenDictionary<string, DropRuleDefinition> RuleDefinitionsById { get; }

  public int Count => GlobalEntries.Length + EntriesByNpcNetId.Values.Sum(entries => entries.Length);
}
