using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Layout;

public sealed class HallLine
{
  public HallLine(
    int lineId,
    int sourceEntryId,
    int targetEntryId,
    DungeonTilePoint sourcePoint,
    DungeonTilePoint targetPoint)
  {
    if (lineId < 0 || sourceEntryId < 0 || targetEntryId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lineId));
    }

    if (sourceEntryId == targetEntryId)
    {
      throw new ArgumentException("A hall line cannot connect an entry to itself.");
    }

    LineId = lineId;
    SourceEntryId = sourceEntryId;
    TargetEntryId = targetEntryId;
    SourcePoint = sourcePoint;
    TargetPoint = targetPoint;
  }

  public int LineId { get; }

  public int SourceEntryId { get; }

  public int TargetEntryId { get; }

  public DungeonTilePoint SourcePoint { get; }

  public DungeonTilePoint TargetPoint { get; }
}
