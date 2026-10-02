namespace Terraria.NonAuthoritative.WorldStorage.TileEntities;

public sealed class TileEntityKindComponent
{
  public TileEntityKindComponent(byte kindId)
  {
    KindId = kindId;
  }

  public byte KindId { get; }
}
