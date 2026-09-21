using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldRecoveryOutcome
{
  public WorldRecoveryOutcome(
    WorldRecoveryStatus status,
    byte[]? data,
    int attempts,
    FilePlatformFailure failure)
  {
    Status = status;
    Data = data is null ? null : data.ToArray();
    Attempts = attempts;
    Failure = failure;
  }

  public WorldRecoveryStatus Status { get; }

  public byte[]? Data { get; }

  public int Attempts { get; }

  public FilePlatformFailure Failure { get; }

  public bool CanPublishWorldLoaded =>
    Status is WorldRecoveryStatus.Loaded or WorldRecoveryStatus.RecoveredFromBackup;
}
