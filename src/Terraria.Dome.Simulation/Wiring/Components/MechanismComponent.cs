using System;

namespace Terraria.Dome.Simulation.Wiring.Components;

public sealed class MechanismComponent
{
  public MechanismComponent(int mechanismId, MechanismType type, int cooldownTicks = 0)
  {
    if (mechanismId <= 0 || cooldownTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mechanismId));
    }

    MechanismId = mechanismId;
    Type = type;
    CooldownTicks = cooldownTicks;
  }

  public int CooldownTicks { get; }
  public bool IsActive { get; private set; }
  public int MechanismId { get; }
  public long LastActivationSequence { get; private set; } = -1;
  public MechanismType Type { get; }

  public bool TryActivate(long sequence)
  {
    if (sequence < 0 || sequence <= LastActivationSequence)
    {
      return false;
    }

    LastActivationSequence = sequence;
    IsActive = !IsActive;
    return true;
  }
}
