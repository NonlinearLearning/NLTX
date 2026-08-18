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
    ItemWorldStateComponent worldState)
  {
    if (replicationId <= 0)
    {
      throw new System.ArgumentOutOfRangeException(nameof(replicationId));
    }

    ReplicationId = replicationId;
    Instance = instance ?? throw new System.ArgumentNullException(nameof(instance));
    Position = position;
    Section = section;
    WorldState = worldState;
  }

  public int ReplicationId { get; }

  public ItemInstanceSnapshot Instance { get; }

  public SimulationVector Position { get; }

  public WorldSectionCoordinates Section { get; }

  public ItemWorldStateComponent WorldState { get; }
}
