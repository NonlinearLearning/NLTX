using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Single-writer owner of NPC instances, slot projections, and identity generations.
/// </summary>
public sealed partial class NpcManager
{
  public const int TerrariaNpcSlotCapacity = 200;

  private readonly ICombatEntityIdentityAuthority _identityAuthority;
  private readonly NpcDefinitionCatalog _definitions;
  private readonly Npc?[] _slots;
  private readonly Dictionary<CombatEntityIdentity, Npc> _byIdentity = [];
  private long _worldScope;
  private long _revision;
  private CommitVersion _commitVersion = CommitVersion.Initial;

  public NpcManager(
    ICombatEntityIdentityAuthority identityAuthority,
    NpcDefinitionCatalog definitions,
    int slotCapacity = TerrariaNpcSlotCapacity)
  {
    ArgumentNullException.ThrowIfNull(identityAuthority);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(slotCapacity);
    if (slotCapacity > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(slotCapacity));
    }

    long worldScope = identityAuthority.CurrentWorldScope;
    if (worldScope <= 0)
    {
      throw new ArgumentException("NPC manager requires an active world scope.", nameof(identityAuthority));
    }

    _identityAuthority = identityAuthority;
    _definitions = definitions;
    _slots = new Npc[slotCapacity];
    _worldScope = worldScope;
  }

  public long Revision => _revision;

  public CommitVersion CommitVersion => _commitVersion;

  public int ActiveCount => IsWorldScopeCurrent() ? _byIdentity.Count : 0;

  /// <summary>Whether this manager's stored identity world scope is still current.</summary>
  internal bool IsReadyForCurrentWorld => IsWorldScopeCurrent();

  public IReadOnlyList<NpcView> Capture()
  {
    if (!IsWorldScopeCurrent())
    {
      return Array.Empty<NpcView>();
    }

    var result = new List<NpcView>(_byIdentity.Count);
    foreach (Npc? npc in _slots)
    {
      if (npc is { IsActive: true } && _identityAuthority.IsActive(npc.Entity))
      {
        result.Add(npc.Capture());
      }
    }

    return result.AsReadOnly();
  }

  public (OwnerCommitResult Result, NpcView? Npc) CommitTick(
    CombatEntityIdentity identity,
    NpcStateUpdate update)
  {
    ArgumentNullException.ThrowIfNull(update);
    if (!_byIdentity.TryGetValue(identity, out Npc? npc) || !npc.IsActive)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The NPC tick targeted an inactive generation."), null);
    }

    NpcPreparation<NpcStatePlan> preparation = PrepareStateUpdate(identity, update);
    if (!preparation.IsPrepared || preparation.Plan is null)
    {
      return (preparation.Result, null);
    }

    return Commit(preparation.Plan);
  }

  /// <summary>
  /// Packet 23 is server-to-client state; clients cannot create or mutate authoritative NPCs.
  /// </summary>
  public OwnerCommitResult RejectClientSnapshot(InboundPacketEnvelope envelope)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    if (envelope.MessageId != 23)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.UnknownPacket, "Expected packet 23.");
    }

    return OwnerCommitResult.Rejected(
      ProtocolErrorCode.Forbidden,
      "NPC packet 23 is a server-produced snapshot and cannot mutate authoritative NPC state.");
  }

  public NpcPreparation<NpcSpawnPlan> PrepareSpawn(NpcSpawnRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!IsWorldScopeCurrent())
    {
      return NpcPreparation<NpcSpawnPlan>.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC state must be cleared after world-scope invalidation before accepting spawns.");
    }

    if (!_definitions.TryGet(request.Type, out NpcDefinition definition))
    {
      return NpcPreparation<NpcSpawnPlan>.Rejected(
        ProtocolErrorCode.NotReady,
        $"NPC type {request.Type} has no migrated Version4 content definition.");
    }

    if (request.MaximumLifeOverride is int maximumLife) {
      if (maximumLife <= 0) {
        return NpcPreparation<NpcSpawnPlan>.Rejected(
            ProtocolErrorCode.InvalidLength, "NPC maximum life override must be positive.");
      }
      definition = definition.WithMaximumLife(maximumLife);
    }

    if ((definition.IsTownNpc || definition.Type == 453) && request.Home is null) {
      return NpcPreparation<NpcSpawnPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Town-like NPC creation requires explicit housing facts.");
    }
    if (request.Home is { HasValidCoordinates: false }) {
      return NpcPreparation<NpcSpawnPlan>.Rejected(
          ProtocolErrorCode.InvalidLength, "NPC home tiles cannot be below the unset sentinel.");
    }

    if (!IsValidState(request.InitialState, definition))
    {
      return NpcPreparation<NpcSpawnPlan>.Rejected(
        ProtocolErrorCode.InvalidLength,
        "NPC spawn state violates finite-value, life, AI, or catchable-release invariants.");
    }

    short slot = FindFreeSlot();
    if (slot < 0)
    {
      return NpcPreparation<NpcSpawnPlan>.Rejected(
        ProtocolErrorCode.NotReady,
        "The NPC slot table is full.");
    }

    return NpcPreparation<NpcSpawnPlan>.Prepared(
      new NpcSpawnPlan(_revision, request, definition, slot));
  }

  public OwnerCommitResult Validate(NpcSpawnPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (!IsWorldScopeCurrent())
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The world scope changed after NPC spawn preparation.");
    }

    if (plan.ManagerRevision != _revision || plan.Slot >= _slots.Length ||
        _slots[plan.Slot] is not null)
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Conflict,
        null,
        ProtocolErrorCode.Conflict,
        "NPC owner or selected slot changed after spawn preparation.");
    }

    if (!IsValidState(plan.Request.InitialState, plan.Definition))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "NPC spawn facts changed or became invalid before commit.");
    }

    return OwnerCommitResult.Applied(_commitVersion);
  }

  public (OwnerCommitResult Result, NpcView? Npc) Commit(NpcSpawnPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return (validation, null);
    }

    if (_revision == long.MaxValue)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC owner revision range is exhausted."), null);
    }

    CommitVersion nextVersion;
    try
    {
      nextVersion = _commitVersion.Next();
    }
    catch (InvalidOperationException)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC commit version range is exhausted."), null);
    }

    CombatEntityIdentity identity = _identityAuthority.Create();
    bool managerConflict = _byIdentity.ContainsKey(identity);
    if (!identity.IsValid || identity.WorldScope != _identityAuthority.CurrentWorldScope ||
        !_identityAuthority.IsActive(identity) || managerConflict)
    {
      if (!managerConflict && identity.IsValid && _identityAuthority.IsActive(identity) &&
          !_identityAuthority.Retire(identity))
      {
        throw new InvalidOperationException("The identity authority could not retire an invalid NPC creation result.");
      }

      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "Identity authority returned an invalid or conflicting NPC identity."), null);
    }

    var npc = new Npc(identity, plan.Slot, plan.Definition, plan.Request.InitialState,
        plan.Request.Home ?? NpcHomeState.ResetState, nextVersion);
    _slots[plan.Slot] = npc;
    _byIdentity.Add(identity, npc);
    CommitMutation(nextVersion);
    return (OwnerCommitResult.Applied(nextVersion), npc.Capture());
  }

  public NpcPreparation<NpcStatePlan> PrepareStateUpdate(
    CombatEntityIdentity identity,
    NpcStateUpdate update)
  {
    ArgumentNullException.ThrowIfNull(update);
    if (!IsWorldScopeCurrent())
    {
      return NpcPreparation<NpcStatePlan>.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The NPC manager must clear the invalidated world scope before accepting state updates.");
    }

    if (!_byIdentity.TryGetValue(identity, out Npc? npc) || !npc.IsActive ||
        !_identityAuthority.IsActive(identity))
    {
      return NpcPreparation<NpcStatePlan>.Rejected(
        ProtocolErrorCode.StaleEntity,
        "NPC identity or generation is no longer active.");
    }

    if (!IsValidState(update, npc))
    {
      return NpcPreparation<NpcStatePlan>.Rejected(
        ProtocolErrorCode.InvalidLength,
        "NPC state update violates finite-value, life, AI, or catchable-release invariants.");
    }

    return NpcPreparation<NpcStatePlan>.Prepared(new NpcStatePlan(_revision, npc, update));
  }

  public OwnerCommitResult Validate(NpcStatePlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (!IsWorldScopeCurrent())
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The world scope changed after NPC state preparation.");
    }

    if (plan.ManagerRevision != _revision || !plan.Npc.IsActive ||
        plan.Npc.Revision != plan.ExpectedNpcRevision ||
        _slots[plan.Npc.Slot] != plan.Npc ||
        !_byIdentity.TryGetValue(plan.Npc.Entity, out Npc? current) || current != plan.Npc ||
        !_identityAuthority.IsActive(plan.Npc.Entity))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The NPC changed or expired after state preparation.");
    }

    if (!IsValidState(plan.Update, plan.Npc))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "NPC state update became invalid before commit.");
    }

    return OwnerCommitResult.Applied(_commitVersion);
  }

  public (OwnerCommitResult Result, NpcView? Npc) Commit(NpcStatePlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return (validation, null);
    }

    if (_revision == long.MaxValue || plan.Npc.Revision == long.MaxValue)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC revision range is exhausted."), null);
    }

    CommitVersion nextVersion;
    try
    {
      nextVersion = _commitVersion.Next();
    }
    catch (InvalidOperationException)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC commit version range is exhausted."), null);
    }

    plan.Npc.Apply(plan.Update, nextVersion);
    CommitMutation(nextVersion);
    return (OwnerCommitResult.Applied(nextVersion), plan.Npc.Capture());
  }

  public NpcPreparation<NpcRetirementPlan> PrepareRetire(CombatEntityIdentity identity)
  {
    if (!IsWorldScopeCurrent())
    {
      return NpcPreparation<NpcRetirementPlan>.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The NPC manager must clear the invalidated world scope before retirement.");
    }

    if (!_byIdentity.TryGetValue(identity, out Npc? npc) || !npc.IsActive ||
        !_identityAuthority.IsActive(identity))
    {
      return NpcPreparation<NpcRetirementPlan>.Rejected(
        ProtocolErrorCode.StaleEntity,
        "NPC identity or generation is no longer active.");
    }

    if (npc.Life != 0)
    {
      return NpcPreparation<NpcRetirementPlan>.Rejected(
        ProtocolErrorCode.Conflict,
        "NPC retirement requires a committed zero-life state.");
    }

    return NpcPreparation<NpcRetirementPlan>.Prepared(new NpcRetirementPlan(_revision, npc));
  }

  public OwnerCommitResult Validate(NpcRetirementPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (!IsWorldScopeCurrent())
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The world scope changed after NPC retirement preparation.");
    }

    if (plan.ManagerRevision != _revision || !plan.Npc.IsActive || plan.Npc.Life != 0 ||
        plan.Npc.Revision != plan.ExpectedNpcRevision || _slots[plan.Npc.Slot] != plan.Npc ||
        !_byIdentity.TryGetValue(plan.Npc.Entity, out Npc? current) || current != plan.Npc ||
        !_identityAuthority.IsActive(plan.Npc.Entity))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The NPC changed or expired after retirement preparation.");
    }

    return OwnerCommitResult.Applied(_commitVersion);
  }

  public (OwnerCommitResult Result, NpcView? RetiredNpc) Commit(NpcRetirementPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return (validation, null);
    }

    if (_revision == long.MaxValue)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC owner revision range is exhausted."), null);
    }

    CommitVersion nextVersion;
    try
    {
      nextVersion = _commitVersion.Next();
    }
    catch (InvalidOperationException)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC commit version range is exhausted."), null);
    }

    NpcView retired = plan.Npc.Capture();
    if (!_identityAuthority.Retire(plan.Npc.Entity))
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The identity authority rejected retirement of an active NPC."), null);
    }

    plan.Npc.Retire();
    _slots[plan.Npc.Slot] = null;
    _byIdentity.Remove(plan.Npc.Entity);
    _coreLoot.Remove(plan.Npc.Entity);
    CommitMutation(nextVersion);
    return (OwnerCommitResult.Applied(nextVersion), retired);
  }

  public IReadOnlyList<NpcView> CaptureDirty()
  {
    if (!IsWorldScopeCurrent())
    {
      return Array.Empty<NpcView>();
    }

    var result = new List<NpcView>();
    foreach (Npc? npc in _slots)
    {
      if (npc is { IsActive: true, IsDirty: true } && _identityAuthority.IsActive(npc.Entity))
      {
        result.Add(npc.Capture());
      }
    }

    return result.AsReadOnly();
  }

  /// <summary>Releases old-world references after the shared identity scope was advanced.</summary>
  public OwnerCommitResult ClearAfterWorldScopeInvalidation()
  {
    long currentWorldScope = _identityAuthority.CurrentWorldScope;
    if (currentWorldScope <= 0 || currentWorldScope == _worldScope)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "NPC world cleanup requires a changed, active world scope.");
    }

    foreach (Npc npc in _byIdentity.Values)
    {
      if (_identityAuthority.IsActive(npc.Entity))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "World NPCs can be cleared only after the identity authority invalidates their scope.");
      }
    }

    if (_byIdentity.Count == 0)
    {
      if (_revision == long.MaxValue)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "NPC owner revision range is exhausted.");
      }

      CommitVersion emptyScopeVersion;
      try
      {
        emptyScopeVersion = _commitVersion.Next();
      }
      catch (InvalidOperationException)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "NPC commit version range is exhausted.");
      }

      _worldScope = currentWorldScope;
      _pendingDeaths.Clear();
      _coreLoot.Clear();
      CommitMutation(emptyScopeVersion);
      return OwnerCommitResult.Applied(emptyScopeVersion);
    }

    if (_revision == long.MaxValue)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC owner revision range is exhausted.");
    }

    CommitVersion nextVersion;
    try
    {
      nextVersion = _commitVersion.Next();
    }
    catch (InvalidOperationException)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC commit version range is exhausted.");
    }

    foreach (Npc npc in _byIdentity.Values)
    {
      npc.Retire();
    }

    Array.Clear(_slots);
    _byIdentity.Clear();
    _pendingDeaths.Clear();
    _coreLoot.Clear();
    _worldScope = currentWorldScope;
    CommitMutation(nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  public void AcknowledgeSnapshot(NpcView published)
  {
    ArgumentNullException.ThrowIfNull(published);
    if (_byIdentity.TryGetValue(published.Entity, out Npc? npc) &&
        npc.IsActive && npc.Revision == published.Revision &&
        npc.CommitVersion == published.CommitVersion)
    {
      npc.Acknowledge(published.Revision);
    }
  }

  public bool TryGet(short slot, out NpcView? npcView)
  {
    if (IsWorldScopeCurrent() && slot >= 0 && slot < _slots.Length &&
        _slots[slot] is Npc npc && npc.IsActive && _identityAuthority.IsActive(npc.Entity))
    {
      npcView = npc.Capture();
      return true;
    }

    npcView = null;
    return false;
  }

  public bool TryGet(
    short slot,
    CombatEntityIdentity expectedIdentity,
    out NpcView? npcView)
  {
    if (IsWorldScopeCurrent() && slot >= 0 && slot < _slots.Length &&
        _slots[slot] is Npc npc && npc.IsActive &&
        npc.Entity == expectedIdentity && _identityAuthority.IsActive(expectedIdentity))
    {
      npcView = npc.Capture();
      return true;
    }

    npcView = null;
    return false;
  }

  private short FindFreeSlot()
  {
    for (short slot = 0; slot < _slots.Length; slot++)
    {
      if (_slots[slot] is null)
      {
        return slot;
      }
    }

    return -1;
  }

  private void CommitMutation(CommitVersion version)
  {
    _revision++;
    _commitVersion = version;
  }

  private bool IsWorldScopeCurrent()
  {
    return _worldScope > 0 && _identityAuthority.CurrentWorldScope == _worldScope;
  }

  private static bool IsValidState(NpcStateUpdate update, NpcDefinition definition)
  {
    return IsFinite(update.Position) && IsFinite(update.Velocity) &&
      update.Ai.Count == 4 && update.Ai.All(float.IsFinite) &&
      update.MaximumLife > 0 && update.Life >= 0 && update.Life <= update.MaximumLife &&
      update.MaximumLife == definition.MaximumLife &&
      update.PlayerCount is null && update.Difficulty is null &&
      (!update.Shimmering || update.SpawnNeedsSyncing) &&
      (definition.Catchable == update.CatchableReleaseOwner.HasValue);
  }

  private static bool IsValidState(NpcStateUpdate update, Npc npc)
  {
    return IsValidState(update, npc.Definition) && (npc.Life != 0 || update.Life == 0);
  }

  private static bool IsFinite(Vector2 value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
