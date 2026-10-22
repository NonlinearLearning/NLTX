using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>P03's read-only preparation of CheckSection for a spectating destination.</summary>
public interface IPlayerSpectatingSectionPreparation {
  SessionEpoch Epoch { get; }

  EntityHandle Target { get; }

  /// <summary>
  /// Ordered immutable 10/11 packets. Empty requires the destination's existing ordered baseline
  /// barrier to be satisfied. Capturing an empty list does not establish that barrier.
  /// </summary>
  IReadOnlyList<JoinBatchPacket> SectionPackets { get; }

  /// <summary>Checks real world and per-session transfer revisions without writing.</summary>
  OwnerCommitResult Validate();

  /// <summary>
  /// Commits once, without business rejection or I/O, on the exclusive server loop. No await may
  /// intervene after validation. Tick preparations for different sessions must compose safely.
  /// </summary>
  void Commit();
}
