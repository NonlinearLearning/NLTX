using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the candidate structure-planning collections for a world-generation scope.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成已规划和受保护的结构区域。</para>
/// <para>拆分来源：Terraria.WorldBuilding.StructureMap。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/StructureMap.cs。</para>
/// <para>主要源成员：_structures（第 11 行）； _protectedStructures（第 14 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p20-world-generation-actions-shapes-component-design.md。
/// </para>
/// <para>依据位置：第 828 行。</para>
/// </remarks>
public sealed class WorldStructurePlanningAndMasksComponent
{
  public WorldStructurePlanningAndMasksComponent(
    long generationId,
    IReadOnlyList<WorldGenerationRectangle>? plannedStructures = null,
    IReadOnlyList<WorldGenerationRectangle>? protectedStructures = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    PlannedStructures = plannedStructures is null
      ? []
      : new List<WorldGenerationRectangle>(plannedStructures).AsReadOnly();
    ProtectedStructures = protectedStructures is null
      ? []
      : new List<WorldGenerationRectangle>(protectedStructures).AsReadOnly();
  }

  public long GenerationId { get; }

  public IReadOnlyList<WorldGenerationRectangle> PlannedStructures { get; }

  public IReadOnlyList<WorldGenerationRectangle> ProtectedStructures { get; }
}
