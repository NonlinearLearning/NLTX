using System.Numerics;
using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// P03-owned world collision adapter. P05 consumes its answer and owns no tile state.
/// </summary>
public interface IProjectileCollisionQuery
{
  bool HasSolidTileCollision(Vector2 position, int width, int height);

  IReadOnlyList<ProjectileCollisionTarget> QueryNpcHits(
    ProjectileView projectile,
    Vector2 position);
}

public sealed record ProjectileCollisionTarget(CombatEntityIdentity Entity);

public sealed record ProjectileCollisionEvaluation(
  ProjectileView Projectile,
  Vector2 CandidatePosition,
  bool TileHit,
  IReadOnlyList<ProjectileCollisionTarget> NpcHits,
  string? Rejection)
{
  public bool IsAccepted => Rejection is null;
}

/// <summary>Converts an authoritative projectile snapshot into explicit collision facts.</summary>
public sealed class ProjectileCollisionService
{
  /// <summary>
  /// Legacy endpoint probe at Position + Velocity; it is not source movement collision.
  /// Do not call it after ProjectileMovementService has already advanced this Arrow.
  /// </summary>
  public ProjectileCollisionEvaluation Evaluate(
    ProjectileView projectile,
    IProjectileCollisionQuery query)
  {
    ArgumentNullException.ThrowIfNull(projectile);
    return EvaluateAt(projectile, query, projectile.Position + projectile.Velocity, checkTiles: true);
  }

  /// <summary>
  /// Queries NPCs at the committed movement position without another displacement or
  /// endpoint tile test. Use only for a surviving ProjectileMovementService commit.
  /// Caller keeps the NPC/world inputs current through damage preparation and commit.
  /// </summary>
  public ProjectileCollisionEvaluation EvaluateAfterMovement(
      ProjectileView projectile, IProjectileCollisionQuery query) {
    ArgumentNullException.ThrowIfNull(projectile);
    return EvaluateAt(projectile, query, projectile.Position, checkTiles: false);
  }

  private static ProjectileCollisionEvaluation EvaluateAt(
      ProjectileView projectile, IProjectileCollisionQuery query,
      Vector2 candidatePosition, bool checkTiles) {
    ArgumentNullException.ThrowIfNull(query);
    if (!projectile.Entity.IsValid)
    {
      return Rejected(projectile, "Collision evaluation requires a valid projectile generation.");
    }

    if (!IsFinite(candidatePosition.X) || !IsFinite(candidatePosition.Y))
    {
      return Rejected(projectile, "Projectile collision position is non-finite.");
    }

    bool tileHit = checkTiles && projectile.TileCollide && query.HasSolidTileCollision(
      candidatePosition,
      projectile.Width,
      projectile.Height);

    IReadOnlyList<ProjectileCollisionTarget> npcHits = Array.Empty<ProjectileCollisionTarget>();
    // The ordinary Arrow dies in tile movement before Update calls Damage.
    if (!(tileHit && projectile.Type == 1) &&
        projectile.Friendly && !projectile.Hostile && projectile.Damage > 0)
    {
      IReadOnlyList<ProjectileCollisionTarget>? queriedHits = query.QueryNpcHits(
        projectile,
        candidatePosition);
      if (queriedHits is null)
      {
        return Rejected(projectile, "The P03 projectile collision adapter returned no hit collection.");
      }

      var distinctHits = new List<ProjectileCollisionTarget>(queriedHits.Count);
      var seen = new HashSet<CombatEntityIdentity>();
      foreach (ProjectileCollisionTarget hit in queriedHits)
      {
        ArgumentNullException.ThrowIfNull(hit);
        if (!hit.Entity.IsValid)
        {
          return Rejected(projectile, "The projectile collision adapter returned an invalid NPC generation.");
        }

        if (seen.Add(hit.Entity))
        {
          distinctHits.Add(hit);
        }
      }

      npcHits = distinctHits.AsReadOnly();
    }

    return new ProjectileCollisionEvaluation(
      projectile,
      candidatePosition,
      tileHit,
      npcHits,
      null);
  }

  private static ProjectileCollisionEvaluation Rejected(ProjectileView projectile, string detail)
  {
    return new ProjectileCollisionEvaluation(
      projectile,
      default,
      false,
      Array.Empty<ProjectileCollisionTarget>(),
      detail);
  }

  private static bool IsFinite(float value)
  {
    return float.IsFinite(value);
  }
}
