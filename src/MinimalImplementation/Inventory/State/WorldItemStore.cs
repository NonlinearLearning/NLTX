using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Rules;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Owns immutable world item states and generation-protected slot reuse.</summary>
public sealed class WorldItemStore
{
  private readonly Dictionary<int, WorldItemSnapshot> _items = new();
  private readonly Dictionary<int, long> _nextGenerationBySlot = new();
  private readonly ItemValidationPolicy _itemPolicy;
  private readonly int _slotCount;
  private WorldItemReservationStore? _reservations;

  public WorldItemStore(int slotCount, ItemValidationPolicy itemPolicy)
    : this(slotCount, itemPolicy, new ServerStateCommitGate())
  {
  }

  public WorldItemStore(
    int slotCount, ItemValidationPolicy itemPolicy, ServerStateCommitGate commitGate)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(slotCount);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(slotCount, short.MaxValue + 1);
    ArgumentNullException.ThrowIfNull(itemPolicy);
    ArgumentNullException.ThrowIfNull(commitGate);
    _slotCount = slotCount;
    _itemPolicy = itemPolicy;
    CommitGate = commitGate;
    Projections = new WorldItemProjectionJournal(commitGate);
  }

  internal ServerStateCommitGate CommitGate { get; }
  public WorldItemProjectionJournal Projections { get; }

  internal void BindReservations(WorldItemReservationStore reservations)
  {
    lock (CommitGate.SyncRoot)
    {
      if (_reservations is not null && !ReferenceEquals(_reservations, reservations))
      {
        throw new InvalidOperationException("A world-item store has one reservation owner.");
      }

      _reservations = reservations;
    }
  }

  public OwnerCommitResult Create(
    WorldScopeId worldScope, int slotId, InventoryItemState item,
    Vector2 position, Vector2 velocity, int timeLeft, bool isShimmering,
    int enemyPickupDelay, out WorldItemSnapshot? snapshot, float shimmerTime = 0)
  {
    lock (CommitGate.SyncRoot)
    {
      var initial = new WorldItemMutation(
        item, position, velocity, timeLeft, isShimmering, enemyPickupDelay, shimmerTime,
        new WorldItemRuntime());
      OwnerCommitResult result = PrepareBatch(
        [], [new WorldItemSpawn(worldScope, slotId, initial)], out PreparedBatch? prepared);
      snapshot = null;
      if (prepared is not null)
      {
        CommitPrepared(prepared);
        snapshot = prepared.After[0];
      }

      return result;
    }
  }

  public bool TryCapture(int slotId, out WorldItemSnapshot? snapshot)
  {
    lock (CommitGate.SyncRoot)
    {
      return _items.TryGetValue(slotId, out snapshot);
    }
  }

  public OwnerCommitResult Update(
    WorldItemHandle handle, CommitVersion expectedRevision, WorldItemMutation mutation,
    out WorldItemSnapshot? snapshot, SessionEpoch? ignoreEpoch = null)
  {
    ArgumentNullException.ThrowIfNull(mutation);
    lock (CommitGate.SyncRoot)
    {
      OwnerCommitResult result = PrepareBatch(
        [new WorldItemChange(handle, expectedRevision, mutation)], [],
        out PreparedBatch? prepared);
      snapshot = null;
      if (prepared is not null)
      {
        CommitPrepared(prepared, publish: false);
        Publish(prepared, ignoreEpoch);
        snapshot = prepared.After[0];
      }

      return result;
    }
  }

  public OwnerCommitResult Despawn(WorldItemHandle handle, CommitVersion expectedRevision)
  {
    lock (CommitGate.SyncRoot)
    {
      OwnerCommitResult result = PrepareBatch(
        [new WorldItemChange(handle, expectedRevision, null)], [], out PreparedBatch? prepared);
      if (prepared is not null)
      {
        WorldItemReservationStore.PreparedRelease? claims = null;
        if (_reservations is not null && !_reservations.PrepareRelease([handle], out claims))
        {
          return Unavailable("The retiring item claim cannot be prepared.");
        }

        if (!Projections.CanPublish(prepared.After, claims is null ? [] : [handle], 0))
        {
          return Unavailable("The projection sequence space is exhausted.");
        }

        CommitPrepared(prepared, publish: false);
        if (claims is not null)
        {
          _reservations!.CommitPrepared(claims, publish: false);
        }

        Publish(prepared);
        if (claims is not null)
        {
          _reservations!.Publish(claims);
        }
      }

      return result;
    }
  }

  /// <summary>Prepares retirement and next-generation reuse of the same world slot.</summary>
  public OwnerCommitResult PrepareRetireAndReuse(
    WorldItemHandle retiringHandle, CommitVersion expectedRevision,
    WorldItemMutation replacement, out PreparedRetireAndReuse? prepared)
  {
    ArgumentNullException.ThrowIfNull(replacement);
    prepared = null;
    lock (CommitGate.SyncRoot)
    {
      if (!retiringHandle.IsValid || _reservations is null)
      {
        return Unavailable("Retire-and-reuse requires a valid handle and bound reservations.");
      }

      OwnerCommitResult result = PrepareBatch(
        [new WorldItemChange(retiringHandle, expectedRevision, null)],
        [new WorldItemSpawn(retiringHandle.WorldScope, retiringHandle.SlotId, replacement)],
        out PreparedBatch? world);
      if (world is null)
      {
        return result;
      }

      if (!_reservations.PrepareRelease([retiringHandle], out var claims))
      {
        return Conflict(world.Before[0].Revision,
          "The retiring item claim cannot be prepared.");
      }

      WorldItemHandle[] ownership = claims.Entries
        .Where(entry => entry.PublishOwnership).Select(entry => entry.Handle).ToArray();
      if (!Projections.CanPublish(world.After, ownership, 0,
        shimmerEffects: world.ShimmerEffects))
      {
        return Unavailable("The projection sequence space is exhausted.");
      }

      prepared = new PreparedRetireAndReuse(this, world, claims, result);
      return result;
    }
  }

  public IReadOnlyList<WorldItemSnapshot> CaptureWorld(WorldScopeId worldScope)
  {
    lock (CommitGate.SyncRoot)
    {
      return Array.AsReadOnly(_items.Values
        .Where(item => item.Handle.WorldScope == worldScope)
        .OrderBy(item => item.Handle.SlotId).ToArray());
    }
  }

  public IReadOnlyList<int> CaptureFreeSlots(int count)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    lock (CommitGate.SyncRoot)
    {
      return Array.AsReadOnly(Enumerable.Range(0, _slotCount)
        .Where(slot => !_items.ContainsKey(slot)
          && _nextGenerationBySlot.GetValueOrDefault(slot, 1) < long.MaxValue)
        .Take(count).ToArray());
    }
  }

  internal OwnerCommitResult PrepareBatch(
    IReadOnlyList<WorldItemChange> changes, IReadOnlyList<WorldItemSpawn> spawns,
    out PreparedBatch? prepared)
  {
    prepared = null;
    var slots = new HashSet<int>();
    var before = new List<WorldItemSnapshot>();
    var after = new List<WorldItemSnapshot>();
    var shimmerEffects = new List<WorldItemSnapshot>();
    var retired = new HashSet<WorldItemHandle>();
    var retiredBySlot = new Dictionary<int, WorldItemHandle>();
    foreach (WorldItemChange change in changes)
    {
      if (!change.Handle.IsValid || !slots.Add(change.Handle.SlotId))
      {
        return Rejected("The world item batch contains invalid or duplicate handles.");
      }

      if (!_items.TryGetValue(change.Handle.SlotId, out WorldItemSnapshot? current))
      {
        return Unavailable("The world item slot is not active.");
      }

      if (current.Handle != change.Handle)
      {
        return new OwnerCommitResult(OwnerCommitStatus.Stale, current.Revision,
          ProtocolErrorCode.StaleEntity, "The world item scope or generation changed.");
      }

      if (current.Revision != change.ExpectedRevision)
      {
        return Conflict(current.Revision, "The world item revision changed.");
      }

      if (current.Revision.Value == long.MaxValue
        || current.Handle.Generation.Value == long.MaxValue)
      {
        return Unavailable("The world item identity or revision space is exhausted.");
      }

      WorldItemMutation? mutation = change.Mutation;
      if (mutation is not null && !IsValid(mutation))
      {
        return Rejected("The world item mutation is invalid.");
      }

      if (change.ShimmerEffect is { } effect)
      {
        WorldItemRuntime runtime = current.Runtime ?? new WorldItemRuntime();
        WorldItemRuntime expectedRuntime = runtime.AdvanceOwnershipTimers() with {
          Wet = true, LavaWet = false, ShimmerWet = true
        };
        if (!IsValid(effect) || !effect.IsShimmering || effect.ShimmerTime != 1f
          || effect.Item.Stack > current.Item.Stack || effect.Item.Prefix != 0
          || effect.Item.Favorited || current.IsShimmering || runtime.BeingGrabbed
          || !WorldItemShimmerFacts.IsSupportedTransform(current.Item.Type, effect.Item.Type)
          || effect.TimeLeft != current.TimeLeft
          || effect.EnemyPickupDelay != current.EnemyPickupDelay
          || effect.Position != current.Position
          || effect.Runtime != expectedRuntime
          || mutation is not null && (mutation.Item != effect.Item || !mutation.IsShimmering
            || mutation.ShimmerTime != effect.ShimmerTime
            || mutation.TimeLeft != effect.TimeLeft))
        {
          return Rejected("The captured direct shimmer effect is invalid.");
        }

        shimmerEffects.Add(Apply(current.Handle, current.Revision.Next(), effect, current.Runtime));
      }

      before.Add(current);
      after.Add(mutation is null
        ? current with { Item = InventoryItemState.Empty, Revision = current.Revision.Next() }
        : Apply(current.Handle, current.Revision.Next(), mutation, current.Runtime));
      if (mutation is null)
      {
        retired.Add(current.Handle);
        retiredBySlot.Add(current.Handle.SlotId, current.Handle);
      }
    }

    var spawnSlots = new HashSet<int>();
    foreach (WorldItemSpawn spawn in spawns)
    {
      if (!spawn.WorldScope.IsValid || spawn.SlotId < 0 || spawn.SlotId >= _slotCount
        || !spawnSlots.Add(spawn.SlotId) || !IsValid(spawn.InitialState))
      {
        return Rejected("The world item spawn is invalid or duplicates a batch slot.");
      }

      bool reusesRetiredSlot = retiredBySlot.TryGetValue(spawn.SlotId, out var retiringHandle);
      if (!reusesRetiredSlot && !slots.Add(spawn.SlotId))
      {
        return Rejected("The world item spawn duplicates a batch slot.");
      }
      if (reusesRetiredSlot && spawn.WorldScope != retiringHandle.WorldScope)
      {
        return Rejected("A reused world item slot must retain its world scope.");
      }

      long generation;
      if (_items.TryGetValue(spawn.SlotId, out WorldItemSnapshot? current))
      {
        if (!reusesRetiredSlot || current.Handle != retiringHandle)
        {
          return Conflict(current.Revision, "The spawn slot is occupied.");
        }

        generation = retiringHandle.Generation.Value + 1;
        if (_nextGenerationBySlot.GetValueOrDefault(spawn.SlotId, 1)
          != retiringHandle.Generation.Value)
        {
          return Conflict(current.Revision, "The retiring slot generation changed.");
        }
      }
      else
      {
        if (reusesRetiredSlot)
        {
          return Unavailable("The retiring item is no longer active.");
        }

        generation = _nextGenerationBySlot.GetValueOrDefault(spawn.SlotId, 1);
      }

      if (generation <= 0 || generation == long.MaxValue)
      {
        return Unavailable("The world item slot generation space is exhausted.");
      }

      var handle = new WorldItemHandle(
        spawn.WorldScope, spawn.SlotId, new WorldItemGeneration(generation));
      after.Add(Apply(handle, CommitVersion.Initial, spawn.InitialState, null));
    }

    if (!Projections.CanPublish(after, [], 0, shimmerEffects: shimmerEffects))
    {
      return Unavailable("The projection sequence space is exhausted.");
    }

    prepared = new PreparedBatch(
      this, before.ToArray(), after.ToArray(), retired, shimmerEffects.ToArray(),
      changes.Where(change => !change.Broadcast).Select(change => change.Handle).ToHashSet());
    return OwnerCommitResult.Applied(after.Count == 0 ? CommitVersion.Initial : after[0].Revision);
  }

  internal bool CanCommit(PreparedBatch prepared)
  {
    return ReferenceEquals(prepared.Owner, this) && !prepared.Committed
      && prepared.Before.All(before => _items.TryGetValue(before.Handle.SlotId, out var now)
        && ReferenceEquals(before, now))
      && prepared.After.Skip(prepared.Before.Count).All(spawn =>
      {
        WorldItemHandle retiringHandle = prepared.Retired.FirstOrDefault(
          handle => handle.SlotId == spawn.Handle.SlotId);
        if (retiringHandle.IsValid)
        {
          return _items.TryGetValue(spawn.Handle.SlotId, out var current)
            && current.Handle == retiringHandle
            && ReferenceEquals(prepared.Before.FirstOrDefault(
              before => before.Handle == retiringHandle), current)
            && spawn.Handle.WorldScope == retiringHandle.WorldScope
            && spawn.Handle.Generation.Value == retiringHandle.Generation.Value + 1
            && _nextGenerationBySlot.GetValueOrDefault(spawn.Handle.SlotId, 1)
              == retiringHandle.Generation.Value;
        }

        return !_items.ContainsKey(spawn.Handle.SlotId)
          && _nextGenerationBySlot.GetValueOrDefault(spawn.Handle.SlotId, 1)
            == spawn.Handle.Generation.Value;
      });
  }

  /// <summary>Only called under the shared gate after every participant has validated.</summary>
  internal void CommitPrepared(PreparedBatch prepared, bool publish = true)
  {
    if (!CanCommit(prepared))
    {
      throw new InvalidOperationException("A world item batch was not validated at commit.");
    }

    foreach (WorldItemSnapshot after in prepared.After)
    {
      if (prepared.Retired.Contains(after.Handle))
      {
        _items.Remove(after.Handle.SlotId);
        _nextGenerationBySlot[after.Handle.SlotId] = after.Handle.Generation.Value + 1;
      }
      else
      {
        _items[after.Handle.SlotId] = after;
      }

    }

    prepared.Committed = true;
    if (publish)
    {
      Publish(prepared);
    }
  }

  internal void Publish(PreparedBatch prepared, SessionEpoch? ignoreEpoch = null)
  {
    foreach (WorldItemSnapshot effect in prepared.ShimmerEffects)
    {
      Projections.PublishShimmer(effect);
    }

    foreach (WorldItemSnapshot after in prepared.After)
    {
      if (!prepared.Silent.Contains(after.Handle)) {
        Projections.PublishWorld(after, ignoreEpoch);
      }
    }
  }

  private bool IsValid(WorldItemMutation mutation)
  {
    WorldItemRuntime runtime = mutation.Runtime ?? new WorldItemRuntime();
    return _itemPolicy.IsValid(mutation.Item) && !mutation.Item.IsEmpty
      && float.IsFinite(mutation.Position.X) && float.IsFinite(mutation.Position.Y)
      && float.IsFinite(mutation.Velocity.X) && float.IsFinite(mutation.Velocity.Y)
      && mutation.TimeLeft >= 0 && mutation.EnemyPickupDelay >= 0
      && float.IsFinite(mutation.ShimmerTime) && mutation.ShimmerTime is >= 0 and <= 1
      && runtime.Age >= 0 && runtime.NoGrabDelay >= 0
      && runtime.OwnTime >= 0 && runtime.KeepTime >= 0
      && runtime.OwnIgnore is >= -1 and < 255;
  }

  private static WorldItemSnapshot Apply(
    WorldItemHandle handle, CommitVersion revision, WorldItemMutation mutation,
    WorldItemRuntime? previousRuntime) => new(
      handle, mutation.Item, mutation.Position, mutation.Velocity, revision,
      mutation.TimeLeft, mutation.IsShimmering, mutation.EnemyPickupDelay,
      mutation.ShimmerTime, mutation.Runtime ?? previousRuntime ?? new WorldItemRuntime());

  private static OwnerCommitResult Rejected(string detail) =>
    OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, detail);

  private static OwnerCommitResult Unavailable(string detail) => new(
    OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);

  private static OwnerCommitResult Conflict(CommitVersion revision, string detail) => new(
    OwnerCommitStatus.Conflict, revision, ProtocolErrorCode.Conflict, detail);

  internal sealed class PreparedBatch(
    WorldItemStore owner, WorldItemSnapshot[] before, WorldItemSnapshot[] after,
    HashSet<WorldItemHandle> retired, WorldItemSnapshot[] shimmerEffects,
    IReadOnlySet<WorldItemHandle> silent)
  {
    internal IReadOnlySet<WorldItemHandle> Silent { get; } = silent;
    internal WorldItemStore Owner { get; } = owner;
    internal IReadOnlyList<WorldItemSnapshot> Before { get; } = Array.AsReadOnly(before);
    internal IReadOnlyList<WorldItemSnapshot> After { get; } = Array.AsReadOnly(after);
    internal IReadOnlySet<WorldItemHandle> Retired { get; } = retired;
    internal IReadOnlyList<WorldItemSnapshot> ShimmerEffects { get; } =
      Array.AsReadOnly(shimmerEffects);
    internal bool Committed { get; set; }
  }

  public sealed class PreparedRetireAndReuse
  {
    private readonly WorldItemStore _owner;
    private readonly PreparedBatch _world;
    private readonly WorldItemReservationStore.PreparedRelease _claims;
    private readonly OwnerCommitResult _result;
    private bool _committed;

    public WorldItemSnapshot Replacement { get; }

    internal PreparedRetireAndReuse(
      WorldItemStore owner, PreparedBatch world,
      WorldItemReservationStore.PreparedRelease claims, OwnerCommitResult result)
    {
      _owner = owner;
      _world = world;
      _claims = claims;
      _result = result;
      Replacement = world.After[world.Before.Count];
    }

    public OwnerCommitResult Validate()
    {
      lock (_owner.CommitGate.SyncRoot)
      {
        if (_committed || !_owner.CanCommit(_world)
          || !_owner._reservations!.CanCommit(_claims))
        {
          return Conflict(_world.Before[0].Revision,
            "The retiring item or its reservation changed after preparation.");
        }

        WorldItemHandle[] ownership = _claims.Entries
          .Where(entry => entry.PublishOwnership).Select(entry => entry.Handle).ToArray();
        if (!_owner.Projections.CanPublish(_world.After, ownership, 0,
          shimmerEffects: _world.ShimmerEffects))
        {
          return Unavailable("The projection sequence space is exhausted.");
        }

        return _result;
      }
    }

    public OwnerCommitResult Commit()
    {
      lock (_owner.CommitGate.SyncRoot)
      {
        OwnerCommitResult validation = Validate();
        if (validation.Status != OwnerCommitStatus.Applied)
        {
          return validation;
        }

        _owner.CommitPrepared(_world, publish: false);
        _owner._reservations!.CommitPrepared(_claims, publish: false);
        _committed = true;
        _owner.Publish(_world);
        _owner._reservations.Publish(_claims);
        return _result;
      }
    }
  }
}
