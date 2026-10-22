using System.Collections.ObjectModel;
using Terraria.Items;

namespace Terraria.WorldInteraction.Structures;

/// <summary>
/// 保存多槽位结构承载的物品状态列表和容量记录。
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
/// <para>
/// 用途：容器或多物品结构流程读取槽位物品及容量，再执行展示、转移或存取规则。
/// </para>
/// <para>
/// 当前实现：构造空列表并提供同一列表的只读视图。ItemCapacity 独立保存，不自动扩充列表；本文件尚未提供写入物品的方法。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C174，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class MultiSlotItemPayloadComponent
{
  private readonly List<ItemState> _items = new();
  private readonly ReadOnlyCollection<ItemState> _itemsView;

  public MultiSlotItemPayloadComponent()
  {
    _itemsView = _items.AsReadOnly();
  }

  /// <summary>
  /// 内部物品状态列表的只读视图；构造时为空。
  /// </summary>
  public IReadOnlyList<ItemState> Items => _itemsView;

  /// <summary>
  /// 声明的物品槽位容量，不等同于当前列表数量。
  /// </summary>
  public int ItemCapacity { get; internal set; }
}
