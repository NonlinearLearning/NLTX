using System;
using System.Collections.Generic;

namespace Terraria.Share.Items;

/// <summary>Immutable-after-sealing directory of item use profiles.</summary>
public sealed class ItemUseCatalog
{
  private readonly Dictionary<ItemId, ItemUse> _definitions = [];
  private bool _sealed;

  public int Count => _definitions.Count;

  public bool IsSealed => _sealed;

  public void Register(ItemUse definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    if (_sealed)
    {
      throw new InvalidOperationException("The item use catalog is sealed.");
    }

    if (!definition.IsValid)
    {
      throw new ArgumentException(
          $"Item use definition {definition.ItemId} is invalid.", nameof(definition));
    }

    if (!_definitions.TryAdd(definition.ItemId, definition))
    {
      throw new InvalidOperationException(
          $"Item use definition {definition.ItemId} is already registered.");
    }
  }

  public void Seal()
  {
    _sealed = true;
  }

  public bool TryGet(ItemId itemId, out ItemUse definition)
    => _definitions.TryGetValue(itemId, out definition!);

  public ItemUse Get(ItemId itemId)
  {
    if (!TryGet(itemId, out ItemUse definition))
    {
      throw new KeyNotFoundException($"Item use definition {itemId} is not registered.");
    }

    return definition;
  }
}
