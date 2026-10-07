namespace Terraria.Items;

/// <summary>
/// 保存容器锁定、访问条件和当前访问租约。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Chest 的锁定检查、解锁和玩家访问流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Chest.cs。</para>
/// <para>重组说明：访问者实体、访问租约和权限位是容器访问模型新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-item-container-and-economy-component-code-draft.md。
/// </para>
/// <para>依据位置：第 495 行。</para>
/// </remarks>
public sealed class ContainerAccessComponent
{
  public ContainerAccessComponent(
    PersistentContainerId persistentContainerId,
    TileCoordinates? tilePosition = null,
    bool isLocked = false,
    ulong requiredAccessFlags = 0,
    RuntimeEntityId? currentAccessor = null,
    long? accessLeaseUntilTick = null,
    bool isShared = true)
  {
    PersistentContainerId = persistentContainerId;
    TilePosition = tilePosition;
    IsLocked = isLocked;
    RequiredAccessFlags = requiredAccessFlags;
    CurrentAccessor = currentAccessor;
    AccessLeaseUntilTick = accessLeaseUntilTick;
    IsShared = isShared;
  }

  public PersistentContainerId PersistentContainerId;
  public TileCoordinates? TilePosition;
  public bool IsLocked;
  public ulong RequiredAccessFlags;
  public RuntimeEntityId? CurrentAccessor;
  public long? AccessLeaseUntilTick;
  public bool IsShared;

  public bool HasActiveAccessorAt(long currentTick) =>
    CurrentAccessor.HasValue &&
    AccessLeaseUntilTick.HasValue &&
    currentTick < AccessLeaseUntilTick.Value;
}
