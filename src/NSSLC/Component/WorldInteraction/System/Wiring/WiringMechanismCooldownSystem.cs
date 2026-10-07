using System;
using System.Collections.Generic;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

// Advances timer state and returns device intents. Tile-specific effects remain integration-owned.
public sealed class WiringMechanismCooldownSystem
{
  private readonly WiringMechanismScheduleComponent _schedule;
  private readonly WiringDeviceCooldownComponent _deviceCooldowns;

  public WiringMechanismCooldownSystem(
    WiringMechanismScheduleComponent schedule,
    WiringDeviceCooldownComponent deviceCooldowns)
  {
    ArgumentNullException.ThrowIfNull(schedule);
    ArgumentNullException.ThrowIfNull(deviceCooldowns);
    _schedule = schedule;
    _deviceCooldowns = deviceCooldowns;
  }

  public WiringMechanismScheduleResult Schedule(
    TileCoordinate position,
    int remainingTicks,
    int worldWidth,
    int worldHeight)
  {
    ValidateWorldDimensions(worldWidth, worldHeight);
    if (remainingTicks < 0)
    {
      return WiringMechanismScheduleResult.Rejected("negative-timer");
    }

    if (!IsWithinBounds(position, worldWidth, worldHeight))
    {
      return WiringMechanismScheduleResult.Rejected("bounds");
    }

    if (_schedule.ContainsPosition(position))
    {
      return WiringMechanismScheduleResult.Rejected("duplicate");
    }

    return _schedule.TrySchedule(new MechanismCooldownEntry(position, remainingTicks))
      ? WiringMechanismScheduleResult.Accepted
      : WiringMechanismScheduleResult.Rejected("capacity");
  }

  public WiringMechanismAdvanceResult Advance(
    int worldWidth,
    int worldHeight)
  {
    ValidateWorldDimensions(worldWidth, worldHeight);
    _deviceCooldowns.AdvanceOneTick();

    List<MechanismCooldownEntry> readyEntries = new();
    int removedInvalidEntries = 0;
    for (int index = _schedule.Count - 1; index >= 0; index--)
    {
      MechanismCooldownEntry entry = _schedule.GetEntry(index);
      if (!IsWithinBounds(entry.Position, worldWidth, worldHeight))
      {
        _schedule.RemoveAt(index);
        removedInvalidEntries++;
        continue;
      }

      int remainingTicks = entry.RemainingTicks;
      if (remainingTicks > 0)
      {
        remainingTicks--;
      }

      if (remainingTicks == 0)
      {
        readyEntries.Add(new MechanismCooldownEntry(entry.Position, 0));
        _schedule.RemoveAt(index);
      }
      else
      {
        _schedule.SetEntry(
          index,
          new MechanismCooldownEntry(entry.Position, remainingTicks));
      }
    }

    readyEntries.Reverse();
    return new WiringMechanismAdvanceResult(
      readyEntries,
      removedInvalidEntries,
      _deviceCooldowns.CannonCooldownTicks,
      _deviceCooldowns.BunnyCannonCooldownTicks,
      _deviceCooldowns.SnowballCannonCooldownTicks);
  }

  public void Reset()
  {
    _schedule.Reset();
    _deviceCooldowns.Reset();
  }

  private static bool IsWithinBounds(
    TileCoordinate position,
    int worldWidth,
    int worldHeight)
  {
    return position.X >= 0 && position.X < worldWidth &&
      position.Y >= 0 && position.Y < worldHeight;
  }

  private static void ValidateWorldDimensions(int worldWidth, int worldHeight)
  {
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (worldHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }
  }
}
