using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Detached pickup or ownership facts. Validate checks the captured Player runtime state and
/// real equipment, inventory and power revisions without writing or I/O. Ownership capture
/// may lack item-use readiness: ItemUseStateKnown is false and ItemAnimation is -1; those
/// values cannot authorize inventory pickup. Actual geometry is captured separately.
/// VoidVaultEnabled includes actual item 4131 ownership and the runtime voidVaultInfo[0]
/// switch. Missing ownership dependencies return no capture.
/// </summary>
public interface IPlayerPickupFactsCapture {
  SessionEpoch Epoch { get; }
  EntityHandle PlayerHandle { get; }
  CommitVersion PlayerVersion { get; }
  PlayerSourceRevision SourceRevision { get; }
  PlayerPickupSourceFacts Facts { get; }
  OwnerCommitResult Validate();
}
