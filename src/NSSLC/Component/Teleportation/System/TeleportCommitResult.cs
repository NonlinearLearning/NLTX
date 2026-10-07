using System;

namespace Terraria.Teleportation;

public readonly record struct TeleportCommitResult(
  TeleportCommitStatus Status,
  Guid CommandId,
  TeleportCommitReason Reason,
  string? FailureReason)
{
  public bool Succeeded => Status == TeleportCommitStatus.Accepted;

  public bool WasDuplicate => Status == TeleportCommitStatus.Duplicate;

  public static TeleportCommitResult Rejected(
    Guid commandId,
    TeleportCommitReason reason,
    string? failureReason = null)
  {
    return new(
      TeleportCommitStatus.Rejected,
      commandId,
      reason,
      failureReason);
  }

  public static TeleportCommitResult Accepted(Guid commandId)
  {
    return new(
      TeleportCommitStatus.Accepted,
      commandId,
      TeleportCommitReason.None,
      null);
  }

  public static TeleportCommitResult Duplicate(Guid commandId)
  {
    return new(
      TeleportCommitStatus.Duplicate,
      commandId,
      TeleportCommitReason.DuplicateCommand,
      null);
  }

  public static TeleportCommitResult Unknown(
    Guid commandId,
    string? failureReason)
  {
    return new(
      TeleportCommitStatus.Unknown,
      commandId,
      TeleportCommitReason.CommitPortUnknown,
      failureReason);
  }
}
