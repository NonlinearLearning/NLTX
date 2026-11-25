using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria.Share.Items;

namespace Terraria.Share.Inventory;

/// <summary>Grants inventory capability; InventorySystem coordinates mutations.</summary>
/// <remarks>
/// 手持概念属于这里：Version4 的 <c>Player.HeldItem</c> 就是 <c>inventory[selectedItem]</c>
/// （<c>Player.cs:3837</c>），选中槽只是库存的一个索引，不是独立的手部关系。因此本组件同时
/// 承载槽位值与 <see cref="HeldSlot"/>；被手持的载荷仍然只有槽里那一份，读取一律走
/// <see cref="TryGetHeld"/>。
/// </remarks>
public sealed class InventoryComponent {
  private readonly InventorySlot[] _slots;
  private readonly ReadOnlyCollection<InventorySlot> _readOnlySlots;
  private HeldSlot? _held;

  /// <summary>A live read-only view for use within the host's exclusive access phase.</summary>
  public IReadOnlyList<InventorySlot> Slots => _readOnlySlots;

  public int Count => _slots.Length;

  public bool IsEmpty => Array.TrueForAll(_slots, static slot => slot.IsEmpty);

  /// <summary>The slot this inventory currently holds; <c>null</c> when nothing is held.</summary>
  public HeldSlot? Held => _held;

  public InventoryComponent(IEnumerable<InventorySlotDefinition> definitions) {
    ArgumentNullException.ThrowIfNull(definitions);
    InventorySlotDefinition[] source = definitions.ToArray();
    if (source.Length == 0) {
      throw new ArgumentException("An inventory requires at least one slot.", nameof(definitions));
    }

    HashSet<int> indexes = new();
    foreach (InventorySlotDefinition definition in source) {
      if (!definition.IsValid) {
        throw new ArgumentException("An inventory slot definition is invalid.", nameof(definitions));
      }

      if (!indexes.Add(definition.Index)) {
        throw new ArgumentException(
            $"Duplicate inventory slot index {definition.Index}.", nameof(definitions));
      }
    }

    Array.Sort(source, static (left, right) => left.Index.CompareTo(right.Index));
    _slots = source.Select(definition => new InventorySlot(definition)).ToArray();
    _readOnlySlots = Array.AsReadOnly(_slots);
  }

  public bool TryGetSlot(int index, out InventorySlot slot) {
    foreach (InventorySlot candidate in _slots) {
      if (candidate.Definition.Index == index) {
        slot = candidate;
        return true;
      }
    }

    slot = default;
    return false;
  }

  /// <summary>
  /// Reads the stack this inventory holds, or <c>null</c> when the held slot is unset, missing or
  /// empty. This is the only way to get the handheld payload: it is the slot's own value.
  /// </summary>
  public ItemStack? TryGetHeld() {
    return _held is HeldSlot held && TryGetSlot(held.Index, out InventorySlot slot)
        ? slot.Content
        : null;
  }

  /// <summary>True when the held slot is set, present and nonempty.</summary>
  public bool HasHeldItem => TryGetHeld() is not null;

  /// <summary>Commits a held-slot change; the inventory owner is the only caller.</summary>
  internal void CommitHeld(HeldSlot? held) {
    _held = held;
  }

  internal InventorySlot[] CaptureSlots() {
    return (InventorySlot[])_slots.Clone();
  }

  /// <summary>Commits a complete plan without invoking external effects.</summary>
  internal void CommitSlots(InventorySlot[] slots) {
    if (slots.Length != _slots.Length) {
      throw new ArgumentException("The plan must cover the complete inventory.", nameof(slots));
    }

    Array.Copy(slots, _slots, slots.Length);
  }
}
