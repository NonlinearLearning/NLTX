using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TrainingDummyStateComponent(
  TrainingDummyIdentityComponent Identity,
  TileEntityAnchorComponent Anchor,
  NpcHandle? Npc)
{
  public bool IsLinked => Npc is NpcHandle handle && handle.IsValid;

  public WorldSectionCoordinates Section => Anchor.Section;
}
