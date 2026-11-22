namespace Terraria.Inventory;

/// <summary>Result of an inventory mutation, including the quantity committed.</summary>
public readonly record struct InventoryTransferResult(
    bool Succeeded,
    int Quantity,
    InventoryTransferFailure Failure) {
  public static InventoryTransferResult Success(int quantity) {
    return new InventoryTransferResult(true, quantity, InventoryTransferFailure.None);
  }

  public static InventoryTransferResult Rejected(InventoryTransferFailure failure) {
    return new InventoryTransferResult(false, 0, failure);
  }
}
