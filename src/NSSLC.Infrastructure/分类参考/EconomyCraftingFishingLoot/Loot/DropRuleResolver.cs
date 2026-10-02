using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Content;

namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class DropRuleResolver
{
  private readonly DropRuleCatalog _catalog;

  public DropRuleResolver(DropRuleCatalog catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public ImmutableArray<DropRuleReference> Resolve(DropResolutionContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    ImmutableArray<DropRuleReference>.Builder resolved =
      ImmutableArray.CreateBuilder<DropRuleReference>();
    resolved.AddRange(_catalog.GlobalEntries);

    HashSet<int> visitedNpcNetIds = [];
    AddNpcEntries(context.NpcNetId, visitedNpcNetIds, resolved);
    foreach (int npcNetId in _catalog.GetNpcNetIdsForType(context.NpcTypeId))
    {
      AddNpcEntries(npcNetId, visitedNpcNetIds, resolved);
    }

    return resolved.ToImmutable();
  }

  private void AddNpcEntries(
    int npcNetId,
    HashSet<int> visitedNpcNetIds,
    ImmutableArray<DropRuleReference>.Builder resolved)
  {
    if (!visitedNpcNetIds.Add(npcNetId))
    {
      return;
    }

    resolved.AddRange(_catalog.GetEntriesForNpcNetId(npcNetId));
  }
}
