using Terraria.Unshared.Session;
using Terraria.Share.Items;

namespace Terraria.Unshared.World;

/// <summary>Authoritative contents of one world chest, keyed by the tile it sits on.</summary>
/// <remarks>
/// Version4 keeps forty item slots per chest and resolves a chest by scanning positions. The
/// server keys the store by position instead and keeps the persisted slot index for projection.
/// Open state is tracked per session: only one session may hold a chest open, which is what the
/// reference expresses through <c>Chest.chestOpen</c> plus the frame animation.
/// </remarks>
public sealed class WorldChestState
{
  /// <summary>Version4 <c>Chest.item.Length</c>.</summary>
  public const int SlotCount = 40;

  private readonly ItemStack?[] _items = new ItemStack?[SlotCount];
  private SessionEpoch? _openedBy;

  public WorldChestState(short index, WorldTileCoordinate position, string name)
  {
    Index = index;
    Position = position;
    Name = name ?? string.Empty;
  }

  public short Index { get; }

  public WorldTileCoordinate Position { get; }

  public string Name { get; }

  public bool IsOpen => _openedBy is not null;

  public SessionEpoch? OpenedBy => _openedBy;

  public IReadOnlyList<ItemStack?> Items => _items;

  public int StoredStackCount
  {
    get
    {
      int count = 0;
      for (int index = 0; index < _items.Length; index++)
      {
        if (_items[index] is ItemStack stack && !stack.IsEmpty)
        {
          count++;
        }
      }

      return count;
    }
  }

  /// <summary>Marks the chest open for one session; a second session is rejected.</summary>
  public bool TryOpen(SessionEpoch actor)
  {
    if (_openedBy is SessionEpoch current)
    {
      return current == actor;
    }

    _openedBy = actor;
    return true;
  }

  /// <summary>Clears the open state; only the holding session may close it.</summary>
  public bool TryClose(SessionEpoch actor)
  {
    if (_openedBy is not SessionEpoch current || current != actor)
    {
      return false;
    }

    _openedBy = null;
    return true;
  }

  /// <summary>Copies the stored stacks in slot order, skipping empty slots.</summary>
  public ItemStack[] CopyItems()
  {
    List<ItemStack> stacks = [];
    for (int index = 0; index < _items.Length; index++)
    {
      if (_items[index] is ItemStack stack && !stack.IsEmpty)
      {
        stacks.Add(stack);
      }
    }

    return [.. stacks];
  }

  /// <summary>Load-time write of one slot; rejected while the chest is open to any session.</summary>
  public bool TrySetItem(int slot, ItemStack? stack)
  {
    if (slot < 0 || slot >= _items.Length)
    {
      return false;
    }

    if (stack is ItemStack value && value.IsEmpty)
    {
      stack = null;
    }

    _items[slot] = stack;
    return true;
  }
}
