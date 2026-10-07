namespace Terraria.Player.Jump;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Buff, Mount, Mobility, and persistence
/// <summary>
/// 保存各类额外跳跃的能力和剩余可用状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：canJumpAgain_Cloud（第 2062 行）； canJumpAgain_Sandstorm（第 2068 行）； canJumpAgain_Blizzard（第
/// 2074 行）； canJumpAgain_Fart（第 2080 行）； canJumpAgain_Sail（第 2086 行）； canJumpAgain_Unicorn（第 2092
/// 行）； canJumpAgain_Santank（第 2098 行）； canJumpAgain_WallOfFleshGoat（第 2104 行）；
/// canJumpAgain_Basilisk（第 2110 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 708 行。</para>
/// </remarks>
public sealed class PlayerJumpAvailabilityComponent
{
  public bool HasCloudOption { get; set; }

  public bool CanJumpAgainCloud { get; set; }

  public bool HasSandstormOption { get; set; }

  public bool CanJumpAgainSandstorm { get; set; }

  public bool HasBlizzardOption { get; set; }

  public bool CanJumpAgainBlizzard { get; set; }

  public bool HasFartOption { get; set; }

  public bool CanJumpAgainFart { get; set; }

  public bool HasSailOption { get; set; }

  public bool CanJumpAgainSail { get; set; }

  public bool HasUnicornOption { get; set; }

  public bool CanJumpAgainUnicorn { get; set; }

  public bool HasSantankOption { get; set; }

  public bool CanJumpAgainSantank { get; set; }

  public bool HasWallOfFleshGoatOption { get; set; }

  public bool CanJumpAgainWallOfFleshGoat { get; set; }

  public bool HasBasiliskOption { get; set; }

  public bool CanJumpAgainBasilisk { get; set; }
}
