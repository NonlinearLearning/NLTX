using System;

namespace Terraria.Dome.Simulation.Players;

public sealed class WellFedStateComponent
{
  public const int MaximumTimePerRank = 72000;

  public int TimeLeftRank1 { get; private set; }

  public int TimeLeftRank2 { get; private set; }

  public int TimeLeftRank3 { get; private set; }

  public int TimeLeft => TimeLeftRank1 + TimeLeftRank2 + TimeLeftRank3;

  public int Rank
  {
    get
    {
      if (TimeLeftRank3 > 0)
      {
        return 3;
      }

      if (TimeLeftRank2 > 0)
      {
        return 2;
      }

      if (TimeLeftRank1 > 0)
      {
        return 1;
      }

      return 0;
    }
  }

  public WellFedStateComponent(
    int timeLeftRank1 = 0,
    int timeLeftRank2 = 0,
    int timeLeftRank3 = 0)
  {
    Validate(timeLeftRank1);
    Validate(timeLeftRank2);
    Validate(timeLeftRank3);
    TimeLeftRank1 = timeLeftRank1;
    TimeLeftRank2 = timeLeftRank2;
    TimeLeftRank3 = timeLeftRank3;
  }

  public void Eat(int foodRank, int foodBuffTime)
  {
    if (foodRank < 0 || foodBuffTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(foodRank));
    }

    int timeLeftToAdd = foodBuffTime;
    if (foodRank >= 3)
    {
      TimeLeftRank3 = AddTime(TimeLeftRank3, ref timeLeftToAdd);
    }

    if (foodRank >= 2)
    {
      TimeLeftRank2 = AddTime(TimeLeftRank2, ref timeLeftToAdd);
    }

    if (foodRank >= 1)
    {
      TimeLeftRank1 = AddTime(TimeLeftRank1, ref timeLeftToAdd);
    }
  }

  public void Update()
  {
    if (TimeLeftRank3 > 0)
    {
      TimeLeftRank3--;
    }
    else if (TimeLeftRank2 > 0)
    {
      TimeLeftRank2--;
    }
    else if (TimeLeftRank1 > 0)
    {
      TimeLeftRank1--;
    }
  }

  public void Clear()
  {
    TimeLeftRank1 = 0;
    TimeLeftRank2 = 0;
    TimeLeftRank3 = 0;
  }

  private static int AddTime(int current, ref int remaining)
  {
    int amount = Math.Min(remaining, MaximumTimePerRank - current);
    remaining -= amount;
    return current + amount;
  }

  private static void Validate(int value)
  {
    if (value < 0 || value > MaximumTimePerRank)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }
  }
}
