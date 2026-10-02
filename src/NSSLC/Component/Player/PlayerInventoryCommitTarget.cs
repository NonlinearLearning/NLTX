namespace Terraria.Player;

public readonly record struct PlayerInventoryCommitTarget(
  PlayerInventoryCommitTargetKind Kind,
  int SlotIndex)
{
  public bool IsMainInventory =>
    Kind == PlayerInventoryCommitTargetKind.MainInventory;

  public bool IsVoidVault =>
    Kind == PlayerInventoryCommitTargetKind.VoidVault;
}
