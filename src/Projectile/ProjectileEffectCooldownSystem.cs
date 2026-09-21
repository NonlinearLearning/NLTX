using System;

namespace Terraria.Projectile;

public static class ProjectileEffectCooldownSystem
{
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
    if (soundDelay < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(soundDelay));
    }

    state.SoundDelay = soundDelay;
  }
}
