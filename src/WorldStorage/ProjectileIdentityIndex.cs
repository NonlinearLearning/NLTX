namespace Terraria.WorldStorage;

public sealed class ProjectileIdentityIndex
{
  private Dictionary<OwnerProjectileIdentity, ProjectileHandle> _byOwnerIdentity = new();
  private Dictionary<ProjectileHandle, OwnerProjectileIdentity> _byProjectile = new();

  public int Count => _byOwnerIdentity.Count;
}
