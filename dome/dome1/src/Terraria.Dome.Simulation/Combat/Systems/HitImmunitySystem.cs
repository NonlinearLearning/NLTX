using System.Collections.Generic;
using Terraria.Dome.Simulation.Combat.Components;

namespace Terraria.Dome.Simulation.Combat.Systems;

public sealed class HitImmunitySystem
{
  public bool IsImmune(HitImmunityComponent immunity, int targetId)
  {
    return immunity.IsImmune(targetId);
  }

  public void Apply(HitImmunityComponent immunity, int targetId, int ticks)
  {
    immunity.Set(targetId, ticks);
  }

  public bool IsOwnerMeleeNpcImmune(
    HitImmunityComponent immunity,
    int ownerId,
    int targetIdentity)
  {
    return immunity.IsOwnerMeleeNpcImmune(ownerId, targetIdentity);
  }

  public void ApplyOwnerMeleeNpc(
    HitImmunityComponent immunity,
    int ownerId,
    int targetIdentity,
    int ticks)
  {
    immunity.SetOwnerMeleeNpc(ownerId, targetIdentity, ticks);
  }

  public void CopyOwnerMeleeNpcTicksToProjectile(
    HitImmunityComponent immunity,
    int ownerId,
    int projectileIdentity)
  {
    immunity.CopyOwnerMeleeNpcTicksToProjectile(ownerId, projectileIdentity);
  }

  public bool IsOwnerNpcImmune(
    HitImmunityComponent immunity,
    int ownerId,
    int targetIdentity)
  {
    return immunity.IsOwnerNpcImmune(ownerId, targetIdentity);
  }

  public void ApplyOwnerNpc(
    HitImmunityComponent immunity,
    int ownerId,
    int targetIdentity,
    int ticks)
  {
    immunity.SetOwnerNpc(ownerId, targetIdentity, ticks);
  }

  public bool IsProjectileImmune(
    HitImmunityComponent immunity,
    int projectileIdentity,
    int targetIdentity)
  {
    return immunity.IsProjectileImmune(projectileIdentity, targetIdentity);
  }

  public bool IsNpcProjectileImmune(
    HitImmunityComponent immunity,
    int projectileIdentity,
    int targetIdentity)
  {
    return immunity.IsNpcProjectileImmune(projectileIdentity, targetIdentity);
  }

  public void ApplyProjectile(
    HitImmunityComponent immunity,
    int projectileIdentity,
    int targetIdentity,
    int ticks)
  {
    immunity.SetProjectile(projectileIdentity, targetIdentity, ticks);
  }

  public bool IsPlayerProjectileImmune(
    HitImmunityComponent immunity,
    int projectileIdentity,
    int targetIdentity)
  {
    return immunity.IsPlayerProjectileImmune(projectileIdentity, targetIdentity);
  }

  public void ApplyPlayerProjectile(
    HitImmunityComponent immunity,
    int projectileIdentity,
    int targetIdentity,
    int ticks)
  {
    immunity.SetPlayerProjectile(projectileIdentity, targetIdentity, ticks);
  }

  public void ApplyProjectilePermanent(
    HitImmunityComponent immunity,
    int projectileIdentity,
    int targetIdentity)
  {
    immunity.SetProjectilePermanent(projectileIdentity, targetIdentity);
  }

  public bool IsStaticNpcImmune(
    HitImmunityComponent immunity,
    int projectileType,
    int targetIdentity)
  {
    return immunity.IsStaticNpcImmune(projectileType, targetIdentity);
  }

  public void ApplyStaticNpc(
    HitImmunityComponent immunity,
    int projectileType,
    int targetIdentity,
    int ticks)
  {
    immunity.SetStaticNpc(projectileType, targetIdentity, ticks);
  }

  public void ResetNpcSlotData(HitImmunityComponent immunity, int npcIndex)
  {
    immunity.ResetNpcSlotData(npcIndex);
  }

  public void Tick(HitImmunityComponent immunity)
  {
    List<int> expired = new();
    foreach (KeyValuePair<int, int> entry in immunity.NpcTicks)
    {
      if (entry.Value <= 1)
      {
        expired.Add(entry.Key);
      }
      else
      {
        immunity.NpcTicks[entry.Key] = entry.Value - 1;
      }
    }

    for (int index = 0; index < expired.Count; index++)
    {
      immunity.NpcTicks.Remove(expired[index]);
    }

    List<(int OwnerId, int TargetIdentity)> expiredOwnerMelee = new();
    foreach (KeyValuePair<(int OwnerId, int TargetIdentity), int> entry in
      immunity.OwnerMeleeNpcTicks)
    {
      if (entry.Value <= 1)
      {
        expiredOwnerMelee.Add(entry.Key);
      }
      else
      {
        immunity.OwnerMeleeNpcTicks[entry.Key] = entry.Value - 1;
      }
    }

    for (int index = 0; index < expiredOwnerMelee.Count; index++)
    {
      immunity.OwnerMeleeNpcTicks.Remove(expiredOwnerMelee[index]);
    }

    List<(int OwnerId, int TargetIdentity)> expiredOwners = new();
    foreach (KeyValuePair<(int OwnerId, int TargetIdentity), int> entry in immunity.OwnerNpcTicks)
    {
      if (entry.Value <= 1)
      {
        expiredOwners.Add(entry.Key);
      }
      else
      {
        immunity.OwnerNpcTicks[entry.Key] = entry.Value - 1;
      }
    }

    for (int index = 0; index < expiredOwners.Count; index++)
    {
      immunity.OwnerNpcTicks.Remove(expiredOwners[index]);
    }

    List<(int ProjectileIdentity, int TargetIdentity)> expiredProjectiles = new();
    foreach (KeyValuePair<(int ProjectileIdentity, int TargetIdentity), int> entry in
      immunity.ProjectileTicks)
    {
      if (entry.Value <= 1)
      {
        expiredProjectiles.Add(entry.Key);
      }
      else
      {
        immunity.ProjectileTicks[entry.Key] = entry.Value - 1;
      }
    }

    for (int index = 0; index < expiredProjectiles.Count; index++)
    {
      immunity.ProjectileTicks.Remove(expiredProjectiles[index]);
    }

    List<(int ProjectileIdentity, int TargetIdentity)> expiredPlayerProjectiles = new();
    foreach (KeyValuePair<(int ProjectileIdentity, int TargetIdentity), int> entry in
      immunity.PlayerProjectileTicks)
    {
      if (entry.Value <= 1)
      {
        expiredPlayerProjectiles.Add(entry.Key);
      }
      else
      {
        immunity.PlayerProjectileTicks[entry.Key] = entry.Value - 1;
      }
    }

    for (int index = 0; index < expiredPlayerProjectiles.Count; index++)
    {
      immunity.PlayerProjectileTicks.Remove(expiredPlayerProjectiles[index]);
    }

    List<(int ProjectileType, int TargetIdentity)> expiredStatic = new();
    foreach (KeyValuePair<(int ProjectileType, int TargetIdentity), int> entry in
      immunity.StaticNpcTicks)
    {
      if (entry.Value <= 1)
      {
        expiredStatic.Add(entry.Key);
      }
      else
      {
        immunity.StaticNpcTicks[entry.Key] = entry.Value - 1;
      }
    }

    for (int index = 0; index < expiredStatic.Count; index++)
    {
      immunity.StaticNpcTicks.Remove(expiredStatic[index]);
    }
  }
}
