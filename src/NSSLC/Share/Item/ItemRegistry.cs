using System;
using System.Collections.Generic;

namespace Terraria.Share.Items;

/// <summary>
/// Stores immutable item definitions and never stores runtime item entities.
/// </summary>
/// <remarks>
/// The registry accepts writes until <see cref="Seal" /> is called; a sealed registry is the
/// immutable catalog form that an ECS system may take as a constructor dependency.
/// </remarks>
public sealed class ItemRegistry {
  private readonly Dictionary<ItemId, Item> _items = [];
  private bool _sealed;

  public IReadOnlyCollection<Item> Items => _items.Values;

  public int Count => _items.Count;

  /// <summary>True once the registry stopped accepting writes.</summary>
  public bool IsSealed => _sealed;

  public void Register(Item item) {
    ArgumentNullException.ThrowIfNull(item);
    if (_sealed) {
      throw new InvalidOperationException("The item registry is sealed.");
    }

    if (!_items.TryAdd(item.Id, item)) {
      throw new InvalidOperationException($"Item {item.Id} is already registered.");
    }
  }

  /// <summary>Stops accepting writes; subsequent <see cref="Register" /> calls throw.</summary>
  public void Seal() {
    _sealed = true;
  }

  public bool TryGet(ItemId id, out Item item) {
    return _items.TryGetValue(id, out item!);
  }

  public Item Get(ItemId id) {
    if (!TryGet(id, out Item item)) {
      throw new KeyNotFoundException($"Item {id} is not registered.");
    }

    return item;
  }
}