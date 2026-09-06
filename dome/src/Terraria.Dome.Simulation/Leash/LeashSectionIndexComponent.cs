using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Leash;

public struct LeashSectionIndexComponent
{
  public WorldSectionCoordinates SectionCoordinates;
  public int Slot;
  public bool IsSectionActive;
  public long? LastActivationTick;
}
