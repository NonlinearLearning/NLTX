using System;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>Captures and guards the real player geometry on the exclusive ServerLoop.</summary>
public sealed class PlayerSpatialFactsService {
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly Func<SessionBinding, Player, IPlayerSpatialFactsCapture?>? _captureSources;
  private PlayerRuntimeFactsCaptureService? _runtimeFacts;

  public PlayerSpatialFactsService(
      PlayerOwner players,
      SessionAdmissionCoordinator sessions,
      Func<SessionBinding, Player, IPlayerSpatialFactsCapture?>? captureSources = null) {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    _players = players;
    _sessions = sessions;
    _captureSources = captureSources;
  }

  public PlayerSpatialFactsService(
    PlayerOwner players,
    SessionAdmissionCoordinator sessions,
    PlayerRuntimeFactsCaptureService runtimeFacts) : this(players, sessions)
  {
    ArgumentNullException.ThrowIfNull(runtimeFacts);
    _runtimeFacts = runtimeFacts;
  }

  public OwnerCommitResult Capture(SessionEpoch epoch, out PlayerSpatialSnapshot? snapshot) {
    snapshot = null;
    if (!_sessions.TryGetStage(epoch, out SessionStage stage) || stage != SessionStage.Active
        || !_sessions.TryGetBinding(epoch, out SessionBinding? binding) || binding is null
        || !_players.TryGetForBinding(binding, out Player? player) || player is null) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The spatial player/session binding is not active.");
    }

    CommitVersion version = player.Version;
    IPlayerSpatialFactsCapture? sources;
    if (_runtimeFacts is not null)
    {
      OwnerCommitResult captureResult = _runtimeFacts.TryCaptureSpatial(
        binding, player, out sources);
      if (captureResult.Status != OwnerCommitStatus.Applied || sources is null)
      {
        return captureResult;
      }
    }
    else
    {
      sources = _captureSources?.Invoke(binding, player);
    }
    if (sources is null) {
      return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
        "Authoritative player collider, mount and stool facts are unavailable.");
    }

    if (sources.Epoch != epoch || sources.PlayerHandle != binding.PlayerHandle
        || sources.PlayerVersion != version || !sources.SourceRevision.IsValid
        || sources.Facts is null || sources.Facts.Width <= 0 || sources.Facts.Height <= 0
        || sources.Facts.TileRangeX is < 0 || sources.Facts.TileRangeY is < 0
        || !float.IsFinite(player.Movement.Position.X)
        || !float.IsFinite(player.Movement.Position.Y)
        || MathF.Abs(player.Movement.Position.X) > PlayerMovementInput.MaximumCoordinateMagnitude
        || MathF.Abs(player.Movement.Position.Y) > PlayerMovementInput.MaximumCoordinateMagnitude) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The spatial source does not identify valid facts for this player version.");
    }

    var captured = new PlayerSpatialSnapshot(_players, _sessions, binding, player, sources);
    OwnerCommitResult result = captured.Validate();
    if (result.Status == OwnerCommitStatus.Applied) {
      snapshot = captured;
    }
    return result;
  }
}
