using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Persistence;

public sealed record DomeStateRecoveryResult(
  DomeSimulationSnapshot? Snapshot,
  string? FailureReason)
{
  public bool IsSuccess => Snapshot is not null;

  public static DomeStateRecoveryResult Failure(string failureReason)
  {
    return new DomeStateRecoveryResult(null, failureReason);
  }

  public static DomeStateRecoveryResult Success(DomeSimulationSnapshot snapshot)
  {
    return new DomeStateRecoveryResult(snapshot, null);
  }
}
