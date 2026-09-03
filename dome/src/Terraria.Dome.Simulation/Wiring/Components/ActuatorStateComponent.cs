namespace Terraria.Dome.Simulation.Wiring.Components;

public sealed class ActuatorStateComponent
{
  public bool IsActive { get; private set; }
  public long Revision { get; private set; }

  public void SetActive(bool isActive)
  {
    if (IsActive != isActive && Revision < long.MaxValue)
    {
      Revision++;
    }

    IsActive = isActive;
  }
}
