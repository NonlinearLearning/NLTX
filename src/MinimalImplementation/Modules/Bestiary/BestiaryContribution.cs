namespace MinimalImplementation.Server.Modules.Bestiary;

public enum BestiaryContributionKind
{
  Kill,
  Sight,
  Chat
}

/// <summary>A P05-committed NPC fact translated into P07-owned Bestiary state.</summary>
public sealed record BestiaryContribution(
  MinimalImplementation.Contracts.CommitVersion SourceCommitVersion,
  BestiaryContributionKind Kind,
  short NpcNetId,
  string PersistentNpcId,
  IReadOnlyCollection<MinimalImplementation.Contracts.SessionEpoch> VisibleTargets);

public enum BestiaryContributionStatus
{
  Applied,
  Duplicate,
  Invalid
}

public sealed record BestiaryContributionResult(
  BestiaryContributionStatus Status,
  MinimalImplementation.Contracts.CommitVersion? Version,
  MinimalImplementation.Contracts.ProtocolErrorCode? ErrorCode,
  IReadOnlyList<MinimalImplementation.Contracts.OutboundPacketIntent> OutboundIntents);
