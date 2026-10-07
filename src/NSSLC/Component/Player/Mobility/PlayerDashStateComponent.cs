namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for input, equipment, Spatial, effects, network, and persistence
/// <summary>
/// 保存冲刺种类、当前冲刺和恢复计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：dashType（第 678 行）； dashTime（第 682 行）； timeSinceLastDashStarted（第 684 行）； dashDelay（第 686
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 413 行。</para>
/// </remarks>
public sealed class PlayerDashStateComponent
{
  public int DashType { get; set; }

  public int ActiveDash { get; set; }

  public int DashTime { get; set; }

  public int TimeSinceLastDashStarted { get; set; }

  public int DashDelay { get; set; }
}
