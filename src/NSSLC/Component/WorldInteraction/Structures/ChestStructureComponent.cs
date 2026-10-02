using Terraria.WorldStorage;
using StorageTileCoordinate = Terraria.WorldStorage.TileCoordinate;

namespace Terraria.WorldInteraction.Structures;

public sealed class ChestStructureComponent
{
  public ChestSlot Slot { get; internal set; }

  public StorageTileCoordinate Anchor { get; internal set; }

  public string Name { get; internal set; } = string.Empty;

  public bool IsLegacyBankChest { get; internal set; }
}
