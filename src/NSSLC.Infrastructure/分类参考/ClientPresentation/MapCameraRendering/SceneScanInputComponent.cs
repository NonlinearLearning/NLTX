using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SceneScanInputComponent
{
  public SceneScanRectangle? VisualScanArea { get; private set; }

  public Vector2 BiomeScanCenterPositionInWorld { get; private set; }

  public bool ScanNpcPositions { get; private set; }

  public Guid? PerspectiveEntityId { get; private set; }

  public SceneScanThresholds Thresholds { get; private set; } =
    SceneScanThresholds.Default;

  public uint Revision { get; private set; }

  internal void Replace(
    SceneScanRectangle? visualScanArea,
    Vector2 biomeScanCenterPositionInWorld,
    bool scanNpcPositions,
    Guid? perspectiveEntityId,
    SceneScanThresholds thresholds)
  {
    VisualScanArea = visualScanArea?.Normalize();
    BiomeScanCenterPositionInWorld = biomeScanCenterPositionInWorld;
    ScanNpcPositions = scanNpcPositions;
    PerspectiveEntityId = perspectiveEntityId;
    Thresholds = thresholds;
    Revision++;
  }
}
