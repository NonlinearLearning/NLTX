namespace Terraria.Unshared.Projectiles;

/// <summary>Result of a projectile lifecycle command.</summary>
public enum ProjectileLifecycleStatus : byte
{
  Applied = 0,
  Rejected = 1,
  DefinitionUnknown = 2,
  OwnerUnavailable = 3,
  StaleReference = 4,
  AlreadyTerminated = 5,
  Deferred = 6
}
