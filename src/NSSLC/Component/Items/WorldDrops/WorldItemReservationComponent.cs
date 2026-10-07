namespace Terraria.Items;

/// <summary>
/// 保存世界掉落物的领取预留和各类拾取限制时限。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldItem。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldItem.cs。</para>
/// <para>
/// 主要源成员：ownTime（第 19 行）； playerIndexTheItemIsReservedFor（第 21 行）； noGrabDelay（第 23 行）；
/// ownIgnore（第 31 行）； timeSinceTheItemHasBeenReservedForSomeone（第 33 行）；
/// timeLeftInWhichTheItemCannotBeTakenByEnemies（第 35 行）。
/// </para>
/// <para>重组说明：预留 UUID、实体关系、版本和绝对到期时刻是领取模型重组后的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 521 行。</para>
/// </remarks>
public sealed class WorldItemReservationComponent
{
  public WorldItemReservationComponent(
    ReservationId? reservationId = null,
    RuntimeEntityId? reservedFor = null,
    long? reservationExpiresAt = null,
    RuntimeEntityId? ignoreOwner = null,
    long? ignoreOwnerUntilTick = null,
    long noGrabUntilTick = 0,
    long enemyPickupBlockedUntilTick = 0,
    long reservationRevision = 0)
  {
    ReservationId = reservationId;
    ReservedFor = reservedFor;
    ReservationExpiresAt = reservationExpiresAt;
    IgnoreOwner = ignoreOwner;
    IgnoreOwnerUntilTick = ignoreOwnerUntilTick;
    NoGrabUntilTick = noGrabUntilTick;
    EnemyPickupBlockedUntilTick = enemyPickupBlockedUntilTick;
    ReservationRevision = reservationRevision;
  }

  public ReservationId? ReservationId;
  public RuntimeEntityId? ReservedFor;
  public long? ReservationExpiresAt;
  public RuntimeEntityId? IgnoreOwner;
  public long? IgnoreOwnerUntilTick;
  public long NoGrabUntilTick;
  public long EnemyPickupBlockedUntilTick;
  public long ReservationRevision;

  public bool HasReservationAt(long currentTick) =>
    ReservedFor.HasValue &&
    ReservationExpiresAt.HasValue &&
    currentTick < ReservationExpiresAt.Value;

  public bool CanBeGrabbedAt(long currentTick) =>
    currentTick >= NoGrabUntilTick;

  public bool IsOwnerIgnoredAt(long currentTick) =>
    IgnoreOwner.HasValue &&
    IgnoreOwnerUntilTick.HasValue &&
    currentTick < IgnoreOwnerUntilTick.Value;
}
