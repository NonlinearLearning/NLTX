namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct DestroyWorldItemCommand(int ReplicationId, long ExpectedRevision);
