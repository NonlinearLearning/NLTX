namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackPlan
{
  public QuickStackPlan(IReadOnlyList<QuickStackTransferIntent> intents)
    : this(intents, Array.Empty<InventorySlotReference>(), Array.Empty<string>())
  {
  }

  public QuickStackPlan(
    IReadOnlyList<QuickStackTransferIntent> intents,
    IReadOnlyList<InventorySlotReference> remainingSources,
    IReadOnlyList<string> blockedDestinations)
  {
    ArgumentNullException.ThrowIfNull(intents);
    ArgumentNullException.ThrowIfNull(remainingSources);
    ArgumentNullException.ThrowIfNull(blockedDestinations);
    Intents = Array.AsReadOnly(intents.ToArray());
    RemainingSources = Array.AsReadOnly(remainingSources.ToArray());
    BlockedDestinations = Array.AsReadOnly(blockedDestinations.ToArray());
  }

  public IReadOnlyList<QuickStackTransferIntent> Intents { get; }

  public IReadOnlyList<InventorySlotReference> RemainingSources { get; }

  public IReadOnlyList<string> BlockedDestinations { get; }
}
