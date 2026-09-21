using System;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerLuckStateComponent
{
  public const float MinimumLuck = -0.7f;
  public const float MaximumLuck = 1.0f;

  public float Luck;
  public byte LuckPotion;
  public int LuckPotionTicks;

  public readonly float CappedLuck => Math.Clamp(Luck, MinimumLuck, MaximumLuck);

  public void SetLuck(float luck)
  {
    if (!float.IsFinite(luck))
    {
      throw new ArgumentOutOfRangeException(nameof(luck));
    }

    Luck = Math.Clamp(luck, MinimumLuck, MaximumLuck);
  }

  public void ApplyLuckPotion(byte level, int durationTicks)
  {
    if (level is < 1 or > 3 || durationTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(level));
    }

    LuckPotion = Math.Max(LuckPotion, level);
    LuckPotionTicks = Math.Max(LuckPotionTicks, durationTicks);
  }

  public void TickLuckPotion()
  {
    if (LuckPotionTicks <= 0)
    {
      LuckPotionTicks = 0;
      LuckPotion = 0;
      return;
    }

    LuckPotionTicks--;
    if (LuckPotionTicks == 0)
    {
      LuckPotion = 0;
    }
  }
}
