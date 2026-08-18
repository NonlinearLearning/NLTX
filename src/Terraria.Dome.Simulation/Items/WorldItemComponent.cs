using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Items;

public readonly record struct WorldItemComponent(
  int ReplicationId,
  ItemStack Stack,
  SimulationVector Position,
  bool IsActive,
  long Revision,
  WorldSectionCoordinates Section,
  ItemWorldStateComponent WorldState = default,
  ItemInstanceStateComponent InstanceState = default);
