using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores weighted progress for the currently executing world-generation pass.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成的消息、当前进度和加权总进度。</para>
/// <para>拆分来源：Terraria.WorldBuilding.GenerationProgress。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenerationProgress.cs。</para>
/// <para>
/// 主要源成员：TotalWeight（第 11 行）； CurrentPassWeight（第 13 行）； Message（第 15 行）； MessageNoFormatting（第
/// 27 行）； Value（第 39 行）； TotalWeightedProgress（第 51 行）； TotalProgress（第 59 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 272 行。</para>
/// </remarks>
public sealed class WorldGenerationProgressComponent
{
  public WorldGenerationProgressComponent(
    string messageNoFormatting = "",
    double value = 0,
    double totalWeightedProgress = 0,
    double totalWeight = 0,
    double currentPassWeight = 1.0)
  {
    ReplaceState(
      messageNoFormatting,
      value,
      totalWeightedProgress,
      totalWeight,
      currentPassWeight);
  }

  public string Message => string.Format(
    MessageNoFormatting.Replace("%", "{0:0.0%}", StringComparison.Ordinal),
    Value);

  public string MessageNoFormatting { get; private set; } = string.Empty;

  public double Value { get; private set; }

  public double TotalWeightedProgress { get; private set; }

  public double TotalWeight { get; private set; }

  public double CurrentPassWeight { get; private set; }

  public double TotalProgress => TotalWeight == 0
    ? 0
    : (Value * CurrentPassWeight + TotalWeightedProgress) / TotalWeight;

  internal void ReplaceState(
    string messageNoFormatting,
    double value,
    double totalWeightedProgress,
    double totalWeight,
    double currentPassWeight)
  {
    ArgumentNullException.ThrowIfNull(messageNoFormatting);
    ValidateFiniteNonNegative(totalWeightedProgress, nameof(totalWeightedProgress));
    ValidateFiniteNonNegative(totalWeight, nameof(totalWeight));
    ValidateFiniteNonNegative(currentPassWeight, nameof(currentPassWeight));

    MessageNoFormatting = messageNoFormatting;
    Value = Clamp(value, nameof(value));
    TotalWeightedProgress = totalWeightedProgress;
    TotalWeight = totalWeight;
    CurrentPassWeight = currentPassWeight;
  }

  private static double Clamp(double value, string parameterName)
  {
    if (!double.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return Math.Clamp(value, 0, 1);
  }

  private static void ValidateFiniteNonNegative(
    double value,
    string parameterName)
  {
    if (!double.IsFinite(value) || value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
