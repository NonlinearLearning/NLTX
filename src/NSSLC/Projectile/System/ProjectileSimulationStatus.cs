namespace Terraria.Projectile;

/// <summary>Outcome of one detached projectile simulation call.</summary>
public enum ProjectileSimulationStatus : byte
{
  Applied = 0,
  TerminationRequested = 1,
  Expired = 2,
  Rejected = 3,
  Conflict = 4,
  NotReady = 5
}
