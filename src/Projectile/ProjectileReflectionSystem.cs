namespace Terraria.Projectile;

public static class ProjectileReflectionSystem
{
  public static void SetReflected(
    ref ProjectileReflectionStateComponent state,
    bool reflected)
  {
    state.Reflected = reflected;
  }

  public static void MarkReflected(
    ref ProjectileReflectionStateComponent state)
  {
    state.Reflected = true;
  }
}
