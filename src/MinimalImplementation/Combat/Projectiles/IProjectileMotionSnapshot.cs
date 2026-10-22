using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Read-only world snapshot for ordinary Arrow movement. All queries must use the same
/// covered tile and world-fact snapshot and must not mutate or reenter either owner.
/// Its version guard must cover wind settings and weather as well as tile changes.
/// Missing coverage is an integration error, not an empty world. The caller keeps both
/// owners exclusive; IsCurrent is a version guard, not a lock.
/// </summary>
public interface IProjectileMotionSnapshot {
  ulong WorldEpoch { get; }

  CommitVersion WorldCommitVersion { get; }

  bool IsCurrent();

  /// <summary>
  /// Captures source LavaCollision(position,width,height), then WetCollision with the same
  /// geometry, including that WetCollision call's honey/shimmer outputs. The wet result
  /// includes water, lava, honey and shimmer. Preserve the source collision algorithms;
  /// point sampling the center is insufficient. Missing coverage must fail explicitly.
  /// Combat advances the generation's sticky flags; the adapter supplies raw facts only.
  /// </summary>
  ProjectileLiquidFacts CaptureArrowLiquid(Vector2 position, int width, int height);

  /// <summary>
  /// Captures all required wind facts at the supplied float Arrow center before movement.
  /// Sample the tile at (int)center.X / 16, (int)center.Y / 16, preserving cast order.
  /// CenterTileExists=false means a real null tile; missing coverage must fail explicitly.
  /// All world and tile facts belong to this snapshot, including when wind is disabled.
  /// </summary>
  ProjectileArrowWindFacts CaptureArrowWind(Vector2 center);

  /// <summary>
  /// Returns the source TileCollision displacement: fallThrough=true, fall2=true,
  /// gravDir=1, ignoreDoors=false, ignoreAetheriumPlatforms=false, hoik=true.
  /// Position is the collision-box origin, not its center or destination.
  /// </summary>
  Vector2 TileCollision(Vector2 position, Vector2 velocity, int width, int height);

  /// <summary>
  /// Returns source SlopeCollision with gravity=0f and fall=true as (x,y,vx,vy).
  /// An adapter must implement the real slope operation, including its coverage needs.
  /// </summary>
  Vector4 SlopeCollision(Vector2 position, Vector2 velocity, int width, int height);
}
