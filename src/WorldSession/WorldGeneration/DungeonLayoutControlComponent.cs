using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Owns dungeon layout scalars and the active index over opaque external dungeon records.
/// </summary>
public sealed class DungeonLayoutControlComponent
{
  private DungeonRecordSnapshot[] _records = Array.Empty<DungeonRecordSnapshot>();

  public DungeonLayoutControlComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int TLeft { get; private set; }

  public int TRight { get; private set; }

  public int TTop { get; private set; }

  public int TBottom { get; private set; }

  public int TRooms { get; private set; }

  public int LAltarX { get; private set; }

  public int LAltarY { get; private set; }

  public int CurrentDungeon { get; private set; }

  public IReadOnlyList<DungeonRecordSnapshot> Records =>
    Array.AsReadOnly(_records);

  public void SelectCurrentDungeon(int value)
  {
    CurrentDungeon = value < 0 ? 0 : value;
  }

  internal void ReplaceState(
    int tLeft,
    int tRight,
    int tTop,
    int tBottom,
    int tRooms,
    int lAltarX,
    int lAltarY,
    IReadOnlyList<DungeonRecordSnapshot> records,
    int currentDungeon)
  {
    ArgumentNullException.ThrowIfNull(records);
    if (currentDungeon < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentDungeon));
    }

    DungeonRecordSnapshot[] copiedRecords = new DungeonRecordSnapshot[records.Count];
    for (int index = 0; index < records.Count; index++)
    {
      copiedRecords[index] = records[index];
    }

    TLeft = tLeft;
    TRight = tRight;
    TTop = tTop;
    TBottom = tBottom;
    TRooms = tRooms;
    LAltarX = lAltarX;
    LAltarY = lAltarY;
    _records = copiedRecords;
    SelectCurrentDungeon(currentDungeon);
  }

  internal void ResetState()
  {
    TLeft = 0;
    TRight = 0;
    TTop = 0;
    TBottom = 0;
    TRooms = 0;
    LAltarX = 0;
    LAltarY = 0;
    _records = Array.Empty<DungeonRecordSnapshot>();
    CurrentDungeon = 0;
  }

  public DungeonLayoutSnapshot CreateSnapshot()
  {
    DungeonRecordSnapshot[] copy = new DungeonRecordSnapshot[_records.Length];
    Array.Copy(_records, copy, _records.Length);
    return new DungeonLayoutSnapshot(
      GenerationId,
      TLeft,
      TRight,
      TTop,
      TBottom,
      TRooms,
      LAltarX,
      LAltarY,
      Array.AsReadOnly(copy),
      CurrentDungeon);
  }
}
