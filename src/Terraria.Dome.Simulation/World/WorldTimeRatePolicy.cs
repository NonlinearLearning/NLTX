using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldTimeRateInput
{
  public WorldTimeRateInput(
    bool isFastForwarding = false,
    bool isTimeFrozen = false,
    int targetRate = 1,
    int activePlayerCount = 0,
    int sleepingPlayerCount = 0,
    bool isGameMenu = false)
  {
    if (targetRate < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(targetRate));
    }

    if (activePlayerCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(activePlayerCount));
    }

    if (sleepingPlayerCount < 0 || sleepingPlayerCount > activePlayerCount)
    {
      throw new ArgumentOutOfRangeException(nameof(sleepingPlayerCount));
    }

    IsFastForwarding = isFastForwarding;
    IsTimeFrozen = isTimeFrozen;
    TargetRate = targetRate;
    ActivePlayerCount = activePlayerCount;
    SleepingPlayerCount = sleepingPlayerCount;
    IsGameMenu = isGameMenu;
  }

  public int ActivePlayerCount { get; }
  public bool IsFastForwarding { get; }
  public bool IsGameMenu { get; }
  public bool IsTimeFrozen { get; }
  public int SleepingPlayerCount { get; }
  public int TargetRate { get; }
}

public sealed class WorldTimeRatePolicy
{
  private const int FastForwardRate = 60;
  private const int MenuRate = 1;
  private const int SleepingMultiplier = 5;

  public WorldTimeRateSnapshot Resolve(WorldTimeRateInput input)
  {
    if (input.IsFastForwarding)
    {
      return new WorldTimeRateSnapshot(FastForwardRate);
    }

    int rate = input.TargetRate;
    if (input.ActivePlayerCount > 0 &&
        input.SleepingPlayerCount == input.ActivePlayerCount)
    {
      rate = checked(input.TargetRate * SleepingMultiplier);
    }

    if (input.IsTimeFrozen)
    {
      rate = 0;
    }

    if (input.IsGameMenu)
    {
      rate = MenuRate;
    }

    return new WorldTimeRateSnapshot(rate);
  }
}
