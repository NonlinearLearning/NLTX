using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTunnelsPassDefinition(
  int ReferenceWorldWidth,
  double BaseDensity,
  double RemixMultiplier,
  int MinimumStartXInset,
  int MaximumStartXInset,
  int CenterExclusionHalfWidth,
  int TunnelPointCount,
  int MinimumVerticalOffset,
  int MaximumVerticalOffsetExclusive,
  int MinimumHorizontalStep,
  int MaximumHorizontalStepExclusive,
  int MaximumTunnelCount)
{
  public int CalculateInvocationCount(int width, bool isRemixWorld)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int count = (int)(width * BaseDensity);
    return isRemixWorld ? (int)(count * RemixMultiplier) : count;
  }

  public bool IsCandidateXAccepted(
    int x,
    int width,
    bool isRemixWorld,
    bool isTenthAnniversaryWorld)
  {
    if (x < MinimumStartXInset || x >= width - MaximumStartXInset)
    {
      return false;
    }

    if (isRemixWorld)
    {
      return true;
    }

    if (isTenthAnniversaryWorld)
    {
      return x >= width * 0.2 && x < width * 0.8;
    }

    return x <= width * 0.4 || x >= width * 0.6;
  }

  public (int MinimumInclusive, int MaximumExclusive) GetCandidateXRange(
    int width,
    bool isRemixWorld,
    bool isTenthAnniversaryWorld)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return !isRemixWorld && isTenthAnniversaryWorld
      ? ((int)(width * 0.2), (int)(width * 0.8))
      : (MinimumStartXInset, width - MaximumStartXInset);
  }

  public void Validate()
  {
    if (ReferenceWorldWidth <= 0 || BaseDensity < 0 || RemixMultiplier < 0 ||
        MinimumStartXInset < 0 || MaximumStartXInset < 0 || CenterExclusionHalfWidth < 0 ||
        TunnelPointCount <= 0 || MinimumVerticalOffset < 0 ||
        MaximumVerticalOffsetExclusive <= MinimumVerticalOffset || MinimumHorizontalStep < 0 ||
        MaximumHorizontalStepExclusive <= MinimumHorizontalStep || MaximumTunnelCount < 0)
    {
      throw new InvalidOperationException("Tunnels pass definition contains an invalid source contract.");
    }
  }

  public static LegacyTunnelsPassDefinition CreateDefault()
  {
    return new LegacyTunnelsPassDefinition(
      ReferenceWorldWidth: 4200,
      BaseDensity: 0.0015,
      RemixMultiplier: 1.5,
      MinimumStartXInset: 450,
      MaximumStartXInset: 450,
      CenterExclusionHalfWidth: 0,
      TunnelPointCount: 10,
      MinimumVerticalOffset: 11,
      MaximumVerticalOffsetExclusive: 16,
      MinimumHorizontalStep: 5,
      MaximumHorizontalStepExclusive: 11,
      MaximumTunnelCount: 1000);
  }
}
