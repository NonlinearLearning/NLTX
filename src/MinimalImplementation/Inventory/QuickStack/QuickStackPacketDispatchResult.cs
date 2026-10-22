using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.QuickStack;

public sealed record QuickStackPacketDispatchResult(
  OwnerCommitResult Commit,
  IReadOnlyList<OutboundPacketIntent> OutboundIntents)
{
  public bool IsApplied => Commit.Status == OwnerCommitStatus.Applied;
}
