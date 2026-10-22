using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存一次世界生成的步骤描述、计划版本和禁用步骤列表。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.WorldGenerator。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>主要源成员：_passes（第 262 行）。</para>
/// <para>重组说明：步骤描述、GenerationId、PlanVersion 和禁用步骤表是生成计划的显式表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-design.md。
/// </para>
/// <para>依据位置：第 184 行。</para>
/// <para>
/// 用途：执行器按描述列表读取步骤编号、实现版本和权重，再结合禁用列表选择执行内容，进度流程可读取总权重。
/// </para>
/// <para>
/// 当前实现：构造时复制输入，要求步骤编号唯一、权重有限且非负、版本为正数；禁用编号必须唯一并属于计划。TotalWeight 是全部描述的权重总和，包括禁用步骤。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C195，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
/// </remarks>
public sealed class WorldGenerationPlanComponent
{
  public WorldGenerationPlanComponent(
    long generationId,
    int planVersion,
    IReadOnlyList<GenerationPassDescriptor> passDescriptors,
    IReadOnlyCollection<string>? disabledPassIds = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (planVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(planVersion));
    }

    ArgumentNullException.ThrowIfNull(passDescriptors);
    List<GenerationPassDescriptor> descriptors = new(passDescriptors.Count);
    HashSet<string> descriptorIds = new(StringComparer.Ordinal);
    double totalWeight = 0;
    foreach (GenerationPassDescriptor descriptor in passDescriptors)
    {
      if (string.IsNullOrWhiteSpace(descriptor.Id) ||
          !double.IsFinite(descriptor.Weight) || descriptor.Weight < 0 ||
          descriptor.Version <= 0)
      {
        throw new ArgumentException(
          "Generation pass descriptors must contain valid identity, weight, and version values.",
          nameof(passDescriptors));
      }

      if (!descriptorIds.Add(descriptor.Id))
      {
        throw new ArgumentException(
          "Generation pass identifiers must be unique.",
          nameof(passDescriptors));
      }

      descriptors.Add(descriptor);
      totalWeight += descriptor.Weight;
      if (!double.IsFinite(totalWeight))
      {
        throw new ArgumentOutOfRangeException(
          nameof(passDescriptors),
          "The total generation pass weight must be finite.");
      }
    }

    List<string> disabledIds = disabledPassIds is null
      ? []
      : new List<string>(disabledPassIds.Count);
    if (disabledPassIds is not null)
    {
      HashSet<string> disabledSet = new(StringComparer.Ordinal);
      foreach (string passId in disabledPassIds)
      {
        ArgumentException.ThrowIfNullOrWhiteSpace(passId);
        if (!descriptorIds.Contains(passId) || !disabledSet.Add(passId))
        {
          throw new ArgumentException(
            "Disabled passes must be unique members of the plan.",
            nameof(disabledPassIds));
        }

        disabledIds.Add(passId);
      }
    }

    GenerationId = generationId;
    PlanVersion = planVersion;
    PassDescriptors = descriptors.AsReadOnly();
    DisabledPassIds = disabledIds.AsReadOnly();
    TotalWeight = totalWeight;
  }

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 此生成计划的正整数版本。
  /// </summary>
  public int PlanVersion { get; }

  /// <summary>
  /// 按输入顺序保存的生成步骤描述只读列表。
  /// </summary>
  public IReadOnlyList<GenerationPassDescriptor> PassDescriptors { get; }

  /// <summary>
  /// 全部步骤描述的权重总和，包括禁用步骤的权重。
  /// </summary>
  public double TotalWeight { get; }

  /// <summary>
  /// 明确禁用且属于此计划的步骤编号只读列表。
  /// </summary>
  public IReadOnlyList<string> DisabledPassIds { get; }
}
