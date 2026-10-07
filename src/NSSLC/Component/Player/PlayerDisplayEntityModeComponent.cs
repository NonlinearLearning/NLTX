namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-741, P09-742
// crossSubsystemOwner: display entity setup and rendering remain integration-review
/// <summary>
/// 保存玩家作为人体模型或帽架展示替身的模式。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：isDisplayDollOrInanimate（第 1079 行）； isHatRackDoll（第 1081 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 166 行。</para>
/// </remarks>
public sealed class PlayerDisplayEntityModeComponent
{
  public bool IsDisplayDollOrInanimate { get; internal set; }

  public bool IsHatRackDoll { get; internal set; }
}
