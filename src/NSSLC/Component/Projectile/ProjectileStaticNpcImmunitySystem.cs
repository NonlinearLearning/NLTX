using System;

using EntityEcs;

using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.Projectile;

/// <summary>
/// Owns Version4's shared per-projectile-type NPC immunity expiry table.
/// </summary>
public static class ProjectileStaticNpcImmunitySystem
{
  /// <summary>
  /// A target remains immune while its absolute expiry tick is greater than
  /// the current game update count; equality means the cooldown has expired.
  /// </summary>
  public static bool IsNpcImmune(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    int projectileType,
    int npcSlot,
    uint gameUpdateCount)
  {
    ArgumentNullException.ThrowIfNull(registry);
    uint[] expiries = GetProjectileTypeExpiries(
      registry,
      projectileType,
      npcSlot);
    return expiries[npcSlot] > gameUpdateCount;
  }

  /// <summary>
  /// Records the static cooldown after the NPC damage owner accepts a hit.
  /// Version4 omits the cooldown for a one-penetration hit unless the type
  /// explicitly applies immunity on single hits.
  /// </summary>
  public static bool RecordAcceptedNpcHit(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    in ProjectileHitImmunityPolicyComponent policy,
    in ProjectilePenetrationStateComponent penetration,
    int projectileType,
    int npcSlot,
    uint gameUpdateCount)
  {
    ArgumentNullException.ThrowIfNull(registry);
    if (!policy.UsesStaticNpcImmunityRegistry ||
      (penetration.RemainingHits == 1 && !policy.AppliesOnSingleHit))
    {
      return false;
    }

    if (policy.StaticNpcCooldownTicks < -1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(policy),
        "Static NPC cooldown must be -1 or non-negative.");
    }

    uint[] expiries = GetProjectileTypeExpiries(
      registry,
      projectileType,
      npcSlot);
    expiries[npcSlot] = unchecked(
      gameUpdateCount + (uint)policy.StaticNpcCooldownTicks);
    return true;
  }

  /// <summary>
  /// Clears one NPC slot's expiry across all projectile types.
  /// </summary>
  public static void ClearStaticNpcSlot(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    int npcSlot)
  {
    ArgumentNullException.ThrowIfNull(registry);
    ValidateNpcSlot(registry, npcSlot);

    uint[][] expiriesByProjectileType = registry.ImmunityExpiryByProjectileType;
    for (int projectileType = 0;
      projectileType < expiriesByProjectileType.Length;
      projectileType++)
    {
      expiriesByProjectileType[projectileType][npcSlot] = 0;
    }
  }

  /// <summary>
  /// Clears static and per-projectile local immunity for a reused NPC slot.
  /// The caller must serialize this operation with projectile storage access.
  /// </summary>
  public static void ResetNpcSlotData(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    EntitySlotStore<WorldEntityState, ProjectileSlot> projectiles,
    EntityRuntime runtime,
    int npcSlot)
  {
    ArgumentNullException.ThrowIfNull(registry);
    ArgumentNullException.ThrowIfNull(projectiles);
    ArgumentNullException.ThrowIfNull(runtime);
    ValidateNpcSlot(registry, npcSlot);

    for (int index = 0; index < projectiles.Capacity; index++)
    {
      if (!TryGetProjectileAt(
        projectiles,
        index,
        runtime,
        out RuntimeEntityHandle runtimeHandle))
      {
        continue;
      }

      bool hasImmunityArray = false;
      if (!runtime.TryInspect<ProjectileHitImmunityStateComponent>(
        runtimeHandle,
        (in ProjectileHitImmunityStateComponent immunity) =>
          hasImmunityArray = immunity.LocalNpcImmunityTicks is not null) ||
        !hasImmunityArray)
      {
        throw new InvalidOperationException(
          "A projectile state has no local NPC immunity array.");
      }
    }

    ClearStaticNpcSlot(registry, npcSlot);
    for (int index = 0; index < projectiles.Capacity; index++)
    {
      if (!TryGetProjectileAt(
        projectiles,
        index,
        runtime,
        out RuntimeEntityHandle runtimeHandle))
      {
        continue;
      }

      if (!runtime.TryEdit<ProjectileHitImmunityStateComponent>(
        runtimeHandle,
        (ref ProjectileHitImmunityStateComponent immunity) =>
          ProjectileHitImmunitySystem.TryResetLocalNpcImmunityAtSlot(
            ref immunity,
            npcSlot)))
      {
        throw new InvalidOperationException(
          "A projectile local-immunity component could not reset the reused NPC slot.");
      }
    }
  }

  private static bool TryGetProjectileAt(
    EntitySlotStore<WorldEntityState, ProjectileSlot> projectiles,
    int index,
    EntityRuntime runtime,
    out RuntimeEntityHandle runtimeHandle)
  {
    ArgumentNullException.ThrowIfNull(runtime);
    if (!projectiles.TryGetOccupiedAt(
      index,
      out _,
      out _,
      out WorldEntityState? entity))
    {
      runtimeHandle = default;
      return false;
    }

    if (entity is not ProjectileRootBinding binding ||
      !runtime.TryGetStatus(binding.RuntimeHandle, out EntityRuntimeStatus status) ||
      status != EntityRuntimeStatus.Running)
    {
      throw new InvalidOperationException(
        "The projectile slot store contains a non-running projectile root binding.");
    }

    runtimeHandle = binding.RuntimeHandle;
    return true;
  }

  private static void ValidateNpcSlot(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    int npcSlot)
  {
    if ((uint)npcSlot >= (uint)registry.NpcCapacity)
    {
      throw new ArgumentOutOfRangeException(nameof(npcSlot));
    }
  }

  private static uint[] GetProjectileTypeExpiries(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    int projectileType,
    int npcSlot)
  {
    uint[][] expiriesByProjectileType = registry.ImmunityExpiryByProjectileType;
    if ((uint)projectileType >= (uint)expiriesByProjectileType.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileType));
    }

    uint[] expiries = expiriesByProjectileType[projectileType];
    if ((uint)npcSlot >= (uint)expiries.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(npcSlot));
    }

    return expiries;
  }
}
