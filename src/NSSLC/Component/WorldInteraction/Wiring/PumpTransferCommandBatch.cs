using System.Collections.Generic;

namespace Terraria.WorldInteraction.Wiring;

public readonly record struct PumpTransferCommandBatch(
  PumpTransferCommandBatchStatus Status,
  IReadOnlyList<PumpTransferCommand> Commands,
  string? FailureReason)
{
  public bool HasCommands => Commands.Count != 0;

  public bool IsEmpty => Status == PumpTransferCommandBatchStatus.Empty;

  public bool IsRejected => Status == PumpTransferCommandBatchStatus.Rejected;

  public static PumpTransferCommandBatch Empty(string reason)
  {
    return new(
      PumpTransferCommandBatchStatus.Empty,
      Array.Empty<PumpTransferCommand>(),
      reason);
  }

  public static PumpTransferCommandBatch Accepted(
    IReadOnlyList<PumpTransferCommand> commands)
  {
    return new(
      PumpTransferCommandBatchStatus.Accepted,
      commands,
      null);
  }

  public static PumpTransferCommandBatch Rejected(string reason)
  {
    return new(
      PumpTransferCommandBatchStatus.Rejected,
      Array.Empty<PumpTransferCommand>(),
      reason);
  }
}
