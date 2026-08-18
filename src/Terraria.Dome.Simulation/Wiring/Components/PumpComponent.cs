using System;

namespace Terraria.Dome.Simulation.Wiring.Components;

public readonly record struct PumpComponent
{
  public PumpComponent(
    int mechanismId,
    int inputX,
    int inputY,
    int outputX,
    int outputY,
    byte capacity,
    int cooldownTicks)
  {
    if (mechanismId <= 0 || capacity == 0 || cooldownTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mechanismId));
    }

    MechanismId = mechanismId;
    InputX = inputX;
    InputY = inputY;
    OutputX = outputX;
    OutputY = outputY;
    Capacity = capacity;
    CooldownTicks = cooldownTicks;
  }

  public int MechanismId { get; }
  public int InputX { get; }
  public int InputY { get; }
  public int OutputX { get; }
  public int OutputY { get; }
  public byte Capacity { get; }
  public int CooldownTicks { get; }
}
