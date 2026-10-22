namespace Terraria.Player;

/// <summary>
/// 保存玩家挖掘、放置墙和放置方块的速度修正输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：pickSpeed（第 1885 行）； wallSpeed（第 1887 行）； tileSpeed（第 1889 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1050 行。</para>
/// <para>
/// 用途：装备与效果流程汇总三个修正值，挖掘和建造流程再按各自的耗时公式读取。
/// </para>
/// <para>
/// 当前实现：三个输入默认都是 1，ResetEffects 恢复到 1。实际速度快慢和物品使用时间由消费这些数值的流程决定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C113，L2（通用可选玩法）。分级理由：挖掘、放置、相邻方块、范围和交互锁是可选建造玩法。
/// </para>
/// </remarks>
public sealed class PlayerBuildAndMiningSpeedComponent
{
  /// <summary>
  /// 挖掘速度或耗时修正输入，基础值 1。
  /// </summary>
  public float PickSpeed { get; internal set; } = 1f;

  /// <summary>
  /// 放置墙速度修正输入，基础值 1。
  /// </summary>
  public float WallSpeed { get; internal set; } = 1f;

  /// <summary>
  /// 放置方块速度修正输入，基础值 1。
  /// </summary>
  public float TileSpeed { get; internal set; } = 1f;

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    PickSpeed = 1f;
    WallSpeed = 1f;
    TileSpeed = 1f;
  }
}
