using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Events;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class WorldItemDestroySystem
{
  public bool TryDestroy(
    WorldItemComponent current,
    DestroyWorldItemCommand command,
    out WorldItemComponent destroyed,
    out WorldItemDestroyedEvent destroyedEvent)
  {
    if (!current.IsActive || current.ReplicationId != command.ReplicationId ||
        current.Revision != command.ExpectedRevision)
    {
      destroyed = current;
      destroyedEvent = default;
      return false;
    }

    long revision = checked(current.Revision + 1);
    destroyed = current with
    {
      Stack = ItemStack.Empty,
      IsActive = false,
      Revision = revision,
      WorldState = current.WorldState with
      {
        IsActive = false,
        Revision = checked(current.WorldState.Revision + 1)
      },
      InstanceState = default
    };
    destroyedEvent = new WorldItemDestroyedEvent(current.ReplicationId, revision);
    return true;
  }
}
