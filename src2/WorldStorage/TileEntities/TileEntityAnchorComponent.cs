namespace Terraria.NonAuthoritative.WorldStorage.TileEntities;

public sealed class TileEntityAnchorComponent
{
  public TileEntityAnchorComponent(short anchorX, short anchorY)
  {
    AnchorX = anchorX;
    AnchorY = anchorY;
  }

  public short AnchorX { get; }

  public short AnchorY { get; }
}
