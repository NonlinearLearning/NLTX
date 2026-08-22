using System;

using Terraria.Dome.Simulation.WorldModel.Systems;

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
    bool isLanternNight = false,
    int invasionType = 0,
    int invasionSize = 0,
    int slimeRainTimeTicks = 0,
    bool isMeteorScheduled = false,
    int slimeRainCooldownTicks = 0,
    int slimeRainWarningTicks = 0,
    bool isNextNightLanternNight = false,
    int lanternNightCooldownTicks = 0,
    int invasionSizeStart = 0,
    int invasionDelayTicks = 0,
    double invasionX = 0,
    bool defeatedGoblins = false,
    bool defeatedFrost = false,
    bool defeatedPirates = false,
    bool defeatedMartians = false)
  {
    if (invasionType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(invasionType));
    }

    if (invasionSize < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(invasionSize));
    }

    if (invasionSizeStart < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(invasionSizeStart));
    }

    if (invasionDelayTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(invasionDelayTicks));
    }

    if (!double.IsFinite(invasionX))
    {
      throw new ArgumentOutOfRangeException(nameof(invasionX));
    }

    if (invasionSize > 0 && invasionSizeStart > 0 && invasionSizeStart < invasionSize)
    {
      throw new ArgumentException(
        "An active invasion cannot exceed its original size.",
        nameof(invasionSizeStart));
    }

    if (slimeRainTimeTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(slimeRainTimeTicks));
    }

    if (slimeRainCooldownTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(slimeRainCooldownTicks));
    }

    if (slimeRainWarningTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(slimeRainWarningTicks));
    }

    if (lanternNightCooldownTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lanternNightCooldownTicks));
    }

    if (slimeRainTimeTicks > 0 && slimeRainCooldownTicks > 0)
    {
      throw new ArgumentException(
        "Slime Rain cannot be active while its cooldown is running.",
        nameof(slimeRainCooldownTicks));
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
    IsLanternNight = isLanternNight;
    InvasionType = invasionType;
    InvasionSize = invasionSize;
    InvasionSizeStart = invasionSizeStart;
    InvasionDelayTicks = invasionDelayTicks;
    InvasionX = invasionX;
    DefeatedGoblins = defeatedGoblins;
    DefeatedFrost = defeatedFrost;
    DefeatedPirates = defeatedPirates;
    DefeatedMartians = defeatedMartians;
    SlimeRainTimeTicks = slimeRainTimeTicks;
    IsMeteorScheduled = isMeteorScheduled;
    SlimeRainCooldownTicks = slimeRainCooldownTicks;
    SlimeRainWarningTicks = slimeRainWarningTicks;
    IsNextNightLanternNight = isNextNightLanternNight;
    LanternNightCooldownTicks = lanternNightCooldownTicks;
  }

  public bool DefeatedEaterOrBrain { get; }
  public bool DefeatedEyeOfCthulhu { get; }
  public bool DefeatedFrost { get; }
  public bool DefeatedGoblins { get; }
  public bool DefeatedGolem { get; }
  public bool DefeatedMartians { get; }
  public bool DefeatedMechanicalBoss { get; }
  public bool DefeatedPirates { get; }
  public bool DefeatedPlantera { get; }
  public bool DefeatedSkeletron { get; }
  public bool DefeatedWallOfFlesh { get; }
  public int InvasionSize { get; }
  public int InvasionSizeStart { get; }
  public int InvasionDelayTicks { get; }
  public int InvasionType { get; }
  public double InvasionX { get; }
  public bool IsBloodMoon { get; }
  public bool IsEclipse { get; }
  public bool IsHardMode { get; }
  public bool IsLanternNight { get; }
  public bool IsMeteorScheduled { get; }
  public bool IsNextNightLanternNight { get; }
  public int LanternNightCooldownTicks { get; }
  public bool IsSlimeRainCoolingDown => SlimeRainCooldownTicks > 0;
  public bool IsSlimeRaining => SlimeRainTimeTicks > 0;
  public int SlimeRainCooldownTicks { get; }
  public int SlimeRainTimeTicks { get; }
  public int SlimeRainWarningTicks { get; }

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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: InvasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: InvasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
  }

  public WorldProgressionState WithInvasion(int invasionType, int invasionSize)
  {
    int invasionSizeStart = invasionType == 0 || invasionSize == 0
      ? 0
      : InvasionType == 0
        ? invasionSize
        : InvasionSizeStart;
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
      IsLanternNight,
      invasionType,
      invasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: invasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
  }

  public WorldProgressionState WithInvasionSizeStart(int invasionSizeStart)
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart,
      InvasionDelayTicks,
      InvasionX);
  }

  public WorldProgressionState WithInvasionDelayTicks(int invasionDelayTicks)
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      InvasionSizeStart,
      invasionDelayTicks,
      InvasionX);
  }

  public WorldProgressionState WithInvasionX(double invasionX)
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      InvasionSizeStart,
      InvasionDelayTicks,
      invasionX,
      DefeatedGoblins,
      DefeatedFrost,
      DefeatedPirates,
      DefeatedMartians);
  }

  public WorldProgressionState WithInvasionClearFlag(WorldInvasionClearFlag clearFlag)
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      InvasionSizeStart,
      InvasionDelayTicks,
      InvasionX,
      DefeatedGoblins || clearFlag == WorldInvasionClearFlag.Goblins,
      DefeatedFrost || clearFlag == WorldInvasionClearFlag.Frost,
      DefeatedPirates || clearFlag == WorldInvasionClearFlag.Pirates,
      DefeatedMartians || clearFlag == WorldInvasionClearFlag.Martians);
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      slimeRainTimeTicks,
      IsMeteorScheduled,
      0,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: InvasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
  }

  public WorldProgressionState WithLanternNight(bool isLanternNight)
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
      isLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: InvasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
  }

  public WorldProgressionState WithMeteorScheduled(bool isMeteorScheduled)
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      isMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: InvasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
  }

  public WorldProgressionState WithSlimeRainCooldown(int cooldownTicks)
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      cooldownTicks,
      SlimeRainWarningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: InvasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
  }

  public WorldProgressionState WithSlimeRainWarning(int warningTicks)
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      warningTicks,
      IsNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: InvasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
  }

  public WorldProgressionState WithNextNightLanternNight(bool isNextNightLanternNight)
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
      IsLanternNight,
      InvasionType,
      InvasionSize,
      SlimeRainTimeTicks,
      IsMeteorScheduled,
      SlimeRainCooldownTicks,
      SlimeRainWarningTicks,
      isNextNightLanternNight,
      LanternNightCooldownTicks,
      invasionSizeStart: InvasionSizeStart,
      invasionDelayTicks: InvasionDelayTicks,
      invasionX: InvasionX,
      defeatedGoblins: DefeatedGoblins,
      defeatedFrost: DefeatedFrost,
      defeatedPirates: DefeatedPirates,
      defeatedMartians: DefeatedMartians);
  }
}
