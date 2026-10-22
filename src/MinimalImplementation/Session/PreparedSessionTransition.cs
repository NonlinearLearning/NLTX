using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

public sealed class PreparedSessionTransition
{
  internal PreparedSessionTransition(
    SessionAdmissionCoordinator owner,
    SessionEpoch epoch,
    PacketSequence sequence,
    long expectedRevision,
    SessionStage expectedStage,
    SessionStage nextStage,
    byte? responseMessageId,
    object? responsePacket,
    byte? playerSlot,
    bool closeAfterResponse)
  {
    Owner = owner;
    Epoch = epoch;
    Sequence = sequence;
    ExpectedRevision = expectedRevision;
    ExpectedStage = expectedStage;
    NextStage = nextStage;
    ResponseMessageId = responseMessageId;
    ResponsePacket = responsePacket;
    PlayerSlot = playerSlot;
    CloseAfterResponse = closeAfterResponse;
  }

  internal SessionAdmissionCoordinator Owner { get; }

  internal long ExpectedRevision { get; }

  internal SessionStage ExpectedStage { get; }

  internal byte? PlayerSlot { get; }

  internal bool IsConsumed { get; set; }

  public SessionEpoch Epoch { get; }

  public PacketSequence Sequence { get; }

  public SessionStage NextStage { get; }

  public byte? ResponseMessageId { get; }

  public object? ResponsePacket { get; }

  public bool CloseAfterResponse { get; }
}
