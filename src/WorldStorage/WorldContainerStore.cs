namespace Terraria.WorldStorage;

public sealed class WorldContainerStore
{
  private WorldChestState?[] _chests = Array.Empty<WorldChestState?>();
  private Dictionary<TileCoordinate, ChestSlot> _chestSlotsByAnchor = new();
  private int _activeChestCount;
  private long _mutationRevision;

  public int Capacity => _chests.Length;
  public int ActiveChestCount => _activeChestCount;
  public long MutationRevision => _mutationRevision;
}
