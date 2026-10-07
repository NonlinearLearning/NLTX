namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存食物盘上的物品。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TEFoodPlatter。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TEFoodPlatter.cs。
/// </para>
/// <para>主要源成员：item（第 9 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-code-draft.md。
/// </para>
/// <para>依据位置：第 56 行。</para>
/// </remarks>
public sealed class FoodPlatterComponent
{
  public StoredItemState Item { get; internal set; }
}
