namespace MinimalImplementation.Persistence;

public enum WorldDocumentBackupStatus
{
  NotAttempted,
  Disabled,
  NoPreviousPrimary,
  Updated,
  SkippedInvalidPreviousPrimary,
  Rejected,
  OutcomeUnknown
}
