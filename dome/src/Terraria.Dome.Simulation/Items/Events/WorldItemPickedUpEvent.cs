using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct WorldItemPickedUpEvent(
  int ReplicationId,
  PlayerHandle Player,
  int AcceptedQuantity,
  long Revision);
