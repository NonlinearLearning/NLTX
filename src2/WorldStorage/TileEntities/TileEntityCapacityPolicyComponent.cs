namespace Terraria.NonAuthoritative.WorldStorage.TileEntities;

public sealed class TileEntityCapacityPolicyComponent
{
  public TileEntityCapacityPolicyComponent(int maxEntitiesPerChunk)
  {
    if (maxEntitiesPerChunk <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxEntitiesPerChunk));
    }

    MaxEntitiesPerChunk = maxEntitiesPerChunk;
  }

  public int MaxEntitiesPerChunk { get; }
}
