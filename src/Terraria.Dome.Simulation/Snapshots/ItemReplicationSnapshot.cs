using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation;

public readonly record struct ItemReplicationSnapshot(
  int ReplicationId,
  ItemStack Stack,
  SimulationVector Position,
  bool IsActive,
  long Revision,
  WorldSectionCoordinates Section,
  ItemInstanceStateComponent InstanceState = default,
  ItemWorldStateComponent WorldState = default);
