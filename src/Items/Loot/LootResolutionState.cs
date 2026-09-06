using System;

namespace Terraria.Items.Loot;

// status: proposed
public sealed class LootResolutionState
{
  public LootResolutionState(
    int lootTableId = 0,
    LootSourceKind sourceKind = LootSourceKind.Unknown,
    bool hasResolvedLoot = false,
    long resolutionSequence = 0,
    long? resolvedAtTick = null,
    string? resolutionKey = null,
    LootCommitState commitState = LootCommitState.Unresolved)
  {
    if (lootTableId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lootTableId));
    }

    if (resolutionSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(resolutionSequence));
    }

    if (resolvedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(resolvedAtTick));
    }

    if (resolutionKey is not null
      && string.IsNullOrWhiteSpace(resolutionKey))
    {
      throw new ArgumentException(
        "A resolution key must contain non-whitespace characters.",
        nameof(resolutionKey));
    }

    if (!hasResolvedLoot && commitState == LootCommitState.Committed)
    {
      throw new InvalidOperationException(
        "Unresolved loot cannot be committed.");
    }

    if (commitState == LootCommitState.Committed
      && resolutionSequence == 0)
    {
      throw new InvalidOperationException(
        "Committed loot requires a positive resolution sequence.");
    }

    LootTableId = lootTableId;
    SourceKind = sourceKind;
    HasResolvedLoot = hasResolvedLoot;
    ResolutionSequence = resolutionSequence;
    ResolvedAtTick = resolvedAtTick;
    ResolutionKey = resolutionKey;
    CommitState = commitState;
  }

  public int LootTableId { get; }

  public LootSourceKind SourceKind { get; }

  public bool HasResolvedLoot { get; }

  public long ResolutionSequence { get; }

  public long? ResolvedAtTick { get; }

  public string? ResolutionKey { get; }

  public LootCommitState CommitState { get; }
}
