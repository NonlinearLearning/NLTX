namespace Terraria.Dome.Simulation.Components;

public enum ProjectileDetachPolicy : byte
{
  Manual,
  OnTargetDespawn,
  OnOwnerDespawn,
  OnLifetimeExpired
}
