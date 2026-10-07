namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Jump, Carpet, Grapple, Mount, Spatial, and persistence
/// <summary>
/// 保存翅膀飞行剩余时间和上限。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：wingTime（第 892 行）； wingTimeMax（第 898 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 639 行。</para>
/// </remarks>
public sealed class PlayerFlightStateComponent
{
  public float WingTime { get; set; }

  public int WingTimeMax { get; set; }
}
