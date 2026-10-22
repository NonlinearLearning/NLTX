using System;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Player;

public sealed class Player
{
  internal Player(SessionEpoch epoch, EntityHandle handle)
  {
    Epoch = epoch;
    Handle = handle;
    Vitals = PlayerVitals.Initial;
    Appearance = PlayerAppearance.Initial;
    Movement = PlayerMovementInput.Initial;
    Team = new PlayerTeam(0, 0);
    PvpStatus = new PlayerPvpStatus(false);
    Zones = new PlayerZones(0, 0, 0, 0, 0, 0);
    Buffs = PlayerBuffs.Empty;
    Immunities = PlayerImmunities.Empty;
    Luck = PlayerLuck.Empty;
    TargetRelation = PlayerTargetRelation.Empty;
    Spectating = PlayerSpectating.None;
    RuntimeSpatial = PlayerRuntimeSpatialState.Initial;
  }

  public SessionEpoch Epoch { get; }

  public EntityHandle Handle { get; }

  /// <summary>Last committed player state version, from the PlayerOwner version sequence.</summary>
  public CommitVersion Version { get; internal set; }

  public PlayerVitals Vitals { get; internal set; }

  public PlayerAppearance Appearance { get; internal set; }

  public PlayerMovementInput Movement { get; internal set; }

  public PlayerRuntimeSpatialState RuntimeSpatial { get; internal set; }

  public PlayerTeam Team { get; internal set; }

  public PlayerPvpStatus PvpStatus { get; internal set; }

  public PlayerZones Zones { get; internal set; }

  public float Stealth { get; internal set; }

  public PlayerBuffs Buffs { get; internal set; }

  public PlayerImmunities Immunities { get; internal set; }

  public PlayerLuck Luck { get; internal set; }

  public PlayerTargetRelation TargetRelation { get; internal set; }

  public PlayerSpectating Spectating { get; internal set; }

  public PlayerUnbreakableWallState UnbreakableWalls { get; internal set; }

  public int FallStart { get; internal set; }

  public byte TeleportStyle { get; internal set; }

  public byte LastTeleportPylonStyleUsed { get; internal set; }

  public int RespawnTimer { get; internal set; }

  /// <summary>Elapsed dead-player updates, used by the 180-tick spectating linger.</summary>
  public int DeadTicks { get; internal set; }

  public short PveDeaths { get; internal set; }

  public short PvpDeaths { get; internal set; }

  public byte SpawnContext { get; internal set; }

  public byte LoadoutIndex { get; internal set; }

  public ushort AccessoryVisibilityMask { get; internal set; }

  /// <summary>Lifecycle death flag; healing alone does not revive a player.</summary>
  public bool IsDead { get; internal set; }

  public bool HasPendingPvpDeath { get; internal set; }

  public bool IsHost { get; internal set; }

  /// <summary>Gameplay host status is independent from HostToken authorization.</summary>
  public bool CountsAsHostForGameplay { get; internal set; }
}
