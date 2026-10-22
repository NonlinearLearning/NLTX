using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Prepared Player movement, spectating clear and wall state for a successful P06 pylon decision.
/// Validate all participants, then Commit in one exclusive ServerLoop window without I/O/await.
/// G2 prepares sections and packet 65 using Destination before writing, and sends all output
/// only after the complete transaction commits. This does not migrate all Teleport side effects.
/// </summary>
public sealed class PlayerPylonTeleportPreparation {
  private readonly PlayerOwner _players;
  private readonly WorldOwner _world;
  private readonly Player _player;
  private readonly IPlayerTeleportWorldCapture _worldSources;
  private readonly PlayerMovementInput _movement;
  private bool _committed;

  internal PlayerPylonTeleportPreparation(
      PlayerOwner players, WorldOwner world, Player player, PlayerSpatialSnapshot spatial,
      IPlayerTeleportWorldCapture worldSources, PacketVector2 destination, byte pylonStyle,
      PlayerUnbreakableWallState wallState, bool wallScanPerformed, CommitVersion version,
      IReadOnlyCollection<SessionEpoch> recipients) {
    _players = players;
    _world = world;
    _player = player;
    _worldSources = worldSources;
    _movement = player.Movement with {
      Position = destination, Velocity = default, VelocityIncluded = false
    };
    Spatial = spatial;
    WorldEpoch = worldSources.WorldEpoch;
    WorldVersion = worldSources.WorldVersion;
    WallScanApplicable = worldSources.DualDungeonsSeed;
    Destination = destination;
    PylonStyle = pylonStyle;
    PreviousWallState = player.UnbreakableWalls;
    WallState = wallState;
    WallScanPerformed = wallScanPerformed;
    Version = version;
    FallStart = (int)(destination.Y / 16f);
    Recipients = Array.AsReadOnly(recipients.ToArray());
    Projections = player.Spectating.IsSpectating
      ? Array.AsReadOnly(new[] {
        new SessionPacketProjection(SessionAdmissionCoordinator.SpectatePlayerMessageId,
          new SpectatePlayerPacket { Player = spatial.Binding.PlayerSlot, TargetPlayer = -1 },
          Recipients, null, version)
      })
      : Array.Empty<SessionPacketProjection>();
  }

  public SessionEpoch Epoch => Spatial.Epoch;
  public EntityHandle PlayerHandle => Spatial.PlayerHandle;
  public PlayerSpatialSnapshot Spatial { get; }
  public ulong WorldEpoch { get; }
  public CommitVersion WorldVersion { get; }
  public CommitVersion Version { get; }
  public PacketVector2 Destination { get; }
  public byte PylonStyle { get; }
  public int FallStart { get; }
  public bool WallScanApplicable { get; }
  public bool WallScanPerformed { get; }
  public PlayerUnbreakableWallState PreviousWallState { get; }
  public PlayerUnbreakableWallState WallState { get; }
  public bool UnbreakableWallChanged => PreviousWallState.Inside != WallState.Inside;
  public IReadOnlyList<SessionEpoch> Recipients { get; }
  public IReadOnlyList<SessionPacketProjection> Projections { get; }

  public OwnerCommitResult Validate() {
    if (_committed || !_players.TryPeekNextCommitVersion(out CommitVersion next)
        || next != Version || _worldSources.WorldScope != PlayerHandle.WorldScope
        || _worldSources.WorldEpoch != WorldEpoch || _worldSources.WorldVersion != WorldVersion
        || _worldSources.DualDungeonsSeed != WallScanApplicable) {
      return Conflict("The prepared teleport or Player commit sequence changed.");
    }
    OwnerCommitResult spatial = Spatial.Validate();
    if (spatial.Status != OwnerCommitStatus.Applied) {
      return spatial;
    }
    if (!_world.IsReady) {
      return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
        "The teleport world is no longer loaded.");
    }
    if (!_world.IsCurrentWorldRevision(WorldEpoch, WorldVersion)) {
      return Conflict("The world changed after the teleport facts or wall scan were captured.");
    }
    OwnerCommitResult source = _worldSources.Validate();
    return source.Status == OwnerCommitStatus.Applied ? OwnerCommitResult.Applied(Version) : source;
  }

  public void Commit() {
    if (_committed) {
      return;
    }
    _players.CommitPreparedPylonTeleport(
      _player, _movement, WallState, FallStart, PylonStyle, Version);
    _committed = true;
  }

  private static OwnerCommitResult Conflict(string detail) {
    return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
      detail);
  }
}
