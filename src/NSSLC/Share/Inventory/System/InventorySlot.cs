using System;
using Terraria.Share.Items;

namespace Terraria.Share.Inventory;

/// <summary>A slot owns one value stack or nothing.</summary>
/// <remarks>
/// 容器宿主只承载载荷值。物品实体的身份引用（`EntityUuid`）已在
/// [2026-11-23 执行计划](../../../../docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md)
/// 的 P1-A 中移除：那条路径没有任何生产者，保留它等于允许同一物品在世界与容器两端各存一份权威。
/// 世界中的物品由 `Terraria.World` 的 `WorldItemStore` 承载，两者只经请求组件移交数量。
/// </remarks>
public readonly record struct InventorySlot {
  internal InventorySlot(
      InventorySlotDefinition definition,
      ItemStack? content = null) {
    if (content.HasValue && content.Value.IsEmpty) {
      throw new ArgumentException("A slot value must be a nonempty stack.");
    }

    Definition = definition;
    Content = content;
  }

  public InventorySlotDefinition Definition { get; }

  public ItemStack? Content { get; }

  public bool IsEmpty => !Content.HasValue;
}