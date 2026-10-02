namespace Terraria.Projectile;

/// <summary>
/// Receives the ordered lifecycle shell without owning slot allocation.
/// Implementations may delegate AI, movement, collision, combat, or
/// presentation work to their respective Systems.
/// </summary>
public interface IProjectileTickAdapter
{
  void PreUpdateAllProjectiles();

  void UpdateProjectile(
    ProjectileTickContext context,
    ProjectileEntityState state);

  void PostUpdateAllProjectiles();
}
