using System.Numerics;
using EntityEcs.Components;
using NSSLC.WorldGeneration;
using Terraria.Content;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Player;
using Terraria.Relationships;
using Terraria.WorldStorage;
using ItemEntityRef = Terraria.Relationships.ItemEntityRef;
using LegacyVector2 = NSSLC.WorldGeneration.Geometry.Vector2;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed partial class RuntimeWorldItemStore
{
  private const int MaximumWorldItems = 400;
  private const int GelTypeId = 23;
  private const float PickupRange = 48f;
  private const int TimeLeftTicks = 6_000;
  private const float Gravity = 0.2f;

  private readonly LoadedWorldSession _session;
  private readonly RuntimeItemRegistry _registry;
  private long _currentTick;
  private long _nextReplicationId;

  public RuntimeWorldItemStore(
    LoadedWorldSession session,
    ContentCatalog catalog,
    RuntimeItemRegistry registry)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
    ArgumentNullException.ThrowIfNull(catalog);
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    if (!ReferenceEquals(session.EntityRuntime, registry.EntityRuntime))
    {
      throw new ArgumentException(
        "World item registry and store must share the loaded session EntityRuntime.",
        nameof(registry));
    }
    if (!catalog.Items.TryGet(GelTypeId, out _))
    {
      throw new InvalidOperationException(
        "The supported NPC drop item is missing from the content catalog.");
    }
  }

  public int ActiveCount => _session.Storage.WorldItems.ActiveCount;

  public int NpcDeathDropCount { get; private set; }

  public int PickupCount { get; private set; }

  public int PartialPickupCount { get; private set; }

  public int ExpiredCount { get; private set; }

  public float? LastPickupDistanceToPlayerHitbox { get; private set; }

  internal bool IsBoundTo(LoadedWorldSession session) =>
    ReferenceEquals(_session, session) &&
    ReferenceEquals(_session.EntityRuntime, session.EntityRuntime);

  public IReadOnlyList<RuntimeWorldItemSnapshot> CreateSnapshot()
  {
    var snapshots = new List<RuntimeWorldItemSnapshot>(_session.Storage.WorldItems.ActiveCount);
    EntitySlotStore<WorldEntityState, WorldItemSlot> items = _session.Storage.WorldItems;
    for (int index = 0; index < items.Capacity; index++)
    {
      if (!items.TryGetOccupiedAt(index, out _, out _, out WorldEntityState? entity))
      {
        continue;
      }

      RuntimeWorldItemState itemState = entity as RuntimeWorldItemState ??
        throw new InvalidOperationException(
          "A world item slot contains an unsupported runtime projection.");
      if (!_registry.TryGet(itemState.Entity, out PlayerInventoryItemSnapshot payload) ||
          !_registry.TryGetWorldItem(itemState.Entity, out RuntimeWorldItemProjection worldItem))
      {
        throw new InvalidOperationException("A world item no longer has its item-root state.");
      }

      WorldItemComponentProjection state = worldItem.WorldItem.Value;
      LocationComponent location = worldItem.Location.Value;
      VelocityComponent velocity = worldItem.Velocity.Value;
      snapshots.Add(new RuntimeWorldItemSnapshot(
        payload.TypeId,
        payload.Stack,
        location.X,
        location.Y,
        velocity.X,
        velocity.Y,
        ToRemainingTicks(state.RemainingTicksAt(_currentTick))));
    }

    return snapshots.AsReadOnly();
  }

  public void SpawnNpcDeathDrops(int npcNetId, Vector2 position)
  {
    if (npcNetId != 3)
    {
      return;
    }

    SpawnWorldItem(GelTypeId, stack: 1, position, new Vector2(0f, -2f), TimeLeftTicks);
    NpcDeathDropCount++;
  }

  public void SpawnProbeItem(int typeId, int stack, Vector2 position, int timeLeft)
  {
    SpawnWorldItem(typeId, stack, position, Vector2.Zero, timeLeft);
  }

  public void Update(long tickNumber, RuntimePlayerStore players)
  {
    ArgumentNullException.ThrowIfNull(players);
    if (tickNumber <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tickNumber));
    }

    _currentTick = tickNumber;
    EntitySlotStore<WorldEntityState, WorldItemSlot> items = _session.Storage.WorldItems;
    for (int index = 0; index < items.Capacity; index++)
    {
      if (!items.TryGetOccupiedAt(
            index,
            out WorldItemSlot slot,
            out uint generation,
            out WorldEntityState? entity))
      {
        continue;
      }

      RuntimeWorldItemState itemState = entity as RuntimeWorldItemState ??
        throw new InvalidOperationException(
          "A world item slot contains an unsupported runtime projection.");
      if (!_registry.TryGet(itemState.Entity, out PlayerInventoryItemSnapshot item) ||
          item.IsEmpty ||
          !_registry.TryGetWorldItem(itemState.Entity, out RuntimeWorldItemProjection worldItem))
      {
        throw new InvalidOperationException(
          "A world item slot no longer matches its item-root state.");
      }

      LocationComponent currentLocation = worldItem.Location.Value;
      VelocityComponent currentVelocity = worldItem.Velocity.Value;
      UpdatePhysics(
        new Vector2(currentLocation.X, currentLocation.Y),
        new Vector2(currentVelocity.X, currentVelocity.Y),
        worldItem.Collider.Value,
        out Vector2 nextPosition,
        out Vector2 nextVelocity);

      bool expired = worldItem.WorldItem.Value.IsExpiredAt(tickNumber);
      if (expired)
      {
        if (!_registry.CanRemove(itemState.Entity) ||
            !_registry.TryDetachWorldPresence(
              itemState.Entity,
              () => Release(slot, generation)) ||
            !_registry.Remove(itemState.Entity))
        {
          throw new InvalidOperationException(
            "An expired world item could not release its world presence and item root.");
        }
        ExpiredCount++;
        continue;
      }

      if (!_registry.TryUpdateWorldItem(
            worldItem,
            new LocationComponent(nextPosition.X, nextPosition.Y),
            new VelocityComponent(nextVelocity.X, nextVelocity.Y)) ||
          !_registry.TryGet(itemState.Entity, out item))
      {
        throw new InvalidOperationException(
          "World item motion could not commit against its captured component revisions.");
      }

      foreach (RuntimePlayerEntity player in players.Players)
      {
        if (player.Lifecycle.IsDead)
        {
          continue;
        }

        if (!CanPlayerPickUp(
          nextPosition,
          worldItem.Collider.Value,
          new Vector2(player.Location.X, player.Location.Y),
          player.Collider,
          out float pickupDistance))
        {
          continue;
        }

        bool worldProjectionReleased = false;
        bool ReleaseWorldProjection()
        {
          if (worldProjectionReleased)
          {
            return true;
          }

          worldProjectionReleased = Release(slot, generation);
          return worldProjectionReleased;
        }

        PlayerInventoryPickupResult pickup =
          player.Inventory.TryPickup(item, ReleaseWorldProjection);
        if (!pickup.Applied)
        {
          continue;
        }

        LastPickupDistanceToPlayerHitbox = pickupDistance;
        if (pickup.RemainingStack == 0)
        {
          if (!worldProjectionReleased)
          {
            if (!Release(slot, generation))
            {
              throw new InvalidOperationException(
                "A fully transferred world item slot could not be released.");
            }
          }

          PickupCount++;
        }
        else
        {
          PartialPickupCount++;
        }

        break;
      }
    }
  }

  private bool Release(WorldItemSlot slot, uint generation)
  {
    return _session.Storage.WorldItems.TryRelease(slot, generation, out _);
  }

  private void SpawnWorldItem(
    int typeId,
    int stack,
    Vector2 position,
    Vector2 velocity,
    int timeLeft)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeLeft);
    EntitySlotStore<WorldEntityState, WorldItemSlot> items = _session.Storage.WorldItems;
    if (items.ActiveCount >= MaximumWorldItems)
    {
      throw new InvalidOperationException(
        "The world item capacity is exhausted; the item was not committed.");
    }

    PlayerInventoryItemSnapshot payload = _registry.CreateWorldDrop(
      typeId,
      stack,
      position.X,
      position.Y,
      velocity.X,
      velocity.Y,
      new ColliderComponent(16.0f, 16.0f),
      _currentTick,
      timeLeft,
      NextReplicationId());
    bool allocated = false;
    try
    {
      if (!items.TryAllocate(
            new RuntimeWorldItemState(payload.Entity),
            out _,
            out _))
      {
        throw new InvalidOperationException("The world item slot store rejected a free slot.");
      }

      allocated = true;
    }
    catch (Exception allocationException)
    {
      if (!allocated)
      {
        try
        {
          if (!_registry.Remove(payload.Entity))
          {
            throw new InvalidOperationException(
              "A world item root could not be rolled back after slot allocation failed.");
          }
        }
        catch (Exception cleanupException)
        {
          throw new AggregateException(
            "World item slot allocation failed and its item root could not be removed.",
            allocationException,
            cleanupException);
        }
      }

      throw;
    }
  }

  private ReplicationId NextReplicationId()
  {
    _nextReplicationId = checked(_nextReplicationId + 1);
    return new ReplicationId(_nextReplicationId);
  }

  private static int ToRemainingTicks(long remainingTicks)
  {
    return (int)Math.Clamp(remainingTicks, 0, int.MaxValue);
  }

  private static void UpdatePhysics(
    Vector2 position,
    Vector2 velocity,
    ColliderComponent collider,
    out Vector2 nextPosition,
    out Vector2 nextVelocity)
  {
    Vector2 requestedVelocity = velocity;
    requestedVelocity.Y = Math.Min(requestedVelocity.Y + Gravity, 10f);
    int colliderWidth = ToCollisionExtent(collider.Width);
    int colliderHeight = ToCollisionExtent(collider.Height);
    LegacyVector2 resolvedVelocity = Collision.TileCollision(
      new LegacyVector2(
        position.X + collider.OffsetX,
        position.Y + collider.OffsetY),
      new LegacyVector2(requestedVelocity.X, requestedVelocity.Y),
      colliderWidth,
      colliderHeight);
    bool collidedX = MathF.Abs(resolvedVelocity.X - requestedVelocity.X) > 0.001f;
    bool collidedY = MathF.Abs(resolvedVelocity.Y - requestedVelocity.Y) > 0.001f;
    nextPosition = position + new Vector2(resolvedVelocity.X, resolvedVelocity.Y);
    nextVelocity = new Vector2(
      collidedX ? 0f : resolvedVelocity.X,
      collidedY ? 0f : resolvedVelocity.Y);
  }

  internal static bool CanPlayerPickUp(
    Vector2 itemPosition,
    ColliderComponent itemCollider,
    Vector2 playerPosition,
    ColliderComponent playerCollider,
    out float distanceToPlayerHitbox)
  {
    float itemLeft = itemPosition.X + itemCollider.OffsetX;
    float itemTop = itemPosition.Y + itemCollider.OffsetY;
    float playerLeft = playerPosition.X + playerCollider.OffsetX;
    float playerTop = playerPosition.Y + playerCollider.OffsetY;
    float itemCenterX = itemLeft + itemCollider.Width * 0.5f;
    float itemCenterY = itemTop + itemCollider.Height * 0.5f;
    float nearestX = Math.Clamp(
      itemCenterX,
      playerLeft,
      playerLeft + playerCollider.Width);
    float nearestY = Math.Clamp(
      itemCenterY,
      playerTop,
      playerTop + playerCollider.Height);
    float deltaX = itemCenterX - nearestX;
    float deltaY = itemCenterY - nearestY;
    distanceToPlayerHitbox = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
    return distanceToPlayerHitbox <= PickupRange;
  }

  private static int ToCollisionExtent(float extent)
  {
    if (!float.IsFinite(extent) || extent <= 0.0f || extent != MathF.Truncate(extent))
    {
      throw new InvalidOperationException(
        "Tile collision requires positive whole-pixel collider dimensions.");
    }

    return checked((int)extent);
  }

  private sealed class RuntimeWorldItemState(ItemEntityRef entity) : WorldEntityState
  {
    public ItemEntityRef Entity { get; } = entity;
  }
}

internal readonly record struct RuntimeWorldItemSnapshot(
  int TypeId,
  int Stack,
  float X,
  float Y,
  float VelocityX,
  float VelocityY,
  int TimeLeft);
