namespace Terraria.Player;

public interface IPlayerInventoryItemQuery
{
  bool TryGetItem(
    ItemEntityRef item,
    out PlayerInventoryItemSnapshot snapshot);

  IReadOnlyList<PlayerInventoryItemSnapshot> VoidVaultItems { get; }

  bool IsVoidVaultEnabled { get; }

  bool CanVoidVaultAccept(PlayerInventoryItemSnapshot item);
}
