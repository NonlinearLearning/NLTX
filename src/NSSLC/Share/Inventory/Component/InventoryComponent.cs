using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Terraria.Share.Inventory;

/// <summary>Grants inventory capability; InventorySystem coordinates mutations.</summary>
public sealed class InventoryComponent {
  private readonly InventorySlot[] _slots;
  private readonly ReadOnlyCollection<InventorySlot> _readOnlySlots;

  /// <summary>A live read-only view for use within the host's exclusive access phase.</summary>
  public IReadOnlyList<InventorySlot> Slots => _readOnlySlots;

  public int Count => _slots.Length;

  public bool IsEmpty => Array.TrueForAll(_slots, static slot => slot.IsEmpty);

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
