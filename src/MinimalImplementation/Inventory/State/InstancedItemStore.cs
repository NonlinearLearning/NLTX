using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Rules;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>
/// Stores per-player item projections separately from the public world-item store.
/// </summary>
public sealed class InstancedItemStore
{
  private readonly Dictionary<InstancedItemKey, InstancedItemState> _items = new();
  private readonly Dictionary<InstancedItemKey, CommitVersion> _revisions = new();
  private readonly ItemValidationPolicy _itemPolicy;
  private readonly ServerStateCommitGate _commitGate;

  public InstancedItemStore(ItemValidationPolicy itemPolicy, ServerStateCommitGate commitGate)
  {
    ArgumentNullException.ThrowIfNull(itemPolicy);
    ArgumentNullException.ThrowIfNull(commitGate);
    _itemPolicy = itemPolicy;
    _commitGate = commitGate;
  }

  internal ServerStateCommitGate CommitGate => _commitGate;

  public OwnerCommitResult Upsert(
    EntityUuid playerId,
    WorldScopeId worldScope,
    int itemIndex,
    InventoryItemState item,
    Vector2 position,
    Vector2 velocity,
    int timeLeft,
    bool isShimmering,
    int enemyPickupDelay,
    CommitVersion? expectedRevision,
    out InstancedItemSnapshot? snapshot)
  {
    return Upsert(playerId, worldScope, itemIndex, item, position, velocity, timeLeft,
      isShimmering, enemyPickupDelay, expectedRevision, default, out snapshot);
  }

  public OwnerCommitResult Upsert(
    EntityUuid playerId,
    WorldScopeId worldScope,
    int itemIndex,
    InventoryItemState item,
    Vector2 position,
    Vector2 velocity,
    int timeLeft,
    bool isShimmering,
    int enemyPickupDelay,
    CommitVersion? expectedRevision,
    SessionEpoch epoch,
    out InstancedItemSnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      snapshot = null;
      if (!playerId.IsValid
        || !worldScope.IsValid
        || itemIndex < 0
        || itemIndex > short.MaxValue
        || !_itemPolicy.IsValid(item)
        || item.IsEmpty
        || !IsFinite(position)
        || !IsFinite(velocity)
        || timeLeft < 0
        || enemyPickupDelay < 0)
      {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The instanced item projection is invalid.");
      }

      var key = new InstancedItemKey(playerId, itemIndex);
      if (_items.TryGetValue(key, out InstancedItemState? current))
      {
        if (current.WorldScope != worldScope || current.Epoch != epoch)
        {
          return new OwnerCommitResult(OwnerCommitStatus.Stale, current.Revision,
            ProtocolErrorCode.StaleEntity, "The instanced item belongs to another world or session.");
        }

        if (expectedRevision is not CommitVersion revision || current.Revision != revision)
        {
          return new OwnerCommitResult(
            OwnerCommitStatus.Conflict,
            current.Revision,
            ProtocolErrorCode.Conflict,
            "The instanced item projection revision changed.");
        }

        if (current.Revision.Value == long.MaxValue)
        {
          return new OwnerCommitResult(
            OwnerCommitStatus.Unavailable,
            null,
            ProtocolErrorCode.NotReady,
            "The instanced item projection revision space is exhausted.");
        }

        current.Item = item;
        current.Position = position;
        current.Velocity = velocity;
        current.TimeLeft = timeLeft;
        current.IsShimmering = isShimmering;
        current.EnemyPickupDelay = enemyPickupDelay;
        current.Revision = current.Revision.Next();
        _revisions[key] = current.Revision;
        snapshot = Capture(key, current);
        return OwnerCommitResult.Applied(current.Revision);
      }

      if (expectedRevision is not null)
      {
        return new OwnerCommitResult(
          OwnerCommitStatus.Stale,
          null,
          ProtocolErrorCode.StaleEntity,
          "The instanced item projection does not exist for the requested revision.");
      }

      CommitVersion nextRevision = CommitVersion.Initial;
      if (_revisions.TryGetValue(key, out CommitVersion previous))
      {
        if (previous.Value == long.MaxValue)
        {
          return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null,
            ProtocolErrorCode.NotReady, "The instanced item revision space is exhausted.");
        }

        nextRevision = previous.Next();
      }

      var state = new InstancedItemState(
        worldScope,
        item,
        position,
        velocity,
        timeLeft,
        isShimmering,
        enemyPickupDelay,
        epoch,
        nextRevision);
      _items.Add(key, state);
      _revisions[key] = state.Revision;
      snapshot = Capture(key, state);
      return OwnerCommitResult.Applied(state.Revision);
    }
  }

  public bool TryCapture(
    EntityUuid playerId,
    int itemIndex,
    out InstancedItemSnapshot? snapshot)
  {
    lock (_commitGate.SyncRoot)
    {
      var key = new InstancedItemKey(playerId, itemIndex);
      if (!_items.TryGetValue(key, out InstancedItemState? state))
      {
        snapshot = null;
        return false;
      }

      snapshot = Capture(key, state);
      return true;
    }
  }

  public OwnerCommitResult Remove(
    EntityUuid playerId,
    int itemIndex,
    CommitVersion expectedRevision)
  {
    return Remove(playerId, itemIndex, expectedRevision, default);
  }

  public OwnerCommitResult Remove(
    EntityUuid playerId,
    int itemIndex,
    CommitVersion expectedRevision,
    SessionEpoch epoch)
  {
    lock (_commitGate.SyncRoot)
    {
      var key = new InstancedItemKey(playerId, itemIndex);
      if (!_items.TryGetValue(key, out InstancedItemState? state))
      {
        return new OwnerCommitResult(
          OwnerCommitStatus.Unavailable,
          null,
          ProtocolErrorCode.NotReady,
          "The instanced item projection is not active.");
      }

      if (state.Epoch != epoch)
      {
        return new OwnerCommitResult(OwnerCommitStatus.Stale, state.Revision,
          ProtocolErrorCode.StaleEntity, "The instanced item belongs to another session.");
      }

      if (state.Revision != expectedRevision)
      {
        return new OwnerCommitResult(
          OwnerCommitStatus.Conflict,
          state.Revision,
          ProtocolErrorCode.Conflict,
          "The instanced item projection revision changed.");
      }

      _items.Remove(key);
      return OwnerCommitResult.Applied(state.Revision);
    }
  }

  public int ReleaseSession(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      return 0;
    }

    lock (_commitGate.SyncRoot)
    {
      return RemoveWhere(state => state.Epoch == epoch);
    }
  }

  public int ReleaseWorldScope(WorldScopeId scope)
  {
    lock (_commitGate.SyncRoot)
    {
      return RemoveWhere(state => state.WorldScope == scope);
    }
  }

  private int RemoveWhere(Func<InstancedItemState, bool> predicate)
  {
    InstancedItemKey[] keys = _items.Where(pair => predicate.Invoke(pair.Value))
      .Select(pair => pair.Key).ToArray();
    foreach (InstancedItemKey key in keys)
    {
      _items.Remove(key);
    }

    return keys.Length;
  }

  private static InstancedItemSnapshot Capture(InstancedItemKey key, InstancedItemState state)
  {
    return new InstancedItemSnapshot(
      key.PlayerId,
      key.ItemIndex,
      state.WorldScope,
      state.Item,
      state.Position,
      state.Velocity,
      state.Revision,
      state.TimeLeft,
      state.IsShimmering,
      state.EnemyPickupDelay,
      state.Epoch);
  }

  private static bool IsFinite(Vector2 vector)
  {
    return float.IsFinite(vector.X) && float.IsFinite(vector.Y);
  }

  private sealed class InstancedItemState(
    WorldScopeId worldScope,
    InventoryItemState item,
    Vector2 position,
    Vector2 velocity,
    int timeLeft,
    bool isShimmering,
    int enemyPickupDelay,
    SessionEpoch epoch,
    CommitVersion revision)
  {
    public WorldScopeId WorldScope { get; } = worldScope;
    public SessionEpoch Epoch { get; } = epoch;
    public InventoryItemState Item { get; set; } = item;
    public Vector2 Position { get; set; } = position;
    public Vector2 Velocity { get; set; } = velocity;
    public CommitVersion Revision { get; set; } = revision;
    public int TimeLeft { get; set; } = timeLeft;
    public bool IsShimmering { get; set; } = isShimmering;
    public int EnemyPickupDelay { get; set; } = enemyPickupDelay;
  }
}

public readonly record struct InstancedItemKey(EntityUuid PlayerId, int ItemIndex);

public sealed record InstancedItemSnapshot(
  EntityUuid PlayerId,
  int ItemIndex,
  WorldScopeId WorldScope,
  InventoryItemState Item,
  Vector2 Position,
  Vector2 Velocity,
  CommitVersion Revision,
  int TimeLeft,
  bool IsShimmering,
  int EnemyPickupDelay,
  SessionEpoch Epoch = default);
