using System.Numerics;
using Terraria.Communication;

namespace Terraria.World;

/// <summary>Owns the bounded 400-slot world item table used by the Version4 protocol.</summary>
public sealed class WorldItemStore : IWorldItemProjectionSource, IWorldItemQuery
{
  public const int SlotCount = 400;

  private readonly object _gate = new();
  private readonly Dictionary<short, WorldItemState> _items = [];
  private readonly Queue<WorldItemMutation> _pending = [];
  private long _revision;

  public WorldItemStore()
  {
  }

  public bool TryCreate(
      int itemType,
      int stack,
      byte prefix,
      Vector2 position,
      Vector2 velocity,
      SessionEpoch? reservedFor,
      out WorldItemState item)
  {
    item = default!;
    if (!WorldItemRules.IsValidItem(itemType, stack, position, velocity))
    {
      return false;
    }

    lock (_gate)
    {
      short slot = WorldItemRules.FindFreeSlot(_items);
      if (slot < 0)
      {
        return false;
      }

      item = new WorldItemState(
          slot,
          itemType,
          stack,
          prefix,
          position,
          velocity,
          reservedFor,
          NextRevisionUnsafe());
      _items.Add(slot, item);
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Spawn, item));
      if (reservedFor is not null)
      {
        _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Ownership, item));
      }

      return true;
    }
  }

  public bool TryGet(short slot, out WorldItemState? item)
  {
    lock (_gate)
    {
      return _items.TryGetValue(slot, out item);
    }
  }

  public IReadOnlyList<WorldItemMutation> DrainMutations()
  {
    lock (_gate)
    {
      if (_pending.Count == 0)
      {
        return Array.Empty<WorldItemMutation>();
      }

      WorldItemMutation[] result = _pending.ToArray();
      _pending.Clear();
      return result;
    }
  }

  public bool TryReserve(short slot, SessionEpoch owner)
  {
    lock (_gate)
    {
      if (!owner.IsValid || !_items.TryGetValue(slot, out WorldItemState? current) ||
          current.ReservedFor is not null && current.ReservedFor != owner)
      {
        return false;
      }

      if (current.ReservedFor == owner)
      {
        return true;
      }

      WorldItemState next = current with { ReservedFor = owner, Revision = NextRevisionUnsafe() };
      _items[slot] = next;
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Ownership, next));
      return true;
    }
  }

  public bool TryRelease(short slot, SessionEpoch owner)
  {
    lock (_gate)
    {
      if (!_items.TryGetValue(slot, out WorldItemState? current) ||
          current.ReservedFor is not SessionEpoch reserved || reserved != owner)
      {
        return false;
      }

      WorldItemState next = current with { ReservedFor = null, Revision = NextRevisionUnsafe() };
      _items[slot] = next;
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Ownership, next, owner));
      return true;
    }
  }

  public bool TryUpdate(
      short slot,
      SessionEpoch actor,
      int itemType,
      int stack,
      byte prefix,
      Vector2 position,
      Vector2 velocity,
      out WorldItemState? item)
  {
    item = null;
    if (!WorldItemRules.IsValidPosition(position) ||
        !WorldItemRules.IsValidVelocity(velocity) ||
        (stack > 0 && itemType <= 0))
    {
      return false;
    }

    lock (_gate)
    {
      if (!_items.TryGetValue(slot, out WorldItemState? current) ||
          !WorldItemRules.CanMutate(current, actor))
      {
        return false;
      }

      if (stack <= 0)
      {
        _items.Remove(slot);
        WorldItemState retired = current with { Stack = 0, Revision = NextRevisionUnsafe() };
        _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Retire, retired, actor));
        item = retired;
        return true;
      }

      if (itemType != current.ItemType || prefix != current.Prefix || stack > current.Stack)
      {
        return false;
      }

      WorldItemState next = current with
      {
        Stack = stack,
        Position = position,
        Velocity = velocity,
        Revision = NextRevisionUnsafe()
      };
      _items[slot] = next;
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Update, next, actor));
      item = next;
      return true;
    }
  }

  public bool TryRetire(short slot, SessionEpoch actor, out WorldItemState? retired)
  {
    retired = null;
    lock (_gate)
    {
      if (!_items.TryGetValue(slot, out WorldItemState? current) ||
          !WorldItemRules.CanMutate(current, actor))
      {
        return false;
      }

      _items.Remove(slot);
      retired = current with { Stack = 0, Revision = NextRevisionUnsafe() };
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Retire, retired, actor));
      return true;
    }
  }

  private long NextRevisionUnsafe()
  {
    _revision = WorldItemRules.NextRevision(_revision);
    return _revision;
  }
}
