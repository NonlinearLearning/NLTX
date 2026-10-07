using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存地牢特殊奖励是否已经生成。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：generatedShadowKey（第 208 行）； generatedRamRune（第 210 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1142 行。</para>
/// </remarks>
public sealed class DungeonSpecialRewardGenerationComponent
{
  public DungeonSpecialRewardGenerationComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public bool GeneratedShadowKey { get; private set; }

  public bool GeneratedRamRune { get; private set; }

  internal void ReplaceState(
    bool generatedShadowKey,
    bool generatedRamRune)
  {
    GeneratedShadowKey = generatedShadowKey;
    GeneratedRamRune = generatedRamRune;
  }

  internal void ResetState()
  {
    GeneratedShadowKey = false;
    GeneratedRamRune = false;
  }

  public DungeonSpecialRewardGenerationSnapshot CreateSnapshot()
  {
    return new DungeonSpecialRewardGenerationSnapshot(
      GenerationId,
      GeneratedShadowKey,
      GeneratedRamRune);
  }
}
