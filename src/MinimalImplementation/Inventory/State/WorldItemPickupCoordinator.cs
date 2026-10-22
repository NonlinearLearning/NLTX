using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Rules;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Credits a prepared inventory/player effect before retiring or reducing an item.</summary>
public sealed class WorldItemPickupCoordinator
{
  private readonly InventoryStore _inventories;
  private readonly WorldItemStore _items;
  private readonly WorldItemReservationStore _claims;
  private readonly WorldItemTransactionCoordinator _transactions;
  private readonly WorldItemPickupPlanner _planner;
  private readonly ItemPickupCatalog _catalog;
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly WorldOwner _world;

  public WorldItemPickupCoordinator(
    InventoryStore inventories, WorldItemStore items, WorldItemReservationStore claims,
    WorldItemPickupPlanner planner, ItemPickupCatalog catalog,
    PlayerOwner players, SessionAdmissionCoordinator sessions, WorldOwner world)
  {
    ArgumentNullException.ThrowIfNull(planner);
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(world);
    _transactions = new WorldItemTransactionCoordinator(inventories, items, claims);
    _inventories = inventories;
    _items = items;
    _claims = claims;
    _planner = planner;
    _catalog = catalog;
    _players = players;
    _sessions = sessions;
    _world = world;
  }

  public OwnerCommitResult Prepare(
    WorldItemHandle handle, CommitVersion expectedRevision,
    EntityUuid playerId, SessionEpoch epoch, DateTimeOffset now,
    IWorldItemPickupPreparation player,
    out WorldItemTransactionCoordinator.PreparedTransaction? transaction)
  {
    ArgumentNullException.ThrowIfNull(player);
    transaction = null;
    lock (_items.CommitGate.SyncRoot)
    {
      if (player.PlayerHandle.Id != playerId || player.PlayerHandle.WorldScope != handle.WorldScope
        || player.Epoch != epoch || player.ItemHandle != handle
        || player.ItemRevision != expectedRevision || player.Width <= 0 || player.Height <= 0)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "The pickup preparation does not match its identities.");
      }

      OwnerCommitResult result = WorldItemOwnerReadGuard.Capture(
        _players, _sessions, _world, epoch, handle.WorldScope, player.WorldEpoch, out var reads);
      if (reads is null || result.Status != OwnerCommitStatus.Applied)
      {
        return result;
      }

      if (!_items.TryCapture(handle.SlotId, out WorldItemSnapshot? item) || item is null
        || item.Handle != handle || item.Revision != expectedRevision)
      {
        return Conflict("The pickup world item changed.");
      }

      WorldItemRuntime runtime = item.Runtime ?? new WorldItemRuntime();
      Vector2 position = new(reads.Movement.Position.X, reads.Movement.Position.Y);
      if (!player.CanAcceptItem || runtime.NoGrabDelay > 0 || item.ShimmerTime != 0
        || item.IsShimmering && item.Velocity.Length() >= 0.2f
        || !Intersects(position, player.Width, player.Height, item.Position)
        || !_claims.TryGet(handle, now, out WorldItemReservation? reservation)
        || reservation is null || reservation.PlayerId != playerId || reservation.Epoch != epoch)
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
          "Pickup requires intersection, accepted item and the current session reservation.");
      }

      WorldItemPickupEffect effect = WorldItemPickupEffect.ForItem(item.Item.Type);
      if (player.Effect != effect)
      {
        return Unavailable("The Player owner has not prepared the required pickup effect.");
      }

      WorldItemMutation? remainder = null;
      WorldItemInventoryChange? inventory = null;
      if (effect == default)
      {
        if (!_catalog.TryGet(item.Item.Type, out ItemPickupFacts? facts) || facts is null)
        {
          return Unavailable("Verified pickup content facts are unavailable for this item.");
        }

        if (!_inventories.TryCapture(playerId, out InventorySnapshot? slots) || slots is null
          || slots.Slots.Count < (player.VoidVaultEnabled ? 900 : 58))
        {
          return Unavailable("The authoritative inventory does not contain the pickup slots.");
        }

        WorldItemPickupPlan plan = _planner.Plan(slots, item.Item, facts, player.VoidVaultEnabled);
        if (plan.Remainder.Stack == item.Item.Stack)
        {
          return Conflict("The inventory cannot accept any of this item stack.");
        }

        inventory = new WorldItemInventoryChange(playerId, epoch, slots.Revision, plan.Mutations);
        if (!plan.Remainder.IsEmpty)
        {
          remainder = WorldItemMutation.FromSnapshot(item) with { Item = plan.Remainder };
        }
      }

      return _transactions.Prepare(
        [new WorldItemChange(handle, expectedRevision, remainder)], [], [], inventory,
        [reads, player, new PickupClaimReadGuard(_claims, reservation, now)], out transaction);
    }
  }

  /// <summary>
  /// Prepares only attraction. Spatial and pickup captures must describe the same Player
  /// version; no pickup effect or Player projection is committed by this transaction.
  /// </summary>
  public OwnerCommitResult PrepareAttraction(
    WorldItemHandle handle, CommitVersion expectedRevision,
    PlayerSpatialSnapshot player, IPlayerPickupFactsCapture sources,
    ulong worldEpoch, DateTimeOffset now,
    out WorldItemTransactionCoordinator.PreparedTransaction? transaction)
  {
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(sources);
    transaction = null;
    lock (_items.CommitGate.SyncRoot)
    {
      PlayerPickupSourceFacts facts = sources.Facts;
      if (player.PlayerHandle.WorldScope != handle.WorldScope || player.Width <= 0
        || player.Height <= 0 || sources.PlayerHandle != player.PlayerHandle
        || sources.Epoch != player.Epoch || sources.PlayerVersion != player.Version
        || !sources.SourceRevision.IsValid || facts is null || facts.ItemAnimation < 0)
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
          "Attraction requires matching Player geometry and pickup source captures.");
      }

      OwnerCommitResult result = WorldItemOwnerReadGuard.Capture(
        _players, _sessions, _world, player.Epoch, handle.WorldScope, worldEpoch, out var reads);
      if (reads is null || result.Status != OwnerCommitStatus.Applied)
      {
        return result;
      }

      if (!_items.TryCapture(handle.SlotId, out WorldItemSnapshot? item) || item is null
        || item.Handle != handle || item.Revision != expectedRevision)
      {
        return Conflict("The attraction world item changed.");
      }

      Vector2 position = new(player.Position.X, player.Position.Y);
      WorldItemRuntime runtime = item.Runtime ?? new WorldItemRuntime();
      int range = facts.GetItemGrabRange(item.Item.Type);
      if (runtime.NoGrabDelay != 0 || item.ShimmerTime != 0
        || item.IsShimmering && (double)item.Velocity.Length() >= 0.2
        || facts.PreventAllItemPickups && !ItemContentFacts.IsPickup(item.Item.Type)
        || Intersects(position, player.Width, player.Height, item.Position)
        || !Intersects(position, player.Width, player.Height, item.Position, range)
        || !_claims.TryGet(handle, now, out WorldItemReservation? reservation)
        || reservation is null || reservation.PlayerId != player.PlayerHandle.Id
        || reservation.Epoch != player.Epoch)
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
          "Attraction requires the current reservation and a nonintersecting item in grab range.");
      }

      if (!_inventories.TryCapture(player.PlayerHandle.Id, out InventorySnapshot? slots)
        || slots is null)
      {
        return Unavailable("The authoritative attraction inventory is unavailable.");
      }

      bool goingToVoid = false;
      if (!ItemContentFacts.IsPickup(item.Item.Type))
      {
        if (!_catalog.TryGet(item.Item.Type, out ItemPickupFacts? itemFacts) || itemFacts is null
          || !_planner.TryGetAttractionSpace(slots, item.Item, itemFacts,
            facts.VoidVaultEnabled, out bool canTake, out goingToVoid))
        {
          return Unavailable("Verified ItemSpace facts or authoritative slots are unavailable.");
        }

        if (!canTake)
        {
          return Conflict("The inventory cannot accept this item for attraction.");
        }
      }

      WorldItemMutation mutation = WorldItemMutation.FromSnapshot(item) with {
        Velocity = GetAttractionVelocity(item, player, reads.Movement, facts, goingToVoid),
        IsShimmering = false,
        Runtime = runtime with { BeingGrabbed = true }
      };
      return _transactions.Prepare([new WorldItemChange(handle, expectedRevision, mutation)],
        [], [], null,
        [reads, new AttractionReadGuard(player, sources, facts, sources.SourceRevision,
          _inventories, slots), new PickupClaimReadGuard(_claims, reservation, now)],
        out transaction);
    }
  }

  private static Vector2 GetAttractionVelocity(
    WorldItemSnapshot item, PlayerSpatialSnapshot player, PlayerMovementInput movement,
    PlayerPickupSourceFacts facts, bool goingToVoid)
  {
    int type = item.Item.Type;
    float speed;
    int acceleration = 5;
    if (facts.ManaMagnet && type is (184 or 1735 or 1868)) {
      speed = 12f;
    } else if (facts.LifeMagnet && type is (58 or 1734 or 1867)) {
      speed = 15f;
    } else if (type is 3453 or 3454 or 3455 || goingToVoid
      || facts.GoldRing && ItemContentFacts.IsCoin(type)) {
      speed = 12f;
    } else if (facts.JourneyFarPlacementEnabled) {
      speed = 7f;
      acceleration = 1;
    } else {
      bool right = (double)player.Position.X + player.Width * 0.5 > (double)item.Position.X + 8;
      bool down = (double)player.Position.Y + player.Height * 0.5 > (double)item.Position.Y + 8;
      return new Vector2(
        PullAxis(item.Velocity.X, right, movement.Velocity.X),
        PullAxis(item.Velocity.Y, down, 0f));
    }

    float x = player.Center.X - (item.Position.X + 8f);
    float y = player.Center.Y - (item.Position.Y + 8f);
    float factor = speed / (float)Math.Sqrt(x * x + y * y);
    return new Vector2(
      (item.Velocity.X * (acceleration - 1) + x * factor) / acceleration,
      (item.Velocity.Y * (acceleration - 1) + y * factor) / acceleration);
  }

  private static float PullAxis(float velocity, bool positive, float playerVelocity)
  {
    const float pullSpeed = 0.45f;
    const float maximumSpeed = 4f;
    const float reversalFactor = 0.75f;
    if (positive)
    {
      if (velocity < maximumSpeed + playerVelocity)
      {
        velocity += pullSpeed;
      }
      if (velocity < 0f)
      {
        velocity += pullSpeed * reversalFactor;
      }
    }
    else
    {
      if (velocity > -maximumSpeed + playerVelocity)
      {
        velocity -= pullSpeed;
      }
      if (velocity > 0f)
      {
        velocity -= pullSpeed * reversalFactor;
      }
    }
    return velocity;
  }

  private static bool Intersects(
    Vector2 player, int width, int height, Vector2 item, int range = 0)
  {
    int playerX = (int)player.X;
    int playerY = (int)player.Y;
    int itemX = (int)item.X;
    int itemY = (int)item.Y;
    return (long)playerX - range < (long)itemX + 16 && (long)playerX + width + range > itemX
      && (long)playerY - range < (long)itemY + 16 && (long)playerY + height + range > itemY;
  }

  private static OwnerCommitResult Conflict(string detail) => new(
    OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict, detail);

  private static OwnerCommitResult Unavailable(string detail) => new(
    OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);

  private sealed class AttractionReadGuard(
    PlayerSpatialSnapshot player,
    IPlayerPickupFactsCapture sources,
    PlayerPickupSourceFacts facts,
    PlayerSourceRevision sourceRevision,
    InventoryStore inventories,
    InventorySnapshot inventory) : IWorldItemTransactionParticipant
  {
    public OwnerCommitResult Validate()
    {
      if (sources.PlayerHandle != player.PlayerHandle || sources.Epoch != player.Epoch
        || sources.PlayerVersion != player.Version || sources.SourceRevision != sourceRevision
        || sources.Facts != facts
        || !inventories.TryCapture(inventory.PlayerId, out InventorySnapshot? current)
        || current is null || current.Revision != inventory.Revision
        || !current.Slots.SequenceEqual(inventory.Slots))
      {
        return Conflict("The attraction source facts or inventory slots changed.");
      }

      OwnerCommitResult result = player.Validate();
      return result.Status == OwnerCommitStatus.Applied ? sources.Validate() : result;
    }

    public void Commit()
    {
    }
  }

  private sealed class PickupClaimReadGuard(
    WorldItemReservationStore claims,
    WorldItemReservation reservation,
    DateTimeOffset capturedAt) : IWorldItemTransactionParticipant
  {
    public OwnerCommitResult Validate()
    {
      return claims.TryGet(reservation.Handle, capturedAt, out WorldItemReservation? current)
        && current == reservation
        ? OwnerCommitResult.Applied(reservation.Revision)
        : Conflict("The pickup reservation changed after preparation.");
    }

    public void Commit()
    {
    }
  }
}
