using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Commits the migrated one-hit Arrow before applying its NPC damage. Failed projectile
/// retirement cannot damage an NPC. Both owners must run on one non-reentrant thread.
/// </summary>
public sealed class ProjectileNpcCombatService {
  private readonly ProjectileManager _projectiles;
  private readonly ProjectileCollisionResolutionService _collisions;
  private readonly NpcManager _npcs;

  public ProjectileNpcCombatService(
      ProjectileManager projectiles, ProjectileCollisionResolutionService collisions,
      NpcManager npcs) {
    ArgumentNullException.ThrowIfNull(projectiles);
    ArgumentNullException.ThrowIfNull(collisions);
    ArgumentNullException.ThrowIfNull(npcs);
    if (!collisions.IsOwnedBy(projectiles)) {
      throw new ArgumentException(
          "The collision service must use the same projectile owner.", nameof(collisions));
    }
    _projectiles = projectiles;
    _collisions = collisions;
    _npcs = npcs;
  }

  public NpcPreparation<ProjectileNpcHitPlan> Prepare(
      ProjectileCollisionResolutionPreparation collision,
      AuthenticatedCombatSender? actor, ProjectileNpcHitFacts facts) {
    ArgumentNullException.ThrowIfNull(collision);
    ArgumentNullException.ThrowIfNull(facts);
    if (collision.Result.Status != OwnerCommitStatus.Applied ||
        collision.DamageIntents.Count != 1) {
      return NpcPreparation<ProjectileNpcHitPlan>.Rejected(
          ProtocolErrorCode.NotReady, "An Arrow hit requires one accepted NPC collision.");
    }
    ProjectileView expected = collision.Evaluation.Projectile;
    if (!_projectiles.TryGet(expected.Entity, out ProjectileView? current) || current is null ||
        current.Revision != expected.Revision || current.CommitVersion != expected.CommitVersion) {
      return NpcPreparation<ProjectileNpcHitPlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "The projectile collision generation or revision changed.");
    }
    if (current.Type != 1 || current.RemainingPenetration != 1) {
      return NpcPreparation<ProjectileNpcHitPlan>.Rejected(
          ProtocolErrorCode.NotReady, "This composition supports only the one-hit type-1 Arrow.");
    }
    if (current.Owner != 255 && (actor is null || current.OwnerEpoch != actor.Value.Epoch ||
        current.Owner != actor.Value.PlayerSlot)) {
      return NpcPreparation<ProjectileNpcHitPlan>.Rejected(
          ProtocolErrorCode.Forbidden, "The hit requires the current projectile owner's session.");
    }
    ProjectileDamageIntent intent = collision.DamageIntents[0];
    var damage = new NpcDamageRequest(intent.TargetNpc, facts.ExpectedNpcRevision,
        current.Damage, current.Knockback, facts.HitDirection, facts.Critical,
        facts.ExpertMode, facts.NoGravity, facts.ShadowFlame, current.OwnerEpoch);
    NpcPreparation<NpcDamagePlan> prepared = _npcs.PrepareDamage(damage);
    if (!prepared.IsPrepared) {
      return NpcPreparation<ProjectileNpcHitPlan>.Rejected(
          prepared.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
          prepared.Result.Detail ?? "NPC damage could not be prepared.");
    }
    return NpcPreparation<ProjectileNpcHitPlan>.Prepared(
        new ProjectileNpcHitPlan(this, collision, damage));
  }

  public ProjectileNpcHitResult Commit(ProjectileNpcHitPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    if (!ReferenceEquals(plan.Owner, this)) {
      return new ProjectileNpcHitResult(OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict, "The hit plan belongs to another combat service."),
          null, 0, null);
    }
    if (plan.Completed is not null) {
      return plan.Completed;
    }
    NpcDamageRequest request = plan.Damage;
    if (plan.ConsumedProjectile is not null &&
        _npcs.TryGet(request.Target, out NpcView? current) && current is not null) {
      // No NPC effect has committed yet. A retry may use a newer revision of the
      // same generation; the captured damage/modifiers are never rerolled.
      request = request with { ExpectedRevision = current.Revision };
    }
    NpcPreparation<NpcDamagePlan> prepared = _npcs.PrepareDamage(request);
    if (!prepared.IsPrepared || prepared.Plan is null) {
      return new ProjectileNpcHitResult(prepared.Result, null, 0, plan.ConsumedProjectile);
    }

    OwnerCommitResult validation = _npcs.Validate(prepared.Plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return new ProjectileNpcHitResult(validation, null, 0, plan.ConsumedProjectile);
    }

    if (plan.ConsumedProjectile is null) {
      ProjectileCollisionResolutionResult consumed = _collisions.Commit(
          plan.Collision, new[] { request.Target });
      if (!consumed.IsApplied) {
        return new ProjectileNpcHitResult(consumed.Result, null, 0, consumed);
      }
      plan.ConsumedProjectile = consumed;
    }
    var committed = _npcs.Commit(prepared.Plan);
    var result = new ProjectileNpcHitResult(committed.Result, committed.Npc,
        committed.Result.Status == OwnerCommitStatus.Applied ? prepared.Plan.Damage : 0,
        plan.ConsumedProjectile,
        committed.Result.Status == OwnerCommitStatus.Applied && committed.Npc is not null
          ? prepared.Plan.CreateDamagePacket(committed.Npc) : null);
    if (result.IsApplied) {
      plan.Completed = result;
    }
    return result;
  }
}
