using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Coalesces unsent values per target; only exact locally-sent receipts clear them.</summary>
public sealed class WorldItemProjectionJournal
{
  private readonly ServerStateCommitGate _gate;
  private readonly Dictionary<WorldScopeId, SessionEpoch[]> _targets = new();
  private readonly Dictionary<ProjectionKey, WorldItemProjection> _pending = new();
  private readonly Dictionary<ProjectionKey, Delivery> _inFlight = new();
  private long _sequence;

  internal WorldItemProjectionJournal(ServerStateCommitGate gate)
  {
    _gate = gate;
  }

  /// <summary>Host supplies authenticated world audiences; admission uses a full snapshot.</summary>
  public void SetTargets(WorldScopeId scope, IReadOnlyCollection<SessionEpoch> targets)
  {
    ArgumentNullException.ThrowIfNull(targets);
    if (!scope.IsValid || targets.Any(target => !target.IsValid))
    {
      throw new ArgumentException("Projection targets require a valid world and sessions.");
    }

    lock (_gate.SyncRoot)
    {
      SessionEpoch[] audience = targets.Distinct().ToArray();
      _targets[scope] = audience;
      foreach (ProjectionKey key in _pending.Keys.Where(key => key.Scope == scope
        && !audience.Contains(key.Target)).ToArray())
      {
        RemoveProjection(key);
      }
    }
  }

  public IReadOnlyList<WorldItemProjection> CapturePending(SessionEpoch target)
  {
    lock (_gate.SyncRoot)
    {
      return Array.AsReadOnly(_pending.Values.Where(value => value.Target == target)
        .Where(value => !_inFlight.ContainsKey(Key(value)))
        .Where(CanDispatch)
        .OrderBy(value => value.Sequence).ToArray());
    }
  }

  public void ReleaseSession(SessionEpoch target)
  {
    lock (_gate.SyncRoot)
    {
      foreach (WorldScopeId scope in _targets.Keys.ToArray())
      {
        _targets[scope] = _targets[scope].Where(epoch => epoch != target).ToArray();
      }

      foreach (ProjectionKey key in _pending.Where(pair => pair.Key.Target == target
        || pair.Value.InventoryOwnerEpoch == target).Select(pair => pair.Key).ToArray())
      {
        RemoveProjection(key);
      }

      // A newer inventory value can share the old owner's delivery key. Release only
      // that delivery, preserving the replacement pending value for the new owner.
      foreach (KeyValuePair<ProjectionKey, Delivery> pair in _inFlight.Where(pair =>
        pair.Value.Value.Target == target
          || pair.Value.Value.InventoryOwnerEpoch == target).ToArray())
      {
        RemoveProjectionInFlight(pair.Key, pair.Value);
        pair.Value.Invalidate();
      }
    }
  }

  public void ReleaseWorldScope(WorldScopeId scope)
  {
    lock (_gate.SyncRoot)
    {
      _targets.Remove(scope);
      foreach (ProjectionKey key in _pending.Keys.Where(key => key.Scope == scope).ToArray())
      {
        RemoveProjection(key);
      }
    }
  }

  internal bool CanPublish(
    IReadOnlyList<WorldItemSnapshot> items, IReadOnlyList<WorldItemHandle> ownership,
    int inventoryValues, IReadOnlyList<SessionPacketProjection>? playerValues = null,
    IReadOnlyList<WorldItemSnapshot>? shimmerEffects = null,
    int privateOwnershipValues = 0)
  {
    long count = (long)inventoryValues + privateOwnershipValues;
    if (shimmerEffects is not null)
    {
      foreach (WorldItemSnapshot effect in shimmerEffects)
      {
        count += Targets(effect.Handle.WorldScope).Length;
      }
    }
    if (playerValues is not null)
    {
      foreach (SessionPacketProjection projection in playerValues)
      {
        count += projection.TargetEpochs.Distinct()
          .Count(target => target != projection.IgnoreEpoch);
      }
    }
    foreach (WorldItemSnapshot item in items)
    {
      count += (long)Targets(item.Handle.WorldScope).Length
        * (item.Item.IsEmpty || item.Revision == CommitVersion.Initial ? 2 : 1);
    }

    foreach (WorldItemHandle handle in ownership)
    {
      count += Targets(handle.WorldScope).Length;
    }

    return count >= 0 && count <= long.MaxValue - _sequence;
  }

  /// <summary>
  /// Atomically binds all packets for one still-current value before dispatch. A value already
  /// captured by another delivery, or superseded by a newer coalesced value, is not rebound.
  /// </summary>
  public bool TryBindDelivery(
    Guid batchId, WorldItemProjection value, IReadOnlyList<OutboundPacketIntent> intents,
    out IOutboundDeliverySettlement? delivery)
  {
    ArgumentNullException.ThrowIfNull(value);
    ArgumentNullException.ThrowIfNull(intents);
    OutboundPacketIntent[] submittedIntents = intents.ToArray();
    if (batchId == Guid.Empty || submittedIntents.Length == 0 || submittedIntents.Any(intent =>
      intent is null || intent.CommitVersion != value.Revision || intent.TargetEpochs.Count != 1
      || intent.TargetEpochs[0] != value.Target || intent.IgnoreEpoch == value.Target))
    {
      throw new ArgumentException("A projection delivery must bind every packet to its target/version.");
    }

    delivery = null;
    lock (_gate.SyncRoot)
    {
      ProjectionKey key = Key(value);
      if (!_pending.TryGetValue(key, out WorldItemProjection? current)
        || !ReferenceEquals(current, value) || _inFlight.ContainsKey(key)
        || !CanDispatch(value))
      {
        return false;
      }

      var candidate = new Delivery(this, batchId, value, submittedIntents);
      _inFlight.Add(key, candidate);
      delivery = candidate;
      return true;
    }
  }

  /// <summary>Compatibility entry point for callers that require a delivery or an exception.</summary>
  public IOutboundDeliverySettlement BindDelivery(
    Guid batchId, WorldItemProjection value, IReadOnlyList<OutboundPacketIntent> intents)
  {
    if (TryBindDelivery(batchId, value, intents, out IOutboundDeliverySettlement? delivery)
      && delivery is not null)
    {
      return delivery;
    }

    throw new InvalidOperationException(
      "The projection is stale or already bound to an in-flight delivery.");
  }

  internal void PublishShimmer(WorldItemSnapshot item)
  {
    lock (_gate.SyncRoot)
    {
      foreach (SessionEpoch target in Targets(item.Handle.WorldScope))
      {
        Put(new WorldItemProjection(NextSequence(), target, item.Handle.WorldScope,
          WorldItemProjectionKind.Shimmer, item.Handle, item.Revision, Item: item));
      }
    }
  }

  internal void PublishWorld(WorldItemSnapshot item, SessionEpoch? ignoreEpoch = null)
  {
    lock (_gate.SyncRoot)
    {
      foreach (SessionEpoch target in Targets(item.Handle.WorldScope))
      {
        if (target == ignoreEpoch) {
          continue;
        }
        Put(new WorldItemProjection(NextSequence(), target, item.Handle.WorldScope,
          WorldItemProjectionKind.Item, item.Handle, item.Revision, Item: item));
        // Creation/retirement resets old-generation client ownership as well as the item body.
        if (item.Revision == CommitVersion.Initial || item.Item.IsEmpty)
        {
          PublishOwnershipTo(target, item.Handle, item.Revision, null, item.Position);
        }
      }
    }
  }

  internal void PublishOwnership(
    WorldItemHandle handle, CommitVersion revision, WorldItemReservation? reservation,
    Vector2 position, SessionEpoch? ignoreEpoch = null)
  {
    lock (_gate.SyncRoot)
    {
      foreach (SessionEpoch target in Targets(handle.WorldScope))
      {
        if (target != ignoreEpoch)
        {
          PublishOwnershipTo(target, handle, revision, reservation, position);
        }
      }
    }
  }

  internal void PublishOwnershipReleaseRequest(WorldItemReservation reservation)
  {
    lock (_gate.SyncRoot)
    {
      Put(new WorldItemProjection(NextSequence(), reservation.Epoch,
        reservation.Handle.WorldScope, WorldItemProjectionKind.OwnershipReleaseRequest,
        reservation.Handle, reservation.Revision, Reservation: reservation));
    }
  }

  internal void PublishInventory(
    WorldItemHandle source, SessionEpoch ownerEpoch, InventorySnapshot inventory)
  {
    PublishInventory(source.WorldScope, ownerEpoch, inventory);
  }

  internal void PublishInventory(
    WorldScopeId scope, SessionEpoch ownerEpoch, InventorySnapshot inventory,
    bool echoToOwner = true)
  {
    lock (_gate.SyncRoot)
    {
      foreach (SessionEpoch target in InventoryTargets(scope, ownerEpoch, echoToOwner))
      {
        Put(new WorldItemProjection(NextSequence(), target, scope,
          WorldItemProjectionKind.Inventory, default, inventory.Revision,
          Inventory: inventory, InventoryOwnerEpoch: ownerEpoch));
      }
    }
  }

  internal int CountInventoryTargets(
    WorldScopeId scope, SessionEpoch ownerEpoch, bool echoToOwner = true)
  {
    lock (_gate.SyncRoot)
    {
      return InventoryTargets(scope, ownerEpoch, echoToOwner).Length;
    }
  }

  private SessionEpoch[] InventoryTargets(
    WorldScopeId scope, SessionEpoch ownerEpoch, bool echoToOwner) => echoToOwner
      ? Targets(scope).Append(ownerEpoch).Distinct().ToArray()
      : Targets(scope).Where(target => target != ownerEpoch).ToArray();

  internal void PublishPlayer(
    WorldScopeId scope, IReadOnlyList<SessionPacketProjection> projections)
  {
    lock (_gate.SyncRoot)
    {
      foreach (SessionPacketProjection projection in projections)
      {
        foreach (SessionEpoch target in projection.TargetEpochs.Distinct())
        {
          if (target == projection.IgnoreEpoch)
          {
            continue;
          }

          Put(new WorldItemProjection(NextSequence(), target, scope,
            WorldItemProjectionKind.Player, default, projection.CommitVersion,
            PlayerProjection: projection));
        }
      }
    }
  }

  private void PublishOwnershipTo(
    SessionEpoch target, WorldItemHandle handle, CommitVersion revision,
    WorldItemReservation? reservation, Vector2 position)
  {
    Put(new WorldItemProjection(NextSequence(), target, handle.WorldScope,
      WorldItemProjectionKind.Ownership, handle, revision, Reservation: reservation,
      Position: position));
  }

  private SessionEpoch[] Targets(WorldScopeId scope) =>
    _targets.GetValueOrDefault(scope) ?? [];

  private long NextSequence() => checked(++_sequence);

  private void Put(WorldItemProjection value)
  {
    if (value.Kind is WorldItemProjectionKind.Item or WorldItemProjectionKind.Ownership)
    {
      // Events must retain the state that preceded them. Only coalesce unbound state
      // after the latest unresolved event, preserving every original in-flight value.
      long barrier = _pending.Values.Where(other => SameWorldSlot(other, value)
        && other.Kind is WorldItemProjectionKind.Shimmer
          or WorldItemProjectionKind.OwnershipReleaseRequest)
        .Select(other => other.Sequence).DefaultIfEmpty(0).Max();
      foreach (ProjectionKey key in _pending.Where(pair => SameWorldSlot(pair.Value, value)
        && pair.Value.Handle == value.Handle && pair.Value.Kind == value.Kind
          && pair.Value.Sequence > barrier
        && !_inFlight.ContainsKey(pair.Key)).Select(pair => pair.Key).ToArray())
      {
        _pending.Remove(key);
      }
    }

    _pending[Key(value)] = value;
  }

  private static ProjectionKey Key(WorldItemProjection value) => new(
    value.Target, value.WorldScope, value.Kind,
    value.Kind == WorldItemProjectionKind.Inventory ? -1 : value.Handle.SlotId,
    value.Inventory?.PlayerId ?? default,
    value.Kind == WorldItemProjectionKind.Inventory ? 0 : value.Sequence);

  private bool CanDispatch(WorldItemProjection value)
  {
    if (value.Kind is not (WorldItemProjectionKind.Item
      or WorldItemProjectionKind.Ownership or WorldItemProjectionKind.Shimmer
      or WorldItemProjectionKind.OwnershipReleaseRequest))
    {
      return true;
    }

    // Serialize each observer's slot, including retirement/reuse and failed deliveries.
    return !_pending.Values.Any(other => SameWorldSlot(other, value)
        && other.Sequence < value.Sequence)
      && !_inFlight.Values.Any(delivery => SameWorldSlot(delivery.Value, value)
        && delivery.Value.Sequence < value.Sequence);
  }

  private static bool SameWorldSlot(WorldItemProjection first, WorldItemProjection second) =>
    first.Kind is WorldItemProjectionKind.Item or WorldItemProjectionKind.Ownership
      or WorldItemProjectionKind.Shimmer or WorldItemProjectionKind.OwnershipReleaseRequest
    && first.Target == second.Target && first.WorldScope == second.WorldScope
    && first.Handle.SlotId == second.Handle.SlotId;

  private void RemoveProjection(ProjectionKey key)
  {
    _pending.Remove(key);
    if (_inFlight.Remove(key, out Delivery? delivery))
    {
      delivery.Invalidate();
    }
  }

  private readonly record struct ProjectionKey(
    SessionEpoch Target, WorldScopeId Scope, WorldItemProjectionKind Kind,
    int Slot, EntityUuid Player, long EventSequence);

  private sealed class Delivery : IOutboundDeliverySettlement
  {
    private readonly WorldItemProjectionJournal _owner;
    private readonly WorldItemProjection _value;
    private bool _settled;

    internal Delivery(
      WorldItemProjectionJournal owner, Guid batchId, WorldItemProjection value,
      IReadOnlyList<OutboundPacketIntent> intents)
    {
      _owner = owner;
      _value = value;
      Intents = Array.AsReadOnly(intents.Select((intent, index) => new OutboundPacketIntent(
        [value.Target], null, intent.MessageId, intent.Payload, value.Revision,
        new OutboundDeliveryToken(OwnerKey, batchId, value.Target, index))).ToArray());
    }

    public string OwnerKey => "P04.WorldItems";
    public IReadOnlyList<OutboundPacketIntent> Intents { get; }
    internal WorldItemProjection Value => _value;

    public OutboundDeliverySettlementResult Settle(IReadOnlyList<OutboundIntentDelivery> deliveries)
    {
      ArgumentNullException.ThrowIfNull(deliveries);
      lock (_owner._gate.SyncRoot)
      {
        if (_settled)
        {
          return OutboundDeliverySettlementResult.Invalid(
            OwnerKey, "Delivery batch is incomplete or settled.");
        }

        ProjectionKey key = Key(_value);
        if (!_owner._inFlight.TryGetValue(key, out Delivery? active)
          || !ReferenceEquals(active, this))
        {
          _settled = true;
          return OutboundDeliverySettlementResult.Invalid(
            OwnerKey, "The delivery was superseded or released.");
        }

        if (deliveries.Count != Intents.Count
          || !ReceiptsAreSuccessful(deliveries))
        {
          _owner.RemoveProjectionInFlight(key, this);
          _settled = true;
          return OutboundDeliverySettlementResult.Invalid(
            OwnerKey, "Packets are not all locally sent.");
        }

        if (!_owner._pending.TryGetValue(key, out WorldItemProjection? current)
          || !ReferenceEquals(current, _value) || current.Sequence != _value.Sequence
          || current.Revision != _value.Revision || current.Handle != _value.Handle)
        {
          _owner.RemoveProjectionInFlight(key, this);
          _settled = true;
          return OutboundDeliverySettlementResult.Invalid(
            OwnerKey, "The projection value changed before settlement.");
        }

        _owner._inFlight.Remove(key);
        _owner._pending.Remove(key);
        _settled = true;
        return new OutboundDeliverySettlementResult(OwnerKey, true, null);
      }
    }

    internal void Invalidate()
    {
      _settled = true;
    }

    private bool ReceiptsAreSuccessful(IReadOnlyList<OutboundIntentDelivery> deliveries)
    {
      var sequences = new HashSet<PacketSequence>();
      for (int index = 0; index < Intents.Count; index++)
      {
        OutboundIntentDelivery? delivery = deliveries[index];
        // Exact submitted objects prevent matching a stale token to different bytes.
        if (delivery is null || !ReferenceEquals(delivery.Intent, Intents[index])
          || delivery.Receipts.Count != 1 || delivery.Receipts[0].Epoch != _value.Target
          || delivery.Receipts[0].Certainty != TransportSendCertainty.LocallySent
          || delivery.Receipts[0].ErrorCode is not null
          || delivery.Receipts[0].BytesSubmitted == 0
          || !sequences.Add(delivery.Receipts[0].Sequence))
        {
          return false;
        }
      }

      return true;
    }
  }

  private void RemoveProjectionInFlight(ProjectionKey key, Delivery delivery)
  {
    if (_inFlight.TryGetValue(key, out Delivery? active)
      && ReferenceEquals(active, delivery))
    {
      _inFlight.Remove(key);
    }
  }
}
