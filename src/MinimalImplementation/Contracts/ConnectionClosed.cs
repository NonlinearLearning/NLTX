namespace MinimalImplementation.Contracts;

public sealed record ConnectionClosed
{
  public SessionEpoch Epoch { get; }
  public string Reason { get; }
  public PacketSequence LastReceivedSequence { get; }
  public TransportSendCertainty LastSendCertainty { get; }

  public ConnectionClosed(
    SessionEpoch epoch,
    string reason,
    PacketSequence lastReceivedSequence,
    TransportSendCertainty lastSendCertainty)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A close event must identify a valid session epoch.", nameof(epoch));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    if (!lastReceivedSequence.IsValid)
    {
      throw new ArgumentException("The last received sequence cannot be negative.", nameof(lastReceivedSequence));
    }

    Epoch = epoch;
    Reason = reason;
    LastReceivedSequence = lastReceivedSequence;
    LastSendCertainty = lastSendCertainty;
  }
}
