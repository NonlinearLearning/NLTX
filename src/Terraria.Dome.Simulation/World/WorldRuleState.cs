using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed record WorldRuleState
{
  private const float MaximumWindSpeedCurrent = 1.25f;

  public WorldRuleState(
    int difficulty = 0,
    bool isExpertMode = false,
    bool isMasterMode = false,
    bool isCrimsonWorld = false,
    int rainTimeTicks = 0,
    float rainStrength = 0.0f,
    float windSpeedTarget = 0.0f,
    float windSpeedCurrent = 0.0f,
    WorldGameMode gameMode = WorldGameMode.Classic,
    bool? isRaining = null,
    float? maximumRainStrength = null)
  {
    if (difficulty < 0 || difficulty > 3)
    {
      throw new ArgumentOutOfRangeException(nameof(difficulty));
    }

    if (!Enum.IsDefined(gameMode))
    {
      throw new ArgumentOutOfRangeException(nameof(gameMode));
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

    if (!float.IsFinite(windSpeedTarget) || MathF.Abs(windSpeedTarget) > 0.8f)
    {
      throw new ArgumentOutOfRangeException(nameof(windSpeedTarget));
    }

    if (!float.IsFinite(windSpeedCurrent) || MathF.Abs(windSpeedCurrent) > MaximumWindSpeedCurrent)
    {
      throw new ArgumentOutOfRangeException(nameof(windSpeedCurrent));
    }

    float effectiveMaximumRainStrength = maximumRainStrength ?? rainStrength;
    if (!float.IsFinite(effectiveMaximumRainStrength) || effectiveMaximumRainStrength < 0.0f ||
        effectiveMaximumRainStrength > 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumRainStrength));
    }

    Difficulty = difficulty;
    IsExpertMode = isExpertMode;
    IsMasterMode = isMasterMode;
    IsCrimsonWorld = isCrimsonWorld;
    RainTimeTicks = rainTimeTicks;
    RainStrength = rainStrength;
    IsRaining = isRaining ?? rainTimeTicks > 0;
    MaximumRainStrength = effectiveMaximumRainStrength;
    WindSpeedTarget = windSpeedTarget;
    WindSpeedCurrent = windSpeedCurrent;
    GameMode = gameMode;
  }

  public int Difficulty { get; }
  public WorldGameMode GameMode { get; }
  public bool IsCrimsonWorld { get; }
  public bool IsExpertMode { get; }
  public bool IsJourneyMode => GameMode == WorldGameMode.Journey;
  public bool IsRaining { get; }
  public bool IsMasterMode { get; }
  public float RainStrength { get; }
  public int RainTimeTicks { get; }
  public float MaximumRainStrength { get; }
  public float WindSpeedCurrent { get; }
  public float WindSpeedTarget { get; }

  public WorldRuleState AdvanceRain(int ticksPerUpdate)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ticksPerUpdate);
    if (!IsRaining)
    {
      return this;
    }

    int remainingTicks = Math.Max(0, RainTimeTicks - ticksPerUpdate);
    if (remainingTicks == 0)
    {
      return WithRain(0, 0.0f);
    }

    return new WorldRuleState(
      Difficulty,
      IsExpertMode,
      IsMasterMode,
      IsCrimsonWorld,
      remainingTicks,
      RainStrength,
      WindSpeedTarget,
      WindSpeedCurrent,
      GameMode,
      isRaining: true,
      maximumRainStrength: MaximumRainStrength);
  }

  public WorldRuleState WithRain(int rainTimeTicks, float rainStrength)
  {
    return new WorldRuleState(
      Difficulty,
      IsExpertMode,
      IsMasterMode,
      IsCrimsonWorld,
      rainTimeTicks,
      rainStrength,
      WindSpeedTarget,
      WindSpeedCurrent,
      GameMode,
      isRaining: rainTimeTicks > 0,
      maximumRainStrength: MaximumRainStrength);
  }

  public WorldRuleState WithRawRain(bool isRaining, float maximumRainStrength)
  {
    return new WorldRuleState(
      Difficulty,
      IsExpertMode,
      IsMasterMode,
      IsCrimsonWorld,
      RainTimeTicks,
      RainStrength,
      WindSpeedTarget,
      WindSpeedCurrent,
      GameMode,
      isRaining,
      maximumRainStrength);
  }

  public WorldRuleState WithGameMode(WorldGameMode gameMode)
  {
    return new WorldRuleState(
      Difficulty,
      IsExpertMode,
      IsMasterMode,
      IsCrimsonWorld,
      RainTimeTicks,
      RainStrength,
      WindSpeedTarget,
      WindSpeedCurrent,
      gameMode,
      IsRaining,
      MaximumRainStrength);
  }

  public WorldRuleState AdvanceWind(int ticksPerUpdate, float rainStrength = 0.0f)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ticksPerUpdate);
    if (!float.IsFinite(rainStrength) || rainStrength < 0.0f || rainStrength > 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(rainStrength));
    }

    float effectiveTarget = WindSpeedTarget * (1.0f + 5.0f / 9.0f * rainStrength);
    float current = WindSpeedCurrent;
    for (int index = 0; index < ticksPerUpdate; index++)
    {
      float distance = effectiveTarget - current;
      if (distance == 0.0f)
      {
        break;
      }

      float step = 0.0003f + MathF.Abs(distance) * 0.0015f;
      current += MathF.Sign(distance) * MathF.Min(step, MathF.Abs(distance));
    }

    return WithWind(WindSpeedTarget, current);
  }

  public WorldRuleState WithWind(float windSpeedTarget, float windSpeedCurrent)
  {
    return new WorldRuleState(
      Difficulty,
      IsExpertMode,
      IsMasterMode,
      IsCrimsonWorld,
      RainTimeTicks,
      RainStrength,
      windSpeedTarget,
      windSpeedCurrent,
      GameMode,
      IsRaining,
      MaximumRainStrength);
  }
}
