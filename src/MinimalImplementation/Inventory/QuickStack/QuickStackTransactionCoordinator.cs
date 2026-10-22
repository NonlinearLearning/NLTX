using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>
/// Re-reads the authenticated player's inventory, prepares from server-selected chest candidates,
/// and commits both owners through their shared synchronous transaction boundary.
/// </summary>
public sealed class QuickStackTransactionCoordinator
{
  private readonly InventoryStore _inventories;
  private readonly InventoryTransactionCoordinator _transactions;
  private readonly QuickStackTransferPlanner _planner;
  private readonly IQuickStackDestinationProvider _destinations;
  private readonly ServerStateCommitGate _commitGate;

  public QuickStackTransactionCoordinator(
    InventoryStore inventories,
    InventoryTransactionCoordinator transactions,
    QuickStackTransferPlanner planner,
    IQuickStackDestinationProvider destinations)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(transactions);
    ArgumentNullException.ThrowIfNull(planner);
    ArgumentNullException.ThrowIfNull(destinations);
    if (!ReferenceEquals(inventories, transactions.Inventories))
    {
      throw new ArgumentException("QuickStack and its transaction coordinator must share one inventory store.", nameof(transactions));
    }

    if (!ReferenceEquals(inventories.CommitGate, transactions.CommitGate))
    {
      throw new ArgumentException("QuickStack and its transaction coordinator must share one commit gate.", nameof(transactions));
    }

    if (!ReferenceEquals(inventories.CommitGate, destinations.CommitGate))
    {
      throw new ArgumentException("Inventory and destination providers must share one commit gate.", nameof(destinations));
    }

    _inventories = inventories;
    _transactions = transactions;
    _planner = planner;
    _destinations = destinations;
    _commitGate = inventories.CommitGate;
  }

  public QuickStackTransactionResult Execute(
    EntityUuid authenticatedPlayerId,
    SessionEpoch epoch,
    WorldScopeId expectedWorldScope,
    QuickStackRequestPayload request,
    DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!authenticatedPlayerId.IsValid || !epoch.IsValid || !expectedWorldScope.IsValid)
    {
      return Failed(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "QuickStack requires an authenticated player identity."),
        null);
    }

    lock (_commitGate.SyncRoot)
    {
      if (!_inventories.TryCapture(authenticatedPlayerId, out InventorySnapshot? inventory)
        || inventory is null)
      {
        return Failed(
          new OwnerCommitResult(
            OwnerCommitStatus.Unavailable,
            null,
            ProtocolErrorCode.NotReady,
            "The authenticated player's inventory is not available."),
          null);
      }

      IReadOnlyList<QuickStackDestinationCandidate>? selectedDestinations =
        _destinations.SelectDestinations(authenticatedPlayerId, epoch, expectedWorldScope, now);
      if (selectedDestinations is null)
      {
        return Failed(
          OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "The QuickStack destination provider returned no snapshot."),
          QuickStackPreparationStatus.InvalidDestinationSnapshot);
      }

      QuickStackPreparation preparation = _planner.Prepare(
        request,
        inventory,
        expectedWorldScope,
        Array.AsReadOnly(selectedDestinations.ToArray()));
      if (!preparation.IsPrepared)
      {
        ProtocolErrorCode errorCode = preparation.Status == QuickStackPreparationStatus.InvalidRequest
          ? ProtocolErrorCode.MalformedFrame
          : ProtocolErrorCode.NotReady;
        return Failed(
          OwnerCommitResult.Rejected(errorCode, "QuickStack could not prepare the server snapshots."),
          preparation.Status);
      }

      InventoryTransactionResult transaction = _transactions.Commit(
        authenticatedPlayerId,
        inventory.Revision,
        preparation.InventoryMutations,
        preparation.ChestMutations);
      if (transaction.Commit.Status != OwnerCommitStatus.Applied
        || transaction.Inventory is null)
      {
        return Failed(transaction.Commit, preparation.Status);
      }

      return new QuickStackTransactionResult(
        transaction.Commit,
        preparation.Status,
        transaction.Inventory,
        transaction.Chests,
        preparation.InventoryMutations,
        preparation.ChestMutations,
        preparation.BlockedChestIds);
    }
  }

  private static QuickStackTransactionResult Failed(
    OwnerCommitResult commit,
    QuickStackPreparationStatus? preparationStatus)
  {
    return new QuickStackTransactionResult(
      commit,
      preparationStatus,
      null,
      Array.Empty<ChestSnapshot>(),
      Array.Empty<InventoryMutation>(),
      Array.Empty<ChestItemMutationBatch>(),
      Array.Empty<ushort>());
  }
}
