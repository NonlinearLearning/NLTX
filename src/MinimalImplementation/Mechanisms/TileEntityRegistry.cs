using Terraria.Items;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Mechanisms;

public sealed class TileEntityRegistry
{
  /// <summary>Unpublished removal; identity retirement belongs to the final commit gate.</summary>
  public sealed class RemovalPreparation
  {
    private readonly TileEntityRegistry _owner;
    private readonly TileEntityRegistry _candidate;
    private readonly long _baseRevision;
    private readonly long _stateRevision;
    private bool _committed;

    public TileEntityRemovalResult Result { get; }

    internal RemovalPreparation(
      TileEntityRegistry owner,
      TileEntityRegistry candidate,
      TileEntityRemovalResult result)
    {
      _owner = owner;
      _candidate = candidate;
      _baseRevision = owner.Revision;
      _stateRevision = result.State.Revision;
      Result = result;
    }

    public bool Validate()
    {
      return !_committed && _owner.Revision == _baseRevision &&
        Result.State.Revision == _stateRevision &&
        Result.State.Lifecycle == MechanismEntityLifecycle.Active &&
        _owner._identityAuthority.IsActive(Result.Handle);
    }

    /// <summary>
    /// Called only after all participants validate and one atomic identity retirement succeeds
    /// in the same exclusive owner window. There are no further business checks or allocations.
    /// </summary>
    internal TileEntityRemovalResult CommitAfterRetirement()
    {
      if (_committed)
      {
        throw new InvalidOperationException("The TileEntity removal was already committed.");
      }
      Result.State.BeginRemoval();
      Result.State.Retire();
      _owner.PublishIndexes(_candidate);
      _committed = true;
      return Result;
    }
  }

  /// <summary>An unpublished entity and all indexes needed to publish it.</summary>
  public sealed class CreationPreparation
  {
    private readonly TileEntityRegistry _owner;
    private readonly TileEntityRegistry _candidate;
    private readonly long _baseRevision;
    private bool _committed;

    public TileEntityRuntimeState State { get; }

    internal CreationPreparation(
      TileEntityRegistry owner,
      TileEntityRegistry candidate,
      TileEntityRuntimeState state)
    {
      _owner = owner;
      _candidate = candidate;
      _baseRevision = owner.Revision;
      State = state;
    }

    public bool Validate()
    {
      return !_committed && _owner.Revision == _baseRevision &&
        State.Revision == 1 && State.Lifecycle == MechanismEntityLifecycle.Active &&
        _owner._identityAuthority.IsActive(State.Handle);
    }

    /// <summary>
    /// Requires successful validation of every participant in one exclusive owner window,
    /// with no intervening writes. Preparation allocated the entity and every index.
    /// </summary>
    public TileEntityRuntimeState Commit()
    {
      if (_committed)
      {
        throw new InvalidOperationException("The TileEntity creation was already committed.");
      }
      _owner.PublishIndexes(_candidate);
      _committed = true;
      return State;
    }
  }

  private readonly MechanismFactsCatalog _facts;
  private readonly IMechanismIdentityAuthority _identityAuthority;
  private readonly IMechanismItemFacts? _itemFacts;
  private readonly long _worldScope;
  private Dictionary<MechanismEntityHandle, TileEntityRuntimeState> _byIdentity = [];
  private Dictionary<int, TileEntityRuntimeState> _byPersistenceId = [];
  private Dictionary<int, TileEntityRuntimeState> _byNetworkId = [];
  private HashSet<int> _observedPersistenceIds = [];
  private HashSet<int> _observedNetworkIds = [];
  private Dictionary<TileCoordinate, TileEntityRuntimeState> _byAnchor = [];
  private Dictionary<MechanismSectionCoordinate, HashSet<MechanismEntityHandle>> _bySection = [];
  private HashSet<MechanismEntityHandle> _tickEntities = [];
  private long _nextPersistenceId;
  private long _nextNetworkId;

  public TileEntityRegistry(
    MechanismFactsCatalog facts,
    IMechanismIdentityAuthority identityAuthority,
    long worldScope,
    IMechanismItemFacts? itemFacts = null)
  {
    _facts = facts ?? throw new ArgumentNullException(nameof(facts));
    _identityAuthority = identityAuthority
      ?? throw new ArgumentNullException(nameof(identityAuthority));
    _itemFacts = itemFacts;
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    _worldScope = worldScope;
  }

  public int Count => _byIdentity.Count;

  public long Revision { get; private set; } = 1;

  public long WorldScope => _worldScope;

  public IReadOnlyCollection<TileEntityRuntimeState> ActiveEntities => _byIdentity.Values;

  public TileEntityRuntimeState Create(
    MechanismEntityHandle identity,
    TileEntityCreateRequest request)
  {
    CreationPreparation preparation = PrepareCreate(identity, request);
    if (!preparation.Validate())
    {
      throw new InvalidOperationException("The prepared TileEntity creation is stale.");
    }
    return preparation.Commit();
  }

  /// <summary>Does not consume ids, alter indexes, or allocate/retire root identities.</summary>
  public CreationPreparation PrepareCreate(
    MechanismEntityHandle identity,
    TileEntityCreateRequest request)
  {
    ValidateIdentity(identity);
    ArgumentNullException.ThrowIfNull(request);
    TileEntityDefinition definition = _facts.GetTileEntity(request.Type);
    ValidateRequest(definition, request.Anchor, request.AnchorTile, request.Items,
      request.NpcIndex, request.LogicCheck, request.LogicOn, request.Pose,
      request.AnchorItemType, restoring: false);
    ValidateSection(request.Section);
    if (_byAnchor.ContainsKey(request.Anchor))
    {
      throw new InvalidOperationException(
        $"A TileEntity already occupies {request.Anchor.X},{request.Anchor.Y}.");
    }

    if (_byIdentity.ContainsKey(identity))
    {
      throw new InvalidOperationException("The supplied identity is already registered.");
    }
    long nextRevision = checked(Revision + 1);
    int persistenceId = FindAvailableId(_nextPersistenceId, _byPersistenceId,
      _observedPersistenceIds, "persistence");
    int networkId = request.NetworkId ?? FindAvailableId(_nextNetworkId, _byNetworkId,
      _observedNetworkIds, "network");
    ValidateNetworkId(networkId);
    var state = new TileEntityRuntimeState(
      identity,
      persistenceId,
      networkId,
      definition,
      request.Anchor,
      request.Section,
      request.Items,
      request.NpcIndex,
      request.LogicCheck,
      request.LogicOn,
      request.Pose,
      request.AnchorItemType);
    TileEntityRegistry candidate = CloneIndexes();
    candidate.Add(state);
    AdvanceIdCursor(ref candidate._nextPersistenceId, persistenceId);
    AdvanceIdCursor(ref candidate._nextNetworkId, networkId);
    candidate.Revision = nextRevision;
    return new CreationPreparation(this, candidate, state);
  }

  public TileEntityRuntimeState Restore(
    MechanismEntityHandle identity,
    TileEntityRestoreRequest request)
  {
    ValidateIdentity(identity);
    ArgumentNullException.ThrowIfNull(request);
    TileEntitySnapshot snapshot = request.Snapshot
      ?? throw new ArgumentException("A TileEntity snapshot is required.", nameof(request));
    TileEntityDefinition definition = _facts.GetTileEntity(snapshot.Type.Value);
    TileEntitySnapshot normalizedSnapshot = NormalizeRestoredAnchorItem(snapshot);
    ValidateRequest(definition, normalizedSnapshot.Anchor, request.AnchorTile, normalizedSnapshot.Items,
      normalizedSnapshot.NpcIndex, normalizedSnapshot.LogicCheck, normalizedSnapshot.LogicOn,
      normalizedSnapshot.Pose, normalizedSnapshot.AnchorItemType, restoring: true);
    ValidateSection(request.Section);
    if (snapshot.Id.Value < 0 ||
        _byPersistenceId.ContainsKey(snapshot.Id.Value) ||
        _observedPersistenceIds.Contains(snapshot.Id.Value))
    {
      throw new InvalidOperationException(
        $"TileEntity persistence id {snapshot.Id.Value} is invalid or already loaded.");
    }
    if (_byAnchor.ContainsKey(snapshot.Anchor))
    {
      throw new InvalidOperationException(
        $"A TileEntity already occupies {snapshot.Anchor.X},{snapshot.Anchor.Y}.");
    }

    if (_byIdentity.ContainsKey(identity))
    {
      throw new InvalidOperationException("The supplied identity is already registered.");
    }
    int networkId = request.NetworkId ?? FindAvailableId(_nextNetworkId, _byNetworkId,
      _observedNetworkIds, "network");
    ValidateNetworkId(networkId);
    long nextRevision = checked(Revision + 1);
    var state = new TileEntityRuntimeState(
      identity,
      normalizedSnapshot.Id.Value,
      networkId,
      definition,
      normalizedSnapshot.Anchor,
      request.Section,
      normalizedSnapshot.Items,
      normalizedSnapshot.NpcIndex,
      normalizedSnapshot.LogicCheck,
      normalizedSnapshot.LogicOn,
      normalizedSnapshot.Pose,
      normalizedSnapshot.AnchorItemType);
    Add(state);
    AdvanceIdCursor(ref _nextPersistenceId, snapshot.Id.Value);
    AdvanceIdCursor(ref _nextNetworkId, networkId);
    Revision = nextRevision;
    return state;
  }

  public PreparedTileEntityLoad PrepareRestore(
    IReadOnlyList<TileEntityRestoreCandidate> candidates)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    if (_byIdentity.Count != 0)
    {
      throw new InvalidOperationException("A world load must prepare against an empty registry.");
    }

    TileEntityRestoreCandidate[] orderedCandidates = candidates
      .OrderBy(candidate => candidate?.Request?.Snapshot?.Id.Value ?? -1)
      .ToArray();
    var identities = new HashSet<MechanismEntityHandle>();
    var persistenceIds = new HashSet<int>();
    var networkIds = new HashSet<int>();
    var anchors = new HashSet<TileCoordinate>();
    var states = new List<TileEntityRuntimeState>(orderedCandidates.Length);
    long nextPersistenceId = _nextPersistenceId;
    long nextNetworkId = _nextNetworkId;

    foreach (TileEntityRestoreCandidate candidate in orderedCandidates)
    {
      ArgumentNullException.ThrowIfNull(candidate);
      ArgumentNullException.ThrowIfNull(candidate.Request);
      ValidateIdentity(candidate.Identity);
      TileEntityRestoreRequest request = candidate.Request;
      TileEntitySnapshot snapshot = request.Snapshot
        ?? throw new ArgumentException("A TileEntity snapshot is required.", nameof(candidates));
      if (snapshot.Id.Value < 0 ||
          _byPersistenceId.ContainsKey(snapshot.Id.Value) ||
          _observedPersistenceIds.Contains(snapshot.Id.Value) ||
          !persistenceIds.Add(snapshot.Id.Value))
      {
        throw new InvalidOperationException(
          $"TileEntity persistence id {snapshot.Id.Value} is invalid or duplicated in the load.");
      }
      if (_byIdentity.ContainsKey(candidate.Identity) || !identities.Add(candidate.Identity))
      {
        throw new InvalidOperationException("The TileEntity load contains a duplicate identity.");
      }
      if (_byAnchor.ContainsKey(snapshot.Anchor) || !anchors.Add(snapshot.Anchor))
      {
        throw new InvalidOperationException(
          $"The TileEntity load contains a duplicate anchor at {snapshot.Anchor.X},{snapshot.Anchor.Y}.");
      }

      TileEntityDefinition definition = _facts.GetTileEntity(snapshot.Type.Value);
      TileEntitySnapshot normalizedSnapshot = NormalizeRestoredAnchorItem(snapshot);
      ValidateRequest(definition, normalizedSnapshot.Anchor, request.AnchorTile,
        normalizedSnapshot.Items, normalizedSnapshot.NpcIndex, normalizedSnapshot.LogicCheck,
        normalizedSnapshot.LogicOn, normalizedSnapshot.Pose, normalizedSnapshot.AnchorItemType,
        restoring: true);
      ValidateSection(request.Section);
      int networkId = request.NetworkId ?? FindAvailableId(nextNetworkId, _byNetworkId,
        _observedNetworkIds, networkIds, "network");
      ValidateNetworkId(networkId);
      if (_byNetworkId.ContainsKey(networkId) ||
          _observedNetworkIds.Contains(networkId) ||
          !networkIds.Add(networkId))
      {
        throw new InvalidOperationException(
          $"TileEntity network id {networkId} is invalid or duplicated in the load.");
      }

      states.Add(new TileEntityRuntimeState(
        candidate.Identity,
        normalizedSnapshot.Id.Value,
        networkId,
        definition,
        normalizedSnapshot.Anchor,
        request.Section,
        normalizedSnapshot.Items,
        normalizedSnapshot.NpcIndex,
        normalizedSnapshot.LogicCheck,
        normalizedSnapshot.LogicOn,
        normalizedSnapshot.Pose,
        normalizedSnapshot.AnchorItemType));
      nextPersistenceId = Math.Max(nextPersistenceId, (long)snapshot.Id.Value + 1);
      nextNetworkId = Math.Max(nextNetworkId, (long)networkId + 1);
    }

    TileEntityRuntimeState[] preparedStates = states.ToArray();
    var appliedResult = new TileEntityLoadCommitResult(
      TileEntityLoadCommitStatus.Applied,
      Array.AsReadOnly(preparedStates.Select(state => state.Handle).ToArray()),
      Array.AsReadOnly(preparedStates.Select(state => state.CapturePersistence()).ToArray()));
    return new PreparedTileEntityLoad(this, Revision, Array.AsReadOnly(preparedStates),
      nextPersistenceId, nextNetworkId, appliedResult);
  }

  public TileEntityLoadCommitResult CommitRestore(PreparedTileEntityLoad prepared)
  {
    ArgumentNullException.ThrowIfNull(prepared);
    if (!ReferenceEquals(prepared.Owner, this))
    {
      throw new ArgumentException("The prepared load belongs to another registry.",
        nameof(prepared));
    }
    if (!prepared.TryConsume() ||
        prepared.RegistryRevision != Revision || _byIdentity.Count != 0)
    {
      return TileEntityLoadCommitResult.Stale;
    }
    if (prepared.States.Any(state => !_identityAuthority.IsActive(state.Handle)))
    {
      return TileEntityLoadCommitResult.Stale;
    }
    if (prepared.Count == 0)
    {
      return prepared.AppliedResult;
    }

    long nextRevision = checked(Revision + 1);
    var added = new List<TileEntityRuntimeState>(prepared.Count);
    try
    {
      foreach (TileEntityRuntimeState state in prepared.States)
      {
        Add(state);
        added.Add(state);
      }
    }
    catch
    {
      foreach (TileEntityRuntimeState state in added)
      {
        RemoveIndexes(state);
        state.BeginRemoval();
        state.Retire();
      }
      MechanismEntityHandle[] failedHandles = prepared.States
        .Select(state => state.Handle)
        .ToArray();
      if (!_identityAuthority.TryRetireMany(failedHandles))
      {
        throw new InvalidOperationException(
          "The identity owner could not retire handles from a failed TileEntity load.");
      }
      throw;
    }

    _nextPersistenceId = prepared.NextPersistenceId;
    _nextNetworkId = prepared.NextNetworkId;
    Revision = nextRevision;
    return prepared.AppliedResult;
  }

  public bool TryGet(MechanismEntityHandle handle, out TileEntityRuntimeState? state)
  {
    return _byIdentity.TryGetValue(handle, out state);
  }

  public bool TryGetByPersistenceId(int persistenceId, out TileEntityRuntimeState? state)
  {
    return _byPersistenceId.TryGetValue(persistenceId, out state);
  }

  public bool TryGetByNetworkId(int networkId, out TileEntityRuntimeState? state)
  {
    return _byNetworkId.TryGetValue(networkId, out state);
  }

  public bool TryGetByAnchor(TileCoordinate anchor, out TileEntityRuntimeState? state)
  {
    return _byAnchor.TryGetValue(anchor, out state);
  }

  public IReadOnlyList<TileEntityRuntimeState> GetTickEntities()
  {
    return _tickEntities
      .Select(handle => _byIdentity[handle])
      .OrderBy(state => state.PersistenceId)
      .ToArray();
  }

  public IReadOnlyList<TileEntityRuntimeState> GetSection(MechanismSectionCoordinate section)
  {
    if (!_bySection.TryGetValue(section, out HashSet<MechanismEntityHandle>? handles))
    {
      return Array.Empty<TileEntityRuntimeState>();
    }
    return handles
      .Select(handle => _byIdentity[handle])
      .OrderBy(state => state.PersistenceId)
      .ToArray();
  }

  public TileEntityRemovalResult Remove(MechanismEntityHandle handle)
  {
    RemovalPreparation preparation = PrepareRemove(handle);
    if (!preparation.Validate())
    {
      throw new InvalidOperationException("The prepared TileEntity removal is stale.");
    }
    if (!_identityAuthority.TryRetireMany([handle]))
    {
      throw new InvalidOperationException("The identity owner rejected TileEntity retirement.");
    }
    return preparation.CommitAfterRetirement();
  }

  public RemovalPreparation PrepareRemove(MechanismEntityHandle handle)
  {
    if (!_byIdentity.TryGetValue(handle, out TileEntityRuntimeState? state))
    {
      throw new KeyNotFoundException("The TileEntity handle is not active in this world scope.");
    }
    ValidateIdentity(handle);
    if (state.Lifecycle != MechanismEntityLifecycle.Active)
    {
      throw new InvalidOperationException("Only an active TileEntity can prepare removal.");
    }
    state.EnsureCanRetire();
    var result = new TileEntityRemovalResult(handle, state, state.CapturePersistence());
    TileEntityRegistry candidate = CloneIndexes();
    candidate.RemoveIndexes(state);
    candidate.Revision = checked(Revision + 1);
    return new RemovalPreparation(this, candidate, result);
  }

  public TileEntityRuntimeState UpdateState(
    MechanismEntityHandle handle,
    IReadOnlyList<Terraria.Items.ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType)
  {
    if (!_byIdentity.TryGetValue(handle, out TileEntityRuntimeState? state))
    {
      throw new KeyNotFoundException("The TileEntity handle is not active in this world scope.");
    }
    ValidatePayload(state.Definition, items, npcIndex, logicCheck, logicOn, pose,
      anchorItemType, restoring: false);
    long nextRevision = checked(Revision + 1);
    state.ApplyState(items, npcIndex, logicCheck, logicOn, pose, anchorItemType);
    Revision = nextRevision;
    return state;
  }

  public bool TryUpdateState(
    MechanismEntityHandle handle,
    long expectedStateRevision,
    IReadOnlyList<ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType,
    out TileEntityRuntimeState? updated)
  {
    ArgumentNullException.ThrowIfNull(items);
    if (!_byIdentity.TryGetValue(handle, out TileEntityRuntimeState? state) ||
        state.Revision != expectedStateRevision)
    {
      updated = null;
      return false;
    }

    ValidatePayload(state.Definition, items, npcIndex, logicCheck, logicOn, pose,
      anchorItemType, restoring: false);
    long nextRegistryRevision = checked(Revision + 1);
    state.ApplyState(items, npcIndex, logicCheck, logicOn, pose, anchorItemType);
    Revision = nextRegistryRevision;
    updated = state;
    return true;
  }

  public IReadOnlyList<TileEntitySnapshot> CapturePersistence()
  {
    return _byPersistenceId.Values
      .OrderBy(state => state.PersistenceId)
      .Select(state => state.CapturePersistence())
      .ToArray();
  }

  public IReadOnlyList<TileEntitySnapshot> CaptureSectionPersistence(
    MechanismSectionCoordinate section)
  {
    return GetSection(section).Select(state => state.CapturePersistence()).ToArray();
  }

  public IReadOnlyList<MechanismEntityHandle> Clear()
  {
    MechanismEntityHandle[] retiredHandles = _byIdentity.Values
      .Select(state => state.Handle)
      .ToArray();
    if (retiredHandles.Length == 0)
    {
      return retiredHandles;
    }
    long nextRevision = checked(Revision + 1);
    foreach (TileEntityRuntimeState state in _byIdentity.Values)
    {
      state.EnsureCanRetire();
    }
    if (!_identityAuthority.TryRetireMany(retiredHandles))
    {
      throw new InvalidOperationException("The identity owner rejected world TileEntity retirement.");
    }
    foreach (TileEntityRuntimeState state in _byIdentity.Values.ToArray())
    {
      state.BeginRemoval();
      state.Retire();
    }
    _byIdentity.Clear();
    _byPersistenceId.Clear();
    _byNetworkId.Clear();
    _byAnchor.Clear();
    _bySection.Clear();
    _tickEntities.Clear();
    Revision = nextRevision;
    return retiredHandles;
  }

  private void Add(TileEntityRuntimeState state)
  {
    if (state.Lifecycle != MechanismEntityLifecycle.Active)
    {
      throw new InvalidOperationException("Only active TileEntities can enter the registry.");
    }
    if (_byIdentity.ContainsKey(state.Handle) ||
        _byPersistenceId.ContainsKey(state.PersistenceId) ||
        _byNetworkId.ContainsKey(state.NetworkId) ||
        _observedPersistenceIds.Contains(state.PersistenceId) ||
        _observedNetworkIds.Contains(state.NetworkId) ||
        _byAnchor.ContainsKey(state.Anchor))
    {
      throw new InvalidOperationException("TileEntity indexes would contain a duplicate key.");
    }
    _observedPersistenceIds.Add(state.PersistenceId);
    _observedNetworkIds.Add(state.NetworkId);
    bool sectionCreated = false;
    if (!_bySection.TryGetValue(state.Section, out HashSet<MechanismEntityHandle>? handles))
    {
      handles = [];
      _bySection.Add(state.Section, handles);
      sectionCreated = true;
    }
    bool tickAdded = false;
    bool sectionHandleAdded = false;
    try
    {
      _byIdentity.Add(state.Handle, state);
      _byPersistenceId.Add(state.PersistenceId, state);
      _byNetworkId.Add(state.NetworkId, state);
      _byAnchor.Add(state.Anchor, state);
      if (state.Definition.RequiresUpdates)
      {
        tickAdded = _tickEntities.Add(state.Handle);
        if (!tickAdded)
        {
          throw new InvalidOperationException("The TileEntity update index already contains its handle.");
        }
      }
      sectionHandleAdded = handles.Add(state.Handle);
      if (!sectionHandleAdded)
      {
        throw new InvalidOperationException("The TileEntity section index already contains its handle.");
      }
    }
    catch
    {
      _byIdentity.Remove(state.Handle);
      _byPersistenceId.Remove(state.PersistenceId);
      _byNetworkId.Remove(state.NetworkId);
      _byAnchor.Remove(state.Anchor);
      if (tickAdded)
      {
        _tickEntities.Remove(state.Handle);
      }
      if (sectionHandleAdded)
      {
        handles.Remove(state.Handle);
      }
      if (sectionCreated && handles.Count == 0)
      {
        _bySection.Remove(state.Section);
      }
      throw;
    }
  }

  private TileEntityRegistry CloneIndexes()
  {
    var candidate = new TileEntityRegistry(_facts, _identityAuthority, _worldScope, _itemFacts);
    candidate._byIdentity = new(_byIdentity);
    candidate._byPersistenceId = new(_byPersistenceId);
    candidate._byNetworkId = new(_byNetworkId);
    candidate._observedPersistenceIds = new(_observedPersistenceIds);
    candidate._observedNetworkIds = new(_observedNetworkIds);
    candidate._byAnchor = new(_byAnchor);
    candidate._bySection = _bySection.ToDictionary(pair => pair.Key,
      pair => new HashSet<MechanismEntityHandle>(pair.Value));
    candidate._tickEntities = new(_tickEntities);
    candidate._nextPersistenceId = _nextPersistenceId;
    candidate._nextNetworkId = _nextNetworkId;
    candidate.Revision = Revision;
    return candidate;
  }

  private void PublishIndexes(TileEntityRegistry candidate)
  {
    _byIdentity = candidate._byIdentity;
    _byPersistenceId = candidate._byPersistenceId;
    _byNetworkId = candidate._byNetworkId;
    _observedPersistenceIds = candidate._observedPersistenceIds;
    _observedNetworkIds = candidate._observedNetworkIds;
    _byAnchor = candidate._byAnchor;
    _bySection = candidate._bySection;
    _tickEntities = candidate._tickEntities;
    _nextPersistenceId = candidate._nextPersistenceId;
    _nextNetworkId = candidate._nextNetworkId;
    Revision = candidate.Revision;
  }

  private void RemoveIndexes(TileEntityRuntimeState state)
  {
    _byIdentity.Remove(state.Handle);
    _byPersistenceId.Remove(state.PersistenceId);
    _byNetworkId.Remove(state.NetworkId);
    _byAnchor.Remove(state.Anchor);
    _tickEntities.Remove(state.Handle);
    if (_bySection.TryGetValue(state.Section, out HashSet<MechanismEntityHandle>? handles))
    {
      handles.Remove(state.Handle);
      if (handles.Count == 0)
      {
        _bySection.Remove(state.Section);
      }
    }
  }

  private static int FindAvailableId<TState>(
    long candidate,
    IReadOnlyDictionary<int, TState> index,
    IReadOnlySet<int> observedIds,
    string idKind)
  {
    while (candidate <= int.MaxValue &&
           (index.ContainsKey((int)candidate) || observedIds.Contains((int)candidate)))
    {
      candidate++;
    }
    if (candidate > int.MaxValue)
    {
      throw new InvalidOperationException($"The TileEntity {idKind} id space is exhausted.");
    }
    return (int)candidate;
  }

  private static int FindAvailableId<TState>(
    long candidate,
    IReadOnlyDictionary<int, TState> index,
    IReadOnlySet<int> observedIds,
    IReadOnlySet<int> reservedIds,
    string idKind)
  {
    while (candidate <= int.MaxValue &&
           (index.ContainsKey((int)candidate) ||
            observedIds.Contains((int)candidate) ||
            reservedIds.Contains((int)candidate)))
    {
      candidate++;
    }
    if (candidate > int.MaxValue)
    {
      throw new InvalidOperationException($"The TileEntity {idKind} id space is exhausted.");
    }
    return (int)candidate;
  }

  private static void AdvanceIdCursor(ref long cursor, int allocatedId)
  {
    cursor = Math.Max(cursor, (long)allocatedId + 1);
  }

  private static void ValidateNetworkId(int networkId)
  {
    if (networkId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(networkId),
        "A TileEntity network id must be nonnegative.");
    }
  }

  private void ValidateRequest(
    TileEntityDefinition definition,
    TileCoordinate anchor,
    MechanismTileSnapshot tile,
    IReadOnlyList<Terraria.Items.ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType,
    bool restoring)
  {
    ArgumentNullException.ThrowIfNull(items);
    if (tile.FrameX < 0 || tile.FrameY < 0)
    {
      throw new InvalidOperationException("TileEntity anchor frames must be nonnegative.");
    }
    if (!tile.Active || tile.Type != definition.TileType)
    {
      throw new InvalidOperationException(
        $"TileEntity {definition.Name} requires active tile {definition.TileType}.");
    }
    if (definition.FrameXModulo > 0 && tile.FrameX % definition.FrameXModulo != 0)
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} has an invalid frameX.");
    }
    if (definition.FrameYModulo > 0 && tile.FrameY % definition.FrameYModulo != 0)
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} has an invalid frameY.");
    }
    if (definition.AnchorRule == TileEntityAnchorRule.TeleportationPylon)
    {
      int pylonStyle = tile.FrameX / definition.FrameXModulo;
      if (pylonStyle < 0 || pylonStyle > byte.MaxValue ||
          !_facts.Pylons.ContainsKey((byte)pylonStyle))
      {
        throw new InvalidOperationException("The TileEntity anchor has an unknown pylon style.");
      }
    }
    if (definition.RequiresFrameYZero && tile.FrameY != 0)
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} requires frameY zero.");
    }
    ValidatePayload(definition, items, npcIndex, logicCheck, logicOn, pose,
      anchorItemType, restoring);
    if (anchor.X < 0 || anchor.Y < 0)
    {
      throw new InvalidOperationException("TileEntity anchors must be inside the world bounds.");
    }
  }

  private void ValidatePayload(
    TileEntityDefinition definition,
    IReadOnlyList<Terraria.Items.ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType,
    bool restoring)
  {
    ArgumentNullException.ThrowIfNull(items);
    if (items.Count != definition.ItemSlotCount)
    {
      throw new InvalidOperationException(
        $"TileEntity {definition.Name} requires {definition.ItemSlotCount} item slots.");
    }
    if (items.Any(item => item.Type < 0 || item.Stack < 0 ||
                          (item.Type == 0) != (item.Stack == 0)))
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} contains an invalid item value.");
    }
    foreach (ItemState item in items)
    {
      if (item.Type > short.MaxValue || item.Stack > short.MaxValue ||
          _itemFacts is not null && !item.IsEmpty && !_itemFacts.IsValid(item))
      {
        throw new InvalidOperationException(
          $"TileEntity {definition.Name} contains an item outside Version4 facts or wire bounds.");
      }
    }
    if (definition.PayloadKind == TileEntityPayloadKind.DisplayJar &&
        !items[0].IsEmpty && items[0].Stack <= 0)
    {
      throw new InvalidOperationException("A Dead Cells display jar item must have a positive stack.");
    }
    if (definition.PayloadKind == TileEntityPayloadKind.NpcIndex
          ? npcIndex < -1
          : npcIndex != -1)
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} contains an invalid NPC link.");
    }
    if (definition.PayloadKind != TileEntityPayloadKind.LogicSensor &&
        (logicCheck != 0 || logicOn))
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} contains logic-sensor state.");
    }
    if (definition.PayloadKind != TileEntityPayloadKind.DisplayDoll && pose != 0)
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} contains display-pose state.");
    }
    if (definition.PayloadKind == TileEntityPayloadKind.AnchorItem && anchorItemType < 0)
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} contains an invalid anchor item type.");
    }
    if (definition.PayloadKind == TileEntityPayloadKind.AnchorItem && anchorItemType != 0 &&
        _itemFacts is not null &&
        !(definition.Type == 9
          ? _itemFacts.FitsKiteAnchor(anchorItemType)
          : _itemFacts.FitsCritterAnchor(anchorItemType)))
    {
      if (!restoring)
      {
        throw new InvalidOperationException(
          $"TileEntity {definition.Name} cannot hold item type {anchorItemType}.");
      }
      throw new InvalidOperationException("Invalid restored anchor items must be normalized before validation.");
    }
    if (definition.PayloadKind != TileEntityPayloadKind.AnchorItem && anchorItemType != 0)
    {
      throw new InvalidOperationException($"TileEntity {definition.Name} contains an anchor item type.");
    }
  }

  private TileEntitySnapshot NormalizeRestoredAnchorItem(TileEntitySnapshot snapshot)
  {
    if (_itemFacts is null || snapshot.Type.Value is not (9 or 10) ||
        snapshot.AnchorItemType == 0)
    {
      return snapshot;
    }

    bool fits = snapshot.Type.Value == 9
      ? _itemFacts.FitsKiteAnchor(snapshot.AnchorItemType)
      : _itemFacts.FitsCritterAnchor(snapshot.AnchorItemType);
    if (fits)
    {
      return snapshot;
    }

    return new TileEntitySnapshot(
      snapshot.Id,
      snapshot.Type,
      snapshot.Anchor,
      snapshot.Items,
      snapshot.NpcIndex,
      snapshot.LogicCheck,
      snapshot.LogicOn,
      snapshot.Pose,
      anchorItemType: 0);
  }

  private static void ValidateSection(MechanismSectionCoordinate section)
  {
    if (section.X < 0 || section.Y < 0)
    {
      throw new InvalidOperationException("TileEntity sections must be inside the world bounds.");
    }
  }

  private void ValidateIdentity(MechanismEntityHandle identity)
  {
    if (!identity.IsValid)
    {
      throw new ArgumentException("A valid root identity and runtime handle are required.",
        nameof(identity));
    }
    if (identity.WorldScope != _worldScope)
    {
      throw new ArgumentException("The identity belongs to a different world scope.",
        nameof(identity));
    }
    if (!_identityAuthority.IsActive(identity))
    {
      throw new ArgumentException("The identity is not active in the root identity registry.",
        nameof(identity));
    }
  }
}
