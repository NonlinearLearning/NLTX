using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>
/// Commits one player inventory and any number of chest updates under their shared state gate.
/// </summary>
public sealed class InventoryTransactionCoordinator
{
  private readonly InventoryStore _inventories;
  private readonly ChestStore _chests;
  private readonly ServerStateCommitGate _commitGate;

  internal InventoryStore Inventories => _inventories;

  internal ServerStateCommitGate CommitGate => _commitGate;

  public InventoryTransactionCoordinator(InventoryStore inventories, ChestStore chests)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(chests);
    if (!ReferenceEquals(inventories.CommitGate, chests.CommitGate))
    {
      throw new ArgumentException("Inventory and chest stores must share one commit gate.", nameof(chests));
    }

    _inventories = inventories;
    _chests = chests;
    _commitGate = inventories.CommitGate;
  }

  public InventoryTransactionResult Commit(
    EntityUuid playerId,
    CommitVersion expectedInventoryRevision,
    IReadOnlyCollection<InventoryMutation> inventoryMutations,
    IReadOnlyCollection<ChestItemMutationBatch> chestBatches)
  {
    ArgumentNullException.ThrowIfNull(inventoryMutations);
    ArgumentNullException.ThrowIfNull(chestBatches);
    lock (_commitGate.SyncRoot)
    {
      OwnerCommitResult prepareResult = _inventories.PrepareApply(
        playerId,
        expectedInventoryRevision,
        inventoryMutations,
        out InventoryStore.PreparedMutation? preparedInventory);
      if (prepareResult.Status != OwnerCommitStatus.Applied || preparedInventory is null)
      {
        return Failed(prepareResult);
      }

      if (!_inventories.CanCommit(preparedInventory))
      {
        return Failed(new OwnerCommitResult(
          OwnerCommitStatus.Conflict,
          null,
          ProtocolErrorCode.Conflict,
          "The player inventory changed before the transaction could commit."));
      }

      OwnerCommitResult chestPrepareResult = _chests.PrepareItemsBatch(
        chestBatches,
        out ChestStore.PreparedItemsBatch? preparedChests);
      if (chestPrepareResult.Status != OwnerCommitStatus.Applied || preparedChests is null)
      {
        return Failed(chestPrepareResult);
      }

      if (!_chests.CanCommit(preparedChests))
      {
        return Failed(new OwnerCommitResult(
          OwnerCommitStatus.Conflict,
          null,
          ProtocolErrorCode.Conflict,
          "A chest changed before the transaction could commit."));
      }

      OwnerCommitResult inventoryResult = _inventories.CommitPrepared(
        preparedInventory,
        out InventorySnapshot? inventorySnapshot);
      if (inventoryResult.Status != OwnerCommitStatus.Applied)
      {
        return Failed(inventoryResult);
      }

      _chests.CommitPrepared(preparedChests);
      return new InventoryTransactionResult(
        inventoryResult,
        inventorySnapshot,
        preparedChests.Snapshots);
    }
  }

  private static InventoryTransactionResult Failed(OwnerCommitResult result) =>
    new(result, null, Array.Empty<ChestSnapshot>());
}
