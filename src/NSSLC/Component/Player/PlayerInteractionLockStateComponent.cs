namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1406
// crossSubsystemOwner: tile interaction command and scheduler remain integration-review
/// <summary>
/// 保存玩家使用方块的释放标记和交互锁定计时。
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
/// </remarks>
public sealed class PlayerInteractionLockStateComponent
{
  public bool ReleaseUseTile { get; internal set; }
  public int LockTileInteractionsTimer { get; internal set; }
}
