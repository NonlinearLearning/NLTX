namespace Terraria.WorldStorage;

public sealed class LiquidWorkQueueState
{
  public const int DefaultCapacity = 25000;

  public Queue<LiquidBufferEntry> BufferedEntries = new();
  public Queue<LiquidWorkEntry> ActiveEntries = new();
  public HashSet<TileCoordinate> ScheduledCoordinates = new();
  public int Capacity = DefaultCapacity;

  public int ActiveCount => ActiveEntries.Count;
  public IReadOnlyCollection<LiquidWorkEntry> ActiveItems => ActiveEntries;
  public int BufferedCount => BufferedEntries.Count;
  public IReadOnlyCollection<LiquidBufferEntry> DeferredItems => BufferedEntries;
  public bool HasCapacity => ActiveCount < Capacity;
  public int ScheduledCount => ScheduledCoordinates.Count;
}
