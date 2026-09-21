using Arch.Core;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Leash;

public struct LeashAnchorStateComponent
{
  public int TileX;
  public int TileY;
  public Entity? AnchorEntity;
  public int AnchorStyle;
  public bool Active;

  public WorldSectionCoordinates SectionCoordinates =>
    new(TileX / WorldGrid.SectionWidth, TileY / WorldGrid.SectionHeight);
}
