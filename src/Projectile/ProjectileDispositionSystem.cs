namespace Terraria.Projectile;

public static class ProjectileDispositionSystem
{
  public static void InitializeFromDefinition(
    ref ProjectileDispositionStateComponent state,
    ProjectileDefinitionComponent definition)
  {
    state.Friendly = definition.FriendlyDefault;
    state.Hostile = definition.HostileDefault;
  }

  public static void SetFriendly(
    ref ProjectileDispositionStateComponent state,
    bool friendly)
  {
    state.Friendly = friendly;
  }

  public static void SetHostile(
    ref ProjectileDispositionStateComponent state,
    bool hostile)
  {
    state.Hostile = hostile;
  }
}
