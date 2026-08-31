using System;
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
  ItemWorldStateComponent WorldState = default)
{
  public void Validate()
  {
    if (ReplicationId <= 0 || Revision < 0 || !float.IsFinite(Position.X) ||
        !float.IsFinite(Position.Y) ||
        (Stack.IsEmpty && Stack != ItemStack.Empty) ||
        IsActive != !Stack.IsEmpty)
    {
      throw new ArgumentOutOfRangeException(nameof(ReplicationId));
    }

    InstanceState.Validate();
    if (WorldState.Revision == 0 && Revision > 0)
    {
      return;
    }

    if (WorldState.IsActive != IsActive || WorldState.PickupDelayTicks < 0 ||
        WorldState.SpawnSource < 0 || WorldState.LastOwnerRevision < 0 ||
        WorldState.Revision < 0 || WorldState.ReservedPlayerId < 0 ||
        WorldState.ReservedPlayerId > ItemWorldStateComponent.UnreservedPlayerId ||
        WorldState.ReservationAgeTicks < ItemWorldStateComponent.NoReservationAge)
    {
      throw new ArgumentOutOfRangeException(nameof(WorldState));
    }
  }
}
