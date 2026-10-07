namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Jump, Wing, Grapple, Mount, Spatial, and presentation
/// <summary>
/// 保存飞毯能力和剩余使用时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：carpet（第 722 行）； canCarpet（第 730 行）； carpetTime（第 732 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 569 行。</para>
/// </remarks>
public sealed class PlayerCarpetTraversalStateComponent
{
  public bool HasCarpet { get; set; }

  public bool CanStart { get; set; }

  public int RemainingTime { get; set; }
}
