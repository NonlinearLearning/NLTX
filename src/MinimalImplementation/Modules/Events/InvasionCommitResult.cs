using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules.Events;

public sealed record InvasionCommitResult(
    WorldEventOutcomeStatus Status, CommitVersion? Version, ProtocolErrorCode? ErrorCode,
    string? Detail, IReadOnlyList<InvasionEffect> Effects) {
  internal static InvasionCommitResult Applied(
      CommitVersion version, IReadOnlyCollection<InvasionEffect> effects) {
    return new InvasionCommitResult(
        WorldEventOutcomeStatus.Applied, version, null, null,
        Array.AsReadOnly(effects.ToArray()));
  }

  internal static InvasionCommitResult Duplicate(CommitVersion version) {
    return new InvasionCommitResult(
        WorldEventOutcomeStatus.Duplicate, version, null, null, Array.Empty<InvasionEffect>());
  }

  internal static InvasionCommitResult Rejected(ProtocolErrorCode errorCode, string detail) {
    return new InvasionCommitResult(
        WorldEventOutcomeStatus.Rejected, null, errorCode, detail, Array.Empty<InvasionEffect>());
  }
}
