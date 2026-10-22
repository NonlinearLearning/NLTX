using System;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Read-only Player preparation after P06 validates the real source/target pylon.</summary>
public sealed class PlayerPylonTeleportPreparationService {
  private const byte _pylonTypeCount = 11;
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly PlayerSpatialFactsService _spatial;
  private readonly WorldOwner _world;
  private readonly Func<SessionBinding, WorldHeaderCapture, IPlayerTeleportWorldCapture?>?
    _captureWorldSources;

  public PlayerPylonTeleportPreparationService(
      PlayerOwner players, SessionAdmissionCoordinator sessions, PlayerSpatialFactsService spatial,
      WorldOwner world,
      Func<SessionBinding, WorldHeaderCapture, IPlayerTeleportWorldCapture?>?
        captureWorldSources = null) {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(spatial);
    ArgumentNullException.ThrowIfNull(world);
    _players = players;
    _sessions = sessions;
    _spatial = spatial;
    _world = world;
    _captureWorldSources = captureWorldSources;
  }

  public OwnerCommitResult Prepare(
      SessionEpoch epoch, EntityUuid playerIdentity, TileCoordinate targetAnchor, byte pylonStyle,
      out PlayerPylonTeleportPreparation? preparation) {
    preparation = null;
    OwnerCommitResult result = _spatial.Capture(epoch, out PlayerSpatialSnapshot? spatial);
    if (result.Status != OwnerCommitStatus.Applied || spatial is null) {
      return result;
    }
    if (!playerIdentity.IsValid || spatial.PlayerHandle.Id != playerIdentity
        || !_players.TryGetForBinding(spatial.Binding, out Player? player) || player is null) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The teleport player identity is no longer active.");
    }
    if (!_world.IsReady) {
      return Unavailable("The teleport world is not loaded.");
    }
    WorldHeaderCapture header = _world.CaptureWorldHeader();
    if (!header.Header.Geometry.Contains(targetAnchor) || pylonStyle >= _pylonTypeCount) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The pylon anchor or type is outside the Version4 world range.");
    }
    IPlayerTeleportWorldCapture? sources = _captureWorldSources?.Invoke(spatial.Binding, header);
    if (sources is null) {
      return Unavailable("The loaded document's world.spawn-points facts are unavailable.");
    }
    if (sources.WorldScope != spatial.PlayerHandle.WorldScope
        || sources.WorldEpoch != header.WorldEpoch || sources.WorldVersion != header.Version) {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The teleport document facts do not match the captured world or player scope.");
    }
    OwnerCommitResult source = sources.Validate();
    if (source.Status != OwnerCommitStatus.Applied) {
      return source;
    }

    // ToWorldCoordinates adds 8 on both axes. HeightOffsetBoost is independent of collider size.
    var destination = new PacketVector2(targetAnchor.X * 16f + 8f,
      targetAnchor.Y * 16f + 8f - spatial.HeightOffsetBoost);
    if (!float.IsFinite(destination.X) || !float.IsFinite(destination.Y)
        || MathF.Abs(destination.X) > PlayerMovementInput.MaximumCoordinateMagnitude
        || MathF.Abs(destination.Y) > PlayerMovementInput.MaximumCoordinateMagnitude) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.InvalidLength,
        "The pylon destination is outside the supported player coordinate range.");
    }

    PlayerUnbreakableWallState wallState = player.UnbreakableWalls;
    bool scanned = false;
    if (sources.DualDungeonsSeed) {
      var center = new PacketVector2(
        destination.X + spatial.Width * 0.5f, destination.Y + spatial.Height * 0.5f);
      int cooldown = wallState.Cooldown - 1;
      float dx = center.X - wallState.LastPosition.X;
      float dy = center.Y - wallState.LastPosition.Y;
      float distance = MathF.Sqrt(dx * dx + dy * dy);
      if (cooldown <= 0 || !(distance < PlayerUnbreakableWallState.RescanDistance)) {
        // Reuse P03's authoritative scanner. Validate rechecks the world revision after this read.
        bool inside = _world.IsInsideUnbreakableWalls(
          new TileCoordinate((int)(center.X / 16f), (int)(center.Y / 16f)));
        wallState = new PlayerUnbreakableWallState(
          inside, PlayerUnbreakableWallState.RescanPeriod, center);
        scanned = true;
      } else {
        wallState = wallState with { Cooldown = cooldown };
      }
    }
    if (!_players.TryPeekNextCommitVersion(out CommitVersion version)) {
      return Unavailable("The Player commit version sequence is exhausted.");
    }
    var captured = new PlayerPylonTeleportPreparation(
      _players, _world, player, spatial, sources, destination, pylonStyle, wallState, scanned,
      version, _sessions.CaptureActiveSessionEpochs());
    result = captured.Validate();
    if (result.Status == OwnerCommitStatus.Applied) {
      preparation = captured;
    }
    return result;
  }

  private static OwnerCommitResult Unavailable(string detail) {
    return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
      detail);
  }
}
