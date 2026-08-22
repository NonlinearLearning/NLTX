using System;

namespace Terraria.Dome.Simulation.Combat.Components;

public struct HealthRegenerationComponent
{
  public const int DefaultDelayTicks = 2;
  public const int DefaultRegenUnitsPerTick = 60;
  public const int RegenUnitsPerHealthPoint = 120;

  public HealthRegenerationComponent(int delayTicks = DefaultDelayTicks)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(delayTicks);
    DelayTicks = delayTicks;
    RegenerationAccumulator = 0;
    EquipmentRegenUnitsPerTick = 0;
  }

  public int DelayTicks;
  public int EquipmentRegenUnitsPerTick;
  public int RegenerationAccumulator;

  public void SetEquipmentRegenUnitsPerTick(int regenUnitsPerTick)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(regenUnitsPerTick);
    EquipmentRegenUnitsPerTick = regenUnitsPerTick;
  }

  public void ResetDelay()
  {
    DelayTicks = DefaultDelayTicks;
    RegenerationAccumulator = 0;
  }
}
