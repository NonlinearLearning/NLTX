using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Owns the generation-time coordinate handoff from fallen-log placement to Flowers.
/// </summary>
/// <remarks>
/// <para>职责：保存倒木生成向花朵生成传递的坐标。</para>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：logX（第 248 行）； logY（第 250 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1401 行。</para>
/// </remarks>
public sealed class FallenLogFlowerHandoffComponent
{
  public FallenLogFlowerHandoffComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    Reset();
  }

  public long GenerationId { get; }

  public int LogX { get; private set; }

  public int LogY { get; private set; }

  public bool HasPendingLog => LogX >= 0;

  public void Publish(TilePosition position)
  {
    LogX = position.X;
    LogY = position.Y;
  }

  public bool TryConsume(out TilePosition position)
  {
    if (!HasPendingLog)
    {
      position = default;
      return false;
    }

    position = new TilePosition(LogX, LogY);
    LogX = -1;
    return true;
  }

  public void Reset()
  {
    LogX = -1;
    LogY = -1;
  }

  public FallenLogFlowerHandoffSnapshot CreateSnapshot()
  {
    return new FallenLogFlowerHandoffSnapshot(
      GenerationId,
      LogX,
      LogY,
      HasPendingLog);
  }
}
