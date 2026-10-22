namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1406
// crossSubsystemOwner: tile interaction command and scheduler remain integration-review
/// <summary>
/// 保存玩家方块交互的输入释放标记和锁定计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：releaseUseTile（第 1251 行）； _lockTileInteractionsTimer（第 2471 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 14 行。</para>
/// <para>
/// 用途：方块交互流程读取释放标记，避免按住输入时重复触发，并用锁定计时协调暂时禁止交互的阶段。
/// </para>
/// <para>
/// 当前实现：两个属性只保存状态。输入边沿判断、计时递减和具体交互许可由外部流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C127，L2（通用可选玩法）。分级理由：挖掘、放置、相邻方块、范围和交互锁是可选建造玩法。
/// </para>
/// </remarks>
public sealed class PlayerInteractionLockStateComponent
{
  /// <summary>
  /// 使用方块的输入释放状态，供交互触发流程读取。
  /// </summary>
  public bool ReleaseUseTile { get; internal set; }
  /// <summary>
  /// 方块交互锁定计时，由外部流程推进。
  /// </summary>
  public int LockTileInteractionsTimer { get; internal set; }
}
