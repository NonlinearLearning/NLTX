namespace Terraria.Dome.Simulation.WorldObjects.Sign.Commands;

public readonly record struct DeleteSignCommand(int SignId, long ExpectedRevision);
