using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed record WorldProgressionState
{
  public WorldProgressionState(
    bool isHardMode = false,
    bool defeatedEyeOfCthulhu = false,
    bool defeatedEaterOrBrain = false,
    bool defeatedSkeletron = false,
    bool defeatedWallOfFlesh = false,
    bool defeatedMechanicalBoss = false,
    bool defeatedPlantera = false,
    bool defeatedGolem = false,
    bool isBloodMoon = false,
    bool isEclipse = false,
    int invasionType = 0,
    int invasionSize = 0,
    int slimeRainTimeTicks = 0)
  {
    if (invasionType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(invasionType));
    }

    if (invasionSize < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(invasionSize));
    }

    if (slimeRainTimeTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(slimeRainTimeTicks));
    }

    IsHardMode = isHardMode;
    DefeatedEyeOfCthulhu = defeatedEyeOfCthulhu;
    DefeatedEaterOrBrain = defeatedEaterOrBrain;
    DefeatedSkeletron = defeatedSkeletron;
    DefeatedWallOfFlesh = defeatedWallOfFlesh;
    DefeatedMechanicalBoss = defeatedMechanicalBoss;
    DefeatedPlantera = defeatedPlantera;
    DefeatedGolem = defeatedGolem;
    IsBloodMoon = isBloodMoon;
    IsEclipse = isEclipse;
    InvasionType = invasionType;
    InvasionSize = invasionSize;
    SlimeRainTimeTicks = slimeRainTimeTicks;
  }

  public bool DefeatedEaterOrBrain { get; }
  public bool DefeatedEyeOfCthulhu { get; }
  public bool DefeatedGolem { get; }
  public bool DefeatedMechanicalBoss { get; }
  public bool DefeatedPlantera { get; }
  public bool DefeatedSkeletron { get; }
  public bool DefeatedWallOfFlesh { get; }
  public int InvasionSize { get; }
  public int InvasionType { get; }
  public bool IsBloodMoon { get; }
  public bool IsEclipse { get; }
  public bool IsHardMode { get; }
  public bool IsSlimeRaining => SlimeRainTimeTicks > 0;
  public int SlimeRainTimeTicks { get; }

  public WorldProgressionState WithBloodMoon(bool isBloodMoon)
  {
    return new WorldProgressionState(
      IsHardMode,
      DefeatedEyeOfCthulhu,
      DefeatedEaterOrBrain,
      DefeatedSkeletron,
      DefeatedWallOfFlesh,
      DefeatedMechanicalBoss,
      DefeatedPlantera,
      DefeatedGolem,
      isBloodMoon,
      IsEclipse,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks);
  }

  public WorldProgressionState WithEclipse(bool isEclipse)
  {
    return new WorldProgressionState(
      IsHardMode,
      DefeatedEyeOfCthulhu,
      DefeatedEaterOrBrain,
      DefeatedSkeletron,
      DefeatedWallOfFlesh,
      DefeatedMechanicalBoss,
      DefeatedPlantera,
      DefeatedGolem,
      IsBloodMoon,
      isEclipse,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks);
  }

  public WorldProgressionState WithInvasion(int invasionType, int invasionSize)
  {
    return new WorldProgressionState(
      IsHardMode,
      DefeatedEyeOfCthulhu,
      DefeatedEaterOrBrain,
      DefeatedSkeletron,
      DefeatedWallOfFlesh,
      DefeatedMechanicalBoss,
      DefeatedPlantera,
      DefeatedGolem,
      IsBloodMoon,
      IsEclipse,
      invasionType,
      invasionSize,
      SlimeRainTimeTicks);
  }

  public WorldProgressionState WithSlimeRain(int slimeRainTimeTicks)
  {
    return new WorldProgressionState(
      IsHardMode,
      DefeatedEyeOfCthulhu,
      DefeatedEaterOrBrain,
      DefeatedSkeletron,
      DefeatedWallOfFlesh,
      DefeatedMechanicalBoss,
      DefeatedPlantera,
      DefeatedGolem,
      IsBloodMoon,
      IsEclipse,
      InvasionType,
      InvasionSize,
      slimeRainTimeTicks);
  }
}
