using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存帽架物品和染色槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TEHatRack。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TEHatRack.cs。</para>
/// <para>主要源成员：_items（第 21 行）； _dyes（第 23 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 774 行。</para>
/// </remarks>
public sealed class HatRackComponent
{
  private readonly StoredItemState[] _items = new StoredItemState[2];
  private readonly StoredItemState[] _dyes = new StoredItemState[2];
  private readonly ReadOnlyCollection<StoredItemState> _itemsView;
  private readonly ReadOnlyCollection<StoredItemState> _dyesView;

  public HatRackComponent()
  {
    _itemsView = Array.AsReadOnly(_items);
    _dyesView = Array.AsReadOnly(_dyes);
  }

  public IReadOnlyList<StoredItemState> Items => _itemsView;
  public IReadOnlyList<StoredItemState> Dyes => _dyesView;
  public bool ContainsItems =>
      _items.Any(item => !item.IsEmpty) ||
      _dyes.Any(item => !item.IsEmpty);
}
