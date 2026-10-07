using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成步骤列表、权重和禁用步骤。
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

  public long GenerationId { get; }

  public int PlanVersion { get; }

  public IReadOnlyList<GenerationPassDescriptor> PassDescriptors { get; }

  public double TotalWeight { get; }

  public IReadOnlyList<string> DisabledPassIds { get; }
}
