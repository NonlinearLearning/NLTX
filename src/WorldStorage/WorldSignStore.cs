namespace Terraria.WorldStorage;

public sealed class WorldSignStore
{
  private WorldSignState?[] _signs = Array.Empty<WorldSignState?>();
  private Dictionary<TileCoordinate, SignSlot> _signSlotsByAnchor = new();
  private int _activeSignCount;
  private long _mutationRevision;

  public int Capacity => _signs.Length;
  public int ActiveSignCount => _activeSignCount;
  public long MutationRevision => _mutationRevision;
}
