using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Owns bounded floating-island house metadata and the separate generation targets.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成的浮岛房屋和空中湖泊记录。</para>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：skyLakes（第 206 行）； skyIslandHouseCount（第 214 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1141 行。</para>
/// </remarks>
public sealed class FloatingIslandPlacementStateComponent
{
  public const int Capacity = 300;

  private readonly FloatingIslandHouseSnapshot[] _houses =
    new FloatingIslandHouseSnapshot[Capacity];

  public FloatingIslandPlacementStateComponent(
    long generationId,
    int skyLakes = 0,
    int skyIslandHouseCount = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    SkyLakes = skyLakes;
    SkyIslandHouseCount = skyIslandHouseCount;
  }

  public long GenerationId { get; }

  public int SkyLakes { get; private set; }

  public int SkyIslandHouseCount { get; private set; }

  public int Count { get; private set; }

  public bool TryAppend(FloatingIslandHouseSnapshot house)
  {
    if (house.GenerationId != GenerationId)
    {
      throw new ArgumentException(
        "Floating-island metadata belongs to another generation.",
        nameof(house));
    }

    if (Count >= Capacity)
    {
      return false;
    }

    _houses[Count] = house;
    Count++;
    return true;
  }

  public void ClearHouses()
  {
    Count = 0;
  }

  internal void ResetProgress()
  {
    Count = 0;
    SkyIslandHouseCount = 0;
  }

  internal void ReplaceState(
    int skyLakes,
    int skyIslandHouseCount,
    int count,
    IReadOnlyList<FloatingIslandHouseSnapshot> houses)
  {
    if (count < 0 || count > Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    ArgumentNullException.ThrowIfNull(houses);
    if (houses.Count != count)
    {
      throw new ArgumentException(
        "Floating-island metadata must contain exactly the used entries.",
        nameof(houses));
    }

    for (int index = 0; index < count; index++)
    {
      if (houses[index].GenerationId != GenerationId)
      {
        throw new ArgumentException(
          "Floating-island metadata belongs to another generation.",
          nameof(houses));
      }

      _houses[index] = houses[index];
    }

    SkyLakes = skyLakes;
    SkyIslandHouseCount = skyIslandHouseCount;
    Count = count;
  }

  public FloatingIslandPlacementSnapshot CreateSnapshot()
  {
    FloatingIslandHouseSnapshot[] copy = new FloatingIslandHouseSnapshot[Count];
    Array.Copy(_houses, copy, Count);
    return new FloatingIslandPlacementSnapshot(
      GenerationId,
      SkyLakes,
      SkyIslandHouseCount,
      Capacity,
      Count,
      Array.AsReadOnly(copy));
  }
}
