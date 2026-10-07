namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体的旧运行编号。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：ID（第 27 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 482 行。</para>
/// </remarks>
public sealed class TileEntityRuntimeIdComponent
{
  public TileEntityRuntimeId Id { get; internal set; }
}
