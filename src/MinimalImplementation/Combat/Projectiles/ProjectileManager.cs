using System;
using System.Collections.Generic;
using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Owns active projectiles, protocol projections, and their runtime slot lifecycle.</summary>
public sealed partial class ProjectileManager
{
  public const int TerrariaRuntimeSlotCapacity = 1000;

  private readonly ICombatEntityIdentityAuthority _identityAuthority;
  private readonly ProjectileDefinitionCatalog _definitions;
  private readonly Projectile?[] _slots;
  private readonly Dictionary<(byte Owner, short Identity), Projectile> _byOwnerIdentity = [];
  private readonly Dictionary<(byte Owner, short Uuid), Projectile> _byOwnerUuid = [];
  private ProjectileTrackerManager? _trackers;
  private long _worldScope;
  private long _revision;
  private CommitVersion _commitVersion = CommitVersion.Initial;
  private long _spawnOrdinal;

  public ProjectileManager(
    ICombatEntityIdentityAuthority identityAuthority,
    ProjectileDefinitionCatalog definitions,
    int slotCapacity = TerrariaRuntimeSlotCapacity)
  {
    ArgumentNullException.ThrowIfNull(identityAuthority);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(slotCapacity);
    long worldScope = identityAuthority.CurrentWorldScope;
    if (worldScope <= 0)
    {
      throw new ArgumentException("Projectile manager requires an active world scope.", nameof(identityAuthority));
    }

    _identityAuthority = identityAuthority;
    _definitions = definitions;
    _slots = new Projectile[slotCapacity];
    _worldScope = worldScope;
  }

  public long Revision => _revision;

  public CommitVersion CommitVersion => _commitVersion;

  public void AttachTrackerManager(ProjectileTrackerManager trackers)
  {
    ArgumentNullException.ThrowIfNull(trackers);
    if (_trackers is not null && !ReferenceEquals(_trackers, trackers))
    {
      throw new InvalidOperationException("A projectile manager can own only one tracker cleanup adapter.");
    }

    _trackers = trackers;
  }

  public int ActiveCount => IsWorldScopeCurrent() ? _byOwnerIdentity.Count : 0;

  internal (OwnerCommitResult Result, ProjectileView? Projectile) CommitTick(
    ProjectileView expected,
    Vector2 position,
    Vector2 velocity,
    float ai0,
    float ai1,
    float ai2,
    int timeLeft,
    ProjectileLiquidState liquidState,
    int extraUpdates)
  {
    ArgumentNullException.ThrowIfNull(expected);
    ArgumentNullException.ThrowIfNull(liquidState);
    Projectile? projectile = _byOwnerIdentity.Values.FirstOrDefault(
      candidate => candidate.Entity == expected.Entity);
    if (!IsWorldScopeCurrent() || !_identityAuthority.IsActive(expected.Entity) ||
        projectile is null || !projectile.IsActive || projectile.Revision != expected.Revision ||
        projectile.CommitVersion != expected.CommitVersion)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The projectile tick generation or revision is stale."), null);
    }

    // The migrated Arrow path supports zero or one extra update, not a universal content limit.
    if (!IsFinite(position.X) || !IsFinite(position.Y) ||
        !IsFinite(velocity.X) || !IsFinite(velocity.Y) ||
        !IsFinite(ai0) || !IsFinite(ai1) || !IsFinite(ai2) || timeLeft < 0 ||
        extraUpdates is < 0 or > 1)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The projectile tick has invalid motion, lifetime or migrated update count."), null);
    }

    if (_revision == long.MaxValue || projectile.Revision == long.MaxValue)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "Projectile tick revision range is exhausted."), null);
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
        "Projectile commit version range is exhausted."), null);
    }

    projectile.ApplyTick(
        position, velocity, ai0, ai1, ai2, timeLeft, liquidState, extraUpdates, nextVersion);
    CommitMutation(nextVersion);
    return (OwnerCommitResult.Applied(nextVersion), projectile.Capture());
  }

  /// <summary>Retires a projectile whose owner-thread tick has reached its lifetime boundary.</summary>
  public ProjectileTerminationCommitResult RetireExpired(CombatEntityIdentity identity)
  {
    Projectile? projectile = _byOwnerIdentity.Values.FirstOrDefault(
      candidate => candidate.Entity == identity);
    if (!IsWorldScopeCurrent() || !_identityAuthority.IsActive(identity) ||
        projectile is null || !projectile.IsActive)
    {
      return new ProjectileTerminationCommitResult(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity,
          "The expired projectile targeted an inactive generation."),
        null);
    }

    if (projectile.TimeLeft > 0)
    {
      return new ProjectileTerminationCommitResult(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "A projectile can be retired by expiry only after its lifetime reaches zero."),
        null);
    }

    if (!TryGetNextCommit(out CommitVersion nextVersion))
    {
      return new ProjectileTerminationCommitResult(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "Projectile expiry commit version range is exhausted."),
        null);
    }

    ProjectileView retired = projectile.Capture();
    if (!RetireProjectile(projectile))
    {
      return new ProjectileTerminationCommitResult(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity,
          "The projectile identity authority rejected expiry retirement."),
        null);
    }

    CommitMutation(nextVersion);
    _trackers?.RemoveEntity(retired.Entity);
    return new ProjectileTerminationCommitResult(
      OwnerCommitResult.Applied(nextVersion),
      retired);
  }

  /// <summary>Commits the migrated Arrow's tile impact, after a trusted world query.</summary>
  internal ProjectileCollisionCommitResult CommitTileCollision(ProjectileView expected) {
    ArgumentNullException.ThrowIfNull(expected);
    if (!TryGet(expected.Entity, out ProjectileView? current) || current is null ||
        current.Revision != expected.Revision || current.CommitVersion != expected.CommitVersion) {
      return RejectedCollision(
          ProtocolErrorCode.StaleEntity, "Projectile tile facts no longer match the generation.");
    }
    if (current.Type != 1 || !current.TileCollide) {
      return RejectedCollision(
          ProtocolErrorCode.NotReady, "Only the ordinary Arrow tile termination is migrated.");
    }
    if (!TryGetNextCommit(out CommitVersion nextVersion)) {
      return RejectedCollision(
          ProtocolErrorCode.NotReady, "Projectile tile retirement version range is exhausted.");
    }
    Projectile projectile = _byOwnerIdentity[(current.Owner, current.ProtocolIdentity)];
    if (!RetireProjectile(projectile)) {
      return RejectedCollision(
          ProtocolErrorCode.StaleEntity, "The identity authority rejected tile retirement.");
    }
    CommitMutation(nextVersion);
    OwnerCommitResult? cleanup = _trackers?.RemoveEntity(current.Entity);
    return new ProjectileCollisionCommitResult(
        OwnerCommitResult.Applied(nextVersion), null, current, cleanup);
  }

  internal ProjectileCollisionCommitResult CommitCollisionHits(
    ProjectileView expected,
    int hitCount)
  {
    ArgumentNullException.ThrowIfNull(expected);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hitCount);

    if (!IsWorldScopeCurrent())
    {
      return RejectedCollision(
        ProtocolErrorCode.StaleEntity,
        "Projectile collision commit targeted an invalidated world scope.");
    }

    if (!_byOwnerIdentity.TryGetValue((expected.Owner, expected.ProtocolIdentity), out Projectile? projectile) ||
        !projectile.IsActive ||
        projectile.Entity != expected.Entity ||
        projectile.Revision != expected.Revision ||
        projectile.CommitVersion != expected.CommitVersion ||
        !_identityAuthority.IsActive(projectile.Entity))
    {
      return RejectedCollision(
        ProtocolErrorCode.StaleEntity,
        "Projectile collision facts no longer describe the active generation.");
    }

    if (projectile.RemainingPenetration < 0)
    {
      return new ProjectileCollisionCommitResult(
        OwnerCommitResult.Applied(_commitVersion),
        projectile.Capture(),
        null,
        null);
    }

    if (hitCount > projectile.RemainingPenetration)
    {
      return RejectedCollision(
        ProtocolErrorCode.Conflict,
        "Projectile collision facts contain more confirmed hits than remaining penetration.");
    }

    if (projectile.Revision == long.MaxValue || !TryGetNextCommit(out CommitVersion nextVersion))
    {
      return RejectedCollision(
        ProtocolErrorCode.NotReady,
        "Projectile penetration commit version range is exhausted.");
    }

    short previousRemainingPenetration = projectile.RemainingPenetration;
    long previousRevision = projectile.Revision;
    CommitVersion previousCommitVersion = projectile.CommitVersion;
    bool previousDirty = projectile.IsDirty;
    projectile.ApplyPenetration(hitCount, nextVersion);
    ProjectileView? retired = null;
    ProjectileView? updated = null;
    if (projectile.RemainingPenetration == 0)
    {
      retired = projectile.Capture();
      if (!RetireProjectile(projectile))
      {
        projectile.RestorePenetrationCommit(
          previousRemainingPenetration,
          previousRevision,
          previousCommitVersion,
          previousDirty);
        return RejectedCollision(
          ProtocolErrorCode.StaleEntity,
          "The projectile identity authority rejected penetration retirement.");
      }
    }
    else
    {
      updated = projectile.Capture();
    }

    CommitMutation(nextVersion);
    OwnerCommitResult? trackerCleanup = retired is null
      ? null
      : _trackers?.RemoveEntity(retired.Entity);
    return new ProjectileCollisionCommitResult(
      OwnerCommitResult.Applied(nextVersion),
      updated,
      retired,
      trackerCleanup);
  }

  public ProjectileSyncPreparation PrepareSync(ProjectileSyncRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);

    if (request.Envelope.MessageId != 27 || request.Envelope.Epoch != request.Sender.Epoch)
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.Forbidden,
        "Projectile synchronization must match the authenticated packet-27 session.");
    }

    if (!IsWorldScopeCurrent())
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.NotReady,
        "Projectile state must be cleared after world-scope invalidation before accepting synchronization.");
    }

    if (!IsFinite(request.Position.X) || !IsFinite(request.Position.Y) ||
        !IsFinite(request.Velocity.X) || !IsFinite(request.Velocity.Y) ||
        !IsFinite(request.Ai0) || !IsFinite(request.Ai1) || !IsFinite(request.Ai2) ||
        (request.Knockback is float knockback && !IsFinite(knockback)))
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Projectile packet 27 contains a non-finite movement or combat value.");
    }

    if (!_definitions.TryGet(request.Type, out ProjectileDefinition definition))
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.NotReady,
        $"Projectile type {request.Type} has no migrated Version4 content definition.");
    }

    byte owner = request.Type == 949 ? byte.MaxValue : request.Sender.PlayerSlot;
    if (owner != byte.MaxValue && request.ClaimedOwner != owner)
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.Forbidden,
        "A client may synchronize only a projectile owned by its authenticated player slot.");
    }

    if (definition.Hostile)
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.Forbidden,
        "Client-originated hostile projectiles are rejected by Version4 server policy.");
    }

    if (definition.NeedsProtocolUuid != request.ProtocolUuid.HasValue)
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Projectile UUID presence does not match the migrated type definition.");
    }

    short? protocolUuid = null;
    if (definition.NeedsProtocolUuid)
    {
      if (request.ProtocolUuid is not short receivedUuid || receivedUuid is < 0 or >= 1000)
      {
        return ProjectileSyncPreparation.Rejected(
          ProtocolErrorCode.MalformedFrame,
          "Projectile UUID must be a nonnegative protocol slot below the Version4 limit.");
      }

      protocolUuid = receivedUuid;
    }
    _byOwnerIdentity.TryGetValue((owner, request.Identity), out Projectile? existing);
    if (existing is not null && existing.OwnerEpoch is SessionEpoch currentEpoch &&
        currentEpoch != request.Sender.Epoch)
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.Forbidden,
        "The projectile identity belongs to another active session epoch.");
    }

    if (protocolUuid is short uuid && uuid >= 0 &&
        _byOwnerUuid.TryGetValue((owner, uuid), out Projectile? uuidMatch) &&
        !ReferenceEquals(existing, uuidMatch))
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.Conflict,
        "The projectile UUID is already associated with another active projectile.");
    }

    if (existing is null && (request.Envelope.EntityGeneration.HasValue || request.Envelope.ExpectedRevision.HasValue))
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.StaleEntity,
        "A new projectile cannot satisfy an envelope bound to an existing entity generation or revision.");
    }

    if (request.Envelope.WorldScope is long requestedWorldScope &&
        requestedWorldScope != _identityAuthority.CurrentWorldScope)
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Packet 27 targets a world scope that is no longer current.");
    }

    int slot = existing?.RuntimeSlot ?? FindFreeSlot();
    Projectile? evicted = null;
    if (slot < 0)
    {
      evicted = FindOldestEvictable();
      if (evicted is null)
      {
        return ProjectileSyncPreparation.Rejected(
          ProtocolErrorCode.NotReady,
          "The projectile pool is full and contains no replaceable projectile.");
      }

      slot = evicted.RuntimeSlot;
    }

    if (request.Envelope.WorldScope is long worldScope &&
        (existing is not null && existing.Entity.WorldScope != worldScope ||
         evicted is not null && evicted.Entity.WorldScope != worldScope))
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Packet 27 targets an entity outside the envelope world scope.");
    }

    if (request.Envelope.EntityGeneration is long generation &&
        existing is not null && existing.Entity.Generation != generation)
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Packet 27 carries a stale server entity generation.");
    }

    if (request.Envelope.ExpectedRevision is long expectedRevision &&
        existing is not null && existing.Revision != expectedRevision)
    {
      return ProjectileSyncPreparation.Rejected(
        ProtocolErrorCode.Conflict,
        "Packet 27 expected a different projectile revision.");
    }

    return ProjectileSyncPreparation.Prepared(new ProjectileSyncPlan(
      _revision,
      request,
      definition,
      owner,
      protocolUuid,
      slot,
      existing,
      existing?.Revision,
      evicted));
  }

  public OwnerCommitResult Validate(ProjectileSyncPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (plan.ManagerRevision != _revision)
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Conflict,
        null,
        ProtocolErrorCode.Conflict,
        "Projectile manager changed after preparation.");
    }

    if (!IsWorldScopeCurrent())
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The world scope changed after projectile preparation.");
    }

    if (plan.Existing is not null &&
        (!plan.Existing.IsActive || plan.Existing.Revision != plan.ExpectedEntityRevision ||
         _slots[plan.RuntimeSlot] != plan.Existing))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The projectile changed or expired after preparation.");
    }

    if (plan.Evicted is not null &&
        (!plan.Evicted.IsActive || _slots[plan.RuntimeSlot] != plan.Evicted || plan.Existing is not null))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The selected projectile replacement slot is no longer available.");
    }

    if (plan.Existing is not null && !_identityAuthority.IsActive(plan.Existing.Entity))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The projectile identity is no longer active.");
    }

    if (plan.Evicted is not null && !_identityAuthority.IsActive(plan.Evicted.Entity))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The replacement projectile identity is no longer active.");
    }

    return new OwnerCommitResult(OwnerCommitStatus.Applied, _commitVersion, null, null);
  }

  public ProjectileSyncCommitResult Commit(ProjectileSyncPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return new ProjectileSyncCommitResult(validation, null, null);
    }

    if (!TryGetNextCommit(out CommitVersion nextVersion))
    {
      return RejectedCommit(ProtocolErrorCode.NotReady, "Projectile manager version range is exhausted.");
    }
    ProjectileView? evictedView = null;
    if (plan.Existing is not null)
    {
      short? priorProtocolUuid = plan.Existing.ProtocolUuid;
      plan.Existing.ApplySync(
        plan.Request,
        plan.Owner,
        plan.ProtocolUuid,
        plan.Definition,
        nextVersion);
      ReplaceProtocolUuidIndex(plan.Existing, priorProtocolUuid, plan.ProtocolUuid);
      CommitMutation(nextVersion);
      return new ProjectileSyncCommitResult(
        OwnerCommitResult.Applied(nextVersion),
        plan.Existing.Capture(),
        null);
    }

    if (_spawnOrdinal == long.MaxValue)
    {
      return RejectedCommit(ProtocolErrorCode.NotReady, "Projectile spawn ordering range is exhausted.");
    }

    CombatEntityIdentity entity = _identityAuthority.Create();
    bool identityConflict = ContainsIdentity(entity);
    if (!entity.IsValid || entity.WorldScope != _identityAuthority.CurrentWorldScope ||
        !_identityAuthority.IsActive(entity) || identityConflict)
    {
      if (!identityConflict && entity.IsValid && _identityAuthority.IsActive(entity) &&
          !_identityAuthority.Retire(entity))
      {
        throw new InvalidOperationException("The projectile identity authority could not retire an invalid creation result.");
      }

      return RejectedCommit(ProtocolErrorCode.StaleEntity, "The projectile identity authority returned an invalid or stale identity.");
    }

    Projectile projectile = new(
      entity,
      plan.RuntimeSlot,
      plan.Request,
      plan.Owner,
      plan.ProtocolUuid,
      plan.Definition,
      revision: 1,
      nextVersion);

    if (plan.Evicted is not null)
    {
      evictedView = plan.Evicted.Capture();
      if (!RetireProjectile(plan.Evicted))
      {
        if (!_identityAuthority.Retire(entity))
        {
          throw new InvalidOperationException("The projectile identity authority could not roll back a prepared creation.");
        }

        return RejectedCommit(ProtocolErrorCode.StaleEntity, "The selected projectile identity could not be retired.");
      }
    }

    _slots[plan.RuntimeSlot] = projectile;
    _byOwnerIdentity.Add((plan.Owner, plan.Request.Identity), projectile);
    AddProtocolUuidIndex(projectile);
    projectile.SetSpawnOrdinal(++_spawnOrdinal);
    CommitMutation(nextVersion);
    return new ProjectileSyncCommitResult(
      OwnerCommitResult.Applied(nextVersion),
      projectile.Capture(),
      evictedView);
  }

  public ProjectileTerminationPreparation PrepareTermination(
    short identity,
    byte claimedOwner,
    InboundPacketEnvelope envelope,
    AuthenticatedCombatSender sender)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    if (envelope.MessageId != 29 || envelope.Epoch != sender.Epoch)
    {
      return ProjectileTerminationPreparation.Rejected(
        ProtocolErrorCode.Forbidden,
        "Projectile termination must match the authenticated packet-29 session.");
    }

    if (claimedOwner != sender.PlayerSlot)
    {
      return ProjectileTerminationPreparation.Rejected(
        ProtocolErrorCode.Forbidden,
        "A client may terminate only a projectile owned by its authenticated player slot.");
    }

    if (!_byOwnerIdentity.TryGetValue((sender.PlayerSlot, identity), out Projectile? projectile) ||
        !projectile.IsActive)
    {
      return ProjectileTerminationPreparation.NoOp();
    }

    if (projectile.OwnerEpoch != sender.Epoch)
    {
      return ProjectileTerminationPreparation.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "The projectile belongs to another session epoch.");
    }

    if (envelope.WorldScope is long worldScope && projectile.Entity.WorldScope != worldScope ||
        envelope.EntityGeneration is long generation && projectile.Entity.Generation != generation)
    {
      return ProjectileTerminationPreparation.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The packet-29 projection does not match the active projectile generation.");
    }

    if (envelope.ExpectedRevision is long expectedRevision && projectile.Revision != expectedRevision)
    {
      return ProjectileTerminationPreparation.Rejected(
        ProtocolErrorCode.Conflict,
        "The projectile revision changed before termination.");
    }

    return ProjectileTerminationPreparation.Prepared(new ProjectileTerminationPlan(
      _revision,
      projectile,
      projectile.Revision,
      sender));
  }

  public OwnerCommitResult Validate(ProjectileTerminationPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (!IsWorldScopeCurrent())
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The world scope changed after projectile termination preparation.");
    }

    if (plan.ManagerRevision != _revision || !plan.Projectile.IsActive ||
        plan.Projectile.Revision != plan.ExpectedEntityRevision ||
        _slots[plan.Projectile.RuntimeSlot] != plan.Projectile ||
        !_identityAuthority.IsActive(plan.Projectile.Entity))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The projectile is no longer the prepared active instance.");
    }

    if (plan.Projectile.Owner != plan.Sender.PlayerSlot || plan.Projectile.OwnerEpoch != plan.Sender.Epoch)
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Rejected,
        null,
        ProtocolErrorCode.Forbidden,
        "The active projectile is not owned by this session.");
    }

    return new OwnerCommitResult(OwnerCommitStatus.Applied, _commitVersion, null, null);
  }

  public ProjectileTerminationCommitResult Commit(ProjectileTerminationPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return new ProjectileTerminationCommitResult(validation, null);
    }

    if (!TryGetNextCommit(out CommitVersion nextVersion))
    {
      return new ProjectileTerminationCommitResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, "Projectile manager version range is exhausted."),
        null);
    }

    ProjectileView view = plan.Projectile.Capture();
    if (!RetireProjectile(plan.Projectile))
    {
      return new ProjectileTerminationCommitResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity, "Projectile identity retirement was rejected."),
        null);
    }

    CommitMutation(nextVersion);
    return new ProjectileTerminationCommitResult(OwnerCommitResult.Applied(nextVersion), view);
  }

  public ProjectilePortalTerminationPreparation PreparePortalTermination(
    ushort targetOwner,
    byte portalIndex,
    InboundPacketEnvelope envelope,
    AuthenticatedCombatSender sender)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    if (envelope.MessageId != 95 || envelope.Epoch != sender.Epoch)
    {
      return ProjectilePortalTerminationPreparation.Rejected(
        ProtocolErrorCode.Forbidden,
        "Portal projectile termination must match the authenticated packet-95 session.");
    }

    if (!IsWorldScopeCurrent())
    {
      return ProjectilePortalTerminationPreparation.Rejected(
        ProtocolErrorCode.NotReady,
        "Projectile state must be cleared after world-scope invalidation before portal termination.");
    }

    if (envelope.WorldScope is long worldScope && worldScope != _worldScope)
    {
      return ProjectilePortalTerminationPreparation.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Packet 95 targets a world scope that is no longer current.");
    }

    foreach (Projectile? projectile in _slots)
    {
      if (projectile is null || !projectile.IsActive || projectile.Owner != targetOwner ||
          projectile.Type != 602 || projectile.Ai1 != portalIndex ||
          !_identityAuthority.IsActive(projectile.Entity))
      {
        continue;
      }

      // Version4 treats the owner as the target portal identity and does not compare it to whoAmI.
      return ProjectilePortalTerminationPreparation.Prepared(new ProjectilePortalTerminationPlan(
        _revision,
        projectile,
        projectile.Revision,
        targetOwner,
        portalIndex));
    }

    return ProjectilePortalTerminationPreparation.NoOp();
  }

  public OwnerCommitResult Validate(ProjectilePortalTerminationPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (!IsWorldScopeCurrent())
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The world scope changed after portal projectile termination preparation.");
    }

    if (plan.ManagerRevision != _revision || !plan.Projectile.IsActive ||
        plan.Projectile.Revision != plan.ExpectedEntityRevision ||
        _slots[plan.Projectile.RuntimeSlot] != plan.Projectile ||
        !_identityAuthority.IsActive(plan.Projectile.Entity) ||
        plan.Projectile.Owner != plan.TargetOwner || plan.Projectile.Type != 602 ||
        plan.Projectile.Ai1 != plan.PortalIndex)
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "The targeted portal projectile is no longer the prepared active instance.");
    }

    return new OwnerCommitResult(OwnerCommitStatus.Applied, _commitVersion, null, null);
  }

  public ProjectileTerminationCommitResult Commit(ProjectilePortalTerminationPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return new ProjectileTerminationCommitResult(validation, null);
    }

    if (!TryGetNextCommit(out CommitVersion nextVersion))
    {
      return new ProjectileTerminationCommitResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, "Projectile manager version range is exhausted."),
        null);
    }

    ProjectileView view = plan.Projectile.Capture();
    if (!RetireProjectile(plan.Projectile))
    {
      return new ProjectileTerminationCommitResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity, "Portal projectile identity retirement was rejected."),
        null);
    }

    CommitMutation(nextVersion);
    return new ProjectileTerminationCommitResult(OwnerCommitResult.Applied(nextVersion), view);
  }

  public IReadOnlyList<ProjectileView> Capture()
  {
    if (!IsWorldScopeCurrent())
    {
      return Array.Empty<ProjectileView>();
    }

    var result = new List<ProjectileView>(_byOwnerIdentity.Count);
    foreach (Projectile projectile in _byOwnerIdentity.Values)
    {
      result.Add(projectile.Capture());
    }

    return result.AsReadOnly();
  }

  public IReadOnlyList<ProjectileView> CaptureDirty()
  {
    if (!IsWorldScopeCurrent())
    {
      return Array.Empty<ProjectileView>();
    }

    var result = new List<ProjectileView>();
    foreach (Projectile projectile in _byOwnerIdentity.Values)
    {
      if (projectile.IsActive && projectile.IsDirty && _identityAuthority.IsActive(projectile.Entity))
      {
        result.Add(projectile.Capture());
      }
    }

    return result.AsReadOnly();
  }

  public void AcknowledgeSnapshot(ProjectileView published)
  {
    ArgumentNullException.ThrowIfNull(published);
    if (_byOwnerIdentity.TryGetValue((published.Owner, published.ProtocolIdentity), out Projectile? projectile) &&
        projectile.IsActive && projectile.Entity == published.Entity &&
        projectile.Revision == published.Revision &&
        projectile.CommitVersion == published.CommitVersion)
    {
      projectile.Acknowledge(published.Revision);
    }
  }

  public ProjectileOwnerCleanupResult RemoveOwner(SessionEpoch epoch)
  {
    if (!IsWorldScopeCurrent())
    {
      return new ProjectileOwnerCleanupResult(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity,
          "The projectile manager must clear the invalidated world scope before session cleanup."),
        Array.Empty<ProjectileView>());
    }

    var removed = new List<ProjectileView>();
    foreach (Projectile projectile in _byOwnerIdentity.Values)
    {
      if (projectile.OwnerEpoch == epoch)
      {
        removed.Add(projectile.Capture());
      }
    }

    if (removed.Count == 0)
    {
      OwnerCommitResult? trackerSessionCleanupWithoutProjectiles = _trackers?.RemoveSession(epoch);
      return new ProjectileOwnerCleanupResult(
        OwnerCommitResult.Applied(_commitVersion),
        removed,
        null,
        trackerSessionCleanupWithoutProjectiles);
    }

    if (!CanAdvanceCommit(removed.Count))
    {
      return new ProjectileOwnerCleanupResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, "Projectile manager version range is exhausted."),
        Array.Empty<ProjectileView>());
    }

    foreach (ProjectileView view in removed)
    {
      if (!_byOwnerIdentity.TryGetValue((view.Owner, view.ProtocolIdentity), out Projectile? projectile) ||
          !_identityAuthority.IsActive(projectile.Entity))
      {
        return new ProjectileOwnerCleanupResult(
          OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity, "Projectile owner cleanup found a stale identity."),
          Array.Empty<ProjectileView>());
      }
    }

    foreach (ProjectileView view in removed)
    {
      if (_byOwnerIdentity.TryGetValue((view.Owner, view.ProtocolIdentity), out Projectile? projectile))
      {
        if (!RetireProjectile(projectile))
        {
          return new ProjectileOwnerCleanupResult(
            OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity, "The projectile identity authority rejected owner cleanup."),
            removed.GetRange(0, removed.FindIndex(candidate => candidate.Entity == projectile.Entity)));
        }

        CommitMutation(_commitVersion.Next());
      }
    }

    OwnerCommitResult? trackerCleanup = _trackers?.RemoveEntities(
      removed.Select(static view => view.Entity).ToArray());
    OwnerCommitResult? trackerSessionCleanup = _trackers?.RemoveSession(epoch);
    return new ProjectileOwnerCleanupResult(
      OwnerCommitResult.Applied(_commitVersion),
      removed,
      trackerCleanup,
      trackerSessionCleanup);
  }

  /// <summary>Releases stale projectile references after the identity authority advances the world scope.</summary>
  public ProjectileOwnerCleanupResult ClearAfterWorldScopeInvalidation()
  {
    long currentWorldScope = _identityAuthority.CurrentWorldScope;
    if (currentWorldScope <= 0 || currentWorldScope == _worldScope)
    {
      return new ProjectileOwnerCleanupResult(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "Projectile world cleanup requires a changed, active world scope."),
        Array.Empty<ProjectileView>());
    }

    foreach (Projectile projectile in _byOwnerIdentity.Values)
    {
      if (_identityAuthority.IsActive(projectile.Entity))
      {
        return new ProjectileOwnerCleanupResult(
          OwnerCommitResult.Rejected(
            ProtocolErrorCode.Conflict,
            "World projectiles can be cleared only after the identity authority invalidates their scope."),
          Array.Empty<ProjectileView>());
      }
    }

    if (!CanAdvanceCommit(1))
    {
      return new ProjectileOwnerCleanupResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, "Projectile manager version range is exhausted."),
        Array.Empty<ProjectileView>());
    }

    ProjectileView[] removed = _byOwnerIdentity.Values
      .Select(static projectile => projectile.Capture())
      .ToArray();
    foreach (Projectile projectile in _byOwnerIdentity.Values)
    {
      projectile.Deactivate();
    }

    Array.Clear(_slots);
    _byOwnerIdentity.Clear();
    _byOwnerUuid.Clear();
    _worldScope = currentWorldScope;
    CommitMutation(_commitVersion.Next());
    OwnerCommitResult trackerCleanup = _trackers?.ClearAfterWorldScopeInvalidation() ??
      OwnerCommitResult.Applied(_commitVersion);
    return new ProjectileOwnerCleanupResult(
      OwnerCommitResult.Applied(_commitVersion),
      removed,
      trackerCleanup);
  }

  public bool TryGet(byte owner, short identity, out ProjectileView? projectile)
  {
    if (IsWorldScopeCurrent() &&
        _byOwnerIdentity.TryGetValue((owner, identity), out Projectile? found) && found.IsActive &&
        _identityAuthority.IsActive(found.Entity))
    {
      projectile = found.Capture();
      return true;
    }

    projectile = null;
    return false;
  }

  /// <summary>Resolves a packet-142 tracker using Version4's owner, identity, and type tuple.</summary>
  public bool TryResolveTrackerReference(
    short owner,
    short identity,
    short type,
    out ProjectileView? projectile)
  {
    if (owner is >= byte.MinValue and <= byte.MaxValue && IsWorldScopeCurrent() &&
        _byOwnerIdentity.TryGetValue(((byte)owner, identity), out Projectile? found) &&
        found.IsActive && found.Owner == (byte)owner && found.ProtocolIdentity == identity &&
        found.Type == type && _identityAuthority.IsActive(found.Entity))
    {
      projectile = found.Capture();
      return true;
    }

    projectile = null;
    return false;
  }

  /// <summary>Resolves a tracker-bound entity generation without relying on a reusable slot index.</summary>
  public bool TryGet(CombatEntityIdentity entity, out ProjectileView? projectile)
  {
    if (entity.IsValid && entity.WorldScope == _worldScope && IsWorldScopeCurrent() &&
        _identityAuthority.IsActive(entity))
    {
      foreach (Projectile candidate in _byOwnerIdentity.Values)
      {
        if (candidate.IsActive && candidate.Entity == entity)
        {
          projectile = candidate.Capture();
          return true;
        }
      }
    }

    projectile = null;
    return false;
  }

  private int FindFreeSlot()
  {
    for (int index = 0; index < _slots.Length; index++)
    {
      if (_slots[index] is null)
      {
        return index;
      }
    }

    return -1;
  }

  private Projectile? FindOldestEvictable()
  {
    Projectile? oldest = null;
    foreach (Projectile? projectile in _slots)
    {
      if (projectile is null || projectile.NetImportant)
      {
        continue;
      }

      if (oldest is null || projectile.SpawnOrdinal < oldest.SpawnOrdinal)
      {
        oldest = projectile;
      }
    }

    return oldest;
  }

  private bool ContainsIdentity(CombatEntityIdentity identity)
  {
    foreach (Projectile projectile in _byOwnerIdentity.Values)
    {
      if (projectile.Entity == identity)
      {
        return true;
      }
    }

    return false;
  }

  private bool RetireProjectile(Projectile projectile)
  {
    if (!_identityAuthority.Retire(projectile.Entity))
    {
      return false;
    }

    projectile.Deactivate();
    _slots[projectile.RuntimeSlot] = null;
    _byOwnerIdentity.Remove((projectile.Owner, projectile.ProtocolIdentity));
    if (projectile.ProtocolUuid is short uuid && uuid >= 0)
    {
      if (_byOwnerUuid.TryGetValue((projectile.Owner, uuid), out Projectile? indexed) &&
          ReferenceEquals(indexed, projectile))
      {
        _byOwnerUuid.Remove((projectile.Owner, uuid));
      }
    }

    return true;
  }

  private void AddProtocolUuidIndex(Projectile projectile)
  {
    if (projectile.ProtocolUuid is short uuid && uuid >= 0)
    {
      _byOwnerUuid.Add((projectile.Owner, uuid), projectile);
    }
  }

  private void ReplaceProtocolUuidIndex(
    Projectile projectile,
    short? priorProtocolUuid,
    short? protocolUuid)
  {
    if (priorProtocolUuid is short priorUuid && priorUuid >= 0)
    {
      if (_byOwnerUuid.TryGetValue((projectile.Owner, priorUuid), out Projectile? indexed) &&
          ReferenceEquals(indexed, projectile))
      {
        _byOwnerUuid.Remove((projectile.Owner, priorUuid));
      }
    }

    if (protocolUuid is short uuid && uuid >= 0)
    {
      _byOwnerUuid.Add((projectile.Owner, uuid), projectile);
    }
  }

  private bool TryGetNextCommit(out CommitVersion nextVersion)
  {
    if (!CanAdvanceCommit(1))
    {
      nextVersion = default;
      return false;
    }

    nextVersion = _commitVersion.Next();
    return true;
  }

  private bool CanAdvanceCommit(int count)
  {
    return count > 0 &&
      _revision <= long.MaxValue - count &&
      _commitVersion.Value <= long.MaxValue - count;
  }

  private void CommitMutation(CommitVersion nextVersion)
  {
    _revision++;
    _commitVersion = nextVersion;
  }

  private bool IsWorldScopeCurrent()
  {
    return _worldScope > 0 && _identityAuthority.CurrentWorldScope == _worldScope;
  }

  private static ProjectileSyncCommitResult RejectedCommit(ProtocolErrorCode code, string detail)
  {
    return new ProjectileSyncCommitResult(OwnerCommitResult.Rejected(code, detail), null, null);
  }

  private static ProjectileCollisionCommitResult RejectedCollision(
    ProtocolErrorCode code,
    string detail)
  {
    return new ProjectileCollisionCommitResult(
      OwnerCommitResult.Rejected(code, detail),
      null,
      null,
      null);
  }

  private static bool IsFinite(float value)
  {
    return float.IsFinite(value);
  }
}
