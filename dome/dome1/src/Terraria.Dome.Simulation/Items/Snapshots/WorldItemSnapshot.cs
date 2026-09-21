using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Items.Snapshots;

public sealed class WorldItemSnapshot
{
  public WorldItemSnapshot(
    int replicationId,
    ItemInstanceSnapshot instance,
    SimulationVector position,
    WorldSectionCoordinates section,
    ItemWorldStateComponent worldState,
    int timeSinceSpawnedTicks = 0)
  {
    if (replicationId <= 0 || !float.IsFinite(position.X) || !float.IsFinite(position.Y) ||
        timeSinceSpawnedTicks < 0 ||
        worldState.PickupDelayTicks < 0 || worldState.SpawnSource < 0 ||
        worldState.LastOwnerRevision < 0 || worldState.Revision < 0 ||
        worldState.ReservedPlayerId < 0 || worldState.ReservedPlayerId >
          ItemWorldStateComponent.UnreservedPlayerId || worldState.ReservationAgeTicks < -1)
    {
      throw new System.ArgumentOutOfRangeException(nameof(replicationId));
    }

    ReplicationId = replicationId;
    Instance = instance ?? throw new System.ArgumentNullException(nameof(instance));
    if (worldState.IsActive != !Instance.Stack.IsEmpty)
    {
      throw new System.ArgumentException(
        "World item activity must match whether the item instance is empty.");
    }

    Position = position;
    Section = section;
    WorldState = worldState;
    TimeSinceSpawnedTicks = timeSinceSpawnedTicks;
  }

  public int ReplicationId { get; }

  public ItemInstanceSnapshot Instance { get; }

  public SimulationVector Position { get; }

  public WorldSectionCoordinates Section { get; }

  public ItemWorldStateComponent WorldState { get; }

  public int TimeSinceSpawnedTicks { get; }
}
