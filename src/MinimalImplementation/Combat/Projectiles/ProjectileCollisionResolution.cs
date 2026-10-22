using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Damage facts handed to the NPC owner; the projectile owner never writes NPC life.
/// </summary>
public sealed record ProjectileDamageIntent(
  CombatEntityIdentity Projectile,
  CombatEntityIdentity TargetNpc,
  byte Owner,
  SessionEpoch? OwnerEpoch,
  int Damage,
  int OriginalDamage,
  float Knockback,
  long ProjectileRevision,
  CommitVersion CommitVersion);

/// <summary>
/// Prepared projectile collision facts. Preparation is read-only; committing requires
/// explicit confirmation from the owner that applies NPC damage.
/// </summary>
public sealed class ProjectileCollisionResolutionPreparation
{
  internal ProjectileCollisionResolutionPreparation(
    ProjectileCollisionEvaluation evaluation,
    IReadOnlyList<ProjectileDamageIntent> damageIntents,
    OwnerCommitResult result)
  {
    Evaluation = evaluation;
    DamageIntents = damageIntents;
    Result = result;
  }

  internal ProjectileCollisionEvaluation Evaluation { get; }

  public IReadOnlyList<ProjectileDamageIntent> DamageIntents { get; }

  public OwnerCommitResult Result { get; }

  internal int HitCount => DamageIntents.Count;
}

public sealed record ProjectileCollisionResolutionResult(
  OwnerCommitResult Result,
  bool TileHit,
  IReadOnlyList<ProjectileDamageIntent> DamageIntents,
  ProjectileView? UpdatedProjectile,
  ProjectileView? RetiredProjectile,
  ProjectilePacketBody? Broadcast,
  OwnerCommitResult? TrackerCleanup)
{
  public bool IsApplied => Result.Status == OwnerCommitStatus.Applied;

  public bool IsRetired => RetiredProjectile is not null;
}

/// <summary>
/// Converts collision facts into bounded damage intents and commits penetration only
/// after the NPC owner confirms the corresponding damage results.
/// </summary>
public sealed class ProjectileCollisionResolutionService
{
  private const byte KillProjectileMessageId = 29;

  private readonly ProjectileManager _projectiles;

  public ProjectileCollisionResolutionService(ProjectileManager projectiles)
  {
    ArgumentNullException.ThrowIfNull(projectiles);
    _projectiles = projectiles;
  }

  internal bool IsOwnedBy(ProjectileManager projectiles) {
    return ReferenceEquals(_projectiles, projectiles);
  }

  public ProjectileCollisionResolutionPreparation Prepare(
    ProjectileCollisionEvaluation evaluation)
  {
    ArgumentNullException.ThrowIfNull(evaluation);
    if (!evaluation.IsAccepted)
    {
      return new ProjectileCollisionResolutionPreparation(
        evaluation,
        Array.Empty<ProjectileDamageIntent>(),
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          evaluation.Rejection ?? "Projectile collision facts were rejected."));
    }

    ProjectileView projectile = evaluation.Projectile;
    if (!projectile.Entity.IsValid)
    {
      return Rejected(evaluation, "Projectile collision facts require a valid generation.");
    }

    if (projectile.RemainingPenetration < -1)
    {
      return Rejected(evaluation, "Projectile penetration has an invalid negative value.");
    }

    if (evaluation.TileHit && projectile.TileCollide) {
      if (projectile.Type != 1) {
        return new ProjectileCollisionResolutionPreparation(
            evaluation, Array.Empty<ProjectileDamageIntent>(), OwnerCommitResult.Rejected(
                ProtocolErrorCode.NotReady, "This projectile's tile-impact behavior is not migrated."));
      }
      return Applied(evaluation, Array.Empty<ProjectileDamageIntent>());
    }

    if (!projectile.Friendly || projectile.Hostile || projectile.Damage <= 0 ||
        evaluation.NpcHits.Count == 0 || projectile.RemainingPenetration == 0)
    {
      return Applied(evaluation, Array.Empty<ProjectileDamageIntent>());
    }

    int hitLimit = projectile.RemainingPenetration == -1
      ? evaluation.NpcHits.Count
      : Math.Min(projectile.RemainingPenetration, evaluation.NpcHits.Count);
    var intents = new List<ProjectileDamageIntent>(hitLimit);
    for (int index = 0; index < hitLimit; index++)
    {
      ProjectileCollisionTarget target = evaluation.NpcHits[index];
      if (!target.Entity.IsValid)
      {
        return Rejected(evaluation, "Projectile collision facts contain an invalid NPC generation.");
      }

      intents.Add(new ProjectileDamageIntent(
        projectile.Entity,
        target.Entity,
        projectile.Owner,
        projectile.OwnerEpoch,
        projectile.Damage,
        projectile.OriginalDamage,
        projectile.Knockback,
        projectile.Revision,
        projectile.CommitVersion));
    }

    return Applied(evaluation, intents.AsReadOnly());
  }

  /// <summary>
  /// Commits only the targets explicitly confirmed by the NPC owner. Unknown targets
  /// do not consume projectile penetration. An Arrow tile impact accepts only an empty
  /// confirmation set and retires without NPC damage.
  /// </summary>
  public ProjectileCollisionResolutionResult Commit(
    ProjectileCollisionResolutionPreparation preparation,
    IReadOnlyCollection<CombatEntityIdentity> confirmedTargets)
  {
    ArgumentNullException.ThrowIfNull(preparation);
    ArgumentNullException.ThrowIfNull(confirmedTargets);
    if (preparation.Result.Status != OwnerCommitStatus.Applied)
    {
      return Failed(preparation, preparation.Result);
    }

    var confirmed = new HashSet<CombatEntityIdentity>(confirmedTargets);
    foreach (CombatEntityIdentity target in confirmed)
    {
      if (!preparation.DamageIntents.Any(intent => intent.TargetNpc == target))
      {
        return Failed(
          preparation,
          OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "The NPC damage confirmation does not match the prepared projectile hits."));
      }
    }

    var confirmedIntents = preparation.DamageIntents
      .Where(intent => confirmed.Contains(intent.TargetNpc))
      .ToArray();
    bool tileTermination = preparation.Evaluation.TileHit &&
      preparation.Evaluation.Projectile.TileCollide;
    if (confirmedIntents.Length == 0 && !tileTermination)
    {
      ProjectileView expected = preparation.Evaluation.Projectile;
      if (!_projectiles.TryGet(expected.Entity, out ProjectileView? current) || current is null ||
          current.Revision != expected.Revision || current.CommitVersion != expected.CommitVersion) {
        return Failed(preparation, OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity, "Projectile collision facts are stale."));
      }
      return new ProjectileCollisionResolutionResult(
        OwnerCommitResult.Applied(preparation.Evaluation.Projectile.CommitVersion),
        preparation.Evaluation.TileHit,
        Array.Empty<ProjectileDamageIntent>(),
        preparation.Evaluation.Projectile,
        null,
        null,
        null);
    }

    ProjectileCollisionCommitResult committed = tileTermination
      ? _projectiles.CommitTileCollision(preparation.Evaluation.Projectile)
      : _projectiles.CommitCollisionHits(
        preparation.Evaluation.Projectile, confirmedIntents.Length);
    if (committed.Result.Status != OwnerCommitStatus.Applied)
    {
      return new ProjectileCollisionResolutionResult(
        committed.Result,
        preparation.Evaluation.TileHit,
        Array.Empty<ProjectileDamageIntent>(),
        committed.UpdatedProjectile,
        committed.RetiredProjectile,
        null,
        committed.TrackerCleanup);
    }

    ProjectilePacketBody? broadcast = committed.RetiredProjectile is null
      ? null
      : new ProjectilePacketBody(
        KillProjectileMessageId,
        ignoreEpoch: null,
        ProjectilePacketPayloadCodec.EncodeKillBody(committed.RetiredProjectile));

    return new ProjectileCollisionResolutionResult(
      committed.Result,
      preparation.Evaluation.TileHit,
      confirmedIntents,
      committed.UpdatedProjectile,
      committed.RetiredProjectile,
      broadcast,
      committed.TrackerCleanup);
  }

  private static ProjectileCollisionResolutionPreparation Applied(
    ProjectileCollisionEvaluation evaluation,
    IReadOnlyList<ProjectileDamageIntent> intents)
  {
    return new ProjectileCollisionResolutionPreparation(
      evaluation,
      intents,
      OwnerCommitResult.Applied(evaluation.Projectile.CommitVersion));
  }

  private static ProjectileCollisionResolutionPreparation Rejected(
    ProjectileCollisionEvaluation evaluation,
    string detail)
  {
    return new ProjectileCollisionResolutionPreparation(
      evaluation,
      Array.Empty<ProjectileDamageIntent>(),
      OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity, detail));
  }

  private static ProjectileCollisionResolutionResult Failed(
    ProjectileCollisionResolutionPreparation preparation,
    OwnerCommitResult result)
  {
    return new ProjectileCollisionResolutionResult(
      result,
      preparation.Evaluation.TileHit,
      Array.Empty<ProjectileDamageIntent>(),
      null,
      null,
      null,
      null);
  }
}
