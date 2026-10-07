namespace Terraria.Player;

/// <summary>
/// 保存玩家召唤物、炮塔容量及综合能力标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：maxMinions（第 832 行）； numMinions（第 834 行）； slotsMinions（第 836 行）； maxTurrets（第 2244 行）；
/// maxTurretsOld（第 2246 行）。
/// </para>
/// <para>重组说明：FeatureFlags 是 NLTX 综合能力位集；各套装的运行状态在专用组件中保存。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 175 行。</para>
/// </remarks>
public sealed class PlayerAbilityComponent
{
  public float MaximumMinionSlots { get; set; } = 1;

  public float UsedMinionSlots { get; set; }

  public int MinionCount { get; set; }

  public int MaximumTurrets { get; set; } = 1;

  public int PreviousMaximumTurrets { get; set; } = 1;

  public MinionFeatureFlags FeatureFlags { get; set; }
}
