namespace MinimalImplementation.Persistence;

public enum WorldStopStage
{
  None,
  StopAdmissions,
  StopTickAdmission,
  DrainAcceptedCommands,
  Save,
  CloseTransport,
  RetireWorldAndResources,
  StopCommitLoop,
  Stopped
}

public sealed record WorldStopResult(
  bool IsStopped,
  bool SaveRequested,
  WorldDocumentSaveResult? SaveResult,
  WorldStopStage FailedStage,
  string? FailureDetail);
