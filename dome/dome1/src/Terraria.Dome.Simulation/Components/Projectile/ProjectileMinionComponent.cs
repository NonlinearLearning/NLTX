namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileMinionComponent(float Slots, int Position)
{
  public float MinionSlots => Slots;

  public int MinionPosition => Position;
}
