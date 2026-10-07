using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的猩红方向和感染翻转选择。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：crimsonLeft（第 276 行）； flipInfections（第 284 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1662 行。</para>
/// </remarks>
public sealed class InfectionAlignmentComponent
{
  public InfectionAlignmentComponent(
    long generationId,
    bool crimsonLeft,
    bool flipInfections)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    CrimsonLeft = crimsonLeft;
    FlipInfections = flipInfections;
  }

  public long GenerationId { get; }

  public bool CrimsonLeft { get; }

  public bool FlipInfections { get; }

  public InfectionAlignmentSnapshot CreateSnapshot()
  {
    return new InfectionAlignmentSnapshot(
      GenerationId,
      CrimsonLeft,
      FlipInfections);
  }
}
