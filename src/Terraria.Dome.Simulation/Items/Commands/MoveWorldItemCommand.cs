using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct MoveWorldItemCommand(
  int ReplicationId,
  SimulationVector Position,
  long ExpectedRevision);
