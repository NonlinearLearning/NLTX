using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>
/// Captures the player-owned source facts and exact debit/spawn plan for one world-item drop.
/// Validate must bind the output to this player, inventory source and world revision, and reject
/// changed source facts or a changed debit/output plan, including drop position and physics.
/// Any requested claim release must target one of the prepared world-item changes; the coordinator
/// validates claim ownership against the active reservation.
/// </summary>
public interface IWorldItemDropPreparation : IWorldItemTransactionParticipant
{
  EntityHandle PlayerHandle { get; }
  SessionEpoch Epoch { get; }
  CommitVersion PlayerVersion { get; }
  CommitVersion SourceRevision { get; }
  ulong WorldEpoch { get; }
  CommitVersion WorldVersion { get; }
  CommitVersion InventoryRevision { get; }
  IReadOnlyList<WorldItemChange> WorldChanges { get; }
  IReadOnlyList<WorldItemSpawn> Spawns { get; }
  IReadOnlyList<InventoryMutation> InventoryDebits { get; }
  IReadOnlyList<WorldItemHandle> ReleaseClaims { get; }
}
