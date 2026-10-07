using System;

using Terraria.WorldStorage;

namespace Terraria.Projectile;

/// <summary>
/// Computes projectile properties from explicit state snapshots.
/// </summary>
public static class ProjectileDerivedPropertiesQuery
{
  private const int _pixelsPerTileShift = 4;
  private const int _sectionWidthInTiles = 200;
  private const int _sectionHeightInTiles = 150;

  public static bool IsOwnedBySomeone(
    ProjectileSourceMetadataComponent source,
    ProjectileTrapCapabilityComponent trap)
  {
    return !source.IsNpcProjectile && !trap.IsTrap;
  }

  public static bool ShouldCareForAttackCooldown(
    ProjectileHitImmunityPolicyComponent hitImmunity,
    ProjectileSourceMetadataComponent source,
    ProjectileTrapCapabilityComponent trap,
    ProjectileIdentityComponent identity)
  {
    return hitImmunity.UsesOwnerMeleeCooldown &&
      IsOwnedBySomeone(source, trap) &&
      identity.OwnerSlot < 255;
  }

  /// <summary>
  /// Projects the owning player's minion target to the NPC subsystem's slot
  /// identity. The NPC owner resolves that slot to current entity state.
  /// </summary>
  public static Terraria.Npc.NpcSlot? GetOwnerMinionAttackTargetNpcSlot(
    ProjectileIdentityComponent identity,
    int ownerPlayerSlot,
    int minionAttackTargetNpcSlot)
  {
    if (identity.OwnerSlot != ownerPlayerSlot)
    {
      throw new ArgumentException(
        "The minion-target snapshot must belong to the projectile owner.",
        nameof(ownerPlayerSlot));
    }

    return minionAttackTargetNpcSlot < 0
      ? null
      : new Terraria.Npc.NpcSlot(minionAttackTargetNpcSlot);
  }

  public static bool IsWipableTurret(
    ProjectileIdentityComponent identity,
    ProjectileDefinitionComponent definition,
    ProjectileSentryCapabilityComponent sentry,
    int localPlayerSlot,
    bool isDefenderEventOngoing)
  {
    return identity.OwnerSlot == localPlayerSlot &&
      sentry.IsSentry &&
      !IsPersistentDefenderSentry(definition.ProjectileType, isDefenderEventOngoing);
  }

  public static SectionCoordinate GetNetSectionCoordinates(
    ProjectileKinematicsStateComponent kinematics)
  {
    int tileX = (int)kinematics.Position.X >> _pixelsPerTileShift;
    int tileY = (int)kinematics.Position.Y >> _pixelsPerTileShift;
    return new SectionCoordinate(
      tileX / _sectionWidthInTiles,
      tileY / _sectionHeightInTiles);
  }

  private static bool IsPersistentDefenderSentry(
    int projectileType,
    bool isDefenderEventOngoing)
  {
    return isDefenderEventOngoing &&
      ProjectileSpecializedDefinitionQuery.IsAdd2Turret(projectileType);
  }
}
