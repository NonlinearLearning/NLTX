using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>A committed player's tick state and the outputs captured at that version.</summary>
public sealed class PlayerTickCommit
{
  internal PlayerTickCommit(
    Player player,
    PlayerTickState state,
    CommitVersion version,
    bool respawnReady,
    IReadOnlyCollection<SessionPacketProjection> projections)
  {
    Epoch = player.Epoch;
    Handle = player.Handle;
    Version = version;
    Buffs = state.Buffs;
    Immunities = state.Immunities;
    Vitals = state.Vitals;
    IsDead = player.IsDead;
    Luck = state.Luck;
    TargetRelation = state.TargetRelation;
    Spectating = state.Spectating;
    RespawnTimer = state.RespawnTimer;
    DeadTicks = state.DeadTicks;
    RespawnReady = respawnReady;
    Projections = Array.AsReadOnly(projections.ToArray());
  }

  public SessionEpoch Epoch { get; }

  public EntityHandle Handle { get; }

  public CommitVersion Version { get; }

  public PlayerBuffs Buffs { get; }

  public PlayerImmunities Immunities { get; }

  public PlayerVitals Vitals { get; }

  public bool IsDead { get; }

  public PlayerLuck Luck { get; }

  public PlayerTargetRelation TargetRelation { get; }

  public PlayerSpectating Spectating { get; }

  public int RespawnTimer { get; }

  public int DeadTicks { get; }

  /// <summary>Timer edge only; revival still requires the authoritative spawn flow.</summary>
  public bool RespawnReady { get; }

  public IReadOnlyList<SessionPacketProjection> Projections { get; }
}
