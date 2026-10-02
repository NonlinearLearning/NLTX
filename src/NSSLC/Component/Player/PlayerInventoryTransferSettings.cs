namespace Terraria.Player;

public readonly record struct PlayerInventoryTransferSettings(
  bool LongText = false,
  bool NoText = false,
  bool CanGoIntoVoidVault = false,
  bool NoSound = false,
  bool NoCoinMerge = false,
  bool MakeNewAndShiny = false)
{
  public static PlayerInventoryTransferSettings PickupItemFromWorld =>
    new(CanGoIntoVoidVault: true);

  public static PlayerInventoryTransferSettings LootAllFromChest =>
    new(CanGoIntoVoidVault: true);

  public static PlayerInventoryTransferSettings QuickTransferFromSlot =>
    new(NoText: true);

  public static PlayerInventoryTransferSettings ReturnItemFromSlot =>
    new(NoText: true);

  public static PlayerInventoryTransferSettings ReturnItemShowAsNew =>
    new(NoText: true, MakeNewAndShiny: true);

  public static PlayerInventoryTransferSettings RefundConsumedItem =>
    new(NoText: true, CanGoIntoVoidVault: true, NoSound: true);

  public static PlayerInventoryTransferSettings ReturnItemShowAsNewNoCoinMerge =>
    new(NoText: true, MakeNewAndShiny: true, NoCoinMerge: true);
}
