using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class WorldItemMotionSystem
{
  public bool TryMove(
    WorldItemComponent current,
    MoveWorldItemCommand command,
    WorldSectionCoordinates section,
    out WorldItemComponent moved)
  {
    if (!current.IsActive || current.ReplicationId != command.ReplicationId ||
        current.Revision != command.ExpectedRevision ||
        current.Revision == long.MaxValue ||
        current.WorldState.Revision == long.MaxValue ||
        !float.IsFinite(command.Position.X) ||
        !float.IsFinite(command.Position.Y))
    {
      moved = current;
      return false;
    }

    moved = current with
    {
      Position = command.Position,
      Section = section,
      Revision = checked(current.Revision + 1),
      WorldState = current.WorldState with { Revision = checked(current.WorldState.Revision + 1) }
    };
    return true;
  }
}
