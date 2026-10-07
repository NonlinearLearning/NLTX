namespace Terraria.Projectile;

public static class ProjectileEffectCooldownSystem
{
  /// <summary>
  /// Applies the shared positive-value decrement. Call once per eligible
  /// projectile substep after Update's early-outs and before AI.
  /// </summary>
  public static void Advance(ref ProjectileEffectCooldownStateComponent state)
  {
    if (state.SoundDelay > 0)
    {
      state.SoundDelay--;
    }
  }

  public static void SetDelay(
    ref ProjectileEffectCooldownStateComponent state,
    int soundDelay)
  {
    state.SoundDelay = soundDelay;
  }
}
