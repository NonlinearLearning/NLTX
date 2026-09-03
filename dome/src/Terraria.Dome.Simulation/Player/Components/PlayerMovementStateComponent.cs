namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerMovementStateComponent
{
  public int FallStart;
  public int FallStart2;

  public void ResetFall()
  {
    FallStart = 0;
    FallStart2 = 0;
  }
}
