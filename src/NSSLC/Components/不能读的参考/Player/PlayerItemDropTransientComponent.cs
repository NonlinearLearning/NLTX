namespace Terraria.Player;

/// <summary>
/// 保存玩家刚刚丢弃物品的短期状态标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：JustDroppedAnItem（第 1847 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 949 行。</para>
/// <para>
/// 用途：物品丢弃与后续交互流程登记并读取这一事实，协调紧接着发生的拾取或物品使用行为。
/// </para>
/// <para>
/// 当前实现：只保存 JustDroppedAnItem。ResetForLifecycle 将其清为 false；标记在正常更新中何时消耗或清除由外部流程决定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C129，L2（通用可选玩法）。分级理由：这些类型当前未被消费。现模拟路径直接计算瞄准方向，丢弃也不要求独立瞬时标记。
/// </para>
/// </remarks>
public sealed class PlayerItemDropTransientComponent
{
  /// <summary>
  /// 玩家是否刚刚丢弃过物品。
  /// </summary>
  public bool JustDroppedAnItem { get; internal set; }

  /// <summary>
  /// 清除刚丢弃物品的标记，用于初始化、重生或复用。
  /// </summary>
  internal void ResetForLifecycle()
  {
    JustDroppedAnItem = false;
  }
}
