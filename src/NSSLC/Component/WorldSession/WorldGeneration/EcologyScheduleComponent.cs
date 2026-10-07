using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生态采样位置、变更预算和感染传播许可。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：AllowedToSpreadInfections（第 4185 行）。</para>
/// <para>重组说明：GenerationId、ScheduleRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-world-generation-and-ecology-component-code-draft.md。
/// </para>
/// <para>依据位置：第 869 行。</para>
/// </remarks>
public sealed class EcologyScheduleComponent
{
  public EcologyScheduleComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public bool IsInfectionSpreadAllowed { get; init; }

  public int? OvergroundSampleX { get; init; }

  public int? OvergroundSampleY { get; init; }

  public int? UndergroundSampleX { get; init; }

  public int? UndergroundSampleY { get; init; }

  public int? WorldUpdateRate { get; init; }

  public int? EcologyMutationBudget { get; init; }

  public ulong ScheduleRevision { get; init; }

  public bool IsEcologyPropagationEnabled =>
    IsInfectionSpreadAllowed && WorldUpdateRate is > 0;

  public bool HasConfiguredBudget => EcologyMutationBudget is >= 0;
}
