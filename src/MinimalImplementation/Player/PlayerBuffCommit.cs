using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>Server buff state and outputs captured in the same commit window.</summary>
public sealed class PlayerBuffCommit {
  internal PlayerBuffCommit(
      OwnerCommitResult result,
      PlayerBuffs? buffs,
      PlayerLuck? luck,
      IReadOnlyCollection<ushort> unprojectedSelfRemovals,
      IReadOnlyCollection<SessionPacketProjection> projections) {
    Result = result;
    Buffs = buffs;
    Luck = luck;
    UnprojectedSelfRemovals = Array.AsReadOnly(unprojectedSelfRemovals.ToArray());
    Projections = Array.AsReadOnly(projections.ToArray());
  }

  public OwnerCommitResult Result { get; }

  public PlayerBuffs? Buffs { get; }

  public PlayerLuck? Luck { get; }

  /// <summary>Packet 50 is ignored by an ordinary client for its own player.</summary>
  public IReadOnlyList<ushort> UnprojectedSelfRemovals { get; }

  public IReadOnlyList<SessionPacketProjection> Projections { get; }
}
