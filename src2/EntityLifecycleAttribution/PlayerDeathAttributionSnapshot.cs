namespace Terraria.EntityLifecycleAttribution;

public readonly record struct PlayerDeathAttributionSnapshot
{
  public PlayerDeathAttributionSnapshot(
    int sourcePlayerIndex,
    int sourceNpcIndex,
    int sourceProjectileLocalIndex,
    int sourceOtherIndex,
    int sourceProjectileType,
    int sourceItemType,
    int sourceItemPrefix,
    string? customReason)
  {
    if (sourcePlayerIndex < -1 ||
        sourceNpcIndex < -1 ||
        sourceProjectileLocalIndex < -1 ||
        sourceOtherIndex < -1 ||
        sourceProjectileType < 0 ||
        sourceItemType < 0 ||
        sourceItemPrefix < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sourcePlayerIndex));
    }

    SourcePlayerIndex = sourcePlayerIndex;
    SourceNpcIndex = sourceNpcIndex;
    SourceProjectileLocalIndex = sourceProjectileLocalIndex;
    SourceOtherIndex = sourceOtherIndex;
    SourceProjectileType = sourceProjectileType;
    SourceItemType = sourceItemType;
    SourceItemPrefix = sourceItemPrefix;
    CustomReason = customReason;
  }

  public int SourcePlayerIndex { get; }

  public int SourceNpcIndex { get; }

  public int SourceProjectileLocalIndex { get; }

  public int SourceOtherIndex { get; }

  public int SourceProjectileType { get; }

  public int SourceItemType { get; }

  public int SourceItemPrefix { get; }

  public string? CustomReason { get; }
}
