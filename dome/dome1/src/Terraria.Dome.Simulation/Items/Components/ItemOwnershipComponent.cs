using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Components;

public readonly record struct ItemOwnershipComponent(
  PlayerHandle? Player,
  int? ContainerId,
  int SourceEntityId,
  long OwnershipRevision);
