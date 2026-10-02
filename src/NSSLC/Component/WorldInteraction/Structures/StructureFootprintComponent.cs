using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Structures;

public sealed class StructureFootprintComponent
{
  // 与 TileEntityAnchorComponent.Origin 的一致性是组合不变量。
  public TileCoordinate Origin { get; internal set; }

  public int Width { get; internal set; } = 1;

  public int Height { get; internal set; } = 1;

  public ushort? HostTileType { get; internal set; }
}
