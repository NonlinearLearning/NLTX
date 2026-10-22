using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Captures real owner reads for an exclusive ServerLoop transaction window.</summary>
public sealed class WorldItemOwnerReadGuard : IWorldItemTransactionParticipant
{
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly WorldOwner _world;
  private readonly SessionBinding _binding;
  private readonly Player.Player _player;
  private readonly PlayerMovementInput _movement;
  private readonly PlayerVitals _vitals;
  private readonly PlayerBuffs _buffs;
  private readonly ulong _worldEpoch;
  private readonly CommitVersion _worldVersion;

  public SessionBinding Binding => _binding;
  public PlayerMovementInput Movement => _movement;

  private WorldItemOwnerReadGuard(
    PlayerOwner players, SessionAdmissionCoordinator sessions, WorldOwner world,
    SessionBinding binding, Player.Player player, ulong worldEpoch, CommitVersion worldVersion)
  {
    _players = players;
    _sessions = sessions;
    _world = world;
    _binding = binding;
    _player = player;
    _movement = player.Movement;
    _vitals = player.Vitals;
    _buffs = player.Buffs;
    _worldEpoch = worldEpoch;
    _worldVersion = worldVersion;
  }

  /// <summary>Host supplies the explicit scope-to-world-epoch mapping.</summary>
  public static OwnerCommitResult Capture(
    PlayerOwner players, SessionAdmissionCoordinator sessions, WorldOwner world,
    SessionEpoch epoch, WorldScopeId scope, ulong worldEpoch,
    out WorldItemOwnerReadGuard? guard)
  {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(world);
    guard = null;
    if (!epoch.IsValid || !scope.IsValid || !world.IsReady
      || !sessions.TryGetStage(epoch, out SessionStage stage) || stage != SessionStage.Active
      || !sessions.TryGetBinding(epoch, out SessionBinding? binding) || binding is null
      || !players.TryGetBoundPlayer(epoch, sessions, out Player.Player? player) || player is null
      || player.IsDead || player.Handle != binding.PlayerHandle
      || player.Handle.WorldScope != scope)
    {
      return Unavailable("The pickup player, session or world is not active.");
    }

    WorldHeaderCapture header = world.CaptureWorldHeader();
    if (header.WorldEpoch != worldEpoch)
    {
      return Unavailable("The supplied scope belongs to another world epoch.");
    }

    guard = new WorldItemOwnerReadGuard(
      players, sessions, world, binding, player, header.WorldEpoch, header.Version);
    return guard.Validate();
  }

  public OwnerCommitResult Validate()
  {
    if (!_world.IsReady || !_world.IsCurrentWorldRevision(_worldEpoch, _worldVersion)
      || !_sessions.TryGetStage(_binding.Epoch, out SessionStage stage)
      || stage != SessionStage.Active
      || !_sessions.TryGetBinding(_binding.Epoch, out SessionBinding? binding)
      || binding != _binding
      || !_players.TryGet(_binding.PlayerHandle, out Player.Player? current)
      || current is null || !ReferenceEquals(current, _player) || current.Epoch != _binding.Epoch
      || current.Movement != _movement || current.Vitals != _vitals || current.Buffs != _buffs
      || current.IsDead)
    {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null,
        ProtocolErrorCode.Conflict, "The captured player/session/world reads changed.");
    }

    return OwnerCommitResult.Applied(_worldVersion);
  }

  public void Commit()
  {
    // Read participant: all changes belong to the prepared owner participants and P04 stores.
  }

  private static OwnerCommitResult Unavailable(string detail) => new(
    OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);
}
