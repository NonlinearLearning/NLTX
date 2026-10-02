namespace Terraria.NonAuthoritative.WorldStorage.Tiles;

public sealed class TileCellMaterialComponent
{
  public TileCellMaterialComponent(ushort tileType, ushort wallType)
  {
    TileType = tileType;
    WallType = wallType;
  }

  public ushort TileType { get; }

  public ushort WallType { get; }
}
