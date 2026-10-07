using Terraria.Items;

namespace Terraria.WorldStorage;

public sealed class WorldChestState
{
  public ChestSlot Slot { get; internal set; }
  public TileCoordinate Anchor { get; internal set; }
  public ItemState[] Items { get; internal set; } = Array.Empty<ItemState>();
  public int ItemCapacity { get; internal set; }
  public string Name { get; internal set; } = string.Empty;
  public bool IsLegacyBankChest { get; internal set; }
}
