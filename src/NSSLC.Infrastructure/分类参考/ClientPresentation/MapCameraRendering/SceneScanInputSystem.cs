using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SceneScanInputSystem
{
  public void Apply(
    SceneScanInputComponent component,
    SceneScanRectangle? visualScanArea,
    Vector2 biomeScanCenterPositionInWorld,
    bool scanNpcPositions,
    Guid? perspectiveEntityId,
    SceneScanThresholds thresholds)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureFinite(biomeScanCenterPositionInWorld, nameof(biomeScanCenterPositionInWorld));
    EnsureThresholds(thresholds);
    component.Replace(
      visualScanArea,
      biomeScanCenterPositionInWorld,
      scanNpcPositions,
      perspectiveEntityId,
      thresholds);
  }

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  private static void EnsureThresholds(SceneScanThresholds thresholds)
  {
    if (thresholds.SnowTileMax <= 0 ||
      thresholds.MushroomTileThreshold < 0 ||
      thresholds.AssumedScreenWidth <= 0 ||
      thresholds.AssumedScreenHeight <= 0 ||
      thresholds.ZoneScanPadding < 0 ||
      thresholds.ZoneScanWidth <= 0 ||
      thresholds.ZoneScanHeight <= 0 ||
      thresholds.TownNpcWidth <= 0 ||
      thresholds.TownNpcHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(thresholds));
    }
  }
}
