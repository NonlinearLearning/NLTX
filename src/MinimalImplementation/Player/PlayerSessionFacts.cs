using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>Immutable identity and phase facts shared with inventory and combat adapters.</summary>
public sealed record PlayerSessionFacts(
  SessionEpoch Epoch,
  EntityHandle PlayerHandle,
  byte PlayerSlot,
  SessionStage Stage,
  CommitVersion PlayerVersion);
