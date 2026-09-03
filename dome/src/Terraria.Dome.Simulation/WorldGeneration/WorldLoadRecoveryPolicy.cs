namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldLoadRecoveryPolicy
{
  public static WorldLoadRecoveryDecision Evaluate(
    bool initialLoadFailed,
    bool retryLoadFailed,
    bool backupExists,
    bool backupLoadFailed)
  {
    if (!initialLoadFailed)
    {
      return new WorldLoadRecoveryDecision(false, false, false, true);
    }

    if (!retryLoadFailed)
    {
      return new WorldLoadRecoveryDecision(true, false, false, true);
    }

    if (!backupExists)
    {
      return new WorldLoadRecoveryDecision(true, false, false, false);
    }

    return new WorldLoadRecoveryDecision(
      RetryPrimary: true,
      RestoreBackup: true,
      RetryBackup: true,
      LoadSucceeded: !backupLoadFailed);
  }
}
