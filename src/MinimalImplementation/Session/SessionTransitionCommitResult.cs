using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

public sealed record SessionTransitionCommitResult(
  OwnerCommitResult Result,
  OutboundPacketIntent? Response,
  bool CloseAfterResponse)
{
  internal static SessionTransitionCommitResult Failed(OwnerCommitResult result) =>
    new(result, null, false);
}
