namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存拴系实体锚点携带的物品类型。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TELeashedEntityAnchorWithItem。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TELeashedEntityAnchorWithItem.cs。
/// </para>
/// <para>主要源成员：itemType（第 8 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md。
/// </para>
/// <para>依据位置：第 566 行。</para>
/// </remarks>
public sealed class LeashedEntityAnchorComponent
{
  public int ItemType { get; internal set; }
  public bool HasItem => ItemType > 0;
}
