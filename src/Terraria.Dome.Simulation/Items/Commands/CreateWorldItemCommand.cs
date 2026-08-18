using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct CreateWorldItemCommand(
  ItemStack Stack,
  SimulationVector Position,
  WorldSectionCoordinates Section,
  int SpawnSource,
  ItemInstanceStateComponent InstanceState = default,
  int PickupDelayTicks = 0);
