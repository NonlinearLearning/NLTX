namespace Terraria.Dome.Simulation;

public readonly record struct NpcHandle(int Value)
{
  public bool IsValid => Value > 0;
}
