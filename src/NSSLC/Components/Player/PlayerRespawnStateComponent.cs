using System;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>Stores a server-authoritative respawn schedule and optional spawn anchor.</summary>
public sealed class PlayerRespawnStateComponent
{
  public PlayerRespawnStateComponent(int delayTicks)
  {
    if (delayTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(delayTicks));
    }

    DelayTicks = delayTicks;
  }

  public int DelayTicks { get; private set; }

  public int RemainingTicks { get; private set; }

  public long RequestTick { get; private set; } = -1;

  public EntityUuid SpawnAnchorId { get; private set; }

  public bool IsScheduled => RequestTick >= 0;

  public void Schedule(long tick, EntityUuid spawnAnchorId = default)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    RequestTick = tick;
    RemainingTicks = DelayTicks;
    SpawnAnchorId = spawnAnchorId;
  }

  public bool Advance()
  {
    if (!IsScheduled)
    {
      return false;
    }

    if (RemainingTicks > 0)
    {
      RemainingTicks--;
    }

    return RemainingTicks == 0;
  }

  public void Cancel()
  {
    RequestTick = -1;
    RemainingTicks = 0;
    SpawnAnchorId = default;
  }

  public void ConfigureDelay(int delayTicks)
  {
    if (delayTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(delayTicks));
    }

    DelayTicks = delayTicks;
  }
}
