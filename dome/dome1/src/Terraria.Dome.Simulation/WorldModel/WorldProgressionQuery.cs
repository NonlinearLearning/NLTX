using System;

namespace Terraria.Dome.Simulation.WorldModel;

/// <summary>Server-only projections for Main world progression fields.</summary>
public static class WorldProgressionQuery
{
  public static bool IsHardMode(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.IsHardMode;
  }

  public static bool IsBloodMoon(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.IsBloodMoon;
  }

  public static bool IsEclipse(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.IsEclipse;
  }

  public static bool IsPumpkinMoon(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.IsPumpkinMoon;
  }

  public static bool IsSnowMoon(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.IsSnowMoon;
  }

  public static bool IsSlimeRaining(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.SlimeRainTimeTicks > 0;
  }

  public static int InvasionProgress(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return Math.Max(0, progression.InvasionSizeStart - progression.InvasionSize);
  }

  public static int InvasionProgressMax(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.InvasionSizeStart;
  }

  public static bool IsInvasionActive(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.InvasionType > 0 && progression.InvasionSize > 0;
  }

  public static bool IsInvasionWarningActive(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.InvasionWarningTicks > 0;
  }

  public static bool IsMeteorScheduled(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.IsMeteorScheduled;
  }

  public static bool IsLanternNight(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.IsLanternNight;
  }

  public static int SlimeRainKillCount(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.SlimeRainKillCount;
  }

  public static bool IsSlimeRainWarningActive(WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return progression.SlimeRainWarningTicks > 0;
  }
}
