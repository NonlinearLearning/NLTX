using System;

namespace Terraria.Combat;

/// <summary>Stores a target-wide damage protection window in simulation ticks.</summary>
public sealed class ProtectionWindowComponent
{
  public int RemainingTicks { get; private set; }

  public bool IsActive => RemainingTicks > 0;

  public void Start(int durationTicks)
  {
    if (durationTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationTicks));
    }

    RemainingTicks = Math.Max(RemainingTicks, durationTicks);
  }

  public void Clear()
  {
    RemainingTicks = 0;
  }

  public void Advance()
  {
    if (RemainingTicks > 0)
    {
      RemainingTicks--;
    }
  }
}
