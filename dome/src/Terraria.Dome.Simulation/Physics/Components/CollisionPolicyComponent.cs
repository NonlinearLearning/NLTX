using System;

namespace Terraria.Dome.Simulation.Physics.Components;

public struct CollisionPolicyComponent
{
  public CollisionPolicyComponent(
    bool canFallThroughPlatforms = false,
    bool isFallingThroughPlatforms = false,
    bool ignoresDoors = false,
    bool ignoresAetheriumPlatforms = false,
    bool allowsHoikTraversal = true,
    SlopeCollisionMode slopeMode = SlopeCollisionMode.Default)
  {
    if (!Enum.IsDefined(slopeMode))
    {
      throw new ArgumentOutOfRangeException(nameof(slopeMode));
    }

    CanFallThroughPlatforms = canFallThroughPlatforms;
    IsFallingThroughPlatforms = canFallThroughPlatforms && isFallingThroughPlatforms;
    IgnoresDoors = ignoresDoors;
    IgnoresAetheriumPlatforms = ignoresAetheriumPlatforms;
    AllowsHoikTraversal = allowsHoikTraversal;
    SlopeMode = slopeMode;
  }

  public bool CanFallThroughPlatforms;
  public bool IsFallingThroughPlatforms;
  public bool IgnoresDoors;
  public bool IgnoresAetheriumPlatforms;
  public bool AllowsHoikTraversal;
  public SlopeCollisionMode SlopeMode;

  public bool CanApplyFallThrough =>
    CanFallThroughPlatforms && IsFallingThroughPlatforms;

  public void SetFallingThroughPlatforms(bool value)
  {
    IsFallingThroughPlatforms = CanFallThroughPlatforms && value;
  }
}
