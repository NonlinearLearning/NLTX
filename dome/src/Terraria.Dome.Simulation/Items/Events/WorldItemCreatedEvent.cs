using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct WorldItemCreatedEvent(
  int ReplicationId,
  ItemStack Stack,
  SimulationVector Position,
  long Revision);
