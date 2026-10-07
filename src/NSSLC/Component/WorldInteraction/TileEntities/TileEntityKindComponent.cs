namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体种类。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：type（第 31 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md。
/// </para>
/// <para>依据位置：第 513 行。</para>
/// </remarks>
public sealed class TileEntityKindComponent
{
  public TileEntityKindId Kind { get; internal set; }
}
