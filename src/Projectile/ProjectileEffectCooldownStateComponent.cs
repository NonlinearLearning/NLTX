using System;

namespace Terraria.Projectile;

public struct ProjectileEffectCooldownStateComponent
{
  public ProjectileEffectCooldownStateComponent(int soundDelay = 0)
  {
    ValidateSoundDelay(soundDelay);
    SoundDelay = soundDelay;
  }

  public int SoundDelay;

  public bool IsDelayed => SoundDelay > 0;

  private static void ValidateSoundDelay(int value)
  {
    if (value < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }
  }
}
