using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Immutable capture of one player for join synchronization.
/// </summary>
public sealed class PlayerJoinSnapshot
{
  private PlayerJoinSnapshot(
    SessionBinding binding,
    Player player,
    PlayerSpawnPacket? spawn = null,
    PlayerVitals? vitals = null,
    PlayerMovementInput? movement = null,
    PlayerTeam? team = null,
    PlayerImmunities? immunities = null,
    bool? isDead = null)
  {
    ArgumentNullException.ThrowIfNull(binding);
    ArgumentNullException.ThrowIfNull(player);
    if (binding.Epoch != player.Epoch || binding.PlayerHandle != player.Handle)
    {
      throw new InvalidOperationException(
        "A join snapshot can only capture the player owned by the authenticated binding.");
    }

    Epoch = binding.Epoch;
    PlayerSlot = binding.PlayerSlot;
    Handle = player.Handle;
    Appearance = player.Appearance;
    Vitals = vitals ?? player.Vitals;
    Movement = movement ?? player.Movement;
    Team = team ?? player.Team;
    PvpStatus = player.PvpStatus;
    Zones = player.Zones;
    Stealth = player.Stealth;
    BuffTypes = Array.AsReadOnly(player.Buffs.Types.ToArray());
    BuffTimes = Array.AsReadOnly(player.Buffs.Times.ToArray());
    Immunities = immunities ?? player.Immunities;
    Luck = player.Luck;
    TargetRelation = player.TargetRelation;
    Spectating = spawn is null ? player.Spectating : PlayerSpectating.None;
    RespawnTimer = spawn?.RespawnTimer ?? player.RespawnTimer;
    PveDeaths = spawn?.PveDeaths ?? player.PveDeaths;
    PvpDeaths = spawn?.PvpDeaths ?? player.PvpDeaths;
    SpawnContext = spawn?.SpawnContext ?? player.SpawnContext;
    LoadoutIndex = player.LoadoutIndex;
    AccessoryVisibilityMask = player.AccessoryVisibilityMask;
    IsDead = isDead ?? player.IsDead;
    IsHost = player.IsHost;
    CountsAsHostForGameplay = player.CountsAsHostForGameplay;
  }

  public SessionEpoch Epoch { get; }

  public byte PlayerSlot { get; }

  public EntityHandle Handle { get; }

  public PlayerAppearance Appearance { get; }

  public PlayerVitals Vitals { get; }

  public PlayerMovementInput Movement { get; }

  public PlayerTeam Team { get; }

  public PlayerPvpStatus PvpStatus { get; }

  public PlayerZones Zones { get; }

  public float Stealth { get; }

  public IReadOnlyList<ushort> BuffTypes { get; }

  public IReadOnlyList<int> BuffTimes { get; }

  public PlayerImmunities Immunities { get; }

  public PlayerLuck Luck { get; }

  public PlayerTargetRelation TargetRelation { get; }

  public PlayerSpectating Spectating { get; }

  public int RespawnTimer { get; }

  public short PveDeaths { get; }

  public short PvpDeaths { get; }

  public byte SpawnContext { get; }

  public byte LoadoutIndex { get; }

  public ushort AccessoryVisibilityMask { get; }

  public bool IsDead { get; }

  public bool IsHost { get; }

  public bool CountsAsHostForGameplay { get; }

  public static PlayerJoinSnapshot Capture(SessionBinding binding, Player player)
  {
    return new PlayerJoinSnapshot(binding, player);
  }

  /// <summary>Captures the proposed owner state without changing the live player.</summary>
  internal static PlayerJoinSnapshot CaptureSpawn(
    SessionBinding binding,
    Player player,
    PlayerSpawnPacket spawn,
    PlayerVitals vitals,
    PlayerMovementInput movement,
    PlayerTeam team,
    PlayerImmunities immunities,
    bool isDead)
  {
    ArgumentNullException.ThrowIfNull(spawn);
    return new PlayerJoinSnapshot(binding, player, spawn, vitals, movement, team, immunities, isDead);
  }
}
