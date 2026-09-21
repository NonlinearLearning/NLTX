using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores weighted progress for the currently executing world-generation pass.
/// </summary>
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
