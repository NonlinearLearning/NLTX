using System;
using Terraria.Physics;

namespace Terraria.Projectile;

/// <summary>
/// Detached simulation output carrying revision and component values to a commit owner.
/// </summary>
public readonly record struct ProjectileSimulationResult
{
  private ProjectileSimulationResult(
      ProjectileSimulationStatus status,
      SpatialWorldRevision worldRevision,
      ProjectileMotionResult motion,
      ProjectileKinematicsStateComponent kinematics,
      bool tileCollision,
      string? detail)
  {
    Status = status;
    WorldRevision = worldRevision;
    Motion = motion;
    Kinematics = kinematics;
    TileCollision = tileCollision;
    Detail = detail;
  }

  public ProjectileSimulationStatus Status { get; }

  public SpatialWorldRevision WorldRevision { get; }

  public ProjectileMotionResult Motion { get; }

  public ProjectileKinematicsStateComponent Kinematics { get; }

  public bool TileCollision { get; }

  public bool ShouldTerminate => Status is
      ProjectileSimulationStatus.TerminationRequested or ProjectileSimulationStatus.Expired;

  public string? Detail { get; }

  internal static ProjectileSimulationResult Create(
      ProjectileSimulationStatus status,
      SpatialWorldRevision revision,
      ProjectileMotionResult motion,
      ProjectileKinematicsStateComponent kinematics,
      bool tileCollision,
      string? detail = null)
  {
    if (status is not ProjectileSimulationStatus.Applied &&
        status is not ProjectileSimulationStatus.TerminationRequested &&
        status is not ProjectileSimulationStatus.Expired &&
        string.IsNullOrWhiteSpace(detail))
    {
      throw new ArgumentException("A rejected projectile result requires detail.", nameof(detail));
    }

    return new ProjectileSimulationResult(
        status, revision, motion, kinematics, tileCollision, detail);
  }
}
