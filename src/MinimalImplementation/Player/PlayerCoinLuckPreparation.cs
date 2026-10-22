using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Prepared coin-luck change for one player. P04 owns the item/shimmer transaction; this
/// preparation owns only the Player luck fact and can be committed in the same server-loop
/// window after the item transaction has validated.
/// </summary>
public sealed class PlayerCoinLuckPreparation
{
  private readonly PlayerOwner _players;
  private readonly Player _player;
  private readonly CommitVersion _expectedPlayerVersion;
  private bool _committed;

  internal PlayerCoinLuckPreparation(
      PlayerOwner players,
      Player player,
      PlayerLuck luck,
      CommitVersion version,
      bool changed)
  {
    _players = players;
    _player = player;
    Luck = luck;
    Version = version;
    Changed = changed;
    _expectedPlayerVersion = player.Version;
  }

  public PlayerLuck Luck { get; }

  public CommitVersion Version { get; }

  public bool Changed { get; }

  public OwnerCommitResult Validate()
  {
    if (_committed || !_players.TryGet(_player.Handle, out Player? current)
        || !ReferenceEquals(current, _player) || current.Version != _expectedPlayerVersion)
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Conflict,
        null,
        ProtocolErrorCode.Conflict,
        "The Player changed after coin-luck facts were prepared.");
    }

    if (!Changed)
    {
      return OwnerCommitResult.Applied(_player.Version);
    }

    return _players.TryPeekNextCommitVersion(out CommitVersion next) && next == Version
      ? OwnerCommitResult.Applied(Version)
      : new OwnerCommitResult(
        OwnerCommitStatus.Conflict,
        null,
        ProtocolErrorCode.Conflict,
        "The Player commit version changed after coin-luck facts were prepared.");
  }

  /// <summary>Publishes the prepared luck fact exactly once on the server commit loop.</summary>
  public void Commit()
  {
    if (_committed)
    {
      return;
    }

    if (Changed)
    {
      _players.CommitPreparedCoinLuck(_player, Luck, Version);
    }

    _committed = true;
  }
}
