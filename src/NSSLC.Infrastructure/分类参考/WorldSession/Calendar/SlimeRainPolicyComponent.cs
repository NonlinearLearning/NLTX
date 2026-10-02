using System;

namespace Terraria.WorldSession.Calendar;

public sealed class SlimeRainPolicyComponent
{
  public const int DefaultWarningDelay = 420;
  public const float DefaultNpcSlotMultiplier = 0.65f;

  public SlimeRainPolicyComponent(
    int warningDelay = DefaultWarningDelay,
    float npcSlotMultiplier = DefaultNpcSlotMultiplier)
  {
    WarningDelay = warningDelay;
    NpcSlotMultiplier = npcSlotMultiplier;
    Validate();
  }

  public int WarningDelay { get; internal set; }

  public float NpcSlotMultiplier { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(WarningDelay);

    if (!float.IsFinite(NpcSlotMultiplier) || NpcSlotMultiplier < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(NpcSlotMultiplier));
    }
  }
}
