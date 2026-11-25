using System.Numerics;
using Terraria.Unshared.Session;
using Terraria.Share.Items;

namespace Terraria.Share.Items;

/// <summary>
/// Owns the authoritative item table: the 400 world slots of the Version4 protocol plus the
/// container-slot index that makes a container-resident item the same record as a world one.
/// </summary>
/// <remarks>
/// <para>
/// 这是合并后的**唯一权威存储**。世界槽与容器槽不是两张表，而是同一条记录的两个地址：
/// <see cref="ItemPresence.Embodied"/> 的记录用槽号寻址（0..399，槽号即网络身份），
/// <see cref="ItemPresence.Dormant"/> 的记录用 <see cref="ItemContainerAddress"/> 寻址。
/// </para>
/// <para>
/// 只有 <c>ItemSystem</c> 在 Commit 阶段调用本类型的写方法；查询端口
/// （<see cref="IItemQuery"/>）只暴露 <see cref="TryGet"/>。运动写入
/// （<see cref="TrySetMotion"/>）刻意不产生变更，因为原版客户端本地外推物品物理，
/// 服务端只在生成、合并、拾取与消失时同步。
/// </para>
/// </remarks>
public sealed class ItemStore : IItemProjectionSource, IItemQuery
{
  /// <summary>Version4 <c>Main.maxItems</c>：世界槽数量。</summary>
  public const int SlotCount = 400;

  private readonly object _gate = new();
  private readonly Dictionary<short, ItemState> _worldSlots = [];
  private readonly Dictionary<ItemContainerAddress, ItemState> _containerSlots = [];
  private readonly Queue<ItemMutation> _pending = [];
  private long _revision;

  public ItemStore()
  {
  }

  /// <summary>Number of live embodied records.</summary>
  public int WorldCount
  {
    get
    {
      lock (_gate)
      {
        return _worldSlots.Count;
      }
    }
  }

  /// <summary>Number of live dormant records.</summary>
  public int ContainerCount
  {
    get
    {
      lock (_gate)
      {
        return _containerSlots.Count;
      }
    }
  }

  public bool TryCreate(
      int itemType,
      int stack,
      byte prefix,
      Vector2 position,
      Vector2 velocity,
      SessionEpoch? reservedFor,
      out ItemState item)
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

  /// <summary>Creates an embodied record with the Version4 lifecycle counters supplied by the caller.</summary>
  /// <remarks>
  /// <paramref name="noGrabDelay"/> 对应原版 <c>WorldItem.grabDelayTime</c>：掉落者刚扔出的物品
  /// 有约 100 tick 的不可拾取期（<c>WorldItem.DefaultGrabDelay</c>）。默认 0 保持既有可观察行为。
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
      out ItemState item)
  {
    item = default!;
    if (noGrabDelay < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(noGrabDelay));
    }

    if (!ItemRules.IsValidItem(itemType, stack, position, velocity) ||
        !ItemIdRules.TryFromNetworkType(itemType, out ItemId itemId))
    {
      return false;
    }

    lock (_gate)
    {
      short slot = ItemRules.FindFreeSlot(_worldSlots);
      if (slot < 0)
      {
        return false;
      }

      item = new ItemState(
          slot,
          new ItemStack(itemId, stack, prefix),
          position,
          velocity,
          reservedFor,
          NextRevisionUnsafe())
      {
        Presence = ItemPresence.Embodied,
        NoGrabDelay = noGrabDelay,
        Shimmered = shimmered
      };
      _worldSlots.Add(slot, item);
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Spawn, item));
      if (reservedFor is not null)
      {
        _pending.Enqueue(new ItemMutation(ItemChangeKind.Ownership, item));
      }

      return true;
    }
  }

  public bool TryGet(short slot, out ItemState? item)
  {
    lock (_gate)
    {
      return _worldSlots.TryGetValue(slot, out item);
    }
  }

  /// <summary>Reads a dormant record by its container address.</summary>
  public bool TryGetContained(ItemContainerAddress address, out ItemState? item)
  {
    lock (_gate)
    {
      return _containerSlots.TryGetValue(address, out item);
    }
  }

  public IReadOnlyList<ItemMutation> DrainMutations()
  {
    lock (_gate)
    {
      if (_pending.Count == 0)
      {
        return Array.Empty<ItemMutation>();
      }

      ItemMutation[] result = _pending.ToArray();
      _pending.Clear();
      return result;
    }
  }

  public bool TryReserve(short slot, SessionEpoch owner)
  {
    lock (_gate)
    {
      if (!owner.IsValid || !_worldSlots.TryGetValue(slot, out ItemState? current) ||
          current.ReservedFor is not null && current.ReservedFor != owner)
      {
        return false;
      }

      if (current.ReservedFor == owner)
      {
        return true;
      }

      ItemState next = current with { ReservedFor = owner, Revision = NextRevisionUnsafe() };
      _worldSlots[slot] = next;
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Ownership, next));
      return true;
    }
  }

  public bool TryRelease(short slot, SessionEpoch owner)
  {
    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(slot, out ItemState? current) ||
          current.ReservedFor is not SessionEpoch reserved || reserved != owner)
      {
        return false;
      }

      ItemState next = current with { ReservedFor = null, Revision = NextRevisionUnsafe() };
      _worldSlots[slot] = next;
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Ownership, next, owner));
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
      out ItemState? item)
  {
    item = null;
    // stack 为 0 表示清除槽位，不携带物品身份；否则类型必须是已知 ItemId。
    bool hasRequestedId = ItemIdRules.TryFromNetworkType(itemType, out ItemId requestedId);
    if (!ItemRules.IsValidPosition(position) ||
        !ItemRules.IsValidVelocity(velocity) ||
        (stack > 0 && (itemType <= 0 || !hasRequestedId)))
    {
      return false;
    }

    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(slot, out ItemState? current) ||
          !ItemRules.CanMutate(current, actor))
      {
        return false;
      }

      if (stack <= 0)
      {
        _worldSlots.Remove(slot);
        ItemState retired = current with { Item = default, Revision = NextRevisionUnsafe() };
        _pending.Enqueue(new ItemMutation(ItemChangeKind.Retire, retired, actor));
        item = retired;
        return true;
      }

      if (requestedId != current.Item.ItemId || prefix != current.Item.Prefix ||
          stack > current.Item.Quantity)
      {
        return false;
      }

      ItemState next = current with
      {
        Item = new ItemStack(requestedId, stack, prefix),
        Position = position,
        Velocity = velocity,
        Revision = NextRevisionUnsafe()
      };
      _worldSlots[slot] = next;
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Update, next, actor));
      item = next;
      return true;
    }
  }

  public bool TryRetire(short slot, SessionEpoch actor, out ItemState? retired)
  {
    retired = null;
    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(slot, out ItemState? current) ||
          !ItemRules.CanMutate(current, actor))
      {
        return false;
      }

      _worldSlots.Remove(slot);
      retired = current with { Item = default, Revision = NextRevisionUnsafe() };
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Retire, retired, actor));
      return true;
    }
  }

  /// <summary>Copies every live embodied record for one tick of the item owner.</summary>
  /// <remarks>
  /// The tick merges, integrates motion and resolves pickups against this snapshot, then re-reads
  /// each slot before writing so a slot consumed earlier in the same pass is never resurrected.
  /// </remarks>
  public ItemState[] SnapshotItems()
  {
    lock (_gate)
    {
      if (_worldSlots.Count == 0)
      {
        return Array.Empty<ItemState>();
      }

      ItemState[] items = new ItemState[_worldSlots.Count];
      _worldSlots.Values.CopyTo(items, 0);
      Array.Sort(items, static (left, right) => left.Slot.CompareTo(right.Slot));
      return items;
    }
  }

  /// <summary>Copies every live dormant record; used by the conservation invariant and persistence.</summary>
  public ItemState[] SnapshotContainedItems()
  {
    lock (_gate)
    {
      if (_containerSlots.Count == 0)
      {
        return Array.Empty<ItemState>();
      }

      ItemState[] items = new ItemState[_containerSlots.Count];
      _containerSlots.Values.CopyTo(items, 0);
      return items;
    }
  }

  /// <summary>
  /// Writes the position and velocity computed by the item tick.
  /// </summary>
  /// <remarks>
  /// Motion is deliberately silent: Version4 clients extrapolate a falling item locally and the
  /// server resynchronizes it through the spawn, pickup, merge and despawn facts instead of
  /// streaming a <c>SyncItem</c> message per item per tick. The revision still advances so a
  /// concurrent reader can tell that the row moved.
  /// </remarks>
  public bool TrySetMotion(short slot, Vector2 position, Vector2 velocity, out ItemState? item)
  {
    item = null;
    if (!ItemRules.IsValidPosition(position) || !ItemRules.IsValidVelocity(velocity))
    {
      return false;
    }

    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(slot, out ItemState? current))
      {
        return false;
      }

      if (current.Position == position && current.Velocity == velocity)
      {
        item = current;
        return true;
      }

      ItemState next = current with
      {
        Position = position,
        Velocity = velocity,
        Revision = NextRevisionUnsafe()
      };
      _worldSlots[slot] = next;
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
      out ItemState? survivor,
      out ItemState? drained)
  {
    survivor = null;
    drained = null;
    if (survivorSlot == drainedSlot || amount <= 0)
    {
      return false;
    }

    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(survivorSlot, out ItemState? survivorCurrent) ||
          !_worldSlots.TryGetValue(drainedSlot, out ItemState? drainedCurrent) ||
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
      ItemState nextSurvivor = survivorCurrent with
      {
        Item = survivorCurrent.Item.WithQuantity(combinedQuantity),
        Position = Vector2.Lerp(drainedCurrent.Position, survivorCurrent.Position, mergedFraction),
        Velocity = Vector2.Lerp(drainedCurrent.Velocity, survivorCurrent.Velocity, mergedFraction),
        Revision = NextRevisionUnsafe()
      };
      _worldSlots[survivorSlot] = nextSurvivor;
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Update, nextSurvivor));

      int remaining = drainedCurrent.Item.Quantity - taken;
      if (remaining > 0)
      {
        ItemState nextDrained = drainedCurrent with
        {
          Item = drainedCurrent.Item.WithQuantity(remaining),
          Revision = NextRevisionUnsafe()
        };
        _worldSlots[drainedSlot] = nextDrained;
        _pending.Enqueue(new ItemMutation(ItemChangeKind.Update, nextDrained));
        drained = nextDrained;
      }
      else
      {
        _worldSlots.Remove(drainedSlot);
        ItemState retired = drainedCurrent with
        {
          Item = default,
          Revision = NextRevisionUnsafe()
        };
        _pending.Enqueue(new ItemMutation(ItemChangeKind.Retire, retired));
        drained = retired;
      }

      survivor = nextSurvivor;
      return true;
    }
  }

  /// <summary>Removes a slot without an ownership check; used by the world tick and cleanup.</summary>
  public bool TryRemove(short slot, out ItemState? retired)
  {
    retired = null;
    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(slot, out ItemState? current))
      {
        return false;
      }

      _worldSlots.Remove(slot);
      retired = current with { Item = default, Revision = NextRevisionUnsafe() };
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Retire, retired));
      return true;
    }
  }

  /// <summary>
  /// Removes <paramref name="quantity"/> units on behalf of <paramref name="actor"/> and retires
  /// the slot when nothing remains.
  /// </summary>
  /// <remarks>
  /// 这是拾取的**世界侧**语义：容器域回报它实际收下的数量，Item 域按该数量扣减。
  /// 合并后的拾取走 <see cref="TryTransferToContainer"/>，本方法保留给"只扣减不转移"的场合
  /// （例如物品被消耗而不是被装进容器）。
  /// </remarks>
  public bool TryConsume(short slot, SessionEpoch actor, int quantity, out ItemState? item)
  {
    item = null;
    if (quantity <= 0)
    {
      return false;
    }

    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(slot, out ItemState? current) ||
          !ItemRules.CanMutate(current, actor))
      {
        return false;
      }

      int taken = Math.Min(quantity, current.Item.Quantity);
      int remaining = current.Item.Quantity - taken;
      if (remaining > 0)
      {
        ItemState next = current with
        {
          Item = current.Item.WithQuantity(remaining),
          Revision = NextRevisionUnsafe()
        };
        _worldSlots[slot] = next;
        _pending.Enqueue(new ItemMutation(ItemChangeKind.Update, next, actor));
        item = next;
        return true;
      }

      _worldSlots.Remove(slot);
      ItemState retired = current with
      {
        Item = default,
        Revision = NextRevisionUnsafe()
      };
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Retire, retired, actor));
      item = retired;
      return true;
    }
  }

  /// <summary>
  /// Embodied → Dormant：把 <paramref name="quantity"/> 件从世界槽搬到一个容器槽。
  /// </summary>
  /// <remarks>
  /// <para>
  /// 这是合并后**唯一的拾取提交点**：改址在同一个 lock 内完成，因此不存在"背包已加、世界未减"
  /// 的中间窗口，也就不再需要 <c>Consume</c> 那种跨 tick 对账请求。
  /// </para>
  /// <para>
  /// 数量守恒由本方法保证：搬走的数量恰好等于调用方回报的实收数量；搬空则释放世界槽，
  /// 搬一部分则剩余仍留在原槽并保持 Embodied。
  /// </para>
  /// </remarks>
  public bool TryTransferToContainer(
      short slot,
      SessionEpoch actor,
      ItemContainerAddress destination,
      int quantity,
      out ItemState? remaining,
      out ItemState? stored)
  {
    remaining = null;
    stored = null;
    if (quantity <= 0 || destination.Container <= 0 || destination.Slot < 0)
    {
      return false;
    }

    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(slot, out ItemState? current) ||
          !ItemRules.CanMutate(current, actor) ||
          _containerSlots.ContainsKey(destination))
      {
        return false;
      }

      int taken = Math.Min(quantity, current.Item.Quantity);
      if (taken <= 0)
      {
        return false;
      }

      ItemState nextStored = new(
          Slot: -1,
          current.Item.WithQuantity(taken),
          Vector2.Zero,
          Vector2.Zero,
          ReservedFor: null,
          NextRevisionUnsafe())
      {
        Presence = ItemPresence.Dormant,
        Container = destination
      };
      _containerSlots[destination] = nextStored;
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Transfer, nextStored, actor));
      stored = nextStored;

      int left = current.Item.Quantity - taken;
      if (left > 0)
      {
        ItemState nextRemaining = current with
        {
          Item = current.Item.WithQuantity(left),
          Revision = NextRevisionUnsafe()
        };
        _worldSlots[slot] = nextRemaining;
        _pending.Enqueue(new ItemMutation(ItemChangeKind.Update, nextRemaining, actor));
        remaining = nextRemaining;
        return true;
      }

      _worldSlots.Remove(slot);
      ItemState retired = current with
      {
        Item = default,
        Revision = NextRevisionUnsafe()
      };
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Retire, retired, actor));
      remaining = retired;
      return true;
    }
  }

  /// <summary>
  /// Dormant → Embodied：把容器槽里的记录搬到世界槽（丢弃、掉落）。
  /// </summary>
  /// <remarks>
  /// 原版的丢弃由客户端本地 <c>Item.NewItem</c> 建世界物品后广播包 21 完成，服务端收到槽号 400
  /// 时自行分配（<c>MessageBuffer.cs:1507-1511</c>）。因此本方法只被服务端内部的转移使用；
  /// 客户端包 21 的入口仍走 <see cref="TryCreate"/>，槽号参数是哨兵。
  /// </remarks>
  public bool TryTransferToWorld(
      ItemContainerAddress source,
      Vector2 position,
      Vector2 velocity,
      int noGrabDelay,
      out ItemState? released,
      out ItemState? spawned)
  {
    released = null;
    spawned = null;
    if (noGrabDelay < 0 ||
        !ItemRules.IsValidPosition(position) ||
        !ItemRules.IsValidVelocity(velocity))
    {
      return false;
    }

    lock (_gate)
    {
      if (!_containerSlots.TryGetValue(source, out ItemState? current))
      {
        return false;
      }

      short slot = ItemRules.FindFreeSlot(_worldSlots);
      if (slot < 0)
      {
        return false;
      }

      ItemState nextSpawned = current with
      {
        Slot = slot,
        Presence = ItemPresence.Embodied,
        Container = null,
        Position = position,
        Velocity = velocity,
        NoGrabDelay = noGrabDelay,
        Revision = NextRevisionUnsafe()
      };
      _worldSlots.Add(slot, nextSpawned);
      _containerSlots.Remove(source);
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Spawn, nextSpawned));
      spawned = nextSpawned;

      released = current with
      {
        Item = default,
        Presence = ItemPresence.Dormant,
        Revision = NextRevisionUnsafe()
      };
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Transfer, released));
      return true;
    }
  }

  /// <summary>Reads one container slot's payload without transferring it.</summary>
  public bool TryGetContainedStack(ItemContainerAddress address, out ItemStack stack)
  {
    stack = default;
    lock (_gate)
    {
      if (!_containerSlots.TryGetValue(address, out ItemState? current) || current.Item.IsEmpty)
      {
        return false;
      }

      stack = current.Item;
      return true;
    }
  }

  /// <summary>Total quantity held by embodied records; used by the conservation invariant.</summary>
  public int TotalWorldQuantity()
  {
    lock (_gate)
    {
      int total = 0;
      foreach (ItemState item in _worldSlots.Values)
      {
        total += item.Item.Quantity;
      }

      return total;
    }
  }

  /// <summary>Total quantity held by dormant records; used by the conservation invariant.</summary>
  public int TotalContainerQuantity()
  {
    lock (_gate)
    {
      int total = 0;
      foreach (ItemState item in _containerSlots.Values)
      {
        total += item.Item.Quantity;
      }

      return total;
    }
  }

  /// <summary>
  /// Advances the Version4 lifecycle counters of one slot by a single tick.
  /// </summary>
  /// <remarks>
  /// Ported decrements: <c>keepTime</c> 与 <c>grabDelayTime</c> 减到 0 为止，<c>timeSinceItemSpawned</c>
  /// 只增不减（Version4 上限 2147483547 防溢出）。这个方法是**纯推进**，不产生网络变更：
  /// Version4 同样只在生成/合并/拾取/消失时同步。
  /// </remarks>
  public bool TryAdvanceLifecycle(short slot, out ItemState? item)
  {
    item = null;
    lock (_gate)
    {
      if (!_worldSlots.TryGetValue(slot, out ItemState? current))
      {
        return false;
      }

      int noGrabDelay = current.NoGrabDelay > 0 ? current.NoGrabDelay - 1 : 0;
      int keepTime = current.KeepTime > 0 ? current.KeepTime - 1 : 0;
      int sinceSpawned = current.TimeSinceItemSpawned < ItemRules.MaximumLifetimeCounter
          ? current.TimeSinceItemSpawned + 1
          : current.TimeSinceItemSpawned;
      if (noGrabDelay == current.NoGrabDelay &&
          keepTime == current.KeepTime &&
          sinceSpawned == current.TimeSinceItemSpawned)
      {
        item = current;
        return true;
      }

      ItemState next = current with
      {
        NoGrabDelay = noGrabDelay,
        KeepTime = keepTime,
        TimeSinceItemSpawned = sinceSpawned,
        Revision = NextRevisionUnsafe()
      };
      _worldSlots[slot] = next;
      item = next;
      return true;
    }
  }

  /// <summary>Creates a dormant record at an empty container slot.</summary>
  /// <remarks>
  /// 容器槽的写入入口：容器域决定"这一格放什么、放多少"，记录本身仍由本存储拥有。
  /// 地址已被占用时拒绝，保证一条记录一个地址。
  /// </remarks>
  public bool TryStoreIntoContainer(ItemContainerAddress address, in ItemStack stack, out ItemState? stored)
  {
    stored = null;
    if (stack.IsEmpty || address.Container <= 0 || address.Slot < 0)
    {
      return false;
    }

    lock (_gate)
    {
      if (_containerSlots.ContainsKey(address))
      {
        return false;
      }

      ItemState created = new(
          Slot: -1,
          stack,
          Vector2.Zero,
          Vector2.Zero,
          ReservedFor: null,
          NextRevisionUnsafe())
      {
        Presence = ItemPresence.Dormant,
        Container = address
      };
      _containerSlots[address] = created;
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Transfer, created));
      stored = created;
      return true;
    }
  }

  /// <summary>Adds <paramref name="quantity"/> units to an existing dormant record.</summary>
  public bool TryGrowContainerStack(ItemContainerAddress address, int quantity, out ItemState? grown)
  {
    grown = null;
    if (quantity <= 0)
    {
      return false;
    }

    lock (_gate)
    {
      if (!_containerSlots.TryGetValue(address, out ItemState? current) || current.Item.IsEmpty)
      {
        return false;
      }

      ItemState next = current with
      {
        Item = current.Item.WithQuantity(current.Item.Quantity + quantity),
        Revision = NextRevisionUnsafe()
      };
      _containerSlots[address] = next;
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Transfer, next));
      grown = next;
      return true;
    }
  }

  /// <summary>Removes <paramref name="quantity"/> units from a dormant record.</summary>
  public bool TryShrinkContainerStack(
      ItemContainerAddress address,
      int quantity,
      out ItemState? remaining,
      out ItemStack removed)
  {
    remaining = null;
    removed = default;
    if (quantity <= 0)
    {
      return false;
    }

    lock (_gate)
    {
      if (!_containerSlots.TryGetValue(address, out ItemState? current) || current.Item.IsEmpty)
      {
        return false;
      }

      int taken = Math.Min(quantity, current.Item.Quantity);
      if (taken <= 0)
      {
        return false;
      }

      removed = current.Item.WithQuantity(taken);
      int left = current.Item.Quantity - taken;
      if (left > 0)
      {
        ItemState next = current with
        {
          Item = current.Item.WithQuantity(left),
          Revision = NextRevisionUnsafe()
        };
        _containerSlots[address] = next;
        _pending.Enqueue(new ItemMutation(ItemChangeKind.Transfer, next));
        remaining = next;
        return true;
      }

      _containerSlots.Remove(address);
      ItemState cleared = current with
      {
        Item = default,
        Revision = NextRevisionUnsafe()
      };
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Transfer, cleared));
      remaining = cleared;
      return true;
    }
  }

  /// <summary>Clears a container slot regardless of its content.</summary>
  public bool TryClearContainer(ItemContainerAddress address, out ItemState? cleared)
  {
    cleared = null;
    lock (_gate)
    {
      if (!_containerSlots.TryGetValue(address, out ItemState? current))
      {
        return false;
      }

      _containerSlots.Remove(address);
      cleared = current with { Item = default, Revision = NextRevisionUnsafe() };
      _pending.Enqueue(new ItemMutation(ItemChangeKind.Transfer, cleared));
      return true;
    }
  }

  /// <summary>Copies every live dormant record that belongs to one container.</summary>
  public ItemState[] SnapshotContainer(int container)
  {
    lock (_gate)
    {
      List<ItemState> items = [];
      foreach (KeyValuePair<ItemContainerAddress, ItemState> pair in _containerSlots)
      {
        if (pair.Key.Container == container)
        {
          items.Add(pair.Value);
        }
      }

      return items.ToArray();
    }
  }

  private long NextRevisionUnsafe()
  {
    _revision = ItemRules.NextRevision(_revision);
    return _revision;
  }
}
