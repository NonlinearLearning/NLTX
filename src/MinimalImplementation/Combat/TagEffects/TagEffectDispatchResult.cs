using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Owner result and optional sender-rebound module payload for P02 routing.</summary>
public sealed record TagEffectDispatchResult(
  OwnerCommitResult Result,
  TagEffectCommand? BroadcastMessage);
