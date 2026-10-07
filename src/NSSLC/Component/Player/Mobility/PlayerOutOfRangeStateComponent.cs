namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for network, visibility, and local-authority scheduling
/// <summary>
/// 保存玩家是否超出有效范围。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：outOfRange（第 640 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 411 行。</para>
/// </remarks>
public sealed class PlayerOutOfRangeStateComponent
{
  public bool IsOutOfRange { get; set; }
}
