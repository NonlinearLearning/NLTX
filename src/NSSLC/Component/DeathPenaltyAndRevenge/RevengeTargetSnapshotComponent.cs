using Terraria.Items;
using Terraria.Npc;

namespace Terraria.DeathPenaltyAndRevenge;

/// <summary>
/// 保存复仇目标的位置、类型、生命比例、金币和生成来源快照。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/CoinLossRevengeSystem.cs。</para>
/// <para>
/// 主要源成员：_location（第 36 行）； _hitbox（第 38 行）； _npcNetID（第 40 行）； _npcHPPercent（第 42 行）；
/// _baseValue（第 44 行）； _coinsValue（第 46 行）； _npcTypeAgainstDiscouragement（第 48 行）；
/// _npcAIStyleAgainstDiscouragement（第 50 行）； _spawnedFromStatue（第 54 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 194 行。</para>
/// </remarks>
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
