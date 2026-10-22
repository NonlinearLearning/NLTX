using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Host;

/// <summary>Stages one selected block debit for the same commit as its tile placement.</summary>
internal sealed class InventoryItemPlacementPreparation : IWorldItemTransactionParticipant {
  private readonly InventoryStore _inventories;
  private readonly InventoryStore.PreparedMutation _prepared;
  private bool _committed;

  private InventoryItemPlacementPreparation(InventoryStore inventories,
      InventoryStore.PreparedMutation prepared) {
    _inventories = inventories;
    _prepared = prepared;
  }

  public InventorySnapshot Snapshot => _prepared.Snapshot;

  public static OwnerCommitResult TryPrepare(InventoryStore inventories,
      InventorySnapshot inventory, InventorySlotSnapshot source,
      out InventoryItemPlacementPreparation? preparation) {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(source);
    preparation = null;
    if (source.SlotId < 0 || source.SlotId >= inventory.Slots.Count ||
        inventory.Slots[source.SlotId] != source || source.Item.IsEmpty) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
        "The selected inventory slot changed before placement.");
    }

    var debit = new InventoryMutation(source.SlotId, source.Generation,
      source.Item.WithStack(source.Item.Stack - 1));
    OwnerCommitResult result = inventories.PrepareApply(inventory.PlayerId,
      inventory.Revision, [debit], out InventoryStore.PreparedMutation? prepared);
    if (prepared is null) {
      return result;
    }

    preparation = new InventoryItemPlacementPreparation(inventories, prepared);
    return result;
  }

  public OwnerCommitResult Validate() {
    return !_committed && _inventories.CanCommit(_prepared)
      ? OwnerCommitResult.Applied(_prepared.NextRevision)
      : OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
        "The selected inventory slot changed after placement was prepared.");
  }

  public void Commit() {
    OwnerCommitResult result = _inventories.CommitPrepared(_prepared, out _);
    if (result.Status != OwnerCommitStatus.Applied) {
      throw new InvalidOperationException("The prepared placement inventory debit was not valid.");
    }
    _committed = true;
  }
}
