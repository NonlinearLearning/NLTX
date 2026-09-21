using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation;

public readonly record struct WorldItemSnapshot(
  int ReplicationId,
  ItemStack Stack,
  SimulationVector Position,
  bool IsActive,
  ItemInstanceStateComponent InstanceState = default,
  int TimeSinceSpawnedTicks = 0);
