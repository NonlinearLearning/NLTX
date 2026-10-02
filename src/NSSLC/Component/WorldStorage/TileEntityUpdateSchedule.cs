namespace Terraria.WorldStorage;

public sealed class TileEntityUpdateSchedule
{
  private HashSet<TileEntityId> _scheduledIds = new();
  private TileEntityId[] _tickSnapshot = Array.Empty<TileEntityId>();
  private long _scheduleRevision;

  public int Count => _scheduledIds.Count;
  public long ScheduleRevision => _scheduleRevision;
}
