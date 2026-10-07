namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存物品框中的物品。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TEItemFrame。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TEItemFrame.cs。</para>
/// <para>主要源成员：item（第 8 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-code-draft.md。
/// </para>
/// <para>依据位置：第 55 行。</para>
/// </remarks>
public sealed class ItemFrameComponent
{
  public StoredItemState Item { get; internal set; }
}
