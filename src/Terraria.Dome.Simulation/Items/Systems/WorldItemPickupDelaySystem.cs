namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class WorldItemPickupDelaySystem
{
  public bool TryAdvance(
    WorldItemComponent current,
    out WorldItemComponent advanced)
  {
    if (!current.IsActive || current.WorldState.PickupDelayTicks <= 0)
    {
      advanced = current;
      return false;
    }

    long revision = checked(current.Revision + 1);
    advanced = current with
    {
      Revision = revision,
      WorldState = current.WorldState with
      {
        PickupDelayTicks = current.WorldState.PickupDelayTicks - 1,
        Revision = checked(current.WorldState.Revision + 1)
      }
    };
    return true;
  }
}
