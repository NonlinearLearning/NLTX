namespace Terraria.WorldInteraction.Wiring;

public readonly record struct WiringTraversalFlushResult(
  WiringTraversalFlushStatus Status,
  int PumpCommandsSubmitted,
  int TeleportCommandsSubmitted,
  bool PumpScratchReset,
  bool TeleportScratchReset,
  bool ScratchCleanupSucceeded,
  string? FailureReason)
{
  public bool Succeeded => Status is
    WiringTraversalFlushStatus.Empty or
    WiringTraversalFlushStatus.Accepted;

  public bool HasCommands =>
    PumpCommandsSubmitted != 0 || TeleportCommandsSubmitted != 0;
}
