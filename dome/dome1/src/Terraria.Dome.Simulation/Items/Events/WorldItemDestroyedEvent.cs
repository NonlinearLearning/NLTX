namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct WorldItemDestroyedEvent(int ReplicationId, long Revision);
