using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存组合特殊种子的生成规则标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：notTheBeesAndForTheWorthyNoCelebration（第 280 行）； noTrapsAndForTheWorthyNoCelebration（第
/// 282 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1664 行。</para>
/// </remarks>
public sealed class SpecialSeedGenerationRuleFlagsComponent
{
  public SpecialSeedGenerationRuleFlagsComponent(
    long generationId,
    bool notTheBeesAndForTheWorthyNoCelebration,
    bool noTrapsAndForTheWorthyNoCelebration)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    NotTheBeesAndForTheWorthyNoCelebration =
      notTheBeesAndForTheWorthyNoCelebration;
    NoTrapsAndForTheWorthyNoCelebration =
      noTrapsAndForTheWorthyNoCelebration;
  }

  public long GenerationId { get; }

  public bool NotTheBeesAndForTheWorthyNoCelebration { get; }

  public bool NoTrapsAndForTheWorthyNoCelebration { get; }

  public SpecialSeedGenerationRuleFlagsSnapshot CreateSnapshot()
  {
    return new SpecialSeedGenerationRuleFlagsSnapshot(
      GenerationId,
      NotTheBeesAndForTheWorthyNoCelebration,
      NoTrapsAndForTheWorthyNoCelebration);
  }
}
