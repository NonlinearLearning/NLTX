using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class DropRuleCatalog
{
  private readonly ImmutableDictionary<int, ImmutableArray<DropRuleReference>>
    _entriesByNpcNetId;
  private readonly ImmutableDictionary<int, ImmutableArray<int>> _npcNetIdsByType;

  public DropRuleCatalog(
    IEnumerable<DropRuleReference> globalEntries,
    IReadOnlyDictionary<int, IReadOnlyList<DropRuleReference>> entriesByNpcNetId,
    IReadOnlyDictionary<int, IReadOnlyList<int>> npcNetIdsByType,
    int masterModeDropRng)
  {
    ArgumentNullException.ThrowIfNull(globalEntries);
    ArgumentNullException.ThrowIfNull(entriesByNpcNetId);
    ArgumentNullException.ThrowIfNull(npcNetIdsByType);
    if (masterModeDropRng < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(masterModeDropRng));
    }

    GlobalEntries = SnapshotRules(globalEntries, nameof(globalEntries));
    _entriesByNpcNetId = SnapshotRuleMap(entriesByNpcNetId);
    _npcNetIdsByType = SnapshotNetIdMap(npcNetIdsByType);
    MasterModeDropRng = masterModeDropRng;
    Count = GlobalEntries.Length + _entriesByNpcNetId.Sum(pair => pair.Value.Length);
  }

  public ImmutableArray<DropRuleReference> GlobalEntries { get; }

  public ImmutableDictionary<int, ImmutableArray<DropRuleReference>> EntriesByNpcNetId =>
    _entriesByNpcNetId;

  public ImmutableDictionary<int, ImmutableArray<int>> NpcNetIdsByType =>
    _npcNetIdsByType;

  public int MasterModeDropRng { get; }

  public int Count { get; }

  public ImmutableArray<DropRuleReference> GetEntriesForNpcNetId(int npcNetId)
  {
    return _entriesByNpcNetId.TryGetValue(npcNetId, out ImmutableArray<DropRuleReference> entries)
      ? entries
      : [];
  }

  public ImmutableArray<int> GetNpcNetIdsForType(int npcTypeId)
  {
    return _npcNetIdsByType.TryGetValue(npcTypeId, out ImmutableArray<int> netIds)
      ? netIds
      : [];
  }

  private static ImmutableArray<DropRuleReference> SnapshotRules(
    IEnumerable<DropRuleReference> rules,
    string parameterName)
  {
    ImmutableArray<DropRuleReference> snapshot = rules.ToImmutableArray();
    if (snapshot.Any(rule => string.IsNullOrWhiteSpace(rule.RuleId)))
    {
      throw new ArgumentException(
        "Drop rules must have non-empty IDs.",
        parameterName);
    }

    return snapshot;
  }

  private static ImmutableDictionary<int, ImmutableArray<DropRuleReference>> SnapshotRuleMap(
    IReadOnlyDictionary<int, IReadOnlyList<DropRuleReference>> source)
  {
    Dictionary<int, ImmutableArray<DropRuleReference>> snapshot = [];
    foreach ((int npcNetId, IReadOnlyList<DropRuleReference> entries) in source)
    {
      ArgumentNullException.ThrowIfNull(entries);
      snapshot.Add(npcNetId, SnapshotRules(entries, nameof(source)));
    }

    return snapshot.ToImmutableDictionary();
  }

  private static ImmutableDictionary<int, ImmutableArray<int>> SnapshotNetIdMap(
    IReadOnlyDictionary<int, IReadOnlyList<int>> source)
  {
    Dictionary<int, ImmutableArray<int>> snapshot = [];
    foreach ((int npcTypeId, IReadOnlyList<int> netIds) in source)
    {
      if (npcTypeId < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(source));
      }

      ArgumentNullException.ThrowIfNull(netIds);
      if (netIds.Any(netId => netId == 0))
      {
        throw new ArgumentException(
          "NPC net ID mappings cannot contain the default net ID.",
          nameof(source));
      }

      snapshot.Add(npcTypeId, netIds.ToImmutableArray());
    }

    return snapshot.ToImmutableDictionary();
  }
}
