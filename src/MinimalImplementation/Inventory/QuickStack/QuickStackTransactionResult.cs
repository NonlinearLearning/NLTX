using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.QuickStack;

public sealed record QuickStackTransactionResult
{
  public OwnerCommitResult Commit { get; }
  public QuickStackPreparationStatus? PreparationStatus { get; }
  public InventorySnapshot? Inventory { get; }
  public IReadOnlyList<ChestSnapshot> Chests { get; }
  public IReadOnlyList<InventoryMutation> InventoryMutations { get; }
  public IReadOnlyList<ChestItemMutationBatch> ChestMutations { get; }
  public IReadOnlyList<ushort> BlockedChestIds { get; }

  public bool IsApplied => Commit.Status == OwnerCommitStatus.Applied;

  internal QuickStackTransactionResult(
    OwnerCommitResult commit,
    QuickStackPreparationStatus? preparationStatus,
    InventorySnapshot? inventory,
    IReadOnlyCollection<ChestSnapshot> chests,
    IReadOnlyCollection<InventoryMutation> inventoryMutations,
    IReadOnlyCollection<ChestItemMutationBatch> chestMutations,
    IReadOnlyCollection<ushort> blockedChestIds)
  {
    ArgumentNullException.ThrowIfNull(commit);
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(blockedChestIds);
    Commit = commit;
    PreparationStatus = preparationStatus;
    Inventory = inventory;
    Chests = Array.AsReadOnly(chests.ToArray());
    InventoryMutations = Array.AsReadOnly(inventoryMutations.ToArray());
    ChestMutations = Array.AsReadOnly(chestMutations.ToArray());
    BlockedChestIds = Array.AsReadOnly(blockedChestIds.ToArray());
  }
}
