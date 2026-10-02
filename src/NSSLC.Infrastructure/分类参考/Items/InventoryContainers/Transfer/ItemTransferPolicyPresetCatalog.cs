namespace Terraria.Items.InventoryContainers;

public static class ItemTransferPolicyPresetCatalog
{
  private static readonly IReadOnlyDictionary<string, ItemTransferPolicyDefinition> Presets =
    new Dictionary<string, ItemTransferPolicyDefinition>(StringComparer.Ordinal)
    {
      ["GiftRecieved"] = new ItemTransferPolicyDefinition(longText: true),
      ["LootAllFromBank"] = new ItemTransferPolicyDefinition(),
      ["LootAllFromChest"] = new ItemTransferPolicyDefinition(canGoIntoVoidVault: true),
      ["PickupItemFromWorld"] = new ItemTransferPolicyDefinition(canGoIntoVoidVault: true),
      ["QuickTransferFromSlot"] = new ItemTransferPolicyDefinition(noText: true),
      ["ReturnItemFromSlot"] = new ItemTransferPolicyDefinition(noText: true),
      ["ReturnItemShowAsNew"] = new ItemTransferPolicyDefinition(
        noText: true,
        postAction: ItemTransferPostAction.MakeNewAndShiny),
      ["ItemCreatedFromItemUsage"] = new ItemTransferPolicyDefinition(),
      ["RefundConsumedItem"] = new ItemTransferPolicyDefinition(
        noText: true,
        canGoIntoVoidVault: true,
        noSound: true),
      ["ReturnItemShowAsNewNoCoinMerge"] = new ItemTransferPolicyDefinition(
        noText: true,
        noCoinMerge: true,
        postAction: ItemTransferPostAction.MakeNewAndShiny)
    };

  public static ItemTransferPolicyDefinition GiftRecieved => Presets["GiftRecieved"];

  public static ItemTransferPolicyDefinition LootAllFromBank => Presets["LootAllFromBank"];

  public static ItemTransferPolicyDefinition LootAllFromChest => Presets["LootAllFromChest"];

  public static ItemTransferPolicyDefinition PickupItemFromWorld => Presets["PickupItemFromWorld"];

  public static ItemTransferPolicyDefinition QuickTransferFromSlot => Presets["QuickTransferFromSlot"];

  public static ItemTransferPolicyDefinition ReturnItemFromSlot => Presets["ReturnItemFromSlot"];

  public static ItemTransferPolicyDefinition ReturnItemShowAsNew => Presets["ReturnItemShowAsNew"];

  public static ItemTransferPolicyDefinition ItemCreatedFromItemUsage => Presets["ItemCreatedFromItemUsage"];

  public static ItemTransferPolicyDefinition RefundConsumedItem => Presets["RefundConsumedItem"];

  public static ItemTransferPolicyDefinition ReturnItemShowAsNewNoCoinMerge =>
    Presets["ReturnItemShowAsNewNoCoinMerge"];

  public static bool TryGet(
    string presetName,
    out ItemTransferPolicyDefinition? policy)
  {
    if (string.IsNullOrWhiteSpace(presetName))
    {
      policy = null;
      return false;
    }

    return Presets.TryGetValue(presetName, out policy);
  }
}
