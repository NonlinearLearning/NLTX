namespace Terraria.Projectile;

public static class ProjectileBehaviorStateResetSystem
{
  public static void Reset(ref ProjectileBehaviorStateComponent state)
  {
    state.ResetAiState();
  }
}
