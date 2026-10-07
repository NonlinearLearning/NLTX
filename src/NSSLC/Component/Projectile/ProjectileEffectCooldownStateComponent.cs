namespace Terraria.Projectile;

public struct ProjectileEffectCooldownStateComponent
{
  public ProjectileEffectCooldownStateComponent(int soundDelay = 0)
  {
    SoundDelay = soundDelay;
  }

  /// <summary>
  /// Version4 AI uses negative values as sentinels and countdown states.
  /// </summary>
  public int SoundDelay;

  public bool IsDelayed => SoundDelay > 0;
}
