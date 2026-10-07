using System.Collections.ObjectModel;
using Terraria.Items;

namespace Terraria.WorldInteraction.Structures;

/// <summary>
/// 保存箱子或结构的多槽位物品载荷。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Chest。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Chest.cs。</para>
/// <para>主要源成员：item（第 42 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 979 行。</para>
/// </remarks>
public sealed class MultiSlotItemPayloadComponent
{
  private readonly List<ItemState> _items = new();
  private readonly ReadOnlyCollection<ItemState> _itemsView;

  public MultiSlotItemPayloadComponent()
  {
    _itemsView = _items.AsReadOnly();
  }

  public IReadOnlyList<ItemState> Items => _itemsView;

  public int ItemCapacity { get; internal set; }
}
