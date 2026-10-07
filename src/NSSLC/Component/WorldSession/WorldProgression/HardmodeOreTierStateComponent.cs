namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存困难模式矿石层级的权威选择。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen.SavedOreTiers。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：Cobalt（第 3328 行）； Mythril（第 3330 行）； Adamantite（第 3332 行）。</para>
/// <para>重组说明：困难模式矿石选择以专用值类型组合保存。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-world-progression-and-transition-actual-code-component-draft.md。
/// </para>
/// <para>依据位置：第 384 行。</para>
/// </remarks>
public sealed class HardmodeOreTierStateComponent
{
  public HardmodeOreTierState Value { get; set; } = HardmodeOreTierState.Uninitialized;
}
