namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerEnvironmentContactComponent
{
  public int Breath;
  public int BreathMax;
  public int LavaTime;
  public int LavaMax;

  public PlayerEnvironmentContactComponent()
  {
    Breath = 200;
    BreathMax = 200;
    LavaTime = 0;
    LavaMax = 0;
  }

  public void RestoreBreath()
  {
    Breath = BreathMax;
  }

  public void SetBreathMax(int maximum)
  {
    if (maximum < 0)
    {
      throw new System.ArgumentOutOfRangeException(nameof(maximum));
    }

    BreathMax = maximum;
    if (Breath > maximum)
    {
      Breath = maximum;
    }
  }

  public void SetLavaMax(int maximum)
  {
    if (maximum < 0)
    {
      throw new System.ArgumentOutOfRangeException(nameof(maximum));
    }

    LavaMax = maximum;
    if (LavaTime > maximum)
    {
      LavaTime = maximum;
    }
  }
}
