using System;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class WorldItemSpawnSystem
{
  public bool TryCreate(
    ref int nextReplicationId,
    CreateWorldItemCommand command,
    out WorldItemComponent item,
    out ItemCommandRejection rejection)
  {
    if (nextReplicationId <= 0 || nextReplicationId == int.MaxValue ||
        command.Stack.IsEmpty ||
        command.SpawnSource < 0 ||
        command.PickupDelayTicks < 0 ||
        !float.IsFinite(command.Position.X) ||
        !float.IsFinite(command.Position.Y))
    {
      item = default;
      rejection = ItemCommandRejection.Invalid("World item creation input is invalid.");
      return false;
    }

    int replicationId = nextReplicationId;
    nextReplicationId = checked(nextReplicationId + 1);
    item = new WorldItemComponent(
      replicationId,
      command.Stack,
      command.Position,
      true,
      1,
      command.Section,
      ItemWorldStateComponent.Active(command.SpawnSource) with
      {
        PickupDelayTicks = command.PickupDelayTicks
      },
      command.InstanceState);
    rejection = default;
    return true;
  }
}
