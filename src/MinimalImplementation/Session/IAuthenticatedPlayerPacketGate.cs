using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

/// <summary>Runs a packet effect only for a currently bound, authenticated player session.</summary>
public interface IAuthenticatedPlayerPacketGate
{
  OwnerCommitResult ExecuteAuthenticatedPlayerPacket(
    InboundPacketEnvelope envelope,
    byte expectedMessageId,
    IReadOnlyCollection<SessionStage> allowedStages,
    Func<SessionBinding, OwnerCommitResult> commit);
}
