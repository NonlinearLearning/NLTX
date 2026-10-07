namespace Terraria.Player.Jump;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Spatial, Mount, Presentation, and effects
/// <summary>
/// 保存当前正在执行的额外跳跃或下冲动作。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：isPerformingPogostickTricks（第 2114 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 788 行。</para>
/// </remarks>
public sealed class PlayerJumpExecutionComponent
{
  public bool IsPerformingDownDash { get; set; }

  public bool IsPerformingCloud { get; set; }

  public bool IsPerformingSandstorm { get; set; }

  public bool IsPerformingBlizzard { get; set; }

  public bool IsPerformingFart { get; set; }

  public bool IsPerformingSail { get; set; }

  public bool IsPerformingUnicorn { get; set; }

  public bool IsPerformingSantank { get; set; }

  public bool IsPerformingWallOfFleshGoat { get; set; }

  public bool IsPerformingBasilisk { get; set; }

  public bool IsPerformingPogostickTricks { get; set; }
}
