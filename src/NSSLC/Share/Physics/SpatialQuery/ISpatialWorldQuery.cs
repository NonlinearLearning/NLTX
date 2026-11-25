using System.Numerics;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;

namespace Terraria.Share.Physics;

/// <summary>
/// Read-only world boundary used by spatial simulation. Implementations provide detached,
/// versioned world data; they do not receive an entity or perform a commit.
/// </summary>
public interface ISpatialWorldQuery
{
  SpatialWorldRevision Revision { get; }

  SpatialTileMotionResult Move(
      Vector2 position,
      Vector2 velocity,
      Vector2 colliderSize,
      TileCollisionCapabilityComponent capability,
      GravityDirection gravityDirection);

  SpatialLiquidContact GetLiquidContact(
      Vector2 position,
      Vector2 colliderSize,
      LiquidInteractionCapabilityComponent capability);
}
