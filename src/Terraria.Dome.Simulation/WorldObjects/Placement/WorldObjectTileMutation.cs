using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public readonly record struct WorldObjectTileMutation(
  int X,
  int Y,
  WorldTile Expected,
  WorldTile Replacement)
{
  public bool IsValid => X >= 0 && Y >= 0;
}
