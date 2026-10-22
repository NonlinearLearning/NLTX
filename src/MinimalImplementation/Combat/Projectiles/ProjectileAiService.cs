using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectileAiTickRequest(CombatEntityIdentity Entity);

public sealed record ProjectileAiTickResult(
  OwnerCommitResult Result,
  ProjectileView? Projectile,
  ProjectileAiProfile Profile,
  bool Expired)
{
  public bool IsApplied => Result.Status == OwnerCommitStatus.Applied && Projectile is not null;
}

/// <summary>
/// Applies a legacy update without spatial collision. Use ProjectileMovementService for
/// movement, damage, then lifetime ordering. Do not call both paths for the same update.
/// Network packets never call this service.
/// </summary>
public sealed class ProjectileAiService
{
  private readonly ProjectileManager _projectiles;

  public ProjectileAiService(ProjectileManager projectiles)
  {
    ArgumentNullException.ThrowIfNull(projectiles);
    _projectiles = projectiles;
  }

  public ProjectileAiTickResult Tick(ProjectileAiTickRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!_projectiles.TryGet(request.Entity, out ProjectileView? current) || current is null)
    {
      return Rejected(
        ProjectileAiProfileCatalog.Resolve(-1),
        ProtocolErrorCode.StaleEntity,
        "The projectile AI tick targeted an inactive generation.");
    }

    ProjectileAiProfile profile = ProjectileAiProfileCatalog.Resolve(current.AiStyle);
    if (!profile.IsSupported)
    {
      return Rejected(
        profile,
        ProtocolErrorCode.NotReady,
        $"Projectile aiStyle {current.AiStyle} has no migrated owner-thread implementation.");
    }

    if (current.TimeLeft <= 0)
    {
      return Rejected(
        profile,
        ProtocolErrorCode.StaleEntity,
        "The projectile lifetime has expired and must be retired by the lifecycle owner.");
    }

    if (profile.Kind != ProjectileAiProfileKind.Arrow || current.Type != 1)
    {
      return Rejected(
        profile,
        ProtocolErrorCode.NotReady,
        $"Projectile type {current.Type} has no migrated dispatch implementation.");
    }

    if (current.ExtraUpdates != 0) {
      return Rejected(
          profile, ProtocolErrorCode.NotReady,
          "An Arrow with extra updates requires the movement frame sequence.");
    }

    AdvanceArrowAi(current, out float nextAi0, out Vector2 nextVelocity);
    Vector2 nextPosition = current.Position + nextVelocity;
    int nextTimeLeft = current.TimeLeft - 1;

    (OwnerCommitResult result, ProjectileView? projectile) = _projectiles.CommitTick(
      current,
      nextPosition,
      nextVelocity,
      nextAi0,
      current.Ai1,
      current.Ai2,
      nextTimeLeft,
      current.LiquidState,
      current.ExtraUpdates);
    return new ProjectileAiTickResult(
        result, projectile, profile,
        result.Status == OwnerCommitStatus.Applied && nextTimeLeft == 0);
  }

  internal static void AdvanceArrowAi(
      ProjectileView current, out float nextAi0, out Vector2 nextVelocity) {
    // AI_001 type 1 increments the timer before testing its gravity threshold.
    // The final style-wide clamp applies even during the initial straight flight.
    nextAi0 = current.Ai0 + 1f;
    nextVelocity = current.Velocity;
    if (nextAi0 >= 15f) {
      nextAi0 = 15f;
      nextVelocity.Y += 0.1f;
    }
    nextVelocity.Y = Math.Min(nextVelocity.Y, 16f);
  }

  private static ProjectileAiTickResult Rejected(
    ProjectileAiProfile profile,
    ProtocolErrorCode code,
    string detail)
  {
    return new ProjectileAiTickResult(
      OwnerCommitResult.Rejected(code, detail),
      null,
      profile,
      false);
  }
}
