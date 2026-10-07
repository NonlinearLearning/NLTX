namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for ItemCheck and reuse scheduling
/// <summary>
/// 保存玩家物品重复使用的延迟和待重用标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：reuseDelay（第 991 行）； pendingItemReuse（第 1007 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 254 行。</para>
/// </remarks>
public struct PlayerItemReuseStateComponent
{
  public int ReuseDelayRemainingTicks;
  public bool PendingItemReuse;
}
