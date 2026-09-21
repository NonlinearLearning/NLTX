using System;
using Terraria.Dome.Simulation.Player.Definitions;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerMountStateComponent
{
  public bool IsMounted { get; private set; }

  public int MountType { get; private set; }

  public int FlightTimeRemaining { get; private set; }

  public int FatigueRemaining { get; private set; }

  public int FlightFrameState { get; private set; }

  public bool IsHoverActive =>
    IsMounted && MountCapabilityRegistry.CanUseHoverFrame(MountType, FlightFrameState);

  public bool IsFlightExhausted => IsMounted && FlightTimeRemaining == 0;

  public bool IsFatigueExhausted => IsMounted && FatigueRemaining == 0;

  public PlayerMountStateComponent()
  {
    IsMounted = false;
    MountType = -1;
    FlightTimeRemaining = 0;
    FatigueRemaining = 0;
    FlightFrameState = 0;
  }

  public void Set(int mountType)
  {
    if (!MountTypeRegistry.IsValid(mountType))
    {
      throw new ArgumentOutOfRangeException(nameof(mountType));
    }

    IsMounted = mountType >= 0;
    MountType = IsMounted ? mountType : -1;
    FlightTimeRemaining = IsMounted
      ? MountCapabilityRegistry.GetFlightTimeMax(mountType)
      : 0;
    FatigueRemaining = IsMounted
      ? MountCapabilityRegistry.GetFatigueMax(mountType)
      : 0;
    FlightFrameState = 0;
  }

  public void SetFlightFrameState(int frameState)
  {
    if (frameState < 0 || frameState > 4)
    {
      throw new ArgumentOutOfRangeException(nameof(frameState));
    }

    FlightFrameState = frameState;
  }

  public bool ConsumeFlightTime(bool isFlying)
  {
    if (!IsMounted || !isFlying || FlightTimeRemaining <= 0)
    {
      return false;
    }

    FlightTimeRemaining--;
    return true;
  }

  public bool TryConsumeFlightInput(bool upPressed)
  {
    if (!upPressed || !IsMounted ||
        !MountCapabilityRegistry.CanFly(MountType) ||
        (!MountCapabilityRegistry.CanUseWings(MountType) &&
         !MountCapabilityRegistry.UsesHover(MountType)) ||
        FlightTimeRemaining <= 0 || FatigueRemaining <= 0)
    {
      return false;
    }

    return ConsumeFlightTime(true) && ConsumeFatigue(true);
  }

  public bool TryConsumeFlightInput(bool upPressed, int frameState)
  {
    if (!MountCapabilityRegistry.CanUseFlightFrame(MountType, frameState))
    {
      return false;
    }

    return TryConsumeFlightInput(upPressed);
  }

  public void RestoreFlightTime()
  {
    FlightTimeRemaining = IsMounted
      ? MountCapabilityRegistry.GetFlightTimeMax(MountType)
      : 0;
  }

  public void RechargeFlightTime(int amount)
  {
    if (amount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    int maximum = IsMounted ? MountCapabilityRegistry.GetFlightTimeMax(MountType) : 0;
    FlightTimeRemaining = Math.Min(maximum, checked(FlightTimeRemaining + amount));
  }

  public bool ConsumeFatigue(bool isFatiguing)
  {
    if (!IsMounted || !isFatiguing || FatigueRemaining <= 0)
    {
      return false;
    }

    FatigueRemaining--;
    return true;
  }

  public void RestoreFatigue()
  {
    FatigueRemaining = IsMounted
      ? MountCapabilityRegistry.GetFatigueMax(MountType)
      : 0;
  }

  public void RecoverFatigue(int amount)
  {
    if (amount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    int maximum = IsMounted ? MountCapabilityRegistry.GetFatigueMax(MountType) : 0;
    FatigueRemaining = Math.Min(maximum, checked(FatigueRemaining + amount));
  }
}
