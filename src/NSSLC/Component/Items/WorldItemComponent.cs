namespace Terraria.Items;

/// <summary>
/// 保存世界掉落物的寿命、拾取、传送带和微光状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldItem。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldItem.cs。</para>
/// <para>
/// 主要源成员：shimmered（第 25 行）； shimmerTime（第 27 行）； instanced（第 29 行）；
/// timeLeftInWhichTheItemCannotBeTakenByEnemies（第 35 行）； timeSinceItemSpawned（第 37 行）；
/// beingGrabbed（第 39 行）； onConveyor（第 41 行）； keepTime（第 43 行）。
/// </para>
/// <para>重组说明：Revision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-design.md。
/// </para>
/// <para>依据位置：第 200 行。</para>
/// </remarks>
public sealed class WorldItemComponent
{
  public WorldItemComponent(
    long spawnedAtTick,
    long? despawnAtTick = null,
    bool isInstanced = false,
    bool isBeingGrabbed = false,
    bool isOnConveyor = false,
    long revision = 0,
    bool isShimmered = false,
    float shimmerTime = 0.0f,
    byte enemyGrabDelayTime = 0)
  {
    if (spawnedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(spawnedAtTick));
    }

    if (despawnAtTick.HasValue && despawnAtTick.Value < spawnedAtTick)
    {
      throw new ArgumentOutOfRangeException(nameof(despawnAtTick));
    }

    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    if (!float.IsFinite(shimmerTime) || shimmerTime < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(shimmerTime));
    }

    SpawnedAtTick = spawnedAtTick;
    DespawnAtTick = despawnAtTick;
    IsInstanced = isInstanced;
    IsBeingGrabbed = isBeingGrabbed;
    IsOnConveyor = isOnConveyor;
    Revision = revision;
    IsShimmered = isShimmered;
    ShimmerTime = shimmerTime;
    EnemyGrabDelayTime = enemyGrabDelayTime;
  }

  public long SpawnedAtTick;
  public long? DespawnAtTick;
  public bool IsInstanced;
  public bool IsBeingGrabbed;
  public bool IsOnConveyor;
  public long Revision;
  public bool IsShimmered;
  public float ShimmerTime;
  public byte EnemyGrabDelayTime;

  public bool IsExpiredAt(long currentTick) =>
    DespawnAtTick.HasValue && currentTick >= DespawnAtTick.Value;

  public long RemainingTicksAt(long currentTick) =>
    DespawnAtTick.HasValue
      ? Math.Max(0, DespawnAtTick.Value - currentTick)
      : long.MaxValue;
}
