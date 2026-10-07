using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存海洋生成阶段的沙漠方块检查控制。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：skipDesertTileCheck（第 136 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 701 行。</para>
/// </remarks>
public sealed class OceanBiomePassControlComponent
{
  public OceanBiomePassControlComponent(
    long generationId,
    bool skipDesertTileCheck = false)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    SkipDesertTileCheck = skipDesertTileCheck;
  }

  public long GenerationId { get; }

  public bool SkipDesertTileCheck { get; private set; }

  internal void SetSkipDesertTileCheck(bool value)
  {
    SkipDesertTileCheck = value;
  }

  internal void Reset()
  {
    SkipDesertTileCheck = false;
  }
}
