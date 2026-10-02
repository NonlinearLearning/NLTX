namespace Terraria.WorldStorage;

public sealed class TileEntityStore
{
  private Dictionary<TileEntityId, TileEntityRecord> _byId = new();
  private Dictionary<TileCoordinate, TileEntityId> _idsByAnchor = new();
  private int _nextId;
  private long _mutationRevision;

  public int Count => _byId.Count;
  public int NextId => _nextId;
  public long MutationRevision => _mutationRevision;
}
