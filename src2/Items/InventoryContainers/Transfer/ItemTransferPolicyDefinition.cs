namespace Terraria.Items.InventoryContainers;

public sealed class ItemTransferPolicyDefinition
{
  public ItemTransferPolicyDefinition(
    bool longText = false,
    bool noText = false,
    bool canGoIntoVoidVault = false,
    bool noSound = false,
    bool noCoinMerge = false,
    ItemTransferPostAction postAction = ItemTransferPostAction.None)
  {
    LongText = longText;
    NoText = noText;
    CanGoIntoVoidVault = canGoIntoVoidVault;
    NoSound = noSound;
    NoCoinMerge = noCoinMerge;
    PostAction = postAction;
  }

  public bool LongText { get; }

  public bool NoText { get; }

  public bool CanGoIntoVoidVault { get; }

  public bool NoSound { get; }

  public bool NoCoinMerge { get; }

  public ItemTransferPostAction PostAction { get; }
}
