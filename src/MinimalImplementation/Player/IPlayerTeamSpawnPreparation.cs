using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// P03's prepared section arrangement for one authenticated UI team change. Preparation and
/// validation read real world/extra-spawn state without writing owners or performing I/O.
/// </summary>
public interface IPlayerTeamSpawnPreparation {
  SessionEpoch Epoch { get; }

  byte Team { get; }

  /// <summary>
  /// True only when this world uses team spawns and this team has an actual extra spawn.
  /// False is a resolved world fact, never a substitute for a missing provider.
  /// </summary>
  bool RequiresExtraSpawn { get; }

  /// <summary>
  /// Immutable section packets in send order. May be empty for an extra spawn only when its
  /// required sections already passed the existing transfer's ordered baseline barrier.
  /// </summary>
  IReadOnlyList<JoinBatchPacket> SectionPackets { get; }

  /// <summary>Checks the captured world/transfer revisions without writing.</summary>
  OwnerCommitResult Validate();

  /// <summary>
  /// Commits the validated arrangement once, without business rejection or I/O, in the same
  /// exclusive server-loop window as the player team commit. No await may intervene.
  /// </summary>
  void Commit();
}
