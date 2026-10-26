namespace Terraria.Projectile;

/// <summary>Reason recorded when a projectile leaves the active simulation.</summary>
public enum ProjectileTerminationReason : byte
{
  Expired = 0,
  HitLimitReached = 1,
  TileCollision = 2,
  OutOfBounds = 3,
  OwnerRemoved = 4,
  WorldUnloaded = 5,
  DefinitionRejected = 6,
  StaleReference = 7,
  AdministrativeCleanup = 8
}
