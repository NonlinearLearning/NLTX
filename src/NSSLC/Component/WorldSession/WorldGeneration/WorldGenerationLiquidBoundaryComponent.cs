using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的水线与熔岩线。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：lavaLine（第 62 行）； waterLine（第 64 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 510 行。</para>
/// </remarks>
public sealed class WorldGenerationLiquidBoundaryComponent
{
  public WorldGenerationLiquidBoundaryComponent(
    long generationId,
    int lavaLine = 0,
    int waterLine = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    LavaLine = lavaLine;
    WaterLine = waterLine;
  }

  public long GenerationId { get; }

  public int LavaLine { get; private set; }

  public int WaterLine { get; private set; }

  internal void ReplaceBoundaries(int lavaLine, int waterLine)
  {
    LavaLine = lavaLine;
    WaterLine = waterLine;
  }

  public WorldGenerationLiquidBoundarySnapshot CreateSnapshot()
  {
    return new WorldGenerationLiquidBoundarySnapshot(
      GenerationId,
      LavaLine,
      WaterLine);
  }
}
