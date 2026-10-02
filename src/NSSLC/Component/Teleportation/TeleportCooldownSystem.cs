using System;

namespace Terraria.Teleportation;

public sealed class TeleportCooldownSystem
{
  private TeleportCooldownStateComponent _state;

  public TeleportCooldownSystem(
    TeleportCooldownStateComponent initialState = default)
  {
    if (initialState.RemainingTicks < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(initialState),
        "Cooldown ticks cannot be negative.");
    }

    _state = initialState;
  }

  public TeleportCooldownSnapshot Snapshot()
  {
    return new TeleportCooldownSnapshot(
      _state.RemainingTicks,
      _state.Source,
      _state.StartedAtTick);
  }

  public TeleportCooldownStateComponent State => _state;

  public void Advance()
  {
    _state.RemainingTicks = Math.Max(0, _state.RemainingTicks - 1);
    if (_state.RemainingTicks == 0)
    {
      _state.Source = TeleportSource.None;
      _state.StartedAtTick = null;
    }
  }

  public bool Arm(
    int cooldownTicks,
    TeleportSource source,
    long? startedAtTick)
  {
    if (cooldownTicks < 0 || source == TeleportSource.None ||
      _state.IsOnCooldown)
    {
      return false;
    }

    _state.RemainingTicks = cooldownTicks;
    _state.Source = cooldownTicks == 0 ? TeleportSource.None : source;
    _state.StartedAtTick = cooldownTicks == 0 ? null : startedAtTick;
    return true;
  }

  public void Reset()
  {
    _state = default;
  }
}
