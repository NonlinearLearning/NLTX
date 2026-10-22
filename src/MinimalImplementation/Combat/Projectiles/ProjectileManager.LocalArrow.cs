using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed partial class ProjectileManager
{
  /// <summary>
  /// Captures the migrated neutral-source NewProjectile birth. The source's -1 owner is
  /// normalized to the server sentinel 255; no item or parent-source modifiers are inferred.
  /// </summary>
  public ProjectileLocalArrowSpawnPreparation PrepareLocalArrowSpawn(
      ProjectileLocalArrowSpawnRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!IsWorldScopeCurrent())
    {
      return ProjectileLocalArrowSpawnPreparation.Rejected(
          ProtocolErrorCode.NotReady,
          "Projectile state must be current before a local Arrow birth.");
    }
    if (request.Owner is not (-1 or byte.MaxValue) ||
        request.CurrentLocalPlayerSlot != byte.MaxValue)
    {
      return ProjectileLocalArrowSpawnPreparation.Rejected(
          ProtocolErrorCode.Forbidden,
          "The local Arrow path requires owner -1/255 and the server local-player sentinel.");
    }
    if (!IsFinite(request.Center.X) || !IsFinite(request.Center.Y) ||
        !IsFinite(request.Velocity.X) || !IsFinite(request.Velocity.Y) ||
        !IsFinite(request.Knockback) || !IsFinite(request.Ai0) ||
        !IsFinite(request.Ai1) || !IsFinite(request.Ai2) ||
        (request.Networked && (request.Damage < short.MinValue || request.Damage > short.MaxValue)))
    {
      return ProjectileLocalArrowSpawnPreparation.Rejected(
          ProtocolErrorCode.InvalidLength,
          "Local Arrow birth values must be finite and network damage must fit packet 27.");
    }
    if (!_definitions.TryGet(1, out ProjectileDefinition definition))
    {
      return ProjectileLocalArrowSpawnPreparation.Rejected(
          ProtocolErrorCode.NotReady,
          "The ordinary Arrow has no migrated Version4 definition.");
    }
    if (!IsCurrentSpawnSnapshot(request.Snapshot, out OwnerCommitResult? snapshotError))
    {
      return ProjectileLocalArrowSpawnPreparation.Rejected(
          snapshotError!.ErrorCode ?? ProtocolErrorCode.Conflict,
          snapshotError.Detail ?? "The Arrow birth world snapshot is stale.");
    }

    int runtimeSlot = FindLocalArrowSlot();
    if (runtimeSlot < 0)
    {
      return ProjectileLocalArrowSpawnPreparation.Rejected(
          ProtocolErrorCode.NotReady,
          "The local Arrow path has no empty protocol slot below 1000.");
    }

    Vector2 position = request.Center - new Vector2(definition.Width * 0.5f,
        definition.Height * 0.5f);
    Vector2 velocity = request.Velocity;
    while (velocity.X >= 16f || velocity.X <= -16f ||
        velocity.Y >= 16f || velocity.Y < -16f)
    {
      velocity *= 0.97f;
    }
    if (!IsFinite(position.X) || !IsFinite(position.Y) ||
        !IsFinite(velocity.X) || !IsFinite(velocity.Y))
    {
      return ProjectileLocalArrowSpawnPreparation.Rejected(
          ProtocolErrorCode.InvalidLength,
          "Local Arrow birth arithmetic produced a non-finite result.");
    }

    ProjectileArrowBirthLiquidFacts liquidFacts =
        request.Snapshot.CaptureArrowBirthLiquid(position, definition.Width, definition.Height);
    ArgumentNullException.ThrowIfNull(liquidFacts);
    return ProjectileLocalArrowSpawnPreparation.Prepared(new ProjectileLocalArrowSpawnPlan(
        _revision,
        request,
        definition,
        runtimeSlot,
        liquidFacts,
        request.Snapshot.WorldEpoch,
        request.Snapshot.WorldCommitVersion,
        position,
        velocity));
  }

  public OwnerCommitResult Validate(ProjectileLocalArrowSpawnPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult? snapshotError = null;
    bool snapshotIsCurrent = IsWorldScopeCurrent() &&
        IsCurrentSpawnSnapshot(plan.Request.Snapshot, out snapshotError);
    if (!snapshotIsCurrent ||
        plan.Request.Snapshot.WorldEpoch != plan.WorldEpoch ||
        plan.Request.Snapshot.WorldCommitVersion != plan.WorldCommitVersion)
    {
      return snapshotError ?? OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity,
          "The local Arrow birth world snapshot is no longer current.");
    }
    if (plan.ManagerRevision != _revision || plan.RuntimeSlot < 0 ||
        plan.RuntimeSlot >= _slots.Length || plan.RuntimeSlot >= TerrariaRuntimeSlotCapacity ||
        _slots[plan.RuntimeSlot] is not null)
    {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The local Arrow birth slot or projectile owner changed after preparation.");
    }
    if (!IsFinite(plan.Position.X) || !IsFinite(plan.Position.Y) ||
        !IsFinite(plan.Velocity.X) || !IsFinite(plan.Velocity.Y))
    {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.InvalidLength,
          "The prepared local Arrow birth contains non-finite motion.");
    }
    return OwnerCommitResult.Applied(_commitVersion);
  }

  public ProjectileLocalArrowSpawnCommitResult Commit(
      ProjectileLocalArrowSpawnPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return new ProjectileLocalArrowSpawnCommitResult(validation, null, null, null);
    }
    if (_revision == long.MaxValue || _spawnOrdinal == long.MaxValue ||
        !CanAdvanceCommit(1))
    {
      return new ProjectileLocalArrowSpawnCommitResult(
          OwnerCommitResult.Rejected(
              ProtocolErrorCode.NotReady,
              "Projectile creation or commit version range is exhausted."),
          null,
          null,
          null);
    }

    CommitVersion nextVersion = _commitVersion.Next();
    CombatEntityIdentity entity = _identityAuthority.Create();
    bool identityConflict = ContainsIdentity(entity);
    if (!entity.IsValid || entity.WorldScope != _identityAuthority.CurrentWorldScope ||
        !_identityAuthority.IsActive(entity) || identityConflict)
    {
      if (!identityConflict && entity.IsValid && _identityAuthority.IsActive(entity) &&
          !_identityAuthority.Retire(entity))
      {
        throw new InvalidOperationException(
            "The projectile identity authority could not retire an invalid local Arrow identity.");
      }
      return new ProjectileLocalArrowSpawnCommitResult(
          OwnerCommitResult.Rejected(
              ProtocolErrorCode.StaleEntity,
              "The projectile identity authority returned an invalid local Arrow identity."),
          null,
          null,
          null);
    }

    byte owner = byte.MaxValue;
    ProjectileLiquidState liquidState = new(
        plan.LiquidFacts.WetCollision,
        false,
        plan.LiquidFacts.Honey,
        plan.LiquidFacts.Shimmer,
        0);
    var projectile = new Projectile(
        entity,
        plan.RuntimeSlot,
        plan.Request,
        owner,
        plan.Definition,
        liquidState,
        plan.Position,
        plan.Velocity,
        nextVersion);
    _slots[plan.RuntimeSlot] = projectile;
    _byOwnerIdentity.Add((owner, projectile.ProtocolIdentity), projectile);
    projectile.SetSpawnOrdinal(++_spawnOrdinal);
    CommitMutation(nextVersion);

    ProjectileView view = projectile.Capture();
    ProjectilePacketBody? broadcast = plan.Request.Networked
        ? new ProjectilePacketBody(
            27,
            ignoreEpoch: null,
            ProjectilePacketPayloadCodec.EncodeSyncBody(view, _definitions))
        : null;
    var channelTracking = new ProjectileChannelTrackingIntent(
        entity, owner, plan.RuntimeSlot);
    return new ProjectileLocalArrowSpawnCommitResult(
        OwnerCommitResult.Applied(nextVersion), view, broadcast, channelTracking);
  }

  private int FindLocalArrowSlot()
  {
    int limit = Math.Min(_slots.Length, TerrariaRuntimeSlotCapacity);
    for (int index = 0; index < limit; index++)
    {
      if (_slots[index] is null &&
          !_byOwnerIdentity.ContainsKey((byte.MaxValue, checked((short)index))))
      {
        return index;
      }
    }
    return -1;
  }

  private bool IsCurrentSpawnSnapshot(
      IProjectileArrowSpawnSnapshot snapshot,
      out OwnerCommitResult? error)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (snapshot.WorldEpoch == 0 || !snapshot.WorldCommitVersion.IsValid ||
        !snapshot.IsCurrent())
    {
      error = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The local Arrow birth world snapshot is not current.");
      return false;
    }
    error = null;
    return true;
  }
}
