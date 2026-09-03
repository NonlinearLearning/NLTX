using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacySurfaceCavesPassDefinition(
  double NarrowDensity,
  double MediumDensity,
  double DeepDensity,
  double HorizontalDensity,
  int RemixVerticalMultiplier,
  int CavererReferenceWorldWidth,
  int CavererReferenceInvocationCount)
{
  public int CalculateInvocationCount(
    LegacySurfaceCavesVerticalFamily family,
    int width,
    bool isRemixWorld)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    double density = family switch
    {
      LegacySurfaceCavesVerticalFamily.Narrow => NarrowDensity,
      LegacySurfaceCavesVerticalFamily.Medium => MediumDensity,
      LegacySurfaceCavesVerticalFamily.Deep => DeepDensity,
      LegacySurfaceCavesVerticalFamily.Horizontal => HorizontalDensity,
      _ => throw new ArgumentOutOfRangeException(nameof(family))
    };
    int count = checked((int)(width * density));
    if (isRemixWorld && family != LegacySurfaceCavesVerticalFamily.Horizontal)
    {
      count = checked(count * RemixVerticalMultiplier);
    }

    return count;
  }

  public int CalculateCavererInvocationCount(int width)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)(width * (double)CavererReferenceInvocationCount /
      CavererReferenceWorldWidth));
  }

  public void Validate()
  {
    if (!double.IsFinite(NarrowDensity) || NarrowDensity < 0.0 ||
        !double.IsFinite(MediumDensity) || MediumDensity < 0.0 ||
        !double.IsFinite(DeepDensity) || DeepDensity < 0.0 ||
        !double.IsFinite(HorizontalDensity) || HorizontalDensity < 0.0 ||
        RemixVerticalMultiplier <= 0 || CavererReferenceWorldWidth <= 0 ||
        CavererReferenceInvocationCount < 0)
    {
      throw new ArgumentException(
        "SurfaceCaves pass definition contains an invalid source contract.",
        nameof(NarrowDensity));
    }
  }
}

public static class LegacySurfaceCavesPassDefinitionFactory
{
  public static LegacySurfaceCavesPassDefinition CreateDefault()
  {
    return new LegacySurfaceCavesPassDefinition(
      NarrowDensity: 0.002,
      MediumDensity: 0.0007,
      DeepDensity: 0.0003,
      HorizontalDensity: 0.0004,
      RemixVerticalMultiplier: 3,
      CavererReferenceWorldWidth: 4200,
      CavererReferenceInvocationCount: 5);
  }
}
