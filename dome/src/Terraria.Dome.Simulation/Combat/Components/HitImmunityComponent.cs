using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Combat.Components;

public sealed class HitImmunityComponent
{
  public Dictionary<int, int> NpcTicks { get; } = new();
  public Dictionary<(int OwnerId, int TargetIdentity), int> OwnerMeleeNpcTicks { get; } = new();
  public Dictionary<(int OwnerId, int TargetIdentity), int> OwnerNpcTicks { get; } = new();
  public Dictionary<
    (int ProjectileIdentity, int TargetIdentity),
    int> ProjectileTicks
  {
    get;
  } = new();
  public Dictionary<
    (int ProjectileIdentity, int TargetIdentity),
    int> PlayerProjectileTicks
  {
    get;
  } = new();
  public HashSet<(int ProjectileIdentity, int TargetIdentity)> PermanentProjectileNpcTargets
  {
    get;
  } = new();
  public Dictionary<(int ProjectileType, int TargetIdentity), int> StaticNpcTicks { get; } = new();

  public IReadOnlyDictionary<(int ProjectileIdentity, int TargetIdentity), int>
    LocalNpcImmunityTicks => ProjectileTicks;

  public IReadOnlyDictionary<(int ProjectileIdentity, int TargetIdentity), int>
    PlayerImmunityTicks => PlayerProjectileTicks;

  public IReadOnlyDictionary<(int ProjectileType, int TargetIdentity), int>
    StaticNpcImmunityTicks => StaticNpcTicks;

  public bool IsImmune(int targetId)
  {
    return NpcTicks.TryGetValue(targetId, out int ticks) && ticks > 0;
  }

  public void Set(int targetId, int ticks)
  {
    NpcTicks[targetId] = ticks;
  }

  public bool IsOwnerMeleeNpcImmune(int ownerId, int targetIdentity)
  {
    return OwnerMeleeNpcTicks.TryGetValue((ownerId, targetIdentity), out int ticks) && ticks > 0;
  }

  public void SetOwnerMeleeNpc(int ownerId, int targetIdentity, int ticks)
  {
    OwnerMeleeNpcTicks[(ownerId, targetIdentity)] = ticks;
  }

  public void CopyOwnerMeleeNpcTicksToProjectile(int ownerId, int projectileIdentity)
  {
    foreach (KeyValuePair<(int OwnerId, int TargetIdentity), int> entry in OwnerMeleeNpcTicks)
    {
      if (entry.Key.OwnerId == ownerId && entry.Value > 0)
      {
        ProjectileTicks[(projectileIdentity, entry.Key.TargetIdentity)] = entry.Value;
      }
    }
  }

  public bool IsOwnerNpcImmune(int ownerId, int targetIdentity)
  {
    return OwnerNpcTicks.TryGetValue((ownerId, targetIdentity), out int ticks) && ticks > 0;
  }

  public void SetOwnerNpc(int ownerId, int targetIdentity, int ticks)
  {
    OwnerNpcTicks[(ownerId, targetIdentity)] = ticks;
  }

  public bool IsProjectileImmune(int projectileIdentity, int targetIdentity)
  {
    return IsNpcProjectileImmune(projectileIdentity, targetIdentity) ||
      IsPlayerProjectileImmune(projectileIdentity, targetIdentity);
  }

  public bool IsNpcProjectileImmune(int projectileIdentity, int targetIdentity)
  {
    return PermanentProjectileNpcTargets.Contains((projectileIdentity, targetIdentity)) ||
      (ProjectileTicks.TryGetValue(
        (projectileIdentity, targetIdentity), out int ticks) && ticks > 0);
  }

  public void SetProjectile(int projectileIdentity, int targetIdentity, int ticks)
  {
    ProjectileTicks[(projectileIdentity, targetIdentity)] = ticks;
  }

  public bool IsPlayerProjectileImmune(int projectileIdentity, int targetIdentity)
  {
    return PlayerProjectileTicks.TryGetValue((projectileIdentity, targetIdentity), out int ticks) &&
      ticks > 0;
  }

  public void SetPlayerProjectile(int projectileIdentity, int targetIdentity, int ticks)
  {
    PlayerProjectileTicks[(projectileIdentity, targetIdentity)] = ticks;
  }

  public void SetProjectilePermanent(int projectileIdentity, int targetIdentity)
  {
    PermanentProjectileNpcTargets.Add((projectileIdentity, targetIdentity));
    ProjectileTicks.Remove((projectileIdentity, targetIdentity));
  }

  public bool IsStaticNpcImmune(int projectileType, int targetIdentity)
  {
    return StaticNpcTicks.TryGetValue((projectileType, targetIdentity), out int ticks) && ticks > 0;
  }

  public void SetStaticNpc(int projectileType, int targetIdentity, int ticks)
  {
    StaticNpcTicks[(projectileType, targetIdentity)] = ticks;
  }

  public void ResetNpcSlotData(int targetIdentity)
  {
    List<(int ProjectileIdentity, int TargetIdentity)> projectileEntries = new();
    foreach (KeyValuePair<(int ProjectileIdentity, int TargetIdentity), int> entry
      in ProjectileTicks)
    {
      if (entry.Key.TargetIdentity == targetIdentity)
      {
        projectileEntries.Add(entry.Key);
      }
    }

    for (int index = 0; index < projectileEntries.Count; index++)
    {
      ProjectileTicks.Remove(projectileEntries[index]);
    }

    List<(int ProjectileIdentity, int TargetIdentity)> permanentEntries = new();
    foreach ((int ProjectileIdentity, int TargetIdentity) entry in PermanentProjectileNpcTargets)
    {
      if (entry.TargetIdentity == targetIdentity)
      {
        permanentEntries.Add(entry);
      }
    }

    for (int index = 0; index < permanentEntries.Count; index++)
    {
      PermanentProjectileNpcTargets.Remove(permanentEntries[index]);
    }

    List<(int ProjectileType, int TargetIdentity)> staticEntries = new();
    foreach (KeyValuePair<(int ProjectileType, int TargetIdentity), int> entry in StaticNpcTicks)
    {
      if (entry.Key.TargetIdentity == targetIdentity)
      {
        staticEntries.Add(entry.Key);
      }
    }

    for (int index = 0; index < staticEntries.Count; index++)
    {
      StaticNpcTicks.Remove(staticEntries[index]);
    }
  }
}
