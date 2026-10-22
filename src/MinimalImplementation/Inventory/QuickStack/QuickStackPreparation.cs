using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>
/// Immutable owner mutations calculated without changing inventory or chest state.
/// </summary>
public sealed record QuickStackPreparation
{
  public QuickStackPreparationStatus Status { get; }
  public IReadOnlyList<InventoryMutation> InventoryMutations { get; }
  public IReadOnlyList<ChestItemMutationBatch> ChestMutations { get; }
  public IReadOnlyList<ushort> BlockedChestIds { get; }

  public bool IsPrepared => Status == QuickStackPreparationStatus.Prepared;

  internal QuickStackPreparation(
    QuickStackPreparationStatus status,
    IReadOnlyCollection<InventoryMutation> inventoryMutations,
    IReadOnlyCollection<ChestItemMutationBatch> chestMutations,
    IReadOnlyCollection<ushort> blockedChestIds)
  {
    ArgumentNullException.ThrowIfNull(inventoryMutations);
    ArgumentNullException.ThrowIfNull(chestMutations);
    ArgumentNullException.ThrowIfNull(blockedChestIds);
    Status = status;
    InventoryMutations = Array.AsReadOnly(inventoryMutations.ToArray());
    ChestMutations = Array.AsReadOnly(chestMutations.ToArray());
    BlockedChestIds = Array.AsReadOnly(blockedChestIds.ToArray());
  }
}
