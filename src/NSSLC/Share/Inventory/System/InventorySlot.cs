using System;
using Terraria.Share.Items;

namespace Terraria.Share.Inventory;

/// <summary>A slot owns a reference to one item record, or nothing.</summary>
/// <remarks>
/// <para>
/// 合并为单一 Item 域后，容器槽**不再持有载荷本体**，只持有指向 <see cref="ItemStore"/> 中
/// 那条 Dormant 记录的地址。数量与身份的权威仍然只有一份——在记录上——因此
/// "世界扣多少、背包收多少"不再是两个存储之间的对账，而是同一条记录改址。
/// </para>
/// <para>
/// 地址用 <see cref="ItemContainerAddress"/> 表达：宿主号由容器域决定（玩家 / 箱子 / 银行各有
/// 自己的编号），槽索引沿用库存槽定义里的 <see cref="InventorySlotDefinition.Index"/>。
/// </para>
/// </remarks>
public readonly record struct InventorySlot {
  internal InventorySlot(
      InventorySlotDefinition definition,
      ItemContainerAddress? content = null) {
    if (content is ItemContainerAddress address &&
        (address.Container <= 0 || address.Slot < 0)) {
      throw new ArgumentException("A slot reference must name a container and a slot.");
    }

    Definition = definition;
    Content = content;
  }

  public InventorySlotDefinition Definition { get; }

  /// <summary>Address of the item record this slot refers to; <c>null</c> when empty.</summary>
  public ItemContainerAddress? Content { get; }

  public bool IsEmpty => Content is null;

  /// <summary>True when this slot refers to the supplied record.</summary>
  public bool References(ItemContainerAddress address) => Content == address;
}
