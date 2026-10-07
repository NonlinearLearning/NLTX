namespace Terraria.WorldInteraction.Interaction;

/// <summary>
/// 保存本次世界交互的发起玩家。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Wiring。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Wiring.cs。</para>
/// <para>主要源成员：CurrentUser（第 67 行）。</para>
/// <para>重组说明：将原静态 CurrentUser 表达为单次交互的发起者上下文。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 71 行。</para>
/// </remarks>
public sealed class InteractionActorContextComponent
{
  // null 表示没有可确认的 Version4 玩家索引。
  // Version4 的兼容无调用者值为 255，但不把 255 暴露为有效玩家。
  public byte? InitiatingPlayerIndex { get; internal set; }
}
