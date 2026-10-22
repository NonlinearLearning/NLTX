using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成计划中的结构区域及需要保护的结构区域。
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
/// <para>
/// 用途：结构放置流程读取 PlannedStructures，后续地形修改流程读取 ProtectedStructures，避免按自身规则修改受保护范围。
/// </para>
/// <para>
/// 当前实现：构造时校验会话编号非负并复制两个矩形列表，未提供列表时为空。区域重叠判断、放置和保护执行由外部生成流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C200，L2（通用可选玩法）。分级理由：生态采样、结构保留区、地层统计、扫描和地形变换适用于可编辑／动态世界，固定小地图可减少。
/// </para>
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

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 计划放置结构的区域只读列表。
  /// </summary>
  public IReadOnlyList<WorldGenerationRectangle> PlannedStructures { get; }

  /// <summary>
  /// 已登记为保护范围的结构区域只读列表。
  /// </summary>
  public IReadOnlyList<WorldGenerationRectangle> ProtectedStructures { get; }
}
