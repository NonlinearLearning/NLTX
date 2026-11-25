using System.Numerics;
using Terraria.Unshared.Session;
using Terraria.Share.Items;

namespace Terraria.Unshared.World;

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
    => TryCreate(
        itemType,
        stack,
        prefix,
        position,
        velocity,
        reservedFor,
        noGrabDelay: 0,
        shimmered: false,
        out item);

  /// <summary>Creates a world item with the Version4 lifecycle counters supplied by the caller.</summary>
  /// <remarks>
  /// <paramref name="noGrabDelay"/> 对应 Version4 <c>WorldItem.noGrabDelay</c>：掉落者刚扔出的物品
  /// 有约 100 tick 的不可拾取期。默认 0 保持既有可观察行为（当前所有掉落都无延迟）。
  /// </remarks>
  public bool TryCreate(
      int itemType,
      int stack,
      byte prefix,
      Vector2 position,
      Vector2 velocity,
      SessionEpoch? reservedFor,
      int noGrabDelay,
      bool shimmered,
      out WorldItemState item)
  {
    item = default!;
    if (noGrabDelay < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(noGrabDelay));
    }

    if (!WorldItemRules.IsValidItem(itemType, stack, position, velocity) ||
        !ItemIdRules.TryFromNetworkType(itemType, out ItemId itemId))
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
          new ItemStack(itemId, stack, prefix),
          position,
          velocity,
          reservedFor,
          NextRevisionUnsafe())
      {
        NoGrabDelay = noGrabDelay,
        Shimmered = shimmered
      };
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
    // stack 为 0 表示清除槽位，不携带物品身份；否则类型必须是已知 ItemId。
    bool hasRequestedId = ItemIdRules.TryFromNetworkType(itemType, out ItemId requestedId);
    if (!WorldItemRules.IsValidPosition(position) ||
        !WorldItemRules.IsValidVelocity(velocity) ||
        (stack > 0 && (itemType <= 0 || !hasRequestedId)))
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
        WorldItemState retired = current with { Item = default, Revision = NextRevisionUnsafe() };
        _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Retire, retired, actor));
        item = retired;
        return true;
      }

      if (requestedId != current.Item.ItemId || prefix != current.Item.Prefix ||
          stack > current.Item.Quantity)
      {
        return false;
      }

      WorldItemState next = current with
      {
        Item = new ItemStack(requestedId, stack, prefix),
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
      retired = current with { Item = default, Revision = NextRevisionUnsafe() };
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Retire, retired, actor));
      return true;
    }
  }

  /// <summary>Copies every live slot for one tick of the world item owner.</summary>
  /// <remarks>
  /// The tick merges, integrates motion and resolves pickups against this snapshot, then re-reads
  /// each slot before writing so a slot consumed earlier in the same pass is never resurrected.
  /// </remarks>
  public WorldItemState[] SnapshotItems()
  {
    lock (_gate)
    {
      if (_items.Count == 0)
      {
        return Array.Empty<WorldItemState>();
      }

      WorldItemState[] items = new WorldItemState[_items.Count];
      _items.Values.CopyTo(items, 0);
      Array.Sort(items, static (left, right) => left.Slot.CompareTo(right.Slot));
      return items;
    }
  }

  /// <summary>
  /// Writes the position and velocity computed by the world item tick.
  /// </summary>
  /// <remarks>
  /// Motion is deliberately silent: Version4 clients extrapolate a falling item locally and the
  /// server resynchronizes it through the spawn, pickup, merge and despawn facts instead of
  /// streaming a <c>SyncItem</c> message per item per tick. The revision still advances so a
  /// concurrent reader can tell that the row moved.
  /// </remarks>
  public bool TrySetMotion(short slot, Vector2 position, Vector2 velocity, out WorldItemState? item)
  {
    item = null;
    if (!WorldItemRules.IsValidPosition(position) || !WorldItemRules.IsValidVelocity(velocity))
    {
      return false;
    }

    lock (_gate)
    {
      if (!_items.TryGetValue(slot, out WorldItemState? current))
      {
        return false;
      }

      if (current.Position == position && current.Velocity == velocity)
      {
        item = current;
        return true;
      }

      WorldItemState next = current with
      {
        Position = position,
        Velocity = velocity,
        Revision = NextRevisionUnsafe()
      };
      _items[slot] = next;
      item = next;
      return true;
    }
  }

  /// <summary>
  /// Moves <paramref name="amount"/> units from <paramref name="drainedSlot"/> into
  /// <paramref name="survivorSlot"/> and blends the survivor towards the drained item's position.
  /// </summary>
  /// <remarks>
  /// Version4 <c>WorldItem.TryCombiningIntoNearbyItems</c> lerps the survivor position and velocity
  /// by the merged fraction, so the surviving pile keeps a visible impulse from the absorbed heap.
  /// </remarks>
  public bool TryCombineStacks(
      short survivorSlot,
      short drainedSlot,
      int amount,
      out WorldItemState? survivor,
      out WorldItemState? drained)
  {
    survivor = null;
    drained = null;
    if (survivorSlot == drainedSlot || amount <= 0)
    {
      return false;
    }

    lock (_gate)
    {
      if (!_items.TryGetValue(survivorSlot, out WorldItemState? survivorCurrent) ||
          !_items.TryGetValue(drainedSlot, out WorldItemState? drainedCurrent) ||
          !survivorCurrent.Item.CanStackWith(drainedCurrent.Item))
      {
        return false;
      }

      int taken = Math.Min(amount, drainedCurrent.Item.Quantity);
      if (taken <= 0)
      {
        return false;
      }

      int combinedQuantity = survivorCurrent.Item.Quantity + taken;
      float mergedFraction = (float)taken / combinedQuantity;
      WorldItemState nextSurvivor = survivorCurrent with
      {
        Item = survivorCurrent.Item.WithQuantity(combinedQuantity),
        Position = Vector2.Lerp(drainedCurrent.Position, survivorCurrent.Position, mergedFraction),
        Velocity = Vector2.Lerp(drainedCurrent.Velocity, survivorCurrent.Velocity, mergedFraction),
        Revision = NextRevisionUnsafe()
      };
      _items[survivorSlot] = nextSurvivor;
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Update, nextSurvivor));

      int remaining = drainedCurrent.Item.Quantity - taken;
      if (remaining > 0)
      {
        WorldItemState nextDrained = drainedCurrent with
        {
          Item = drainedCurrent.Item.WithQuantity(remaining),
          Revision = NextRevisionUnsafe()
        };
        _items[drainedSlot] = nextDrained;
        _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Update, nextDrained));
        drained = nextDrained;
      }
      else
      {
        _items.Remove(drainedSlot);
        WorldItemState retired = drainedCurrent with
        {
          Item = default,
          Revision = NextRevisionUnsafe()
        };
        _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Retire, retired));
        drained = retired;
      }

      survivor = nextSurvivor;
      return true;
    }
  }

  /// <summary>Removes a slot without an ownership check; used by the world tick and cleanup.</summary>
  public bool TryRemove(short slot, out WorldItemState? retired)
  {
    retired = null;
    lock (_gate)
    {
      if (!_items.TryGetValue(slot, out WorldItemState? current))
      {
        return false;
      }

      _items.Remove(slot);
      retired = current with { Item = default, Revision = NextRevisionUnsafe() };
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Retire, retired));
      return true;
    }
  }

  /// <summary>
  /// Removes <paramref name="quantity"/> units on behalf of <paramref name="actor"/> and retires
  /// the slot when nothing remains.
  /// </summary>
  /// <remarks>
  /// This is the world-item side of a pickup: the inventory domain reports how many units it
  /// accepted, and the owner removes exactly that many so the two stores cannot drift apart.
  /// </remarks>
  public bool TryConsume(short slot, SessionEpoch actor, int quantity, out WorldItemState? item)
  {
    item = null;
    if (quantity <= 0)
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

      int taken = Math.Min(quantity, current.Item.Quantity);
      int remaining = current.Item.Quantity - taken;
      if (remaining > 0)
      {
        WorldItemState next = current with
        {
          Item = current.Item.WithQuantity(remaining),
          Revision = NextRevisionUnsafe()
        };
        _items[slot] = next;
        _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Update, next, actor));
        item = next;
        return true;
      }

      _items.Remove(slot);
      WorldItemState retired = current with
      {
        Item = default,
        Revision = NextRevisionUnsafe()
      };
      _pending.Enqueue(new WorldItemMutation(WorldItemChangeKind.Retire, retired, actor));
      item = retired;
      return true;
    }
  }

  /// <summary>
  /// Advances the Version4 lifecycle counters of one slot by a single tick.
  /// </summary>
  /// <remarks>
  /// Ported decrements: <c>ownTime</c> 与 <c>keepTime</c> 减到 0 为止，<c>noGrabDelay</c> 减到 0 为止，
  /// <c>timeSinceItemSpawned</c> 只增不减（Version4 上限 2147483547 防溢出）。
  /// 这个方法是**纯推进**，不产生网络变更：Version4 同样只在生成/合并/拾取/消失时同步。
  /// </remarks>
  public bool TryAdvanceLifecycle(short slot, out WorldItemState? item)
  {
    item = null;
    lock (_gate)
    {
      if (!_items.TryGetValue(slot, out WorldItemState? current))
      {
        return false;
      }

      int noGrabDelay = current.NoGrabDelay > 0 ? current.NoGrabDelay - 1 : 0;
      int ownTime = current.OwnTime > 0 ? current.OwnTime - 1 : 0;
      int keepTime = current.KeepTime > 0 ? current.KeepTime - 1 : 0;
      int sinceSpawned = current.TimeSinceItemSpawned < WorldItemRules.MaximumLifetimeCounter
          ? current.TimeSinceItemSpawned + 1
          : current.TimeSinceItemSpawned;
      if (noGrabDelay == current.NoGrabDelay &&
          ownTime == current.OwnTime &&
          keepTime == current.KeepTime &&
          sinceSpawned == current.TimeSinceItemSpawned)
      {
        item = current;
        return true;
      }

      WorldItemState next = current with
      {
        NoGrabDelay = noGrabDelay,
        OwnTime = ownTime,
        KeepTime = keepTime,
        TimeSinceItemSpawned = sinceSpawned,
        Revision = NextRevisionUnsafe()
      };
      _items[slot] = next;
      item = next;
      return true;
    }
  }

  private long NextRevisionUnsafe()
  {
    _revision = WorldItemRules.NextRevision(_revision);
    return _revision;
  }
}
