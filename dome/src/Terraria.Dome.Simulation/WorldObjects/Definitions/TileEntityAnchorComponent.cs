using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TileEntityAnchorComponent(int TileX, int TileY)
{
  public WorldSectionCoordinates Section =>
    new(TileX / WorldGrid.SectionWidth, TileY / WorldGrid.SectionHeight);
}
