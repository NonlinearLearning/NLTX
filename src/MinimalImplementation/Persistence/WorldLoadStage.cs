namespace MinimalImplementation.Persistence;

public enum WorldLoadStage
{
  None,
  Read,
  Prepare,
  Commit,
  Abort,
  CandidateReady
}
