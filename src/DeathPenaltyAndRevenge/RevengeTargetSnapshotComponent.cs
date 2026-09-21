using Terraria.Items;
using Terraria.Npc;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeTargetSnapshotComponent
{
  private readonly RevengeMarkerValueComponent _valueState;
  private readonly NpcTypeId _discouragementNpcTypeId;

  public RevengeTargetSnapshotComponent(
    WorldPosition location,
    NpcNetId npcNetId,
    NpcTypeId npcTypeId,
    int legacyAiStyle,
    float lifeFraction,
    int coinValue,
    float baseValue,
    bool spawnedFromStatue)
    : this(
      location,
      npcNetId,
      npcTypeId,
      npcTypeId,
      legacyAiStyle,
      lifeFraction,
      coinValue,
      baseValue,
      spawnedFromStatue)
  {
  }

  private RevengeTargetSnapshotComponent(
    WorldPosition location,
    NpcNetId spawnNpcNetId,
    NpcTypeId snapshotNpcTypeId,
    NpcTypeId discouragementNpcTypeId,
    int legacyAiStyle,
    float lifeFraction,
    int coinValue,
    float baseValue,
    bool spawnedFromStatue)
  {
    Location = location;
    NpcNetId = spawnNpcNetId;
    NpcTypeId = snapshotNpcTypeId;
    _discouragementNpcTypeId = discouragementNpcTypeId;
    LegacyAiStyle = legacyAiStyle;
    LifeFraction = lifeFraction;
    _valueState = new RevengeMarkerValueComponent(baseValue, coinValue);
    SpawnedFromStatue = spawnedFromStatue;
  }

  public static RevengeTargetSnapshotComponent Capture(
    WorldPosition location,
    NpcNetId spawnNpcNetId,
    NpcTypeId discouragementNpcTypeId,
    int legacyAiStyle,
    float lifeFraction,
    int coinValue,
    float baseValue,
    bool spawnedFromStatue)
  {
    return new RevengeTargetSnapshotComponent(
      location,
      spawnNpcNetId,
      discouragementNpcTypeId,
      discouragementNpcTypeId,
      legacyAiStyle,
      lifeFraction,
      coinValue,
      baseValue,
      spawnedFromStatue);
  }

  public WorldPosition Location { get; }

  public RevengeProximityBox EnemyHitbox =>
    RevengeEnemyContextPolicy.CreateEnemyHitbox(Location);

  public NpcNetId NpcNetId { get; }

  public NpcNetId SpawnNpcNetId => NpcNetId;

  public NpcTypeId NpcTypeId { get; }

  public NpcTypeId DiscouragementNpcTypeId => _discouragementNpcTypeId;

  public int LegacyAiStyle { get; }

  public int DiscouragementAiStyle => LegacyAiStyle;

  public float LifeFraction { get; }

  public float CapturedLifeRatio => LifeFraction;

  public int CoinValue => _valueState.CoinsValue;

  public float BaseValue => _valueState.BaseValue;

  public RevengeMarkerValueComponent ValueState => _valueState;

  public bool SpawnedFromStatue { get; }
}
