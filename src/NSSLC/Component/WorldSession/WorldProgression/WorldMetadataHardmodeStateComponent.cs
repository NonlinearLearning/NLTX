namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存世界持久元数据中的困难模式标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.IO.WorldFileData。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.IO/WorldFileData.cs。</para>
/// <para>主要源成员：IsHardMode（第 67 行）。</para>
/// <para>重组说明：持久世界身份用于明确元数据归属。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-world-progression-and-transition-actual-code-component-draft.md。
/// </para>
/// <para>依据位置：第 588 行。</para>
/// </remarks>
public sealed class WorldMetadataHardmodeStateComponent
{
  public PersistentWorldId? PersistentWorldId { get; set; }
  public bool IsHardMode { get; set; }
}
