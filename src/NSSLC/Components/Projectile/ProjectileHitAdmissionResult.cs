namespace Terraria.Projectile;

/// <summary>Result of projectile-side immunity and lifecycle admission.</summary>
public enum ProjectileHitAdmissionResult : byte
{
  Accepted = 0,
  ProjectileUnavailable = 1,
  ProjectileInactive = 2,
  TargetImmune = 3
}
