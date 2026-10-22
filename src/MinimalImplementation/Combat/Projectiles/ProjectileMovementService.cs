using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Each Arrow update: AI -> wind -> liquid/Shimmer -> movement/quiver -> NPC damage ->
/// CompleteTick -> expiry. BeginArrowFrame fixes the update budget before any quiver change.
/// G2 owns that ordering on one non-reentrant owner thread and retains outbound effects.
/// This covers current packet-27 instances, whose source defaults set npcProj=false.
/// Liquid visual/RNG effects, SwitchTiles and tile cutting remain separate.
/// </summary>
public sealed class ProjectileMovementService {
  private readonly ProjectileManager _projectiles;

  public ProjectileMovementService(ProjectileManager projectiles) {
    ArgumentNullException.ThrowIfNull(projectiles);
    _projectiles = projectiles;
  }

  /// <summary>Captures this frame's budget without advancing AI, lifetime or owner state.</summary>
  public (OwnerCommitResult Result, ProjectileArrowFrame? Frame) BeginArrowFrame(
      CombatEntityIdentity entity, long expectedRevision) {
    if (!_projectiles.TryGet(entity, out ProjectileView? current) || current is null ||
        current.Revision != expectedRevision) {
      return (OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "The Arrow frame generation or revision is stale."), null);
    }
    if (!IsSupportedArrow(current) || current.ExtraUpdates is < 0 or > 1) {
      return (OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady, "The frame requires a live ordinary one-hit Arrow."), null);
    }
    return (OwnerCommitResult.Applied(current.CommitVersion),
        new ProjectileArrowFrame(this, current));
  }

  /// <summary>
  /// Single-update entry. Use an explicit frame when ExtraUpdates is already one.
  /// </summary>
  public ProjectileMovementPreparation PrepareArrowMovement(
      CombatEntityIdentity entity, long expectedRevision, IProjectileMotionSnapshot snapshot,
      ProjectileArrowOwnerFacts ownerFacts) {
    return PrepareSingleArrowMovement(entity, expectedRevision, snapshot, ownerFacts, false);
  }

  public ProjectileMovementPreparation PrepareArrowMovement(
      ProjectileArrowFrame frame, IProjectileMotionSnapshot snapshot,
      ProjectileArrowOwnerFacts ownerFacts) {
    return PrepareArrowMovement(frame, snapshot, ownerFacts, requireDry: false);
  }

  /// <summary>
  /// Compatibility entry that rejects an Arrow whose resulting liquid state is wet.
  /// </summary>
  public ProjectileMovementPreparation PrepareDryArrowMovement(
      CombatEntityIdentity entity, long expectedRevision, IProjectileMotionSnapshot snapshot,
      ProjectileArrowOwnerFacts ownerFacts) {
    return PrepareSingleArrowMovement(entity, expectedRevision, snapshot, ownerFacts, true);
  }

  private ProjectileMovementPreparation PrepareSingleArrowMovement(
      CombatEntityIdentity entity, long expectedRevision, IProjectileMotionSnapshot snapshot,
      ProjectileArrowOwnerFacts ownerFacts, bool requireDry) {
    var opened = BeginArrowFrame(entity, expectedRevision);
    if (opened.Frame is null) {
      return ProjectileMovementPreparation.Rejected(
          opened.Result.ErrorCode ?? ProtocolErrorCode.NotReady,
          opened.Result.Detail ?? "The Arrow frame could not be captured.");
    }
    if (opened.Frame.UpdateCount != 1) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.NotReady, "An Arrow with extra updates requires an explicit frame.");
    }
    return PrepareArrowMovement(opened.Frame, snapshot, ownerFacts, requireDry);
  }

  private ProjectileMovementPreparation PrepareArrowMovement(
      ProjectileArrowFrame frame, IProjectileMotionSnapshot snapshot,
      ProjectileArrowOwnerFacts ownerFacts,
      bool requireDry) {
    ArgumentNullException.ThrowIfNull(frame);
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(ownerFacts);
    if (!IsReadyForMovement(frame)) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.Conflict, "The Arrow frame has no update ready for movement.");
    }
    ProjectileView expected = frame.ExpectedProjectile;
    long managerRevision = _projectiles.Revision;
    if (!_projectiles.TryGet(expected.Entity, out ProjectileView? current) || current is null ||
        current.Revision != expected.Revision || current.CommitVersion != expected.CommitVersion) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.StaleEntity, "The Arrow movement generation or revision is stale.");
    }
    if (!IsSupportedArrow(current)) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.NotReady, "Movement requires a live ordinary one-hit Arrow.");
    }
    if (ownerFacts.Entity != current.Entity ||
        ownerFacts.SourceProjectileRevision != current.Revision ||
        ownerFacts.Owner != current.Owner || ownerFacts.OwnerEpoch != current.OwnerEpoch) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.StaleEntity, "The quiver facts do not match this Arrow update.");
    }
    ulong worldEpoch = snapshot.WorldEpoch;
    CommitVersion worldVersion = snapshot.WorldCommitVersion;
    if (worldEpoch == 0 || !worldVersion.IsValid || !snapshot.IsCurrent()) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.Conflict, "The projectile motion snapshot is not current.");
    }
    if (!float.IsFinite(current.Ai0) || !float.IsFinite(current.Ai1) ||
        !float.IsFinite(current.Ai2)) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.InvalidLength, "Arrow AI state must be finite.");
    }
    ProjectileAiService.AdvanceArrowAi(current, out float ai0, out Vector2 velocity);
    var center = new Vector2(
        current.Position.X + current.Width / 2f, current.Position.Y + current.Height / 2f);
    if (!float.IsFinite(ai0) || !float.IsFinite(center.X) || !float.IsFinite(center.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y)) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.InvalidLength, "Arrow AI or geometry is non-finite.");
    }
    ProjectileArrowWindFacts wind = snapshot.CaptureArrowWind(center);
    ArgumentNullException.ThrowIfNull(wind);
    if (!wind.TryApply(center.Y, velocity, out Vector2 windVelocity)) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.InvalidLength, "Arrow wind inputs or results are non-finite.");
    }
    ProjectileLiquidFacts liquidFacts = snapshot.CaptureArrowLiquid(
        current.Position, current.Width, current.Height);
    ArgumentNullException.ThrowIfNull(liquidFacts);
    ProjectileLiquidState liquidState = current.LiquidState.AdvanceArrow(liquidFacts);
    if (requireDry && liquidState.Wet) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.NotReady, "The dry Arrow entry requires a dry liquid result.");
    }
    // Type 1 has an explicit Shimmer branch before the generic aiStyle-1 bounce/Kill path.
    if (liquidState.ShimmerWet && !current.Hostile) {
      windVelocity.Y -= 0.4f;
    }
    if (!ProjectileArrowMotionRules.TryCalculate(
            current, windVelocity, liquidState, snapshot,
            out ProjectileArrowMotionRules.Result motion)) {
      return ProjectileMovementPreparation.Rejected(
          ProtocolErrorCode.InvalidLength, "Arrow motion inputs or query results are non-finite.");
    }

    // Packet-27 instances have npcProj=false. The type-1 defaults set arrow=true.
    // Source grants this only after HandleMovement survives, before Damage and lifetime.
    int extraUpdates = current.ExtraUpdates;
    if (!motion.TileHit && current.Friendly && ownerFacts.MagicQuiver && extraUpdates < 1) {
      extraUpdates = 1;
    }
    var plan = new ProjectileMovementPlan(
        this, managerRevision, current, snapshot, worldEpoch, worldVersion,
        motion.Position, motion.Velocity, ai0, motion.TileHit, liquidState, extraUpdates, frame);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return ProjectileMovementPreparation.Rejected(
          validation.ErrorCode ?? ProtocolErrorCode.Conflict,
          validation.Detail ?? "Arrow motion changed during preparation.");
    }
    return ProjectileMovementPreparation.Prepared(plan);
  }

  public OwnerCommitResult Validate(ProjectileMovementPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    if (!ReferenceEquals(plan.Owner, this)) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict, "The Arrow movement plan belongs to another service.");
    }
    IProjectileMotionSnapshot snapshot = plan.Snapshot;
    if (snapshot.WorldEpoch != plan.WorldEpoch ||
        snapshot.WorldCommitVersion != plan.WorldCommitVersion || !snapshot.IsCurrent()) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict, "The world changed after Arrow movement preparation.");
    }
    // Check the owner after the external guard, too. Queries may not reenter a write.
    if (!IsReadyForMovement(plan.Frame) ||
        plan.Frame.ExpectedProjectile.Revision != plan.Source.Revision ||
        plan.Frame.ExpectedProjectile.CommitVersion != plan.Source.CommitVersion ||
        _projectiles.Revision != plan.ManagerRevision ||
        !_projectiles.TryGet(plan.Source.Entity, out ProjectileView? current) || current is null ||
        current.Revision != plan.Source.Revision ||
        current.CommitVersion != plan.Source.CommitVersion) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "The Arrow movement owner or generation changed.");
    }
    return OwnerCommitResult.Applied(_projectiles.CommitVersion);
  }

  /// <summary>
  /// Commits AI, liquid state and movement without decrementing lifetime. A tile impact retires at
  /// the source Kill invocation position and returns packet 29 for caller-owned delivery.
  /// Reusing a committed plan is rejected; this method does not replay its broadcast.
  /// </summary>
  public ProjectileCollisionResolutionResult Commit(ProjectileMovementPlan plan) {
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return new ProjectileCollisionResolutionResult(
          validation, plan.TileHit, Array.Empty<ProjectileDamageIntent>(), null, null, null, null);
    }
    ProjectileCollisionCommitResult committed = _projectiles.CommitArrowMovement(plan);
    if (committed.Result.Status == OwnerCommitStatus.Applied) {
      plan.MovedProjectile = committed.UpdatedProjectile;
      if (committed.RetiredProjectile is not null) {
        plan.Frame.Terminated = true;
      } else {
        plan.Frame.PendingMovement = plan;
      }
    }
    ProjectilePacketBody? broadcast = new ProjectileTerminationCommitResult(
        committed.Result, committed.RetiredProjectile).CreateKillBody();
    return new ProjectileCollisionResolutionResult(
        committed.Result, plan.TileHit, Array.Empty<ProjectileDamageIntent>(),
        committed.UpdatedProjectile, committed.RetiredProjectile, broadcast,
        committed.TrackerCleanup);
  }

  /// <summary>
  /// Call after post-movement NPC damage only if this Arrow survived. It decrements
  /// lifetime once per update, without AI or movement. A successful completion permits
  /// the frame's next update. On Expired, call RetireExpired and stop this frame.
  /// A revision conflict requires coordination, not a second AI/movement update.
  /// </summary>
  public ProjectileAiTickResult CompleteTick(ProjectileMovementPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    ProjectileAiProfile profile = ProjectileAiProfileCatalog.Resolve(plan.Source.AiStyle);
    ProjectileView? moved = plan.MovedProjectile;
    ProjectileArrowFrame frame = plan.Frame;
    if (!ReferenceEquals(plan.Owner, this) || moved is null || plan.TileHit ||
        !ReferenceEquals(frame.PendingMovement, plan) || frame.Terminated) {
      return new ProjectileAiTickResult(
          OwnerCommitResult.Rejected(
              ProtocolErrorCode.Conflict,
              "Lifetime completion requires this service's moved Arrow."),
          null, profile, false);
    }
    int timeLeft = moved.TimeLeft - 1;
    var committed = _projectiles.CommitTick(
        moved, moved.Position, moved.Velocity, moved.Ai0, moved.Ai1, moved.Ai2,
        timeLeft, moved.LiquidState, moved.ExtraUpdates);
    if (committed.Result.Status == OwnerCommitStatus.Applied && committed.Projectile is not null) {
      frame.ExpectedProjectile = committed.Projectile;
      frame.PendingMovement = null;
      frame.CompletedUpdates++;
      frame.Terminated = timeLeft == 0;
    }
    return new ProjectileAiTickResult(
        committed.Result, committed.Projectile, profile,
        committed.Result.Status == OwnerCommitStatus.Applied && timeLeft == 0);
  }

  /// <summary>
  /// Completes one Arrow substep and immediately performs the source inactive/kill
  /// transition when lifetime reaches zero. The returned packet-29 body is still an
  /// outbound intent for P01; this method never sends it.
  /// </summary>
  public ProjectileTickCompletionResult CompleteTickAndRetire(
      ProjectileMovementPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ProjectileAiTickResult tick = CompleteTick(plan);
    if (tick.Result.Status != OwnerCommitStatus.Applied || !tick.Expired)
    {
      return new ProjectileTickCompletionResult(tick.Result, tick, null, null);
    }

    ProjectileTerminationCommitResult retirement = _projectiles.RetireExpired(plan.Source.Entity);
    ProjectilePacketBody? broadcast = retirement.CreateKillBody();
    return new ProjectileTickCompletionResult(
        retirement.Result,
        tick,
        retirement,
        broadcast);
  }

  /// <summary>
  /// Converts captured SwitchTiles/CutTiles results into an explicit world intent. P05
  /// does not execute either effect or retain a second world tile store.
  /// </summary>
  public ProjectileArrowWorldEffectIntent CreateWorldEffectIntent(
      ProjectileMovementPlan plan,
      ProjectileArrowWorldEffectFacts facts)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(facts);
    if (!facts.IsValid)
    {
      throw new ArgumentException(
          "Arrow world-effect facts contain an invalid SwitchTiles object type.",
          nameof(facts));
    }

    bool cutApplied = facts.CanCutTiles && facts.CutTilesApplied;
    return new ProjectileArrowWorldEffectIntent(
        plan.Source.Entity,
        plan.Source.Revision,
        plan.Source.CommitVersion,
        facts.SwitchTilesChanged,
        facts.SwitchTilesObjectType,
        cutApplied);
  }

  private bool IsReadyForMovement(ProjectileArrowFrame frame) {
    return ReferenceEquals(frame.Owner, this) && !frame.Terminated &&
        frame.CompletedUpdates < frame.UpdateCount && frame.PendingMovement is null;
  }

  private static bool IsSupportedArrow(ProjectileView current) {
    return current.Type == 1 && current.AiStyle == 1 && current.Width == 10 &&
        current.Height == 10 && current.TileCollide && current.Penetrate == 1 &&
        current.RemainingPenetration == 1 && current.TimeLeft > 0;
  }
}
