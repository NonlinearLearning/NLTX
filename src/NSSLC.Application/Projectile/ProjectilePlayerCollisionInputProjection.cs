using System;
using System.Numerics;

using EntityEcs.Components;

using Terraria.Player;
using Terraria.Projectile;

namespace Terraria.NonAuthoritative.Projectile;

/// <summary>
/// Projects Player-owned collision facts into Projectile domain inputs.
/// </summary>
public static class ProjectilePlayerCollisionInputProjection
{
  public static ProjectileAi19OwnerSnapshot CreateAi19OwnerSnapshot(
    in PlayerItemMountAndRuntimePropertiesInput itemAndRuntime,
    PlayerAttackSpeedModifierComponent attackSpeed)
  {
    ArgumentNullException.ThrowIfNull(attackSpeed);
    return new ProjectileAi19OwnerSnapshot(
      true,
      itemAndRuntime.ItemAnimation,
      itemAndRuntime.ItemAnimationMax,
      attackSpeed.MeleeSpeed);
  }

  public static ProjectileAi19ExtensionSnapshot EvaluateAi19SpearExtension(
    in ProjectileCollisionGeometryInput projectile,
    in PlayerItemMountAndRuntimePropertiesInput itemAndRuntime,
    PlayerAttackSpeedModifierComponent attackSpeed)
  {
    ProjectileAi19OwnerSnapshot owner = CreateAi19OwnerSnapshot(
      in itemAndRuntime,
      attackSpeed);
    return ProjectileAi19ExtensionQuery.Evaluate(in projectile, in owner);
  }

  public static Vector2 GetMountedCenter(in PlayerSpatialSnapshot spatial)
  {
    return PlayerSpatialDerivedPropertiesQuery.MountedCenter(in spatial);
  }
}
