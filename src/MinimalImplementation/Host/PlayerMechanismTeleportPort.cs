using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>Guards pylon requests until all production preparations are available.</summary>
public sealed class PlayerMechanismTeleportPort : IMechanismTeleportPort {
  private readonly ServerLoop _loop;
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly WorldOwner _world;
  private readonly TeleportPylonRegistry _pylons;
  private readonly PlayerTeleportWorldCaptureSource _worldSources;
  private readonly long _worldScope;

  public PlayerMechanismTeleportPort(
    ServerLoop loop,
    PlayerOwner players,
    SessionAdmissionCoordinator sessions,
    WorldOwner world,
    TeleportPylonRegistry pylons,
    PlayerTeleportWorldCaptureSource worldSources,
    long worldScope) {
    ArgumentNullException.ThrowIfNull(loop);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(pylons);
    ArgumentNullException.ThrowIfNull(worldSources);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldScope);
    _loop = loop;
    _players = players;
    _sessions = sessions;
    _world = world;
    _pylons = pylons;
    _worldSources = worldSources;
    _worldScope = worldScope;
  }

  public MechanismTeleportPortResult Submit(MechanismTeleportRequest request) {
    ArgumentNullException.ThrowIfNull(request);
    if (!request.Epoch.IsValid || !request.Player.IsValid ||
        request.PlayerSlot == byte.MaxValue || request.TargetX < 0 || request.TargetY < 0 ||
        request.PylonType > 10) {
      return MechanismTeleportPortResult.Rejected(PylonTeleportFailure.SourceMissing,
        "The teleport request does not contain a valid authenticated player target.");
    }
    if (!_loop.IsCurrent) {
      return MechanismTeleportPortResult.NotReady(
        "Pylon fact capture requires the exclusive server loop.");
    }
    if (!_sessions.TryGetStage(request.Epoch, out SessionStage stage) ||
        stage != SessionStage.Active ||
        !_sessions.TryGetBinding(request.Epoch, out SessionBinding? binding) || binding is null ||
        binding.PlayerSlot != request.PlayerSlot || binding.PlayerHandle.Id != request.Player ||
        binding.PlayerHandle.WorldScope.Value != _worldScope ||
        !_players.TryGetForBinding(binding, out var player) || player is null) {
      return MechanismTeleportPortResult.Rejected(PylonTeleportFailure.SourceMissing,
        "The authenticated player slot, identity or world scope is no longer active.");
    }
    if (_pylons.WorldScope != _worldScope || !_world.IsReady) {
      return MechanismTeleportPortResult.NotReady(
        "The pylon registry and published teleport world are not ready.");
    }

    WorldHeaderCapture header = _world.CaptureWorldHeader();
    IPlayerTeleportWorldCapture? sources = _worldSources.Capture(binding, header);
    if (sources is null) {
      return MechanismTeleportPortResult.NotReady(
        "The published world has no matching loaded world.spawn-points document binding.");
    }
    OwnerCommitResult guard = sources.Validate();
    if (guard.Status != OwnerCommitStatus.Applied) {
      return MechanismTeleportPortResult.Conflict(guard.Detail ??
        "The teleport document capture is no longer current.");
    }

    // No owner may commit until eligibility, Player preparation, encoding and sections can all
    // be guarded together. These sources are still absent from the production composition.
    return MechanismTeleportPortResult.NotReady(
      "Pylon teleport awaits P02 equipment/runtime-world spatial sources; " +
      "P05 revision-bound NPC home, homeless and town-like facts; " +
      "P07 revision-bound runtime DownedPlantBoss; " +
      "P03 arbitrary-destination section preparation; " +
      "and Host prepared packet65/section output settlement.");
  }
}
