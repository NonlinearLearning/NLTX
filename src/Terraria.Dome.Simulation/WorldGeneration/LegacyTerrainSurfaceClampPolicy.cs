using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTerrainSurfaceClampResult(
  double Surface,
  bool ResetFeatureRun,
  bool IsBeachColumn);

public static class LegacyTerrainSurfaceClampPolicy
{
  public static LegacyTerrainSurfaceClampResult Apply(
    double surface,
    int columnX,
    int leftBeachEnd,
    int rightBeachStart,
    int flatBeachPadding,
    double lowerSurface,
    double upperSurface,
    double beachSurfaceCap)
  {
    if (columnX < 0 || leftBeachEnd < 0 || rightBeachStart < leftBeachEnd ||
        flatBeachPadding < 0 || lowerSurface > upperSurface ||
        beachSurfaceCap < lowerSurface)
    {
      throw new ArgumentOutOfRangeException(nameof(columnX));
    }

    bool isBeachColumn = columnX < leftBeachEnd + flatBeachPadding ||
      columnX > rightBeachStart - flatBeachPadding;
    if (isBeachColumn)
    {
      return new LegacyTerrainSurfaceClampResult(
        Math.Clamp(surface, lowerSurface, beachSurfaceCap),
        ResetFeatureRun: false,
        IsBeachColumn: true);
    }

    if (surface < lowerSurface)
    {
      return new LegacyTerrainSurfaceClampResult(
        lowerSurface,
        ResetFeatureRun: true,
        IsBeachColumn: false);
    }

    if (surface > upperSurface)
    {
      return new LegacyTerrainSurfaceClampResult(
        upperSurface,
        ResetFeatureRun: true,
        IsBeachColumn: false);
    }

    return new LegacyTerrainSurfaceClampResult(
      surface,
      ResetFeatureRun: false,
      IsBeachColumn: false);
  }
}
