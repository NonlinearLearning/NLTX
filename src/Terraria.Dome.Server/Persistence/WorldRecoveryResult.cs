namespace Terraria.Dome.Server.Persistence;

using Terraria.Dome.Simulation.WorldModel;

public sealed record WorldRecoveryResult(WorldGridSnapshot? Snapshot, string? FailureReason)
{
  public bool IsSuccess => Snapshot is not null;

  public static WorldRecoveryResult Failure(string failureReason)
  {
    return new WorldRecoveryResult(null, failureReason);
  }

  public static WorldRecoveryResult Success(WorldGridSnapshot snapshot)
  {
    return new WorldRecoveryResult(snapshot, null);
  }
}
