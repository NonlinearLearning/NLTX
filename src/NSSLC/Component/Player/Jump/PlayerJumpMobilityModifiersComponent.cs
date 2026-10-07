namespace Terraria.Player.Jump;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Spatial, Collision, and persistence
/// <summary>
/// 保存自动跳跃、跳跃增益和额外下落状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：downDashTime（第 2058 行）； autoJump（第 2116 行）； justJumped（第 2118 行）； jumpSpeedBoost（第 2120
/// 行）； extraFall（第 2122 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 861 行。</para>
/// </remarks>
public sealed class PlayerJumpMobilityModifiersComponent
{
  public int DownDashTime { get; set; }

  public bool AutoJump { get; set; }

  public bool JustJumped { get; set; }

  public float JumpSpeedBoost { get; set; }

  public int ExtraFall { get; set; }
}
