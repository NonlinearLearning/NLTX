using Terraria.Physics;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.COLLISION_POLICY
// crossSubsystemOwner: integration-review
public struct CollisionPolicyComponent
{
  public CollisionPolicyComponent(
    bool collidesWithTiles,
    bool collidesWithEntities,
    bool ignoresLiquids,
    bool canFallThroughPlatforms = false,
    bool isFallingThroughPlatforms = false,
    bool ignoresDoors = false,
    bool ignoresAetheriumPlatforms = false,
    bool allowsHoikTraversal = true,
    SlopeCollisionMode slopeMode = SlopeCollisionMode.Default)
  {
    CollidesWithTiles = collidesWithTiles;
    CollidesWithEntities = collidesWithEntities;
    IgnoresLiquids = ignoresLiquids;
    CanFallThroughPlatforms = canFallThroughPlatforms;
    IsFallingThroughPlatforms = isFallingThroughPlatforms;
    IgnoresDoors = ignoresDoors;
    IgnoresAetheriumPlatforms = ignoresAetheriumPlatforms;
    AllowsHoikTraversal = allowsHoikTraversal;
    SlopeMode = slopeMode;
  }

  public bool CollidesWithTiles;
  public bool CollidesWithEntities;
  public bool IgnoresLiquids;
  public bool CanFallThroughPlatforms;
  public bool IsFallingThroughPlatforms;
  public bool IgnoresDoors;
  public bool IgnoresAetheriumPlatforms;
  public bool AllowsHoikTraversal;
  public SlopeCollisionMode SlopeMode;

  public bool CanApplyFallThrough =>
    CanFallThroughPlatforms && IsFallingThroughPlatforms;
}
