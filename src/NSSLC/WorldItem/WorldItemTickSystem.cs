using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Unshared.Session;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.World;

/// <summary>
/// Advances the authoritative world item table once per server tick.
/// </summary>
/// <remarks>
/// Ported from Version4 <c>WorldItem.UpdateItem</c> and its helpers. The owner performs the same
/// per-item sequence — merge into a nearby slot, integrate motion, sweep the tiles, retire what
/// left the world — and additionally turns an eligible proximity case into an
/// <see cref="InventoryPickupRequest"/> instead of writing a player inventory itself.
///
/// <para>
/// Motion is written silently (no mutation) because Version4 clients extrapolate item physics and
/// only resynchronize on spawn, merge, pickup and despawn facts. The per-slot counters
/// (<c>noGrabDelay</c>, <c>ownTime</c>, <c>keepTime</c>, <c>timeSinceItemSpawned</c>) are advanced by
/// <see cref="LifecyclePass"/> after the passes that read them, mirroring where Version4 decrements
/// them at the end of <c>UpdateItem</c>. Drops currently arrive with no grab delay, so the only
/// observable difference from Version4 remains the missing pickup animation delay.
/// </para>
/// </remarks>
public sealed class WorldItemTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  /// <summary>Version4 <c>WorldItem.UpdateEntityFields</c> uses a 16x16 item hitbox.</summary>
  private const float ItemExtent = 16f;

  private static readonly Vector2 ItemSize = new(ItemExtent);
  private static readonly TileCollisionCapabilityComponent TileCollision = new(Enabled: true);
  private static readonly LiquidInteractionCapabilityComponent LiquidContact = new(
      Enabled: true,
      AffectedLiquids: LiquidContactFlags.Water | LiquidContactFlags.Honey |
          LiquidContactFlags.Lava | LiquidContactFlags.Shimmer);

  private readonly WorldItemStore _items;
  private readonly ISpatialWorldQuery _world;
  private readonly IWorldTileGrid _tiles;
  private readonly ISessionPlayerSource _sessionPlayers;
  private readonly int _maximumStackSize;
  private readonly List<SessionPlayer> _grabbers = [];
  private InventoryPickupRequestQueueComponent? _pickupRequests;

  public WorldItemTickSystem(
      ArchWorld ecsWorld,
      WorldItemStore items,
      ISpatialWorldQuery world,
      IWorldTileGrid tiles,
      ISessionPlayerSource sessionPlayers,
      int maximumStackSize)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _items = items ?? throw new ArgumentNullException(nameof(items));
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
    _sessionPlayers = sessionPlayers ?? throw new ArgumentNullException(nameof(sessionPlayers));
    if (maximumStackSize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumStackSize));
    }

    _maximumStackSize = maximumStackSize;
  }

  // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
  private InventoryPickupRequestQueueComponent PickupRequests =>
      _pickupRequests ??= InventoryPickupRequestQueue.Require(World);

  /// <summary>
  /// Mints the grab delay Version4 applies to a freshly thrown item.
  /// </summary>
  /// <remarks>
  /// Version4 在玩家/NPC 丢弃路径上手写 <c>noGrabDelay = 100</c>（`Player.cs:4998`、`NPC.cs:45151`），
  /// 避免刚扔出的物品立刻被自己吸回去。这里作为常量暴露，由请求方决定是否使用。
  /// </remarks>
  public const int ThrownItemGrabDelay = 100;

  public override void Update(in EcsServerTickContext context)
  {
    WorldItemState[] items = _items.SnapshotItems();
    if (items.Length == 0)
    {
      _grabbers.Clear();
      return;
    }

    MergePass(items);
    MotionPass(items);
    PickupPass(items);
    LifecyclePass(items);
  }

  /// <summary>Advances the per-slot Version4 lifecycle counters after the passes that read them.</summary>
  /// <remarks>
  /// 顺序与 Version4 `WorldItem.UpdateItem` 一致：先按本 tick 的 `noGrabDelay` 判定是否能被拾取，
  /// 末尾再统一递减计数。若先递减，刚生成的物品会在同一 tick 少掉一格延迟。
  /// </remarks>
  private void LifecyclePass(WorldItemState[] items)
  {
    for (int index = 0; index < items.Length; index++)
    {
      _items.TryAdvanceLifecycle(items[index].Slot, out _);
    }
  }

  /// <summary>Merges stacks forward, exactly like <c>TryCombiningIntoNearbyItems</c>.</summary>
  private void MergePass(WorldItemState[] items)
  {
    for (int survivorIndex = 0; survivorIndex < items.Length; survivorIndex++)
    {
      for (int drainedIndex = survivorIndex + 1; drainedIndex < items.Length; drainedIndex++)
      {
        // 每一对都在写的瞬间重新读槽位：本轮前半段被合并掉的槽位不能复活。
        if (!_items.TryGet(items[survivorIndex].Slot, out WorldItemState? survivor) ||
            survivor is null ||
            !_items.TryGet(items[drainedIndex].Slot, out WorldItemState? drained) ||
            drained is null)
        {
          continue;
        }

        int amount = WorldItemMergeRules.MergeAmount(survivor, drained, _maximumStackSize);
        if (amount <= 0)
        {
          continue;
        }

        _items.TryCombineStacks(survivor.Slot, drained.Slot, amount, out _, out _);
      }
    }
  }

  /// <summary>Integrates gravity, friction and the tile sweep for every live item.</summary>
  private void MotionPass(WorldItemState[] items)
  {
    for (int index = 0; index < items.Length; index++)
    {
      if (!_items.TryGet(items[index].Slot, out WorldItemState? live) || live is null)
      {
        continue;
      }

      SpatialLiquidContact liquid = _world.GetLiquidContact(live.Position, ItemSize, LiquidContact);
      WorldItemMotionProfile profile = WorldItemMotionSolver.ProfileFor(liquid.Flags);
      Vector2 velocity = WorldItemMotionSolver.Integrate(live.Velocity, profile);
      Vector2 displacement = WorldItemMotionSolver.ToDisplacement(velocity, profile);
      if (displacement == Vector2.Zero)
      {
        _items.TrySetMotion(live.Slot, live.Position, velocity, out _);
        continue;
      }

      SpatialTileMotionResult motion = _world.Move(
          live.Position,
          displacement,
          ItemSize,
          TileCollision,
          GravityDirection.Normal);
      if (!motion.IsFinite)
      {
        _items.TryRemove(live.Slot, out _);
        continue;
      }

      _items.TrySetMotion(live.Slot, motion.Position, motion.Velocity, out _);
      if (IsBelowWorld(motion.Position) && !HasSupport(motion.Position))
      {
        _items.TryRemove(live.Slot, out _);
      }
    }
  }

  /// <summary>Offers every grabbable item to the inventory of the nearest eligible player.</summary>
  private void PickupPass(WorldItemState[] items)
  {
    CollectGrabbers();
    if (_grabbers.Count == 0)
    {
      return;
    }

    for (int index = 0; index < items.Length; index++)
    {
      if (!_items.TryGet(items[index].Slot, out WorldItemState? live) || live is null)
      {
        continue;
      }

      // Version4 在 Player.GetItem(WorldItem, …) 里先看 noGrabDelay，再看装不装得下；
      // 这里提前到候选筛选，避免为一个明知不可抓的物品发起请求。
      if (!WorldItemRules.CanBePickedUp(live))
      {
        continue;
      }

      SessionPlayer? nearest = FindNearestGrabber(live);
      if (nearest is not SessionPlayer grabber)
      {
        continue;
      }

      if (!TryClaim(live, grabber.Epoch))
      {
        continue;
      }

      PickupRequests.Enqueue(new InventoryPickupRequest(
          grabber.Handle,
          grabber.Epoch,
          live.Slot,
          live.Item));
    }
  }

  /// <summary>
  /// Claims the slot for the grabbing session, releasing a reservation left behind by a session
  /// that no longer has a live player.
  /// </summary>
  private bool TryClaim(WorldItemState item, SessionEpoch epoch)
  {
    if (item.ReservedFor is SessionEpoch reserved)
    {
      if (reserved == epoch)
      {
        return true;
      }

      if (HasGrabber(reserved))
      {
        // 另一名在场玩家仍持有预约：Version4 在处理时保持原预约，不夺走。
        return false;
      }

      _items.TryRelease(item.Slot, reserved);
    }

    return _items.TryReserve(item.Slot, epoch);
  }

  private bool HasGrabber(SessionEpoch epoch)
  {
    for (int index = 0; index < _grabbers.Count; index++)
    {
      if (_grabbers[index].Epoch == epoch)
      {
        return true;
      }
    }

    return false;
  }

  private SessionPlayer? FindNearestGrabber(in WorldItemState item)
  {
    SessionPlayer? nearest = null;
    int bestCost = int.MaxValue;
    for (int index = 0; index < _grabbers.Count; index++)
    {
      SessionPlayer grabber = _grabbers[index];
      if (!WorldItemPickupRules.IsWithinGrabRange(
          item.Position,
          ItemSize,
          grabber.Position,
          grabber.Size,
          WorldItemPickupRules.DefaultGrabRange))
      {
        continue;
      }

      int cost = WorldItemPickupRules.ManhattanCost(
          item.Position,
          ItemSize,
          grabber.Position,
          grabber.Size);
      if (cost < bestCost)
      {
        bestCost = cost;
        nearest = grabber;
      }
    }

    return nearest;
  }

  /// <summary>
  /// Collects one grabber per active session.
  /// </summary>
  /// <remarks>
  /// 会话纪元与玩家句柄的绑定是会话身份，世界物品预约本来就以纪元表达（
  /// <see cref="WorldItemState.ReservedFor" />），所以这里只读投影，不写通信域状态。
  /// </remarks>
  private void CollectGrabbers()
  {
    _sessionPlayers.Collect(_grabbers);
  }

  private bool IsBelowWorld(Vector2 position)
  {
    float worldHeight = _tiles.Height * (float)_tiles.TileSize;
    return position.Y + ItemExtent >= worldHeight - 0.01f;
  }

  /// <summary>True when a solid tile sits on the row the item rests on.</summary>
  private bool HasSupport(Vector2 position)
  {
    float tileSize = _tiles.TileSize;
    int minimumX = (int)MathF.Floor(position.X / tileSize);
    int maximumX = (int)MathF.Floor((position.X + ItemExtent - 0.0001f) / tileSize);
    int row = (int)MathF.Floor((position.Y + ItemExtent) / tileSize);
    if (row < 0 || row >= _tiles.Height)
    {
      return false;
    }

    int firstColumn = Math.Clamp(minimumX, 0, _tiles.Width - 1);
    int lastColumn = Math.Clamp(maximumX, 0, _tiles.Width - 1);
    for (int x = firstColumn; x <= lastColumn; x++)
    {
      if (_tiles.ReadTile(new WorldTileCoordinate(x, row)).Active)
      {
        return true;
      }
    }

    return false;
  }
}
