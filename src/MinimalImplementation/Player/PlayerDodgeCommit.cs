using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>Captured immunity/buff consequences and the normalized packet 62 outputs.</summary>
public sealed class PlayerDodgeCommit {
  internal PlayerDodgeCommit(
      OwnerCommitResult result,
      bool recognized,
      PlayerImmunities immunities,
      PlayerBuffs? buffs,
      IReadOnlyCollection<SessionPacketProjection> projections) {
    Result = result;
    Recognized = recognized;
    Immunities = immunities;
    Buffs = buffs;
    Projections = Array.AsReadOnly(projections.ToArray());
  }

  public OwnerCommitResult Result { get; }

  /// <summary>Other subtypes are forwarded by Version4 without a player effect.</summary>
  public bool Recognized { get; }

  public PlayerImmunities Immunities { get; }

  public PlayerBuffs? Buffs { get; }

  public IReadOnlyList<SessionPacketProjection> Projections { get; }
}
