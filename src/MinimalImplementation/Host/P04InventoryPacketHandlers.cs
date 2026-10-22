using MinimalImplementation.Server.Inventory.Crafting;
using MinimalImplementation.Server.Inventory.QuickStack;
using MinimalImplementation.Server.Modules;

namespace MinimalImplementation.Server.Host;

/// <summary>Root-owned registration bundle for P04's direct packet and Packet 82 module handlers.</summary>
public sealed class P04InventoryPacketHandlers
{
  public P04InventoryPacketHandlers(
    QuickStackPacket85Handler quickStackPacket85,
    CraftingModule11Handler craftingModule11)
  {
    ArgumentNullException.ThrowIfNull(quickStackPacket85);
    ArgumentNullException.ThrowIfNull(craftingModule11);
    if (craftingModule11.ModuleId != 11)
    {
      throw new ArgumentException("P04 crafting must own Packet 82 module 11.", nameof(craftingModule11));
    }

    QuickStackPacket85 = quickStackPacket85;
    CraftingModule11 = craftingModule11;
    ModuleHandlers = Array.AsReadOnly<IModuleHandler>([craftingModule11]);
  }

  public QuickStackPacket85Handler QuickStackPacket85 { get; }

  public CraftingModule11Handler CraftingModule11 { get; }

  /// <summary>Module handlers to merge into the root Packet 82 registry.</summary>
  public IReadOnlyList<IModuleHandler> ModuleHandlers { get; }
}
