namespace Terraria.Projectile;

/// <summary>
/// Maps the Version4 Damage_CanDealDamage state predicate. Target collision,
/// damage ownership, and hit commitment remain separate operations. The pet
/// flag is a snapshot of Main.projPet[type], and Animation.FrameCount maps
/// Main.projFrames[type].
/// </summary>
public static class ProjectileDamageGateQuery
{
  public static bool CanEnterDamagePath(
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorStateComponent behavior,
    ProjectilePenetrationStateComponent penetration,
    ProjectileKinematicsStateComponent kinematics,
    ProjectileAnimationStateComponent animation,
    bool isProjectilePet)
  {
    int projectileType = definition.ProjectileType;
    int aiStyle = definition.BehaviorKey;

    if (HasUnconditionalDamageSuppression(projectileType, aiStyle) ||
      HasConditionalDamageSuppression(
        projectileType,
        aiStyle,
        behavior,
        penetration.RemainingHits,
        kinematics.Velocity))
    {
      return false;
    }

    if (HasAiDamageSuppression(projectileType, aiStyle, behavior))
    {
      return false;
    }

    return !isProjectilePet || IsPetDamageException(
      projectileType,
      behavior,
      animation);
  }

  private static bool HasUnconditionalDamageSuppression(
    int projectileType,
    int aiStyle)
  {
    return projectileType is
        18 or 72 or 86 or 87 or 226 or 378 or 439 or 444 or 460 or 500 or
        535 or 600 or 601 or 602 or 613 or 633 or 650 or 651 or 653 or 831 or
        861 or 882 or 888 or 895 or 896 or 970 or 1007 or 1018 or 1056 or 1090 ||
      aiStyle is 31 or 32 or 138;
  }

  private static bool HasConditionalDamageSuppression(
    int projectileType,
    int aiStyle,
    ProjectileBehaviorStateComponent behavior,
    int penetrate,
    System.Numerics.Vector2 velocity)
  {
    return (projectileType == 434 && behavior.LocalAi0 != 0.0f) ||
      ((projectileType is 833 or 834 or 835) && behavior.Ai0 == 4.0f) ||
      (projectileType == 451 &&
        ((int)(behavior.Ai0 - 1.0f) / penetrate == 0 || behavior.Ai1 < 5.0f) &&
        behavior.Ai0 != 0.0f) ||
      (projectileType == 631 && behavior.LocalAi1 == 0.0f) ||
      (projectileType == 537 && behavior.LocalAi0 <= 30.0f) ||
      (projectileType == 188 && behavior.LocalAi0 < 5.0f) ||
      (aiStyle == 137 && behavior.Ai0 != 0.0f) ||
      (projectileType == 261 && velocity.Length() < 1.5f) ||
      (projectileType == 818 && behavior.Ai0 < 1.0f) ||
      (projectileType == 281 && behavior.Ai0 == -3.0f) ||
      ((projectileType is 598 or 636 or 614 or 971 or 975 or 1024) &&
        behavior.Ai0 == 1.0f) ||
      (projectileType == 923 && behavior.LocalAi0 <= 60.0f) ||
      (projectileType == 919 && behavior.LocalAi0 <= 60.0f) ||
      (aiStyle == 15 && behavior.Ai0 == 0.0f && behavior.LocalAi1 <= 12.0f) ||
      (projectileType >= 511 && projectileType <= 513 && behavior.Ai1 >= 1.0f) ||
      (projectileType == 1022 && behavior.Ai2 > 0.0f) ||
      (projectileType == 1092 && behavior.Ai0 <= 1.0f);
  }

  private static bool HasAiDamageSuppression(
    int projectileType,
    int aiStyle,
    ProjectileBehaviorStateComponent behavior)
  {
    return (aiStyle == 93 && behavior.Ai0 != 0.0f && behavior.Ai0 != 2.0f) ||
      (aiStyle == 10 && behavior.LocalAi1 == -1.0f) ||
      ((projectileType is 85 or 1106) && behavior.LocalAi0 >= 54.0f) ||
      (projectileType == 1091 && behavior.LocalAi0 <= 0.0f) ||
      (aiStyle == 25 &&
        projectileType is not (1021 or 1047 or 1005 or 1014) &&
        behavior.LocalAi2 <= 7.0f);
  }

  private static bool IsPetDamageException(
    int projectileType,
    ProjectileBehaviorStateComponent behavior,
    ProjectileAnimationStateComponent animation)
  {
    float ai0 = behavior.Ai0;

    return projectileType is 266 or 407 or 317 or 1093 or 758 or 951 or 963 or
        1022 or 833 or 834 or 835 or 864 ||
      (projectileType == 388 && ai0 == 2.0f) ||
      (projectileType >= 390 && projectileType <= 395) ||
      (projectileType == 533 && ai0 >= 6.0f && ai0 <= 8.0f) ||
      (projectileType >= 625 && projectileType <= 628) ||
      (projectileType == 755 && ai0 != 0.0f) ||
      (projectileType == 946 && ai0 != 0.0f) ||
      (projectileType == 759 &&
        animation.Frame == animation.FrameCount - 1) ||
      (projectileType == 623 && ai0 == 2.0f);
  }
}
