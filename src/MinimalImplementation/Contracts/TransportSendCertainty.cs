namespace MinimalImplementation.Contracts;

public enum TransportSendCertainty
{
  NotSubmitted,
  Queued,
  LocallySent,
  OutcomeUnknown,
  Failed
}
