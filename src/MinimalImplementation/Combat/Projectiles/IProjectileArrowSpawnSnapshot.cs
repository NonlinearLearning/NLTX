using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Read-only world boundary for the Arrow birth query. Capture only WetCollision and its
/// honey/shimmer outputs; NewProjectile does not call LavaCollision during creation.
/// </summary>
public interface IProjectileArrowSpawnSnapshot
{
  ulong WorldEpoch { get; }

  CommitVersion WorldCommitVersion { get; }

  bool IsCurrent();

  ProjectileArrowBirthLiquidFacts CaptureArrowBirthLiquid(
      Vector2 position, int width, int height);
}
