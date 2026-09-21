namespace Terraria.WorldSession.Components;

public sealed class WorldTimeSkipStateComponent
{
  public bool FastForwardToDawn { get; internal set; }

  public int SundialCooldownTicks { get; internal set; }

  public bool FastForwardToDusk { get; internal set; }

  public int MoondialCooldownTicks { get; internal set; }

  internal bool TryRequestDawn()
  {
    if (SundialCooldownTicks != 0)
    {
      return false;
    }

    FastForwardToDawn = true;
    SundialCooldownTicks = 8;
    return true;
  }

  internal bool TryRequestDusk()
  {
    if (MoondialCooldownTicks != 0)
    {
      return false;
    }

    FastForwardToDusk = true;
    MoondialCooldownTicks = 8;
    return true;
  }

  internal void TickCooldowns()
  {
    if (SundialCooldownTicks > 0)
    {
      SundialCooldownTicks--;
    }

    if (MoondialCooldownTicks > 0)
    {
      MoondialCooldownTicks--;
    }
  }

  internal WorldTimeSkipSnapshot ConsumeSnapshot()
  {
    WorldTimeSkipSnapshot snapshot = new(
      FastForwardToDawn,
      SundialCooldownTicks,
      FastForwardToDusk,
      MoondialCooldownTicks);
    FastForwardToDawn = false;
    FastForwardToDusk = false;
    return snapshot;
  }
}
