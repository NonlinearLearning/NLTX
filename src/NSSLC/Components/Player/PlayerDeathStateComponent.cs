using System;

namespace Terraria.Player;

/// <summary>Stores the authoritative death transition for a player entity.</summary>
public sealed class PlayerDeathStateComponent
{
  public bool IsDead { get; private set; }

  public long DeathTick { get; private set; } = -1;

  public long CauseId { get; private set; }

  public bool MarkDead(long tick, long causeId = 0)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (IsDead)
    {
      return false;
    }

    IsDead = true;
    DeathTick = tick;
    CauseId = causeId;
    return true;
  }

  public bool Clear()
  {
    if (!IsDead)
    {
      return false;
    }

    IsDead = false;
    DeathTick = -1;
    CauseId = 0;
    return true;
  }
}
