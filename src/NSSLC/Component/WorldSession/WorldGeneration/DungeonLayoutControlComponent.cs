using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Owns dungeon layout scalars and the active index over opaque external dungeon records.
/// </summary>
/// <remarks>
/// <para>职责：保存地牢生成的范围、房间、祭坛和当前布局记录。</para>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：tLeft（第 186 行）； tRight（第 188 行）； tTop（第 190 行）； tBottom（第 192 行）； tRooms（第 194 行）；
/// lAltarX（第 196 行）； lAltarY（第 198 行）； _currentDungeon（第 202 行）； CurrentDungeon（第 286 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1131 行。</para>
/// </remarks>
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
