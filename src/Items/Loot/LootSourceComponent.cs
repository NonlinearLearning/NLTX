namespace Terraria.Items.Loot;

public sealed class LootSourceComponent
{
  public LootSourceComponent(
    int lootTableId = 0,
    LootSourceKind sourceKind = LootSourceKind.Unknown,
    bool hasResolvedLoot = false,
    long resolutionSequence = 0,
    long? resolvedAtTick = null)
  {
    LootTableId = lootTableId;
    SourceKind = sourceKind;
    HasResolvedLoot = hasResolvedLoot;
    ResolutionSequence = resolutionSequence;
    ResolvedAtTick = resolvedAtTick;
  }

  public int LootTableId;
  public LootSourceKind SourceKind;
  public bool HasResolvedLoot;
  public long ResolutionSequence;
  public long? ResolvedAtTick;

  public bool CanResolveLoot => LootTableId > 0 && !HasResolvedLoot;
}
