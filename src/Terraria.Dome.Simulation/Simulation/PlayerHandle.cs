namespace Terraria.Dome.Simulation;

public readonly record struct PlayerHandle(int Value)
{
  public bool IsValid => Value > 0;
}
