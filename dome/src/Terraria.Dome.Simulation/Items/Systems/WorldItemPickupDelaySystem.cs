using System;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class WorldItemPickupDelaySystem
{
  public bool TryAdvance(
    WorldItemComponent current,
    out WorldItemComponent advanced)
  {
    if (!current.IsActive || current.TimeSinceSpawnedTicks == int.MaxValue ||
        current.Revision == long.MaxValue ||
        current.WorldState.Revision == long.MaxValue)
    {
      advanced = current;
      return false;
    }

    long revision = checked(current.Revision + 1);
    advanced = current with
    {
      Revision = revision,
      TimeSinceSpawnedTicks = checked(current.TimeSinceSpawnedTicks + 1),
      WorldState = current.WorldState with
      {
        PickupDelayTicks = Math.Max(0, current.WorldState.PickupDelayTicks - 1),
        Revision = checked(current.WorldState.Revision + 1)
      }
    };
    return true;
  }
}
