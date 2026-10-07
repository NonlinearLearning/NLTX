namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for client input, UI and selection command routing
/// <summary>
/// 保存玩家界面捕获输入和禁止投掷状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：creativeInterface（第 997 行）； mouseInterface（第 999 行）； lastMouseInterface（第 1001 行）；
/// noThrow（第 1003 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 257 行。</para>
/// </remarks>
public struct PlayerInteractionUiStateComponent
{
  public bool CreativeInterface;
  public bool MouseInterface;
  public bool LastMouseInterface;
  public int NoThrow;
}
