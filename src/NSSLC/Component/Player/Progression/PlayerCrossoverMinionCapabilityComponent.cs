namespace Terraria.Player.Progression;

// status: local-rebuild-verified; production-integration: unknown
// crossSubsystemOwner: integration-review for crossover content definition and source mapping
// source-members: deadCellsMushroomBoiMinion, palworldCattivaMinion, palworldFoxsparksMinion
/// <summary>
/// 保存玩家联动召唤物的能力标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：deadCellsMushroomBoiMinion（第 886 行）； palworldCattivaMinion（第 888 行）；
/// palworldFoxsparksMinion（第 890 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 213 行。</para>
/// </remarks>
public sealed class PlayerCrossoverMinionCapabilityComponent
{
  public bool DeadCellsMushroomBoiMinion { get; internal set; }

  public bool PalworldCattivaMinion { get; internal set; }

  public bool PalworldFoxsparksMinion { get; internal set; }
}
