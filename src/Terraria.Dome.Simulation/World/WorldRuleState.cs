using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed record WorldRuleState
{
  public WorldRuleState(
    int difficulty = 0,
    bool isExpertMode = false,
    bool isMasterMode = false,
    bool isCrimsonWorld = false,
    int rainTimeTicks = 0,
    float rainStrength = 0.0f)
  {
    if (difficulty < 0 || difficulty > 3)
    {
      throw new ArgumentOutOfRangeException(nameof(difficulty));
    }

    if (isMasterMode && !isExpertMode)
    {
      throw new ArgumentException(
        "Master mode requires expert mode.",
        nameof(isMasterMode));
    }

    if (rainTimeTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rainTimeTicks));
    }

    if (!float.IsFinite(rainStrength) || rainStrength < 0.0f || rainStrength > 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(rainStrength));
    }

    if (rainTimeTicks == 0 && rainStrength != 0.0f ||
        rainTimeTicks > 0 && rainStrength == 0.0f)
    {
      throw new ArgumentException("Rain duration and strength must be active together.");
    }

    Difficulty = difficulty;
    IsExpertMode = isExpertMode;
    IsMasterMode = isMasterMode;
    IsCrimsonWorld = isCrimsonWorld;
    RainTimeTicks = rainTimeTicks;
    RainStrength = rainStrength;
  }

  public int Difficulty { get; }
  public bool IsCrimsonWorld { get; }
  public bool IsExpertMode { get; }
  public bool IsRaining => RainTimeTicks > 0;
  public bool IsMasterMode { get; }
  public float RainStrength { get; }
  public int RainTimeTicks { get; }

  public WorldRuleState AdvanceRain(int ticksPerUpdate)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ticksPerUpdate);
    if (!IsRaining)
    {
      return this;
    }

    int remainingTicks = Math.Max(0, RainTimeTicks - ticksPerUpdate);
    return remainingTicks == 0
      ? WithRain(0, 0.0f)
      : WithRain(remainingTicks, RainStrength);
  }

  public WorldRuleState WithRain(int rainTimeTicks, float rainStrength)
  {
    return new WorldRuleState(
      Difficulty,
      IsExpertMode,
      IsMasterMode,
      IsCrimsonWorld,
      rainTimeTicks,
      rainStrength);
  }
}
