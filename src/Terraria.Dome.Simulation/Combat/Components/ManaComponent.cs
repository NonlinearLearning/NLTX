using System;

namespace Terraria.Dome.Simulation.Combat.Components;

public struct ManaComponent
{
  public ManaComponent(int current, int maximum, int regenerationDelayTicks = 0)
  {
    if (maximum < 0 || current < 0 || current > maximum || regenerationDelayTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(current));
    }

    Current = current;
    Maximum = maximum;
    EquipmentIncrease = 0;
    RegenerationDelayTicks = regenerationDelayTicks;
    RegenerationAccumulator = 0;
  }

  public int Current;
  public int Maximum;
  public int EquipmentIncrease;
  public int RegenerationAccumulator;
  public int RegenerationDelayTicks;
}
