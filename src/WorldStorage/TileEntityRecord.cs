namespace Terraria.WorldStorage;

public sealed class TileEntityRecord
{
  public TileEntityId Id { get; internal set; }
  public TileEntityTypeId Type { get; internal set; }
  public TileCoordinate Anchor { get; internal set; }
  public bool RequiresUpdates { get; internal set; }
}
