namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileTileCollisionComponent(bool Enabled = true)
{
  public bool TileCollide => Enabled;
}
