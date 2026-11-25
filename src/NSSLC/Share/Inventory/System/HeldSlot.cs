using System;

namespace Terraria.Share.Inventory;

/// <summary>One inventory slot an owner currently holds.</summary>
/// <remarks>
/// Version4 keeps the handheld concept in two places: <c>Player.selectedItemState.Selected</c>
/// (<c>Player.cs:3835</c>) and the derived read <c>Player.HeldItem => inventory[selectedItem]</c>
/// (<c>Player.cs:3837</c>). Packet 13 carries the same byte into
/// <c>player.selectedItemState.Select(...)</c> (<c>MessageBuffer.cs:996</c>), so the selection is a
/// property of the inventory, not of a separate hand component.
/// <para>
/// 服务端把同一关系表达成一个索引值。手持的载荷不在这里：它始终是
/// <c>inventory[Held.Index]</c> 那一格的内容，经 <see cref="InventoryComponent.TryGetHeld"/>
/// 读出，因此不存在第二份物品副本（[物品载荷与单一物品系统设计约束](../../../../docs/architecture/物品载荷与单一物品系统设计约束.md)
/// 的 INV-1/INV-2）。
/// </para>
/// <para>
/// "没有手持任何槽"用 <c>null</c> 表达而不是哨兵下标：<see cref="InventoryComponent.Held"/> 是
/// <see cref="HeldSlot"/>? 类型，默认构造的组件因此天然处于未手持状态，不需要额外的初值约定。
/// </para>
/// </remarks>
public readonly record struct HeldSlot {
  public HeldSlot(int index) {
    if (index < 0) {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    Index = index;
  }

  /// <summary>Index of the held slot inside its inventory.</summary>
  public int Index { get; }

  /// <summary>True when this selection points at the same slot as <paramref name="other"/>.</summary>
  public bool Matches(HeldSlot other) => Index == other.Index;
}
